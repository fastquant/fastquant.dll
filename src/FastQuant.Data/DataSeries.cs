using System;
using System.IO;

namespace SmartQuant
{
    public class DataSeries : IDataSeries
    {
        //enum SearchOptin
        //{
        //    Exact,
        //    Prev,
        //    Next,

        //}
        private DataFile dataFile;

        internal DataKey lastUpdateDKey;
        internal DataKey lastReadDKey;
        internal DataKey lastDeleteDKey;
        internal DataKey lastWriteDKey;

        internal bool readOpened;
        internal bool writeOpened;
    //    internal bool bool_2;
        internal bool changed;
        internal IdArray<DataKey> dKeys;

        internal long count;
        internal long position1;
        internal long position2;
        internal long dKeysKeyPosition;

        internal int bufCount;
        internal ObjectKey objectKey_0;
        internal ObjectKey dKeysKey;

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
            if (this.lastReadDKey.count == this.lastReadDKey.capacity)
            {
                SaveWriteDKey(this.lastReadDKey);
                if (!CacheObjects && this.lastReadDKey != this.lastUpdateDKey && this.lastReadDKey != this.lastWriteDKey && this.lastReadDKey != this.lastDeleteDKey)
                {
                    this.lastReadDKey.dataObjs = null;
                }
                this.lastReadDKey = new DataKey(this.dataFile, null, this.lastReadDKey.position, -1);
                this.lastReadDKey.number = this.bufCount;
                this.lastReadDKey.index1 = this.count - 1;
                this.lastReadDKey.index2 = this.count - 1;
                this.lastReadDKey.changed = true;
                this.bufCount++;
                this.dKeys[this.lastReadDKey.number] = this.lastReadDKey;
            }
            this.lastReadDKey.Add(obj);
            this.count += 1L;
            this.changed = true;
            this.dataFile.changed = true;
        }

        public void Clear()
        {
            if (this.dKeys == null)
            {
                this.InitDataKeys();
            }
            if (this.position1 != -1L)
            {
                DataKey key = this.GetDataKey(this.position1);
                while (true)
                {
                    this.dataFile.DeleteObjectKey(key, false, true);
                    if (key.next == -1L)
                    {
                        break;
                    }
                    key = GetDataKey(key.next);
                }
            }
            this.count = 0;
            this.bufCount = 0;
            DateTime1 = new DateTime(0);
            DateTime2 = new DateTime(0);
            this.position1 = -1L;
            this.position2 = -1L;
            this.readOpened = false;
            this.writeOpened = false;
            this.dKeys = new IdArray<DataKey>(4096);
            this.dKeysKey.dkeyIdArray = new DataKeyIdArray(this.dKeys);
            this.lastUpdateDKey = null;
            this.lastReadDKey = null;
            this.lastDeleteDKey = null;
            this.lastWriteDKey = null;
            this.changed = true;
            this.Flush();
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
                    this.lastUpdateDKey.dataObjs = null;
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
                    this.lastDeleteDKey.dataObjs = null;
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
            {
                OpenRead();
            }
            DataKey key = GetKey(index, this.lastUpdateDKey);
            if (key == null)
                return null;

            if (key != this.lastUpdateDKey)
            {
                if (!CacheObjects && this.lastUpdateDKey != null && this.lastUpdateDKey != this.lastReadDKey && this.lastUpdateDKey != this.lastWriteDKey && this.lastUpdateDKey != this.lastDeleteDKey)
                {
                    this.lastUpdateDKey.dataObjs = null;
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
                    this.lastUpdateDKey.dataObjs = null;
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
            Console.WriteLine("Buffer count = " + this.bufCount);
            Console.WriteLine();
            Console.WriteLine("Keys in cache:");
            Console.WriteLine();
            if (this.dKeys != null)
            {
                for (int i = 0; i < this.bufCount; i++)
                {
                    if (this.dKeys[i] != null)
                    {
                        Console.WriteLine(this.dKeys[i]);
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
            writer.Write(bufCount);
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
                bufCount = reader.ReadInt32(),
                dKeysKeyPosition = reader.ReadInt64(),
                DateTime2 = new DateTime(reader.ReadInt64()),
                DateTime1 = new DateTime(reader.ReadInt64()),
                position1 = reader.ReadInt64(),
                position2 = reader.ReadInt64(),
                Name = reader.ReadString()
            };
        }
        #endregion

        internal void Init(DataFile dataFile, ObjectKey key)
        {
            this.dataFile = dataFile;
            this.objectKey_0 = key;
            key.CompressionLevel = 0;
            key.CompressionMethod = 0;
            if (this.dKeysKeyPosition == -1)
            {
                if (this.bufCount < 4096)
                {
                    this.dKeys = new IdArray<DataKey>(4096);
                }
                else
                {
                    this.dKeys = new IdArray<DataKey>(this.bufCount);
                }
                this.dKeysKey = new ObjectKey(dataFile, "", new DataKeyIdArray(this.dKeys));
            }
        }

        internal void OpenRead()
        {
            if (this.readOpened)
            {
                Console.WriteLine("DataSeries::OpenRead already read open");
                return;
            }
            if (this.dKeys == null)
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
            if (this.dKeys == null)
                this.InitDataKeys();

            if (this.bufCount != 0 && this.dKeys[this.bufCount - 1] != null)
            {
                this.lastReadDKey = this.dKeys[this.bufCount - 1];
                this.lastReadDKey.LoadDataObjects();
            }
            else
            {
                if (this.position2 != -1)
                {
                    this.lastReadDKey = this.GetDataKey(this.position2);
                    this.lastReadDKey.number = this.bufCount - 1;
                    this.lastReadDKey.LoadDataObjects();
                }
                else
                {
                    this.lastReadDKey = new DataKey(this.dataFile, null, -1L, -1L);
                    this.lastReadDKey.number = 0;
                    this.lastReadDKey.changed = true;
                    this.bufCount = 1;
                }
                this.dKeys[this.lastReadDKey.number] = this.lastReadDKey;
            }
            if (this.dKeysKey.position != -1L)
            {
                this.dataFile.DeleteObjectKey(this.dKeysKey, false, false);
            }
            this.dKeysKeyPosition = -1L;
            this.dataFile.WriteObjectKey(this.objectKey_0);
            this.writeOpened = true;
        }

        private void InitDataKeys()
        {
            this.dKeysKey = this.dataFile.ReadObjectKey(this.dKeysKeyPosition, 38);
            this.dKeys = ((DataKeyIdArray)this.dKeysKey.GetObject()).Keys;
            for (int i = 0; i < this.dKeys.Size; i++)
            {
                if (this.dKeys[i] != null)
                {
                    this.dKeys[i].dataFile = this.dataFile;
                    this.dKeys[i].number = i;
                }
            }
        }

        private void SaveDataKeysKey()
        {
            if (this.dKeysKey == null)
                this.dKeysKey = new ObjectKey(this.dataFile, "", new DataKeyIdArray(this.dKeys));
            this.dataFile.WriteObjectKey(this.dKeysKey);
            this.dKeysKeyPosition = this.dKeysKey.position;
        }

        private void SaveDataObject(DataObject obj)
        {
            this.count += 1;
            if (obj.DateTime >= this.lastReadDKey.dateTime_1 && obj.DateTime <= this.lastReadDKey.dateTime_2)
            {
                this.lastReadDKey.Add(obj);
                if (this.lastReadDKey.count >= this.lastReadDKey.capacity)
                {
                    SaveWriteDKey(this.lastReadDKey);
                    this.lastReadDKey = new DataKey(this.dataFile, null, this.lastReadDKey.position, -1L);
                    this.lastReadDKey.number = this.bufCount;
                    this.lastReadDKey.index1 = this.count;
                    this.lastReadDKey.index2 = this.count;
                    this.lastReadDKey.changed = true;
                    this.bufCount++;
                    this.dKeys[this.lastReadDKey.number] = this.lastReadDKey;
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
                    this.lastWriteDKey.dataObjs = null;
                }
                this.lastWriteDKey = key;
            }
            this.lastWriteDKey.LoadDataObjects();
            if (this.lastWriteDKey.count < this.lastWriteDKey.capacity)
            {
                this.lastWriteDKey.Add(obj);
                if (this.lastWriteDKey.count == this.lastWriteDKey.capacity)
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
                        key.Add(this.lastWriteDKey.dataObjs[i]);
                        this.lastWriteDKey.dataObjs[i] = null;
                    }
                    this.lastWriteDKey.count = num;
                    this.lastWriteDKey.index2 = this.lastWriteDKey.index1 + (long)this.lastWriteDKey.count - 1;
                    if (this.lastWriteDKey.count != 0)
                    {
                        this.lastWriteDKey.dateTime_2 = this.lastWriteDKey.dataObjs[this.lastWriteDKey.count - 1].DateTime;
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
            for (int i = this.bufCount; i > class19_5.number + 1; i--)
            {
                this.dKeys[i] = this.dKeys[i - 1];
                if (this.dKeys[i] != null)
                {
                    this.dKeys[i].number = i;
                }
            }
            this.bufCount++;
            class19_4.number = class19_5.number + 1;
            this.dKeys[class19_4.number] = class19_4;
            class19_4.prev = class19_5.position;
            class19_4.next = class19_5.next;
            SaveWriteDKey(class19_4);
            this.dataFile.WriteObjectKey(this.objectKey_0);
        }

        private void SaveWriteDKey(DataKey class19_4)
        {
            long num = class19_4.position;
            this.dataFile.WriteObjectKey(class19_4);
            if (class19_4.position != num)
            {
                DataKey @class = null;
                if (class19_4.number != 0)
                {
                    @class = this.dKeys[class19_4.number - 1];
                }
                if (@class != null)
                {
                    @class.next = class19_4.position;
                    if (!@class.changed)
                    {
                        WriteDKeyNext(class19_4.prev, class19_4.position);
                    }
                }
                else if (class19_4.prev != -1)
                {
                    WriteDKeyNext(class19_4.prev, class19_4.position);
                }
                DataKey class2 = null;
                if (class19_4.number != this.bufCount - 1)
                {
                    class2 = this.dKeys[class19_4.number + 1];
                }
                if (class2 != null)
                {
                    class2.prev = class19_4.position;
                    if (!class2.changed)
                    {
                        this.WriteDKeyPrev(class19_4.next, class19_4.position);
                    }
                }
                else if (class19_4.next != -1)
                {
                    this.WriteDKeyPrev(class19_4.next, class19_4.position);
                }
            }
            if (class19_4 == this.lastReadDKey)
            {
                if (this.position1 == -1)
                {
                    this.position1 = this.lastReadDKey.position;
                }
                this.position2 = this.lastReadDKey.position;
            }
            this.dataFile.WriteObjectKey(this.objectKey_0);
        }

        private void UpdateDKeyPrev(DataKey key, long long_4)
        {
            key.prev = long_4;
            WriteDKeyPrev(key.position, long_4);
        }

        private void UpdateDKeyNext(DataKey key, long long_4)
        {
            key.next = long_4;
            WriteDKeyNext(key.position, long_4);
        }

        private void WriteDKeyPrev(long long_4, long long_5)
        {
            var mstream = new MemoryStream();
            var writer = new BinaryWriter(mstream);
            writer.Write(long_5);
            var buf = mstream.ToArray();
            this.dataFile.WriteBuffer(buf, long_4 + 61, 8);
        }

        private void WriteDKeyNext(long long_4, long long_5)
        {
            var mstream = new MemoryStream();
            var writer = new BinaryWriter(mstream);
            writer.Write(long_5);
            var buf = mstream.ToArray();
            this.dataFile.WriteBuffer(buf, long_4 + 69, 8);
        }

        private void Delete(DataKey class19_4)
        {
            if (class19_4.position == this.position2)
            {
                if (class19_4.prev != -1L)
                {
                    this.position2 = class19_4.prev;
                }
                else
                {
                    this.position1 = -1L;
                    this.position2 = -1L;
                }
            }
            this.dataFile.DeleteObjectKey(class19_4, false, true);
            if (class19_4.prev != -1)
            {
                DataKey @class = this.dKeys[class19_4.number - 1];
                if (@class != null)
                {
                    @class.next = class19_4.next;
                    if (!@class.changed)
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
                DataKey class2 = this.dKeys[class19_4.number + 1];
                if (class2 != null)
                {
                    class2.prev = class19_4.prev;
                    if (!class2.changed)
                    {
                        this.WriteDKeyPrev(class19_4.next, class19_4.prev);
                    }
                }
                else
                {
                    this.WriteDKeyPrev(class19_4.next, class19_4.prev);
                }
            }
            for (int i = class19_4.number; i < this.bufCount - 1; i++)
            {
                this.dKeys[i] = this.dKeys[i + 1];
                if (this.dKeys[i] != null)
                {
                    this.dKeys[i].number = i;
                }
            }
            this.bufCount--;
            this.dataFile.WriteObjectKey(this.objectKey_0);
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
            var key = this.dKeys[0];
            if (key == null)
            {
                key = this.GetDataKey(this.position1);
                this.dKeys[0] = key;
            }
            key.number = 0;
            key.index1 = 0L;
            key.index2 = (long)(key.count - 1);
            return key;
        }

        private DataKey GetNextKey(DataKey key)
        {
            if (key.number == -1)
            {
                Console.WriteLine("DataSeries::GetNextKey Error: key.number is not set");
            }
            DataKey @class = this.dKeys[key.number + 1];
            if (@class == null)
            {
                if (key.next == -1)
                {
                    Console.WriteLine("DataSeries::GetNextKey Error: key.next is not set");
                }
                @class = this.GetDataKey(key.next);
                @class.number = key.number + 1;
                this.dKeys[@class.number] = @class;
            }
            @class.index1 = key.index2 + 1;
            @class.index2 = @class.index1 + (long)@class.count - 1;
            return @class;
        }

        internal DataKey GetKey(long index, DataKey key = null)
        {
            if (index >= 0L && index < this.count)
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
                        @class = this.GetNextKey(key);
                    }
                }
                if (@class == null)
                {
                    @class = this.GetFirstKey();
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
                if (dateTime >= key.dateTime_1 && dateTime <= key.dateTime_2)
                {
                    return key;
                }
                if (dateTime > key.dateTime_2)
                {
                    class2 = key;
                    @class = this.GetNextKey(class2);
                }
            }
            if (@class == null)
            {
                @class = this.GetFirstKey();
            }
            while (option == IndexOption.Null || class2 == null || !(dateTime > class2.dateTime_2) || !(dateTime < @class.dateTime_1))
            {
                if (dateTime >= @class.dateTime_1 && dateTime <= @class.dateTime_2)
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
                    this.SaveWriteDKey(this.lastWriteDKey);
                }
                if (this.lastReadDKey != null && this.lastReadDKey.changed)
                {
                    this.SaveWriteDKey(this.lastReadDKey);
                }
                if (this.lastDeleteDKey != null && this.lastDeleteDKey.changed)
                {
                    this.SaveWriteDKey(this.lastDeleteDKey);
                }
                this.SaveDataKeysKey();
                this.dataFile.WriteObjectKey(this.objectKey_0);
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

        public DataObject GetNext()
        {
            return this.current > this.index2 ? null : this.series.Get(this.current++);
        }
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