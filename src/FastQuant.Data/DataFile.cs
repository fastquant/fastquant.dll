using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SmartQuant
{
    public class DataKey : ObjectKey
    {
        public DataKey(DataFile dataFile, object obj = null, long prev = -1, long next = -1) : base(dataFile, "", obj)
        {
            this.label = "DK01";
            this.size = 77;
            this.prev = prev;
            this.next = next;
        }

        public void Add(DataObject obj)
        {
            if (this.count == this.size)
            {
                Console.WriteLine("DataKey::Add Can not add object. Buffer is full.");
                return;
            }
            if (this.dataObjs == null)
            {
                this.dataObjs = new DataObject[this.size];
            }
            if (this.count == 0)
            {
                this.dataObjs[this.count++] = obj;
                this.dateTime_1 = obj.DateTime;
                this.dateTime_2 = obj.DateTime;
            }
            else if (obj.DateTime >= this.dataObjs[this.count - 1].DateTime)
            {
                this.dataObjs[this.count++] = obj;
                this.dateTime_2 = obj.DateTime;
            }
            else
            {
                int index = this.count;
                while (true)
                {
                    this.dataObjs[index] = this.dataObjs[index - 1];
                    if (obj.DateTime >= this.dataObjs[index].DateTime)
                    {
                        break;
                    }
                    if (index == 1)
                    {
                        break;
                    }
                    index--;
                }
                this.dataObjs[index - 1] = obj;
                if (index == 1)
                {
                    this.dateTime_1 = obj.DateTime;
                }
                this.count++;
            }
            if (this.count > 1)
            {
                this.index2 += 1;
            }
            this.changed = true;
        }

        public void Update(int index, DataObject obj)
        {
            this.dataObjs[index] = obj;
            this.changed = true;
        }

        public void Remove(long long_5)
        {
            if (this.dataObjs == null)
                LoadDataObjects();

            for (long num = long_5; num < (long)(this.count - 1); num += 1L)
            {
                checked
                {
                    this.dataObjs[(int)((IntPtr)num)] = this.dataObjs[(int)((IntPtr)(unchecked(num + 1L)))];
                }
            }
            this.count--;
            this.changed = true;
            if (this.count == 0)
            {
                return;
            }
            if (long_5 == 0L)
            {
                this.dateTime_1 = this.dataObjs[0].DateTime;
            }
            if (long_5 == (long)this.count)
            {
                this.dateTime_2 = this.dataObjs[this.count - 1].DateTime;
            }
        }

        public DataObject[] LoadDataObjects()
        {
            if (this.dataObjs != null)
                return this.dataObjs;

            this.dataObjs = new DataObject[this.size];
            if (this.contentSize == -1)
            {
                return this.dataObjs;
            }
            var input = new MemoryStream(ReadObjectData(true));
            var reader = new BinaryReader(input);
            for (int i = 0; i < this.count; i++)
            {
                this.dataObjs[i] = (DataObject)this.dataFile.StreamerManager.Deserialize(reader);
            }
            return this.dataObjs;
        }

        public DataObject Get(int index)
        {
            return LoadDataObjects()[index];
        }

        public DataObject Get(DateTime dateTime)
        {
            if (this.dataObjs == null)
                LoadDataObjects();

            return this.dataObjs.FirstOrDefault(d => d.DateTime >= dateTime);
        }

        public int GetIndex(DateTime dateTime, SearchOption option = SearchOption.Next)
        {
            if (this.dataObjs == null)
                LoadDataObjects();

            for (int i = 0; i < this.count; i++)
            {
                if (this.dataObjs[i].DateTime >= dateTime)
                {
                    switch (option)
                    {
                        case SearchOption.Next:
                            return i;
                        case SearchOption.Prev:
                            if (this.dataObjs[i].DateTime == dateTime)
                            {
                                return i;
                            }
                            return i - 1;
                        case SearchOption.ExactFirst:
                            if (this.dataObjs[i].DateTime != dateTime)
                            {
                                return -1;
                            }
                            return i;
                        default:
                            Console.WriteLine($"DataKey::GetIndex Unknown search option: {option}");
                            break;
                    }
                }
            }
            return -1;
        }

        internal override void Read(BinaryReader reader, bool readLabel = true)
        {
            if (readLabel)
            {
                this.label = reader.ReadString();
                if (!this.label.StartsWith("DK"))
                {
                    Console.WriteLine($"ObjectKey::ReadKey This is not DataKey! version = {this.label}");
                }
            }
            ReadHeader(reader);
            this.size = reader.ReadInt32();
            this.count = reader.ReadInt32();
            this.dateTime_1 = new DateTime(reader.ReadInt64());
            this.dateTime_2 = new DateTime(reader.ReadInt64());
            this.prev = reader.ReadInt64();
            this.next = reader.ReadInt64();
        }

        public override string ToString()
        {
            return $"DataKey position = {this.position} prev = {this.prev} next = {this.next}  number {this.number} size = {this.size}  count = {this.count} {this.changed}  index1 = {this.index1}  index2 = {this.index2}";
        }

        internal override void Write(BinaryWriter writer)
        {
            var buf = WriteObjectData(true);
            this.contentSize = buf.Length;
            if (this.int_2 == -1)
            {
                this.int_2 = this.size + this.contentSize;
            }
            WriteKey(writer);
            writer.Write(buf, 0, buf.Length);
        }

        internal override void WriteKey(BinaryWriter writer)
        {
            WriteHeader(writer);
            writer.Write(this.size);
            writer.Write(this.count);
            writer.Write(this.dateTime_1.Ticks);
            writer.Write(this.dateTime_2.Ticks);
            writer.Write(this.prev);
            writer.Write(this.next);
        }

        internal override byte[] WriteObjectData(bool compress = true)
        {
            var mstream = new MemoryStream();
            var writer = new BinaryWriter(mstream);
            var smanager = this.dataFile.StreamerManager;
            byte typeId = this.dataObjs[0].TypeId;
            var streamer = smanager.Get(typeId);
            for (int i = 0; i < this.count; i++)
            {
                if (this.dataObjs[i].TypeId != typeId)
                {
                    typeId = this.dataObjs[i].TypeId;
                    streamer = smanager.Get(typeId);
                }
                writer.Write(streamer.TypeId);
                writer.Write(streamer.GetVersion(this.dataObjs[i]));
                streamer.Write(writer, this.dataObjs[i]);
            }
            var data = mstream.ToArray();
            return compress && CompressionLevel != 0 ? new QuickLZ().Compress(data) : data;
        }

        internal DataObject[] dataObjs;

        internal DateTime dateTime_1;

        internal DateTime dateTime_2;

        internal new int size;

        internal int count;

        internal int number;

        internal long index1;

        internal long index2;

        internal long prev;

        internal long next;

        internal int capacity;
    }

    public class FreeKeyList
    {
        public List<FreeKey> Keys { get; }

        public FreeKeyList(List<FreeKey> keys)
        {
            Keys = keys;
        }

        #region Extra

        public static FreeKeyList FromReader(BinaryReader reader, byte version)
        {
            var keys = new List<FreeKey>();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                var key = new FreeKey();
                key.ReadKey(reader, true);
                keys.Add(key);
            }
            return new FreeKeyList(keys);
        }

        public void ToWriter(BinaryWriter writer)
        {
            writer.Write(Keys.Count);
            foreach (var key in Keys)
                key.WriteKey(writer);
        }

        #endregion
    }


    public class ObjectKeyList
    {
        public ObjectKeyList(Dictionary<string, ObjectKey> keys)
        {
            Keys = keys;
        }
        public Dictionary<string, ObjectKey> Keys { get; }

        #region Extra

        public static ObjectKeyList FromReader(BinaryReader reader, byte version)
        {
            var keys = new Dictionary<string, ObjectKey>();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                var key = new ObjectKey();
                key.Read(reader, true);
                keys.Add(key.Name, key);
            }
            return new ObjectKeyList(keys);
        }

        public void ToWriter(BinaryWriter writer)
        {
            writer.Write(Keys.Count);
            foreach (var key in Keys.Values)
                key.WriteKey(writer);
        }

        #endregion
    }

    public class FreeKey : IComparable<FreeKey>
    {
        public FreeKey()
        {
        }

        public FreeKey(ObjectKey key):this(key.dataFile, key.position, key.int_2)
        {
        }

        public FreeKey(DataFile dataFile, long position = -1, int int_1 = -1)
        {
            this.dataFile = dataFile;
            this.position = position;
            this.size = int_1;
        }

        public int CompareTo(FreeKey other) => other == null ? 1 : this.size.CompareTo(other.size);

        public void WriteKey(BinaryWriter writer)
        {
            writer.Write(this.label);
            writer.Write(this.position);
            writer.Write(this.size);
        }

        public void ReadKey(BinaryReader reader, bool readLabel = true)
        {
            if (readLabel)
            {
                this.label = reader.ReadString();
                if (!this.label.StartsWith("FK"))
                {
                    Console.WriteLine("FreeKey::ReadKey This is not FreeKey! version = " + this.label);
                }
            }
            this.position = reader.ReadInt64();
            this.size = reader.ReadInt32();
        }

        internal DataFile dataFile;

        internal int size = -1;

        internal long position = -1;

        internal string label = "FK01";
    }

    public class DataFile
    {
        private static int HEAD_SIZE = 62;
        public StreamerManager StreamerManager { get; }

        public DataFile(string name, StreamerManager streamerManager)
        {
            this.name = name;
            StreamerManager = streamerManager;
            this.mstream = new MemoryStream();
            this.writer = new BinaryWriter(this.mstream);
        }

        public virtual void Close()
        {
            if (!this.opened)
            {
                Console.WriteLine($"DataFile::Close File is not open: {this.name}");
                return;
            }
            Flush();
            CloseFileStream();
            this.opened = false;
        }

        protected virtual void CloseFileStream()
        {
#if DNXCORE50
            this.stream.Flush();
            this.stream.Dispose();
#else
            this.stream.Close();
#endif
        }

        public virtual void Delete(string name)
        {
            ObjectKey key;
            Keys.TryGetValue(name, out key);
            if (key != null)
            {
                this.DeleteObjectKey(key, true, true);
            }
        }

        public void Dispose()
        {
            Console.WriteLine("DataFile::Dispose");
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Dump()
        {
            if (this.opened)
            {
                Console.WriteLine($"DataFile::Dump DataFile  {this.name} is open in {this.mode}  mode and contains {Keys.Values.Count} objects:");
                foreach (var k in Keys.Values)
                    k.Dump();
                Console.WriteLine($"Free objects = {this.fKeysCount}");
            }
            else
            {
                Console.WriteLine($"DataFile::Dump DataFile {this.name} is closed");
            }
        }

        ~DataFile()
        {
            Dispose(false);
        }

        public virtual void Flush()
        {
            if (!this.opened)
            {
                Console.WriteLine($"DataFile::Flush Can not flush file which is not open {this.name}");
                return;
            }
            if (this.changed)
            {
                foreach (var k in Keys.Values)
                {
                    if (k.TypeId == ObjectType.DataSeries && k.obj != null)
                    {
                        var dataSeries = (DataSeries)k.obj;
                        if (dataSeries.changed)
                        {
                            dataSeries.Flush();
                        }
                    }
                }
                this.SaveOKeys();
                this.SaveFKeys();
                WriteHeader();
                this.stream.Flush();
            }
            this.changed = false;
        }

        public virtual object Get(string name)
        {
            ObjectKey key;
            Keys.TryGetValue(name, out key);
            return key?.GetObject();
        }

        internal bool ReadHeader()
        {
            var buffer = new byte[HEAD_SIZE];
            ReadBuffer(buffer, 0, buffer.Length);
            using(var input = new MemoryStream(buffer))
            {
                using (var reader = new BinaryReader(input))
                {
                    var label = reader.ReadString();
                    if (label != "SmartQuant")
                    {
                        Console.WriteLine("DataFile::ReadHeader This is not SmartQuant data file!");
                        return false;
                    }
                    this.label = label;
                    this.version = reader.ReadByte();
                    this.headSize = reader.ReadInt64();
                    this.newKeyPosition = reader.ReadInt64();
                    this.oKeysKeyPosition = reader.ReadInt64();
                    this.fKeysKeyPosition = reader.ReadInt64();
                    this.oKeysKeySize = reader.ReadInt32();
                    this.fKeysKeySize = reader.ReadInt32();
                    this.oKeysCount = reader.ReadInt32();
                    this.fKeysCount = reader.ReadInt32();
                    CompressionMethod = reader.ReadByte();
                    CompressionLevel = reader.ReadByte();
                    return true;
                }
            }
        }

        internal void WriteHeader()
        {
            using (var mstream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(mstream))
                {
                    writer.Write(this.label);                 // 11
                    writer.Write(this.version);               // 12
                    writer.Write(this.headSize);              // 20
                    writer.Write(this.newKeyPosition);        // 28
                    writer.Write(this.oKeysKeyPosition);      // 36
                    writer.Write(this.fKeysKeyPosition);      // 44
                    writer.Write(this.oKeysKeySize);          // 48
                    writer.Write(this.fKeysKeySize);          // 52
                    writer.Write(this.oKeysCount);            // 56
                    writer.Write(this.fKeysCount);            // 60
                    writer.Write(CompressionMethod);          // 61
                    writer.Write(CompressionLevel);           // 62
                    var buf = mstream.ToArray();
                    WriteBuffer(buf, 0, buf.Length);
                }
            }
        }

        private void Dispose(bool disposing)
        {
            if (this.disposed)
                return;
            if (disposing)
                Close();
            this.disposed = true;
        }

        internal void Reset()
        {
            this.changed = true;
            this.oKeysKeyPosition = -1;
            this.fKeysKeyPosition = -1;
            WriteHeader();
        }

        internal ObjectKey ReadKey(long pos)
        {
            int size = 37;
            var buffer = new byte[size];
            ReadBuffer(buffer, pos, size);
            var input = new MemoryStream(buffer);
            var reader = new BinaryReader(input);
            string text = reader.ReadString();

            var key = new ObjectKey(this, null, null);
            key.label = text;
            key.ReadHeader(reader);

            buffer = new byte[key.size];
            ReadBuffer(buffer, pos, buffer.Length);
            input = new MemoryStream(buffer);
            reader = new BinaryReader(input);
            if (text.StartsWith("OK"))
            {
                key = new ObjectKey(this, null, null);
            }
            else
            {
                if (!text.StartsWith("DK"))
                {
                    Console.WriteLine($"DataFile::ReadKey This is not object or data key : {text}");
                    return null;
                }
                key = new DataKey(this, null, -1, -1);
            }
            key.Read(reader, true);
            return key;
        }

        private FreeKey GetFreeKeyWithEnoughLength(int int_4)
        {
            foreach (FreeKey k in this.fKeys)
            {
                if (k.size >= int_4)
                {
                    return k;
                }
            }
            return null;
        }

        internal void DeleteObjectKey(ObjectKey key, bool remove = true, bool recycle = true)
        {
            key.freed = true;
            WriteBuffer(new byte[] { 1 }, key.position + 5, 1);
            if (remove)
            {
                Keys.Remove(key.Name);
                this.oKeysCount--;
            }
            if (recycle)
            {
                this.fKeys.Add(new FreeKey(key));
                this.fKeys.Sort();
                this.fKeysCount++;
            }
            this.changed = true;
        }

        internal ObjectKey ReadObjectKey(long position, int length)
        {
            var buffer = new byte[length];
            ReadBuffer(buffer, position, buffer.Length);
            var reader = new BinaryReader(new MemoryStream(buffer));
            var key = new ObjectKey(this, null, null);
            key.Read(reader, true);
            key.position = position;
            return key;
        }

        internal void WriteObjectKey(ObjectKey key)
        {
            this.mstream.SetLength(0);
            key.Write(this.writer);
            if (key.position != -1)
            {
                if (this.mstream.Length > (long)key.int_2)
                {
                    DeleteObjectKey(key, false, true);
                    key.int_2 = (int)this.mstream.Length;
                    var fKey = key == this.fKeysKey ? GetFreeKeyWithEnoughLength(key.size + key.contentSize - 17) : GetFreeKeyWithEnoughLength(key.size + key.contentSize);
                    if (fKey != null)
                    {
                        key.position = fKey.position;
                        key.int_2 = fKey.size;
                        this.fKeys.Remove(fKey);
                        this.fKeysCount--;
                        if (key == this.fKeysKey)
                        {
                            this.mstream.SetLength(0);
                            key.Write(this.writer);
                        }
                    }
                    else
                    {
                        key.position = this.stream.Length;
                        this.newKeyPosition = key.position;
                    }
                }
            }
            else
            {
                key.position = this.stream.Length;
                this.newKeyPosition = key.position;
            }
            var buf = this.mstream.ToArray();
            WriteBuffer(buf, key.position, buf.Length);
            key.changed = false;
            this.changed = true;
        }

        private void SaveOKeys()
        {
            if (this.oKeysKey != null)
                DeleteObjectKey(this.oKeysKey, false, true);

            this.oKeysKey = new ObjectKey(this, "ObjectKeys", new ObjectKeyList(Keys));
            this.oKeysKey.CompressionLevel = 0;
            WriteObjectKey(this.oKeysKey);
            this.oKeysKeyPosition = this.oKeysKey.position;
            this.oKeysKeySize = this.oKeysKey.size + this.oKeysKey.contentSize;
        }

        private void SaveFKeys()
        {
            if (this.fKeysKey != null)
                DeleteObjectKey(this.fKeysKey, false, true);

            this.fKeysKey = new ObjectKey(this, "FreeKeys", new FreeKeyList(this.fKeys));
            this.fKeysKey.CompressionLevel = 0;
            WriteObjectKey(this.fKeysKey);
            this.fKeysKeyPosition = this.fKeysKey.position;
            this.fKeysKeySize = this.fKeysKey.size + this.fKeysKey.contentSize;
        }

        public virtual void Open(FileMode mode = FileMode.OpenOrCreate)
        {
            if (mode != FileMode.OpenOrCreate && mode != FileMode.Create)
            {
                Console.WriteLine($"DataFile::Open Can not open file in {mode} mode. DataFile suppports FileMode.OpenOrCreate and FileMode.Create modes.");
                return;
            }
            if (this.opened)
            {
                Console.WriteLine($"DataFile::Open File is already open: {this.name}");
                return;
            }

            this.mode = mode;
            if (!OpenFileStream(this.name, mode))
            {
                this.headSize = HEAD_SIZE;
                this.newKeyPosition = HEAD_SIZE;
                this.Reset();
            }
            else
            {
                if (!ReadHeader())
                {
                    Console.WriteLine($"DataFile::Open Error opening file {this.name}");
                    return;
                }
                if (this.oKeysKeyPosition == -1 || this.fKeysKeyPosition == -1)
                {
                    Console.WriteLine("DataFile::Open The file was not properly closed and needs to be recovered!");
                    Recover();
                }
                ReadKeys();
                ReadFree();
            }
            this.opened = true;
        }

        protected virtual bool OpenFileStream(string name, FileMode mode)
        {
            this.stream = new FileStream(name, mode);
            return this.stream.Length != 0;
        }

        protected internal virtual void ReadBuffer(byte[] buffer, long position, int length)
        {
            lock (this)
            {
                this.stream.Seek(position, SeekOrigin.Begin);
                this.stream.Read(buffer, 0, length);
            }
        }

        protected internal virtual void WriteBuffer(byte[] buffer, long position, int length)
        {
            lock (this)
            {
                this.stream.Seek(position, SeekOrigin.Begin);
                this.stream.Write(buffer, 0, length);
                if (!this.changed)
                {
                    Reset();
                }
            }
        }

        protected void ReadFree()
        {
            if (this.fKeysKeySize == 0)
                return;

            var buffer = new byte[this.fKeysKeySize];
            ReadBuffer(buffer, this.fKeysKeyPosition, this.fKeysKeySize);
            var input = new MemoryStream(buffer);
            var reader = new BinaryReader(input);
            var okey = new ObjectKey(this, null, null);
            okey.Read(reader, true);
            okey.position = this.fKeysKeyPosition;
            this.fKeys = ((FreeKeyList)okey.GetObject()).Keys;
            this.fKeysKey = okey;
        }

        protected void ReadKeys()
        {
            if (this.oKeysKeySize == 0)
                return;

            var buffer = new byte[this.oKeysKeySize];
            ReadBuffer(buffer, this.oKeysKeyPosition, buffer.Length);

            var reader = new BinaryReader(new MemoryStream(buffer));
            var okey = new ObjectKey(this, null, null);
            okey.Read(reader, true);
            okey.position = this.oKeysKeyPosition;
            Keys = ((ObjectKeyList)okey.GetObject()).Keys;
            foreach (var k in Keys.Values)
                k.Init(this);
            this.oKeysKey = okey;
        }

        public virtual void Recover()
        {
            Keys.Clear();
            this.fKeys.Clear();
            long num = this.headSize;
            new BinaryReader(this.stream);
            ObjectKey objectKey = null;
            while (true)
            {
                try
                {
                    objectKey = ReadKey(num);
                    goto IL_F6;
                }
                catch (Exception arg)
                {
                    Console.WriteLine("DataFile::Recover exception " + arg);
                    break;
                }
                goto IL_4E;
                IL_6A:
                if (objectKey.freed)
                {
                    this.fKeys.Add(new FreeKey(objectKey));
                }
                else if (!(objectKey is DataKey) && objectKey.TypeId != ObjectType.DataKeyIdArray)
                {
                    if (objectKey.TypeId != ObjectType.ObjectKeyList)
                    {
                        if (objectKey.TypeId != ObjectType.FreeKeyList)
                        {
                            Keys.Add(objectKey.Name, objectKey);
                            goto IL_CA;
                        }
                    }
                    this.DeleteObjectKey(objectKey, false, true);
                }
                IL_CA:
                Console.WriteLine(objectKey);
                num += (long)objectKey.int_2;
                if (objectKey.int_2 <= 0)
                {
                    break;
                }
                if (num >= this.stream.Length)
                {
                    break;
                }
                continue;
                IL_4E:
                Console.WriteLine("DataFile::Recover Key position is -1 , setting to " + num);
                objectKey.position = num;
                goto IL_6A;
                IL_F6:
                if (objectKey.position == -1)
                {
                    goto IL_4E;
                }
                goto IL_6A;
            }
            this.changed = true;
            Flush();
        }

        public virtual void Refresh()
        {
        }

        public virtual void Write(string name, object obj)
        {
            ObjectKey key;
            Keys.TryGetValue(name, out key);
            if (key != null)
            {
                key.dkeyIdArray = (DataKeyIdArray)obj;
                key.Init(this);
            }
            else
            {
                key = new ObjectKey(this, name, obj);
                Keys.Add(name, key);
                this.oKeysCount++;
            }
            key.DateTime = DateTime.Now;
            if (key.TypeId == ObjectType.DataSeries)
            {
                ((DataSeries)obj).Init(this, key);
            }
            WriteObjectKey(key);
        }


        public byte CompressionLevel { get; set; } = 1;

        public byte CompressionMethod { get; set; } = 1;

        public Dictionary<string, ObjectKey> Keys { get; private set; } = new Dictionary<string, ObjectKey>();

        private BinaryWriter writer;

        private bool opened;

        internal bool changed;

        private bool disposed;

        private byte version = 1;

        //internal byte byte_1;

        //internal byte byte_2;

        //internal Dictionary<string, ObjectKey> dictionary_0 = new Dictionary<string, ObjectKey>();

        private FileMode mode;

        private int oKeysKeySize;

        private int fKeysKeySize;

        private int oKeysCount;

        private int fKeysCount;

        private List<FreeKey> fKeys = new List<FreeKey>();

        private long headSize;

        private long newKeyPosition;

        private long oKeysKeyPosition;

        private long fKeysKeyPosition;

        private MemoryStream mstream;

        private ObjectKey oKeysKey;

        private ObjectKey fKeysKey;

        private Stream stream;

        private string label = "SmartQuant";

        private string name;
    }
}