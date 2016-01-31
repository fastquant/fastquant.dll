using System;
using System.IO;

namespace SmartQuant
{
    public class DataSeries : IDataSeries
    {
        private DataFile dataFile;

        internal DataKey lastUpdateDKey;
        internal DataKey lastReadDKey;
        internal DataKey lastDeleteDKey;
        internal DataKey lastWriteDKey;

        internal bool readOpened;
        internal bool writeOpened;
        internal bool changed;

        internal IdArray<DataKey> dataKeys;

        internal long position1;
        internal long position2;
        internal long dKeysKeyPosition;

        internal int bufferCount;
        internal ObjectKey oKey;
        internal ObjectKey dataKeysKey;

        public string Name { get; private set; }
            
        public string Description { get; }

        public bool CacheObjects { get; set; }

        public long Count { get; private set; }

        public DataObject this[long index] => Get(index);

        public DataObject this[DateTime dateTime] => Get(dateTime);

        public DateTime DateTime1 { get; private set; }

        public DateTime DateTime2 { get; private set; }

        public object Sync { get; } = new object();

        public DataSeries()
        {
        }

        public DataSeries(string name)
        {
            Name = name;
        }

        public void Add(DataObject obj)
        {
            lock (Sync)
            {
                if (obj.DateTime.Ticks == 0)
                {
                    Console.WriteLine("DataSeries::Add Error: can not add object with DateTime = 0");
                    return;
                }

                if (!this.writeOpened)
                    OpenWrite();

                if (Count == 0)
                {
                    DateTime1 = obj.DateTime;
                    DateTime2 = obj.DateTime;
                }
                else
                {
                    if (obj.DateTime < DateTime2)
                    {
                        SaveDataObject(obj);
                        return;
                    }
                    DateTime2 = obj.DateTime;
                }
                Count++;
                this.lastReadDKey.Add(obj);
                if (this.lastReadDKey.count == this.lastReadDKey.capcity)
                {
                    SaveWriteDKey(this.lastReadDKey);
                    if (!CacheObjects && this.lastReadDKey != this.lastUpdateDKey && this.lastReadDKey != this.lastWriteDKey && this.lastReadDKey != this.lastDeleteDKey)
                        this.lastReadDKey.objs = null;

                    this.lastReadDKey = new DataKey(this.dataFile, null, this.lastReadDKey.position, -1);
                    this.lastReadDKey.number = this.bufferCount;
                    this.lastReadDKey.index1 = Count - 1;
                    this.lastReadDKey.index2 = Count - 1;
                    this.lastReadDKey.changed = true;
                    this.bufferCount++;
                    this.dataKeys[this.lastReadDKey.number] = this.lastReadDKey;
                }
                this.changed = true;
                this.dataFile.changed = true;
            }
        }

        public void Clear()
        {
            lock (Sync)
            {
                this.dataKeys = this.dataKeys ?? InitDataKeys();
                if (this.position1 != -1)
                {
                    var key = GetDataKey(this.position1);
                    while (true)
                    {
                        this.dataFile.DeleteObjectKey(key, false, true);
                        if (key.next == -1)
                            break;
                        key = GetDataKey(key.next);
                    }
                }
                Count = 0;
                this.bufferCount = 0;
                DateTime1 = new DateTime(0);
                DateTime2 = new DateTime(0);
                this.position1 = -1;
                this.position2 = -1;
                this.readOpened = false;
                this.writeOpened = false;
                this.dataKeys = new IdArray<DataKey>(4096);
                this.dataKeysKey.obj = new DataKeyIdArray(this.dataKeys);
                this.lastUpdateDKey = null;
                this.lastReadDKey = null;
                this.lastDeleteDKey = null;
                this.lastWriteDKey = null;
                this.changed = true;
                Flush();
            }
        }

        public virtual bool Contains(DateTime dateTime) => GetIndex(dateTime, SearchOption.ExactFirst) != -1;

        public virtual long GetIndex(DateTime dateTime, SearchOption option = SearchOption.Prev)
        {
            lock (Sync)
            {
                if (!this.readOpened)
                    OpenRead();

                if (Count == 0)
                {
                    Console.WriteLine($"DataSeries::GetIndex Error, data series has no elements {Name}");
                    return -1;
                }

                if (dateTime <= DateTime1)
                    return 0;

                if (dateTime >= DateTime2)
                    return Count - 1;

                var key = GetKey(dateTime, this.lastUpdateDKey, IndexOption.Null);
                if (key == null)
                    return -1;

                if (key != this.lastUpdateDKey)
                {
                    if (!CacheObjects && this.lastUpdateDKey != null && this.lastUpdateDKey != this.lastReadDKey &&
                        this.lastUpdateDKey != this.lastWriteDKey && this.lastUpdateDKey != this.lastDeleteDKey)
                        this.lastUpdateDKey.objs = null;
                    this.lastUpdateDKey = key;
                }
                return this.lastUpdateDKey.index1 + this.lastUpdateDKey.GetIndex(dateTime, option);
            }
        }

        public void Remove(long index)
        {
            lock (Sync)
            {
                if (!this.writeOpened)
                    OpenWrite();

                var key = GetKey(index, this.lastDeleteDKey);
                if (key == null)
                    return;

                if (this.lastDeleteDKey == null)
                    this.lastDeleteDKey = key;
                else if (this.lastDeleteDKey != key)
                {
                    if (this.lastDeleteDKey.changed)
                        SaveWriteDKey(this.lastDeleteDKey);

                    if (!CacheObjects && this.lastDeleteDKey != this.lastUpdateDKey && this.lastDeleteDKey != this.lastReadDKey && this.lastDeleteDKey != this.lastWriteDKey)
                        this.lastDeleteDKey.objs = null;
                    this.lastDeleteDKey = key;
                }
                key.Remove(index - key.index1);
                key.index2 -= 1;
                if (this.lastUpdateDKey != null && this.lastUpdateDKey.number > key.number)
                {
                    this.lastUpdateDKey.index1 -= 1;
                    this.lastUpdateDKey.index2 -= 1;
                }
                if (this.lastReadDKey != null && this.lastReadDKey.number > key.number)
                {
                    this.lastReadDKey.index1 -= 1;
                    this.lastReadDKey.index2 -= 1;
                }
                if (this.lastWriteDKey != null && this.lastWriteDKey.number > key.number)
                {
                    this.lastWriteDKey.index1 -= 1;
                    this.lastWriteDKey.index2 -= 1;
                }
                if (key.count == 0)
                {
                    Delete(key);
                    this.lastDeleteDKey = null;
                }

                Count--;
                if (Count != 0)
                {
                    if (index == 0)
                        DateTime1 = Get(0).DateTime;
                    if (index == Count)
                        DateTime2 = Get(Count - 1).DateTime;
                }
                this.changed = true;
                this.dataFile.changed = true;
            }
        }

        public virtual void Refresh()
        {
            // noop
        }

        public virtual void Update(long index, DataObject obj)
        {
            lock (Sync)
            {
                var dataObject = Get(index);
                if (dataObject.DateTime != obj.DateTime)
                {
                    Console.WriteLine("DataSeries::Update Can not update object with different datetime");
                    return;
                }

                bool changed = this.lastUpdateDKey.changed;
                this.lastUpdateDKey.Update((int) (index - this.lastUpdateDKey.index1), obj);
                if (!changed)
                    SaveWriteDKey(this.lastUpdateDKey);
                this.dataFile.changed = true;
            }
        }

        public virtual DataObject Get(long index)
        {
            lock (Sync)
            {
                if (!this.readOpened)
                    OpenRead();

                var key = GetKey(index, this.lastUpdateDKey);
                if (key == null)
                    return null;

                if (key != this.lastUpdateDKey)
                {
                    if (!CacheObjects && this.lastUpdateDKey != null && this.lastUpdateDKey != this.lastReadDKey && this.lastUpdateDKey != this.lastWriteDKey && this.lastUpdateDKey != this.lastDeleteDKey)
                        this.lastUpdateDKey.objs = null;
                    this.lastUpdateDKey = key;
                }
                return key.LoadDataObjects()[(int)unchecked(index - key.index1)];
            }
        }

        public virtual DataObject Get(DateTime dateTime)
        {
            lock (Sync)
            {
                if (!this.readOpened)
                    OpenRead();

                if (Count == 0 || dateTime > DateTime2)
                {
                    Console.WriteLine($"DataSeries::Get dateTime is out of range : {Name} {dateTime}");
                    return null;
                }

                if (dateTime <= DateTime1)
                    return Get(0);

                var key = GetKey(dateTime, this.lastUpdateDKey, IndexOption.Null);
                if (key == null)
                    return null;

                if (key != this.lastUpdateDKey)
                {
                    if (!CacheObjects && this.lastUpdateDKey != null && this.lastUpdateDKey != this.lastReadDKey && this.lastUpdateDKey != this.lastWriteDKey && this.lastUpdateDKey != this.lastDeleteDKey)
                        this.lastUpdateDKey.objs = null;
                    this.lastUpdateDKey = key;
                }
                return this.lastUpdateDKey.Get(dateTime);
            }
        }

        public override string ToString() => Name;

        public void Dump()
        {
            lock (Sync)
            {
                Console.WriteLine("Data series " + Name);
                Console.WriteLine("Count = " + Count);
                Console.WriteLine("Position1 = " + this.position1);
                Console.WriteLine("Position2 = " + this.position2);
                Console.WriteLine("DateTime1 = " + DateTime1.Ticks);
                Console.WriteLine("DateTime2 = " + DateTime2.Ticks);
                Console.WriteLine("Buffer count = " + this.bufferCount);
                Console.WriteLine();
                Console.WriteLine("Keys in cache:");
                Console.WriteLine();
                if (this.dataKeys != null)
                {
                    for (int i = 0; i < this.bufferCount; i++)
                    {
                        if (this.dataKeys[i] != null)
                        {
                            Console.WriteLine(this.dataKeys[i]);
                        }
                    }
                }
                Console.WriteLine();
                Console.WriteLine("Keys on disk:");
                Console.WriteLine();
                if (this.position1 != -1)
                {
                    var key = GetDataKey(this.position1);
                    while (true)
                    {
                        Console.WriteLine(key);
                        if (key.next == -1)
                            break;
                        key = GetDataKey(key.next);
                    }
                }
                Console.WriteLine();
                if (this.lastReadDKey != null)
                    Console.WriteLine("Write Key : " + this.changed);
                else
                    Console.WriteLine("Write Key : null");
                Console.WriteLine("\nEnd dump\n");
            }
        }

        #region Extra
        public void ToWriter(BinaryWriter writer)
        {
            writer.Write(Count);
            writer.Write(bufferCount);
            writer.Write(dKeysKeyPosition);
            writer.Write(DateTime2.Ticks);
            writer.Write(DateTime1.Ticks);
            writer.Write(position1);
            writer.Write(position2);
            writer.Write(Name);
        }

        public static DataSeries FromReader(BinaryReader reader, byte version)
        {
            return new DataSeries("")
            {
                Count = reader.ReadInt64(),
                bufferCount = reader.ReadInt32(),
                dKeysKeyPosition = reader.ReadInt64(),
                DateTime2 = new DateTime(reader.ReadInt64()),
                DateTime1 = new DateTime(reader.ReadInt64()),
                position1 = reader.ReadInt64(),
                position2 = reader.ReadInt64(),
                Name = reader.ReadString()
            };
        }
        #endregion

        internal void Init(DataFile file, ObjectKey key)
        {
            lock (Sync)
            {
                this.dataFile = file;
                this.oKey = key;
                key.CompressionLevel = 0;
                key.CompressionMethod = 0;

                // Init dataKey list
                if (this.dKeysKeyPosition == -1)
                {
                    this.dataKeys = new IdArray<DataKey>(Math.Max(4096, this.bufferCount));
                    this.dataKeysKey = new ObjectKey(file, "", new DataKeyIdArray(this.dataKeys));
                }
            }
        }

        internal void OpenRead()
        {
            lock (Sync)
            {
                if (this.readOpened)
                {
                    Console.WriteLine("DataSeries::OpenRead already read open");
                    return;
                }
                this.dataKeys = this.dataKeys ?? InitDataKeys();
                this.readOpened = true;                
            }
        }

        internal void OpenWrite()
        {
            lock (Sync)
            {
                if (this.writeOpened)
                {
                    Console.WriteLine("DataSeries::OpenWrite already write open");
                    return;
                }
                this.dataKeys = this.dataKeys ?? InitDataKeys();

                if (this.bufferCount != 0 && this.dataKeys[this.bufferCount - 1] != null)
                {
                    this.lastReadDKey = this.dataKeys[this.bufferCount - 1];
                    this.lastReadDKey.LoadDataObjects();
                }
                else
                {
                    if (this.position2 != -1)
                    {
                        this.lastReadDKey = GetDataKey(this.position2);
                        this.lastReadDKey.number = this.bufferCount - 1;
                        this.lastReadDKey.LoadDataObjects();
                    }
                    else
                    {
                        this.lastReadDKey = new DataKey(this.dataFile)
                        {
                            number = 0,
                            changed = true
                        };
                        this.bufferCount = 1;
                    }
                    this.dataKeys[this.lastReadDKey.number] = this.lastReadDKey;
                }
                if (this.dataKeysKey.position != -1)
                    this.dataFile.DeleteObjectKey(this.dataKeysKey, false, false);

                this.dKeysKeyPosition = -1;
                this.dataFile.WriteObjectKey(this.oKey);
                this.writeOpened = true;
            }
        }

        private IdArray<DataKey> InitDataKeys()
        {
            this.dataKeysKey = this.dataFile.ReadObjectKey(this.dKeysKeyPosition, ObjectKey.EMPTYNAME_KEY_SIZE);
            var dataKeys = ((DataKeyIdArray)this.dataKeysKey.GetObject()).Keys;
            for (int i = 0; i < dataKeys.Size; i++)
            {
                var dk = dataKeys[i];
                if (dk != null)
                {
                    dk.dataFile = this.dataFile;
                    dk.number = i;
                }
            }
            return dataKeys;
        }

        private void SaveDataKeysKey()
        {
            if (this.dataKeysKey == null)
                this.dataKeysKey = new ObjectKey(this.dataFile, "", new DataKeyIdArray(this.dataKeys));
            this.dataFile.WriteObjectKey(this.dataKeysKey);
            this.dKeysKeyPosition = this.dataKeysKey.position;
        }

        private void SaveDataObject(DataObject obj)
        {
            Count++;
            if (this.lastReadDKey.dateTime1 <= obj.DateTime && obj.DateTime <= this.lastReadDKey.dateTime2)
            {
                this.lastReadDKey.Add(obj);
                if (this.lastReadDKey.count >= this.lastReadDKey.capcity)
                {
                    SaveWriteDKey(this.lastReadDKey);
                    this.lastReadDKey = new DataKey(this.dataFile, null, this.lastReadDKey.position, -1L)
                    {
                        number = this.bufferCount,
                        index1 = Count,
                        index2 = Count,
                        changed = true
                    };
                    this.bufferCount++;
                    this.dataKeys[this.lastReadDKey.number] = this.lastReadDKey;
                }
                else
                {
                    this.changed = true;
                }
                this.dataFile.changed = true;
                return;
            }

            var key = GetKey(obj.DateTime, this.lastWriteDKey, IndexOption.Prev);
            if (this.lastWriteDKey == null)
            {
                this.lastWriteDKey = key;
            }
            else if (this.lastWriteDKey != key)
            {
                if (this.lastWriteDKey.changed)
                    SaveWriteDKey(this.lastWriteDKey);
                if (!CacheObjects && this.lastWriteDKey != this.lastUpdateDKey && this.lastWriteDKey != this.lastReadDKey && this.lastWriteDKey != this.lastDeleteDKey)
                    this.lastWriteDKey.objs = null;
                this.lastWriteDKey = key;
            }
            this.lastWriteDKey.LoadDataObjects();
            if (this.lastWriteDKey.count < this.lastWriteDKey.capcity)
            {
                this.lastWriteDKey.Add(obj);
                if (this.lastWriteDKey.count == this.lastWriteDKey.capcity)
                    SaveWriteDKey(this.lastWriteDKey);
            }
            else
            {
                key = new DataKey(this.dataFile, null, -1L, -1L);
                int num = this.lastWriteDKey.GetIndex(obj.DateTime, SearchOption.Next);
                if (num == -1)
                {
                    key.Add(obj);
                }
                else
                {
                    for (int i = num; i < this.lastWriteDKey.count; i++)
                    {
                        key.Add(this.lastWriteDKey.objs[i]);
                        this.lastWriteDKey.objs[i] = null;
                    }
                    this.lastWriteDKey.count = num;
                    this.lastWriteDKey.index2 = this.lastWriteDKey.index1 + this.lastWriteDKey.count - 1;
                    if (this.lastWriteDKey.count != 0)
                        this.lastWriteDKey.dateTime2 = this.lastWriteDKey.objs[this.lastWriteDKey.count - 1].DateTime;
                    this.lastWriteDKey.Add(obj);
                }
                Insert(key, this.lastWriteDKey);
            }
            if (this.lastUpdateDKey != null && this.lastUpdateDKey.number > this.lastWriteDKey.number)
            {
                this.lastUpdateDKey.index1 += 1;
                this.lastUpdateDKey.index2 += 1;
            }
            if (this.lastReadDKey != null && this.lastReadDKey.number > this.lastWriteDKey.number)
            {
                this.lastReadDKey.index1 += 1;
                this.lastReadDKey.index2 += 1;
            }
            if (this.lastDeleteDKey != null && this.lastDeleteDKey.number > this.lastWriteDKey.number)
            {
                this.lastDeleteDKey.index1 += 1;
                this.lastDeleteDKey.index2 += 1;
            }
            this.lastWriteDKey.changed = true;
            this.changed = true;
            this.dataFile.changed = true;
        }

        private void Insert(DataKey key, DataKey keyAt)
        {
            for (int i = this.bufferCount; i > keyAt.number + 1; i--)
            {
                this.dataKeys[i] = this.dataKeys[i - 1];
                if (this.dataKeys[i] != null)
                    this.dataKeys[i].number = i;
            }
            this.bufferCount++;
            key.number = keyAt.number + 1;
            this.dataKeys[key.number] = key;
            key.prev = keyAt.position;
            key.next = keyAt.next;
            SaveWriteDKey(key);
            this.dataFile.WriteObjectKey(this.oKey);
        }

        private void SaveWriteDKey(DataKey key)
        {
            long num = key.position;
            this.dataFile.WriteObjectKey(key);
            if (key.position != num)
            {
                DataKey @class = null;
                if (key.number != 0)
                    @class = this.dataKeys[key.number - 1];
                if (@class != null)
                {
                    @class.next = key.position;
                    if (!@class.changed)
                        WriteDKeyNext(key.prev, key.position);
                }
                else if (key.prev != -1)
                    WriteDKeyNext(key.prev, key.position);

                DataKey class2 = null;
                if (key.number != this.bufferCount - 1)
                    class2 = this.dataKeys[key.number + 1];

                if (class2 != null)
                {
                    class2.prev = key.position;
                    if (!class2.changed)
                        WriteDKeyPrev(key.next, key.position);
                }
                else if (key.next != -1)
                    WriteDKeyPrev(key.next, key.position);
            }
            if (key == this.lastReadDKey)
            {
                if (this.position1 == -1)
                    this.position1 = this.lastReadDKey.position;
                this.position2 = this.lastReadDKey.position;
            }
            this.dataFile.WriteObjectKey(this.oKey);
        }

        private void UpdateDKeyPrev(DataKey key, long number)
        {
            key.prev = number;
            WriteDKeyPrev(key.position, number);
        }

        private void UpdateDKeyNext(DataKey key, long number)
        {
            key.next = number;
            WriteDKeyNext(key.position, number);
        }

        private void WriteDKeyPrev(long position, long number)
        {
            var buf = GetBuffer(number);
            this.dataFile.WriteBuffer(buf, position + DataKey.PREV_OFFSET, sizeof(long));
        }

        private void WriteDKeyNext(long position, long number)
        {
            var buf = GetBuffer(number);
            this.dataFile.WriteBuffer(buf, position + DataKey.NEXT_OFFSET, sizeof(long));
        }

        [NotOriginal]
        private byte[] GetBuffer(long value)
        {
            using (var mstream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(mstream))
                {
                    writer.Write(value);
                    return mstream.ToArray();
                }
            }
        }

        private void Delete(DataKey key)
        {
            if (key.position == this.position2)
            {
                if (key.prev != -1)
                {
                    this.position2 = key.prev;
                }
                else
                {
                    this.position1 = -1;
                    this.position2 = -1;
                }
            }
            this.dataFile.DeleteObjectKey(key, false, true);
            if (key.prev != -1)
            {
                var k = this.dataKeys[key.number - 1];
                if (k != null)
                {
                    k.next = key.next;
                    if (!k.changed)
                        WriteDKeyNext(key.prev, key.next);
                }
                else
                {
                    WriteDKeyNext(key.prev, key.next);
                }
            }
            if (key.next != -1)
            {
                var k = this.dataKeys[key.number + 1];
                if (k != null)
                {
                    k.prev = key.prev;
                    if (!k.changed)
                        WriteDKeyPrev(key.next, key.prev);
                }
                else
                {
                    WriteDKeyPrev(key.next, key.prev);
                }
            }
            for (int i = key.number; i < this.bufferCount - 1; i++)
            {
                this.dataKeys[i] = this.dataKeys[i + 1];
                if (this.dataKeys[i] != null)
                    this.dataKeys[i].number = i;
            }
            this.bufferCount--;
            this.dataFile.WriteObjectKey(this.oKey);
        }

        private DataKey GetDataKey(long position)
        {
            var buffer = new byte[DataKey.HEADER_SIZE]; // why 79?
            this.dataFile.ReadBuffer(buffer, position, buffer.Length);
            var key = new DataKey(this.dataFile);
            using (var ms = new MemoryStream(buffer))
            using (var rdr = new BinaryReader(ms))
                key.Read(rdr, true);
            key.position = position;
            return key;
        }

        private DataKey GetFirstKey()
        {
            var key = this.dataKeys[0] = this.dataKeys[0] ?? GetDataKey(this.position1);
            key.number = 0;
            key.index1 = 0;
            key.index2 = key.count - 1;
            if (key.index2 < 0)
                key.index2 = 0;
            return key;
        }

        private DataKey GetNextKey(DataKey key)
        {
            if (key.number == -1)
                Console.WriteLine("DataSeries::GetNextKey Error: key.number is not set");

            var nextKey = this.dataKeys[key.number + 1];
            if (nextKey == null)
            {
                if (key.next == -1)
                    Console.WriteLine("DataSeries::GetNextKey Error: key.next is not set");
                nextKey = GetDataKey(key.next);
                nextKey.number = key.number + 1;
                this.dataKeys[nextKey.number] = nextKey;
            }
            nextKey.index1 = key.index2 + 1;
            nextKey.index2 = nextKey.index1 + nextKey.count - 1;
            if (nextKey.index2 < 0)
                nextKey.index2 = 0;
            return nextKey;
        }

        // TODO: rewrite it!
        internal DataKey GetKey(long index, DataKey key = null)
        {
            lock (Sync)
            {
                if (0 <= index && index < Count)
                {
                    if (key == null)
                        key = this.lastUpdateDKey;
                    DataKey @class = null;
                    if (key != null)
                    {
                        if (key.index1 <= index && index <= key.index2)
                        {
                            return key;
                        }
                        if (index > key.index2)
                        {
                            @class = GetNextKey(key);
                        }
                    }
                    if (@class == null)
                    {
                        @class = GetFirstKey();
                    }
                    while (index < @class.index1 || index > @class.index2)
                    {
                        @class = GetNextKey(@class);
                    }
                    return @class;
                }
                Console.WriteLine($"DataSeries::GetKey Error: index is out of range : {Name} {index}");
                return null;
            }
        }

        internal DataKey GetKey(DateTime dateTime, DataKey key = null, IndexOption option = IndexOption.Null)
        {
            lock (Sync)
            {
                if (Count == 0 || dateTime > DateTime2)
                {
                    Console.WriteLine($"DataSeries::GetKey dateTime is out of range : {dateTime}");
                    return null;
                }
                if (key == null)
                    key = this.lastUpdateDKey;

                DataKey @class = null;
                DataKey class2 = null;
                if (dateTime <= DateTime1)
                    return GetFirstKey();

                if (key != null)
                {
                    if (key.dateTime1 <= dateTime && dateTime <= key.dateTime2)
                        return key;

                    if (dateTime > key.dateTime2)
                    {
                        class2 = key;
                        @class = GetNextKey(class2);
                    }
                }
                if (@class == null)
                    @class = GetFirstKey();

                while (option == IndexOption.Null || class2 == null || !(dateTime > class2.dateTime2) || !(dateTime < @class.dateTime1))
                {
                    if (@class.dateTime1 <= dateTime && dateTime <= @class.dateTime2)
                    {
                        return @class;
                    }
                    class2 = @class;
                    @class = GetNextKey(class2);
                }
                if (option == IndexOption.Next)
                {
                    return @class;
                }
                return class2;
            }
        }

        internal void Flush()
        {
            lock (Sync)
            {
                if (this.changed)
                {
                    if (this.lastWriteDKey != null && this.lastWriteDKey.changed)
                        SaveWriteDKey(this.lastWriteDKey);

                    if (this.lastReadDKey != null && this.lastReadDKey.changed)
                        SaveWriteDKey(this.lastReadDKey);

                    if (this.lastDeleteDKey != null && this.lastDeleteDKey.changed)
                        this.SaveWriteDKey(this.lastDeleteDKey);

                    SaveDataKeysKey();
                    this.dataFile.WriteObjectKey(this.oKey);
                    this.changed = false;
                }                
            }
        }
    }

    public class DataSeriesIterator
    {
        private DataSeries series;
        private long index1;
        private long index2;
        private long current;

        public DataSeriesIterator(DataSeries series, long index1 = -1, long index2 = -1)
        {
            this.series = series;
            this.index1 = index1 != -1 ? index1 : 0;
            this.index2 = index2 != -1 ? series.Count - 1 : 0;
            this.current = index1;
        }

        public DataObject GetNext() => this.current > this.index2 ? null : this.series.Get(this.current++);
    }

    public class DataSeriesListEventArgs
    {
        public DataSeries[] SeriesList { get; }

        public DataSeriesListEventArgs(params DataSeries[] seriesList)
        {
            SeriesList = seriesList;
        }
    }

    public class NetDataSeries : DataSeries
    {
        public NetDataSeries(string name)
        {
            throw new NotImplementedException();
        }
    }
}