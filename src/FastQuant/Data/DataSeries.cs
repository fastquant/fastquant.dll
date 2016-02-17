using System;
using System.IO;

namespace FastQuant
{
    public class DataSeries : IDataSeries
    {
        private DataFile file;

        internal DataKey readKey;
        internal DataKey writeKey;
        internal DataKey deleteKey;
        internal DataKey insertKey;

        internal bool readOpened;
        internal bool writeOpened;
        internal bool changed;

        internal IdArray<DataKey> cache;

        internal long position1;
        internal long position2;
        internal long cachePosition;

        internal int bufferCount;
        internal ObjectKey key;
        internal ObjectKey cacheKey;

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
                        Insert(obj);
                        return;
                    }
                    DateTime2 = obj.DateTime;
                }
                Count++;
                this.writeKey.AddObject(obj);
                if (this.writeKey.count == this.writeKey.size)
                {
                    WriteKey(this.writeKey);
                    if (!CacheObjects && this.writeKey != this.readKey && this.writeKey != this.insertKey && this.writeKey != this.deleteKey)
                        this.writeKey.objects = null;

                    this.writeKey = new DataKey(this.file, null, this.writeKey.position, -1);
                    this.writeKey.number = this.bufferCount;
                    this.writeKey.index1 = Count - 1;
                    this.writeKey.index2 = Count - 1;
                    this.writeKey.changed = true;
                    this.bufferCount++;
                    this.cache[this.writeKey.number] = this.writeKey;
                }
                this.changed = true;
                this.file.isModified = true;
            }
        }

        public void Clear()
        {
            lock (Sync)
            {
                this.cache = this.cache ?? ReadCache();
                if (this.position1 != -1)
                {
                    var key = ReadKey(this.position1);
                    while (true)
                    {
                        this.file.DeleteKey(key, false, true);
                        if (key.next == -1)
                            break;
                        key = ReadKey(key.next);
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
                this.cache = new IdArray<DataKey>(4096);
                this.cacheKey.obj = new DataKeyIdArray(this.cache);
                this.readKey = null;
                this.writeKey = null;
                this.deleteKey = null;
                this.insertKey = null;
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

                var key = GetKey(dateTime, this.readKey, IndexOption.Null);
                if (key == null)
                    return -1;

                if (key != this.readKey)
                {
                    if (!CacheObjects && this.readKey != null && this.readKey != this.writeKey &&
                        this.readKey != this.insertKey && this.readKey != this.deleteKey)
                        this.readKey.objects = null;
                    this.readKey = key;
                }
                return this.readKey.index1 + this.readKey.GetIndex(dateTime, option);
            }
        }

        public void Remove(long index)
        {
            lock (Sync)
            {
                if (!this.writeOpened)
                    OpenWrite();

                var key = GetKey(index, this.deleteKey);
                if (key == null)
                    return;

                if (this.deleteKey == null)
                    this.deleteKey = key;
                else if (this.deleteKey != key)
                {
                    if (this.deleteKey.changed)
                        WriteKey(this.deleteKey);

                    if (!CacheObjects && this.deleteKey != this.readKey && this.deleteKey != this.writeKey && this.deleteKey != this.insertKey)
                        this.deleteKey.objects = null;
                    this.deleteKey = key;
                }
                key.RemoveObject(index - key.index1);
                key.index2 -= 1;
                if (this.readKey != null && this.readKey.number > key.number)
                {
                    this.readKey.index1 -= 1;
                    this.readKey.index2 -= 1;
                }
                if (this.writeKey != null && this.writeKey.number > key.number)
                {
                    this.writeKey.index1 -= 1;
                    this.writeKey.index2 -= 1;
                }
                if (this.insertKey != null && this.insertKey.number > key.number)
                {
                    this.insertKey.index1 -= 1;
                    this.insertKey.index2 -= 1;
                }
                if (key.count == 0)
                {
                    DeleteKey(key);
                    this.deleteKey = null;
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
                this.file.isModified = true;
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

                bool changed = this.readKey.changed;
                this.readKey.UpdateObject((int) (index - this.readKey.index1), obj);
                if (!changed)
                    WriteKey(this.readKey);
                this.file.isModified = true;
            }
        }

        public virtual DataObject Get(long index)
        {
            lock (Sync)
            {
                if (!this.readOpened)
                    OpenRead();

                var key = GetKey(index, this.readKey);
                if (key == null)
                    return null;

                if (key != this.readKey)
                {
                    if (!CacheObjects && this.readKey != null && this.readKey != this.writeKey && this.readKey != this.insertKey && this.readKey != this.deleteKey)
                        this.readKey.objects = null;
                    this.readKey = key;
                }
                return key.GetObjects()[(int)unchecked(index - key.index1)];
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

                var key = GetKey(dateTime, this.readKey, IndexOption.Null);
                if (key == null)
                    return null;

                if (key != this.readKey)
                {
                    if (!CacheObjects && this.readKey != null && this.readKey != this.writeKey && this.readKey != this.insertKey && this.readKey != this.deleteKey)
                        this.readKey.objects = null;
                    this.readKey = key;
                }
                return this.readKey.GetObject(dateTime);
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
                if (this.cache != null)
                {
                    for (int i = 0; i < this.bufferCount; i++)
                    {
                        if (this.cache[i] != null)
                        {
                            Console.WriteLine(this.cache[i]);
                        }
                    }
                }
                Console.WriteLine();
                Console.WriteLine("Keys on disk:");
                Console.WriteLine();
                if (this.position1 != -1)
                {
                    var key = ReadKey(this.position1);
                    while (true)
                    {
                        Console.WriteLine(key);
                        if (key.next == -1)
                            break;
                        key = ReadKey(key.next);
                    }
                }
                Console.WriteLine();
                if (this.writeKey != null)
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
            writer.Write(cachePosition);
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
                cachePosition = reader.ReadInt64(),
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
                this.file = file;
                this.key = key;
                key.CompressionLevel = 0;
                key.CompressionMethod = 0;

                // Init dataKey list
                if (this.cachePosition == -1)
                {
                    this.cache = new IdArray<DataKey>(Math.Max(4096, this.bufferCount));
                    this.cacheKey = new ObjectKey(file, "", new DataKeyIdArray(this.cache));
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
                this.cache = this.cache ?? ReadCache();
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
                this.cache = this.cache ?? ReadCache();

                if (this.bufferCount != 0 && this.cache[this.bufferCount - 1] != null)
                {
                    this.writeKey = this.cache[this.bufferCount - 1];
                    this.writeKey.GetObjects();
                }
                else
                {
                    if (this.position2 != -1)
                    {
                        this.writeKey = ReadKey(this.position2);
                        this.writeKey.number = this.bufferCount - 1;
                        this.writeKey.GetObjects();
                    }
                    else
                    {
                        this.writeKey = new DataKey(this.file)
                        {
                            number = 0,
                            changed = true
                        };
                        this.bufferCount = 1;
                    }
                    this.cache[this.writeKey.number] = this.writeKey;
                }
                if (this.cacheKey.position != -1)
                    this.file.DeleteKey(this.cacheKey, false, false);

                this.cachePosition = -1;
                this.file.WriteKey(this.key);
                this.writeOpened = true;
            }
        }

        private IdArray<DataKey> ReadCache()
        {
            this.cacheKey = this.file.ReadKey(this.cachePosition, ObjectKey.EMPTYNAME_KEY_SIZE);
            var dataKeys = ((DataKeyIdArray)this.cacheKey.GetObject()).Keys;
            for (int i = 0; i < dataKeys.Size; i++)
            {
                var dk = dataKeys[i];
                if (dk != null)
                {
                    dk.file = this.file;
                    dk.number = i;
                }
            }
            return dataKeys;
        }

        private void WriteCache()
        {
            if (this.cacheKey == null)
                this.cacheKey = new ObjectKey(this.file, "", new DataKeyIdArray(this.cache));
            this.file.WriteKey(this.cacheKey);
            this.cachePosition = this.cacheKey.position;
        }

        private void Insert(DataObject obj)
        {
            Count++;
            if (this.writeKey.dateTime1 <= obj.DateTime && obj.DateTime <= this.writeKey.dateTime2)
            {
                this.writeKey.AddObject(obj);
                if (this.writeKey.count >= this.writeKey.size)
                {
                    WriteKey(this.writeKey);
                    this.writeKey = new DataKey(this.file, null, this.writeKey.position, -1L)
                    {
                        number = this.bufferCount,
                        index1 = Count,
                        index2 = Count,
                        changed = true
                    };
                    this.bufferCount++;
                    this.cache[this.writeKey.number] = this.writeKey;
                }
                else
                {
                    this.changed = true;
                }
                this.file.isModified = true;
                return;
            }

            var key = GetKey(obj.DateTime, this.insertKey, IndexOption.Prev);
            if (this.insertKey == null)
            {
                this.insertKey = key;
            }
            else if (this.insertKey != key)
            {
                if (this.insertKey.changed)
                    WriteKey(this.insertKey);
                if (!CacheObjects && this.insertKey != this.readKey && this.insertKey != this.writeKey && this.insertKey != this.deleteKey)
                    this.insertKey.objects = null;
                this.insertKey = key;
            }
            this.insertKey.GetObjects();
            if (this.insertKey.count < this.insertKey.size)
            {
                this.insertKey.AddObject(obj);
                if (this.insertKey.count == this.insertKey.size)
                    WriteKey(this.insertKey);
            }
            else
            {
                key = new DataKey(this.file, null, -1L, -1L);
                int num = this.insertKey.GetIndex(obj.DateTime, SearchOption.Next);
                if (num == -1)
                {
                    key.AddObject(obj);
                }
                else
                {
                    for (int i = num; i < this.insertKey.count; i++)
                    {
                        key.AddObject(this.insertKey.objects[i]);
                        this.insertKey.objects[i] = null;
                    }
                    this.insertKey.count = num;
                    this.insertKey.index2 = this.insertKey.index1 + this.insertKey.count - 1;
                    if (this.insertKey.count != 0)
                        this.insertKey.dateTime2 = this.insertKey.objects[this.insertKey.count - 1].DateTime;
                    this.insertKey.AddObject(obj);
                }
                InsertKey(key, this.insertKey);
            }
            if (this.readKey != null && this.readKey.number > this.insertKey.number)
            {
                this.readKey.index1 += 1;
                this.readKey.index2 += 1;
            }
            if (this.writeKey != null && this.writeKey.number > this.insertKey.number)
            {
                this.writeKey.index1 += 1;
                this.writeKey.index2 += 1;
            }
            if (this.deleteKey != null && this.deleteKey.number > this.insertKey.number)
            {
                this.deleteKey.index1 += 1;
                this.deleteKey.index2 += 1;
            }
            this.insertKey.changed = true;
            this.changed = true;
            this.file.isModified = true;
        }

        private void InsertKey(DataKey key, DataKey keyAt)
        {
            for (int i = this.bufferCount; i > keyAt.number + 1; i--)
            {
                this.cache[i] = this.cache[i - 1];
                if (this.cache[i] != null)
                    this.cache[i].number = i;
            }
            this.bufferCount++;
            key.number = keyAt.number + 1;
            this.cache[key.number] = key;
            key.prev = keyAt.position;
            key.next = keyAt.next;
            WriteKey(key);
            this.file.WriteKey(this.key);
        }

        private void WriteKey(DataKey key)
        {
            long num = key.position;
            this.file.WriteKey(key);
            if (key.position != num)
            {
                DataKey @class = null;
                if (key.number != 0)
                    @class = this.cache[key.number - 1];
                if (@class != null)
                {
                    @class.next = key.position;
                    if (!@class.changed)
                        SetNext(key.prev, key.position);
                }
                else if (key.prev != -1)
                    SetNext(key.prev, key.position);

                DataKey class2 = null;
                if (key.number != this.bufferCount - 1)
                    class2 = this.cache[key.number + 1];

                if (class2 != null)
                {
                    class2.prev = key.position;
                    if (!class2.changed)
                        SetPrev(key.next, key.position);
                }
                else if (key.next != -1)
                    SetPrev(key.next, key.position);
            }
            if (key == this.writeKey)
            {
                if (this.position1 == -1)
                    this.position1 = this.writeKey.position;
                this.position2 = this.writeKey.position;
            }
            this.file.WriteKey(this.key);
        }

        private void SetPrev(DataKey key, long position)
        {
            key.prev = position;
            SetPrev(key.position, position);
        }

        private void SetNext(DataKey key, long position)
        {
            key.next = position;
            SetNext(key.position, position);
        }

        private void SetPrev(long key, long position)
        {
            var buf = GetBuffer(position);
            this.file.WriteBuffer(buf, key + DataKey.PREV_OFFSET, sizeof(long));
        }

        private void SetNext(long key, long position)
        {
            var buf = GetBuffer(position);
            this.file.WriteBuffer(buf, key + DataKey.NEXT_OFFSET, sizeof(long));
        }

        [NotOriginal]
        private static byte[] GetBuffer(long position)
        {
            using (var mstream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(mstream))
                {
                    writer.Write(position);
                    return mstream.ToArray();
                }
            }
        }

        private void DeleteKey(DataKey key)
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
            this.file.DeleteKey(key, false, true);
            if (key.prev != -1)
            {
                var k = this.cache[key.number - 1];
                if (k != null)
                {
                    k.next = key.next;
                    if (!k.changed)
                        SetNext(key.prev, key.next);
                }
                else
                {
                    SetNext(key.prev, key.next);
                }
            }
            if (key.next != -1)
            {
                var k = this.cache[key.number + 1];
                if (k != null)
                {
                    k.prev = key.prev;
                    if (!k.changed)
                        SetPrev(key.next, key.prev);
                }
                else
                {
                    SetPrev(key.next, key.prev);
                }
            }
            for (int i = key.number; i < this.bufferCount - 1; i++)
            {
                this.cache[i] = this.cache[i + 1];
                if (this.cache[i] != null)
                    this.cache[i].number = i;
            }
            this.bufferCount--;
            this.file.WriteKey(this.key);
        }

        private DataKey ReadKey(long position)
        {
            var buffer = new byte[DataKey.HEADER_SIZE]; // why 79?
            this.file.ReadBuffer(buffer, position, buffer.Length);
            var key = new DataKey(this.file);
            using (var ms = new MemoryStream(buffer))
            using (var rdr = new BinaryReader(ms))
                key.Read(rdr, true);
            key.position = position;
            return key;
        }

        private DataKey GetFirstKey()
        {
            var key = this.cache[0] = this.cache[0] ?? ReadKey(this.position1);
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

            var nextKey = this.cache[key.number + 1];
            if (nextKey == null)
            {
                if (key.next == -1)
                    Console.WriteLine("DataSeries::GetNextKey Error: key.next is not set");
                nextKey = ReadKey(key.next);
                nextKey.number = key.number + 1;
                this.cache[nextKey.number] = nextKey;
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
                        key = this.readKey;
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
                    key = this.readKey;

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
                    if (this.insertKey != null && this.insertKey.changed)
                        WriteKey(this.insertKey);

                    if (this.writeKey != null && this.writeKey.changed)
                        WriteKey(this.writeKey);

                    if (this.deleteKey != null && this.deleteKey.changed)
                        WriteKey(this.deleteKey);

                    WriteCache();
                    this.file.WriteKey(this.key);
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