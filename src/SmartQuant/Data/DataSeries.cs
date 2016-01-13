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

        internal long count;
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

        public DataSeries(string name)
        {
            Name = name;
        }

        public void Add(DataObject obj)
        {
            if (obj.DateTime.Ticks == 0)
            {
                Console.WriteLine("DataSeries::Add Error: can not add object with DateTime = 0");
                return;
            }

            if (!this.writeOpened)
                OpenWrite();

            if (this.count == 0)
            {
                DateTime1 = obj.DateTime;
                DateTime2 = obj.DateTime;
            }
            else
            {
                if (obj.DateTime < this.DateTime2)
                {
                    SaveDataObject(obj);
                    return;
                }
                DateTime2 = obj.DateTime;
            }
            if (this.lastReadDKey.count == this.lastReadDKey.capcity)
            {
                SaveWriteDKey(this.lastReadDKey);
                if (!CacheObjects && this.lastReadDKey != this.lastUpdateDKey && this.lastReadDKey != this.lastWriteDKey && this.lastReadDKey != this.lastDeleteDKey)
                {
                    this.lastReadDKey.objs = null;
                }
                this.lastReadDKey = new DataKey(this.dataFile, null, this.lastReadDKey.position, -1);
                this.lastReadDKey.number = this.bufferCount;
                this.lastReadDKey.index1 = this.count - 1;
                this.lastReadDKey.index2 = this.count - 1;
                this.lastReadDKey.changed = true;
                this.bufferCount++;
                this.dataKeys[this.lastReadDKey.number] = this.lastReadDKey;
            }
            this.lastReadDKey.Add(obj);
            this.count += 1L;
            this.changed = true;
            this.dataFile.changed = true;
        }

        public void Clear()
        {
            if (this.dataKeys == null)
                InitDataKeys();

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
            this.count = 0;
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

        public virtual bool Contains(DateTime dateTime) => GetIndex(dateTime, SearchOption.ExactFirst) != -1;

        public virtual long GetIndex(DateTime dateTime, SearchOption option = SearchOption.Prev)
        {
            if (!this.readOpened)
                OpenRead();

            if (this.count == 0)
            {
                Console.WriteLine($"DataSeries::GetIndex Error, data series has no elements {Name}");
                return -1;
            }
            if (dateTime <= DateTime1)
                return 0;
            if (dateTime >= DateTime2)
                return this.count - 1;

            var key = GetKey(dateTime, this.lastUpdateDKey, IndexOption.Null);
            if (key == null)
            {
                return -1;
            }
            if (key != this.lastUpdateDKey)
            {
                if (!CacheObjects && this.lastUpdateDKey != null && this.lastUpdateDKey != this.lastReadDKey && this.lastUpdateDKey != this.lastWriteDKey && this.lastUpdateDKey != this.lastDeleteDKey)
                {
                    this.lastUpdateDKey.objs = null;
                }
                this.lastUpdateDKey = key;
            }
            return this.lastUpdateDKey.index1 + (long)this.lastUpdateDKey.GetIndex(dateTime, option);
        }

        public void Remove(long index)
        {
            if (!this.writeOpened)
                OpenWrite();

            var key = GetKey(index, this.lastDeleteDKey);
            if (key == null)
                return;

            if (this.lastDeleteDKey == null)
            {
                this.lastDeleteDKey = key;
            }
            else if (this.lastDeleteDKey != key)
            {
                if (this.lastDeleteDKey.changed)
                {
                    this.SaveWriteDKey(this.lastDeleteDKey);
                }
                if (!CacheObjects && this.lastDeleteDKey != this.lastUpdateDKey && this.lastDeleteDKey != this.lastReadDKey && this.lastDeleteDKey != this.lastWriteDKey)
                {
                    this.lastDeleteDKey.objs = null;
                }
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
            this.count -= 1;
            if (this.count != 0)
            {
                if (index == 0)
                    DateTime1 = Get(0).DateTime;
                if (index == this.count)
                    DateTime2 = Get(this.count - 1L).DateTime;
            }
            this.changed = true;
            this.dataFile.changed = true;
        }

        public virtual void Refresh()
        {
            // noop
        }

        public virtual void Update(long index, DataObject obj)
        {
            var dataObject = Get(index);
            if (dataObject.DateTime != obj.DateTime)
            {
                Console.WriteLine("DataSeries::Update Can not update object with different datetime");
                return;
            }
            bool changed = this.lastUpdateDKey.changed;
            this.lastUpdateDKey.Update((int)(index - this.lastUpdateDKey.index1), obj);
            if (!changed)
                SaveWriteDKey(this.lastUpdateDKey);
            this.dataFile.changed = true;
        }

        public virtual DataObject Get(long index)
        {
            if (!this.readOpened)
                OpenRead();

            var key = GetKey(index, this.lastUpdateDKey);
            if (key == null)
                return null;

            if (key != this.lastUpdateDKey)
            {
                if (!CacheObjects && this.lastUpdateDKey != null && this.lastUpdateDKey != this.lastReadDKey && this.lastUpdateDKey != this.lastWriteDKey && this.lastUpdateDKey != this.lastDeleteDKey)
                {
                    this.lastUpdateDKey.objs = null;
                }
                this.lastUpdateDKey = key;
            }
            return key.LoadDataObjects()[(int)(checked((IntPtr)(unchecked(index - key.index1))))];
        }

        public virtual DataObject Get(DateTime dateTime)
        {
            if (!this.readOpened)
                OpenRead();

            if (this.count == 0 || dateTime > DateTime2)
            {
                Console.WriteLine($"DataSeries::Get dateTime is out of range : {Name} {dateTime}");
                return null;
            }
            if (dateTime <= DateTime1)
                return Get(0);

            var key = GetKey(dateTime, this.lastUpdateDKey, IndexOption.Null);
            if (key == null)
            {
                return null;
            }
            if (key != this.lastUpdateDKey)
            {
                if (!CacheObjects && this.lastUpdateDKey != null && this.lastUpdateDKey != this.lastReadDKey && this.lastUpdateDKey != this.lastWriteDKey && this.lastUpdateDKey != this.lastDeleteDKey)
                {
                    this.lastUpdateDKey.objs = null;
                }
                this.lastUpdateDKey = key;
            }
            return this.lastUpdateDKey.Get(dateTime);
        }

        public void Dump()
        {
            Console.WriteLine("Data series " + Name);
            Console.WriteLine("Count = " + this.count);
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
                    {
                        break;
                    }
                    key = GetDataKey(key.next);
                }
            }
            Console.WriteLine();
            if (this.lastReadDKey != null)
            {
                Console.WriteLine("Write Key : " + this.changed);
            }
            else
            {
                Console.WriteLine("Write Key : null");
            }
            Console.WriteLine();
            Console.WriteLine("End dump");
            Console.WriteLine();

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

        internal void OpenRead()
        {
            if (this.readOpened)
            {
                Console.WriteLine("DataSeries::OpenRead already read open");
                return;
            }
            if (this.dataKeys == null)
                InitDataKeys();

            this.readOpened = true;
        }

        internal void OpenWrite()
        {
            if (this.writeOpened)
            {
                Console.WriteLine("DataSeries::OpenWrite already write open");
                return;
            }
            if (this.dataKeys == null)
                InitDataKeys();

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
                    this.lastReadDKey = new DataKey(this.dataFile, null, -1L, -1L);
                    this.lastReadDKey.number = 0;
                    this.lastReadDKey.changed = true;
                    this.bufferCount = 1;
                }
                this.dataKeys[this.lastReadDKey.number] = this.lastReadDKey;
            }
            if (this.dataKeysKey.position != -1)
            {
                this.dataFile.DeleteObjectKey(this.dataKeysKey, false, false);
            }
            this.dKeysKeyPosition = -1;
            this.dataFile.WriteObjectKey(this.oKey);
            this.writeOpened = true;
        }

        private void InitDataKeys()
        {
            this.dataKeysKey = this.dataFile.ReadObjectKey(this.dKeysKeyPosition, ObjectKey.EMPTYNAME_KEY_SIZE);
            this.dataKeys = ((DataKeyIdArray)this.dataKeysKey.GetObject()).Keys;
            for (int i = 0; i < this.dataKeys.Size; i++)
            {
                var dk = this.dataKeys[i];
                if (dk != null)
                {
                    dk.dataFile = this.dataFile;
                    dk.number = i;
                }
            }
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
            this.count += 1;
            if (obj.DateTime >= this.lastReadDKey.dateTime1 && obj.DateTime <= this.lastReadDKey.dateTime2)
            {
                this.lastReadDKey.Add(obj);
                if (this.lastReadDKey.count >= this.lastReadDKey.capcity)
                {
                    SaveWriteDKey(this.lastReadDKey);
                    this.lastReadDKey = new DataKey(this.dataFile, null, this.lastReadDKey.position, -1L);
                    this.lastReadDKey.number = this.bufferCount;
                    this.lastReadDKey.index1 = this.count;
                    this.lastReadDKey.index2 = this.count;
                    this.lastReadDKey.changed = true;
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
                {
                    this.SaveWriteDKey(this.lastWriteDKey);
                }
                if (!CacheObjects && this.lastWriteDKey != this.lastUpdateDKey && this.lastWriteDKey != this.lastReadDKey && this.lastWriteDKey != this.lastDeleteDKey)
                {
                    this.lastWriteDKey.objs = null;
                }
                this.lastWriteDKey = key;
            }
            this.lastWriteDKey.LoadDataObjects();
            if (this.lastWriteDKey.count < this.lastWriteDKey.capcity)
            {
                this.lastWriteDKey.Add(obj);
                if (this.lastWriteDKey.count == this.lastWriteDKey.capcity)
                {
                    SaveWriteDKey(this.lastWriteDKey);
                }
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
                    this.lastWriteDKey.index2 = this.lastWriteDKey.index1 + (long)this.lastWriteDKey.count - 1;
                    if (this.lastWriteDKey.count != 0)
                    {
                        this.lastWriteDKey.dateTime2 = this.lastWriteDKey.objs[this.lastWriteDKey.count - 1].DateTime;
                    }
                    this.lastWriteDKey.Add(obj);
                }
                this.Insert(key, this.lastWriteDKey);
            }
            if (this.lastUpdateDKey != null && this.lastUpdateDKey.number > this.lastWriteDKey.number)
            {
                this.lastUpdateDKey.index1 += 1L;
                this.lastUpdateDKey.index2 += 1L;
            }
            if (this.lastReadDKey != null && this.lastReadDKey.number > this.lastWriteDKey.number)
            {
                this.lastReadDKey.index1 += 1L;
                this.lastReadDKey.index2 += 1L;
            }
            if (this.lastDeleteDKey != null && this.lastDeleteDKey.number > this.lastWriteDKey.number)
            {
                this.lastDeleteDKey.index1 += 1L;
                this.lastDeleteDKey.index2 += 1L;
            }
            this.lastWriteDKey.changed = true;
            this.changed = true;
            this.dataFile.changed = true;
        }

        private void Insert(DataKey class19_4, DataKey class19_5)
        {
            for (int i = this.bufferCount; i > class19_5.number + 1; i--)
            {
                this.dataKeys[i] = this.dataKeys[i - 1];
                if (this.dataKeys[i] != null)
                {
                    this.dataKeys[i].number = i;
                }
            }
            this.bufferCount++;
            class19_4.number = class19_5.number + 1;
            this.dataKeys[class19_4.number] = class19_4;
            class19_4.prev = class19_5.position;
            class19_4.next = class19_5.next;
            SaveWriteDKey(class19_4);
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
                {
                    @class = this.dataKeys[key.number - 1];
                }
                if (@class != null)
                {
                    @class.next = key.position;
                    if (!@class.changed)
                    {
                        WriteDKeyNext(key.prev, key.position);
                    }
                }
                else if (key.prev != -1)
                {
                    WriteDKeyNext(key.prev, key.position);
                }
                DataKey class2 = null;
                if (key.number != this.bufferCount - 1)
                {
                    class2 = this.dataKeys[key.number + 1];
                }
                if (class2 != null)
                {
                    class2.prev = key.position;
                    if (!class2.changed)
                    {
                        this.WriteDKeyPrev(key.next, key.position);
                    }
                }
                else if (key.next != -1)
                {
                    this.WriteDKeyPrev(key.next, key.position);
                }
            }
            if (key == this.lastReadDKey)
            {
                if (this.position1 == -1)
                {
                    this.position1 = this.lastReadDKey.position;
                }
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

        private void Delete(DataKey class19_4)
        {
            if (class19_4.position == this.position2)
            {
                if (class19_4.prev != -1)
                {
                    this.position2 = class19_4.prev;
                }
                else
                {
                    this.position1 = -1;
                    this.position2 = -1;
                }
            }
            this.dataFile.DeleteObjectKey(class19_4, false, true);
            if (class19_4.prev != -1)
            {
                var k = this.dataKeys[class19_4.number - 1];
                if (k != null)
                {
                    k.next = class19_4.next;
                    if (!k.changed)
                    {
                        this.WriteDKeyNext(class19_4.prev, class19_4.next);
                    }
                }
                else
                {
                    this.WriteDKeyNext(class19_4.prev, class19_4.next);
                }
            }
            if (class19_4.next != -1)
            {
                var k = this.dataKeys[class19_4.number + 1];
                if (k != null)
                {
                    k.prev = class19_4.prev;
                    if (!k.changed)
                    {
                        WriteDKeyPrev(class19_4.next, class19_4.prev);
                    }
                }
                else
                {
                    WriteDKeyPrev(class19_4.next, class19_4.prev);
                }
            }
            for (int i = class19_4.number; i < this.bufferCount - 1; i++)
            {
                this.dataKeys[i] = this.dataKeys[i + 1];
                if (this.dataKeys[i] != null)
                {
                    this.dataKeys[i].number = i;
                }
            }
            this.bufferCount--;
            this.dataFile.WriteObjectKey(this.oKey);
        }

        private DataKey GetDataKey(long position)
        {
            var buffer = new byte[79];
            var input = new MemoryStream(buffer);
            var reader = new BinaryReader(input);
            this.dataFile.ReadBuffer(buffer, position, 77);
            var key = new DataKey(this.dataFile, null, -1, -1);
            key.Read(reader, true);
            key.position = position;
            return key;
        }

        private DataKey GetFirstKey()
        {
            var key = this.dataKeys[0];
            if (key == null)
            {
                key = GetDataKey(this.position1);
                this.dataKeys[0] = key;
            }
            key.number = 0;
            key.index1 = 0;
            key.index2 = (long)(key.count - 1);
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
                {
                    Console.WriteLine("DataSeries::GetNextKey Error: key.next is not set");
                }
                nextKey = GetDataKey(key.next);
                nextKey.number = key.number + 1;
                this.dataKeys[nextKey.number] = nextKey;
            }
            nextKey.index1 = key.index2 + 1;
            nextKey.index2 = nextKey.index1 + (long)nextKey.count - 1;
            return nextKey;
        }

        internal DataKey GetKey(long index, DataKey key = null)
        {
            if (index >= 0 && index < this.count)
            {
                if (key == null)
                {
                    key = this.lastUpdateDKey;
                }
                DataKey @class = null;
                if (key != null)
                {
                    if (index >= key.index1 && index <= key.index2)
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

        internal DataKey GetKey(DateTime dateTime, DataKey key = null, IndexOption option = IndexOption.Null)
        {
            if (this.count == 0L || dateTime > DateTime2)
            {
                Console.WriteLine($"DataSeries::GetKey dateTime is out of range : {dateTime}");
                return null;
            }
            if (key == null)
            {
                key = this.lastUpdateDKey;
            }
            DataKey @class = null;
            DataKey class2 = null;
            if (dateTime <= DateTime1)
            {
                return this.GetFirstKey();
            }
            if (key != null)
            {
                if (dateTime >= key.dateTime1 && dateTime <= key.dateTime2)
                {
                    return key;
                }
                if (dateTime > key.dateTime2)
                {
                    class2 = key;
                    @class = this.GetNextKey(class2);
                }
            }
            if (@class == null)
            {
                @class = GetFirstKey();
            }
            while (option == IndexOption.Null || class2 == null || !(dateTime > class2.dateTime2) || !(dateTime < @class.dateTime1))
            {
                if (dateTime >= @class.dateTime1 && dateTime <= @class.dateTime2)
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

        internal void Flush()
        {
            if (this.changed)
            {
                if (this.lastWriteDKey != null && this.lastWriteDKey.changed)
                {
                    SaveWriteDKey(this.lastWriteDKey);
                }
                if (this.lastReadDKey != null && this.lastReadDKey.changed)
                {
                    SaveWriteDKey(this.lastReadDKey);
                }
                if (this.lastDeleteDKey != null && this.lastDeleteDKey.changed)
                {
                    this.SaveWriteDKey(this.lastDeleteDKey);
                }
                SaveDataKeysKey();
                this.dataFile.WriteObjectKey(this.oKey);
                this.changed = false;
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
}