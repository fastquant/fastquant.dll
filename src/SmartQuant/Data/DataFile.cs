using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SmartQuant
{
    public class DataKey : ObjectKey
    {
        internal static string LABEL = "DKey";
        private static int LABEL_SIZE = 5;
        internal static int HEADER_SIZE = 77;
        internal static int PREV_OFFSET = 61;
        internal static int NEXT_OFFSET = 69;

        public DataKey(DataFile dataFile, object obj = null, long prev = -1, long next = -1) : base(dataFile, "", obj)
        {
            Label = "DK01";
            this.capcity = HEADER_SIZE;
            this.prev = prev;
            this.next = next;
        }

        public void Add(DataObject obj)
        {
            if (this.count == this.capcity)
            {
                Console.WriteLine("DataKey::Add Can not add object. Buffer is full.");
                return;
            }
            if (this.objs == null)
                this.objs = new DataObject[this.capcity];

            if (this.count == 0)
            {
                this.objs[this.count++] = obj;
                this.dateTime1 = obj.DateTime;
                this.dateTime2 = obj.DateTime;
            }
            else if (obj.DateTime >= this.objs[this.count - 1].DateTime)
            {
                this.objs[this.count++] = obj;
                this.dateTime2 = obj.DateTime;
            }
            else
            {
                int index = this.count;
                while (true)
                {
                    this.objs[index] = this.objs[index - 1];
                    if (obj.DateTime >= this.objs[index].DateTime)
                    {
                        break;
                    }
                    if (index == 1)
                    {
                        break;
                    }
                    index--;
                }
                this.objs[index - 1] = obj;
                if (index == 1)
                {
                    this.dateTime1 = obj.DateTime;
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
            this.objs[index] = obj;
            this.changed = true;
        }

        public void Remove(long long_5)
        {
            if (this.objs == null)
                LoadDataObjects();

            for (long num = long_5; num < (long)(this.count - 1); num += 1)
            {
                checked
                {
                    this.objs[(int)((IntPtr)num)] = this.objs[(int)((IntPtr)(unchecked(num + 1L)))];
                }
            }
            this.count--;
            this.changed = true;
            if (this.count == 0)
            {
                return;
            }
            if (long_5 == 0)
            {
                this.dateTime1 = this.objs[0].DateTime;
            }
            if (long_5 == (long)this.count)
            {
                this.dateTime2 = this.objs[this.count - 1].DateTime;
            }
        }

        public DataObject[] LoadDataObjects()
        {
            if (this.objs != null)
                return this.objs;

            this.objs = new DataObject[this.capcity];
            if (this.contentSize == -1)
            {
                return this.objs;
            }
            var input = new MemoryStream(ReadObjectData(true));
            var reader = new BinaryReader(input);
            for (int i = 0; i < this.count; i++)
                this.objs[i] = (DataObject)this.dataFile.StreamerManager.Deserialize(reader);
            return this.objs;
        }

        public DataObject Get(int index)
        {
            return LoadDataObjects()[index];
        }

        public DataObject Get(DateTime dateTime)
        {
            if (this.objs == null)
                LoadDataObjects();

            return this.objs.FirstOrDefault(d => d.DateTime >= dateTime);
        }

        public int GetIndex(DateTime dateTime, SearchOption option = SearchOption.Next)
        {
            if (this.objs == null)
                LoadDataObjects();

            for (int i = 0; i < this.count; i++)
            {
                if (this.objs[i].DateTime >= dateTime)
                {
                    switch (option)
                    {
                        case SearchOption.Next:
                            return i;
                        case SearchOption.Prev:
                            if (this.objs[i].DateTime == dateTime)
                            {
                                return i;
                            }
                            return i - 1;
                        case SearchOption.ExactFirst:
                            if (this.objs[i].DateTime != dateTime)
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
                Label = reader.ReadString();
                if (!Label.StartsWith("DK"))
                {
                    Console.WriteLine($"ObjectKey::ReadKey This is not DataKey! version = {Label}");
                }
            }
            ReadHeader(reader);
            this.capcity = reader.ReadInt32();
            this.count = reader.ReadInt32();
            this.dateTime1 = new DateTime(reader.ReadInt64());
            this.dateTime2 = new DateTime(reader.ReadInt64());
            this.prev = reader.ReadInt64();
            this.next = reader.ReadInt64();
        }

        public override string ToString()
        {
            return $"DataKey position = {this.position} prev = {this.prev} next = {this.next}  number {this.number} size = {this.capcity}  count = {this.count} {this.changed}  index1 = {this.index1}  index2 = {this.index2}";
        }

        internal override void Write(BinaryWriter writer)
        {
            var buf = WriteObjectData(true);
            this.contentSize = buf.Length;
            if (this.totalSize == -1)
                this.totalSize = this.capcity + this.contentSize;
            WriteKey(writer);
            writer.Write(buf, 0, buf.Length);
        }

        internal override void WriteKey(BinaryWriter writer)
        {
            WriteHeader(writer);                   // 37 == ObjectKey.HEADER_SIZE
            writer.Write(this.capcity);               // 41
            writer.Write(this.count);              // 45
            writer.Write(this.dateTime1.Ticks);    // 53    
            writer.Write(this.dateTime2.Ticks);    // 61
            writer.Write(this.prev);               // 69
            writer.Write(this.next);               // 77
        }

        internal override byte[] WriteObjectData(bool compress = true)
        {
            var mstream = new MemoryStream();
            var writer = new BinaryWriter(mstream);
            var smanager = this.dataFile.StreamerManager;
            byte typeId = this.objs[0].TypeId;
            var streamer = smanager.Get(typeId);
            for (int i = 0; i < this.count; i++)
            {
                if (this.objs[i].TypeId != typeId)
                {
                    typeId = this.objs[i].TypeId;
                    streamer = smanager.Get(typeId);
                }
                writer.Write(streamer.TypeId);
                writer.Write(streamer.GetVersion(this.objs[i]));
                streamer.Write(writer, this.objs[i]);
            }
            var data = mstream.ToArray();
            return compress && CompressionLevel != 0 ? new QuickLZ().Compress(data) : data;
        }

        internal DataObject[] objs;

        internal DateTime dateTime1;

        internal DateTime dateTime2;

        internal int capcity;

        internal int count;

        internal int number;

        internal long index1;

        internal long index2;

        internal long prev;

        internal long next;
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
        internal static string LABEL = "FKey";
        private static int LABEL_SIZE = 5;
        internal static int HEADER_SIZE = 17;

        public FreeKey()
        {
        }

        public FreeKey(ObjectKey key) : this(key.dataFile, key.position, key.totalSize)
        {
        }

        public FreeKey(DataFile dataFile, long position = -1, int size = -1)
        {
            this.dataFile = dataFile;
            this.position = position;
            this.size = size;
        }

        public int CompareTo(FreeKey other) => other == null ? 1 : this.size.CompareTo(other.size);

        public void WriteKey(BinaryWriter writer)
        {
            writer.Write(this.label);       // 5 == length("FKey")    
            writer.Write(this.position);    // 13
            writer.Write(this.size);        // 17
        }

        public void ReadKey(BinaryReader reader, bool readLabel = true)
        {
            if (readLabel)
            {
                this.label = reader.ReadString();
                if (!this.label.StartsWith("FK"))
                {
                    Console.WriteLine($"FreeKey::ReadKey This is not FreeKey! version = {this.label}");
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
                DeleteObjectKey(key, true, true);
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
            using (var input = new MemoryStream(buffer))
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

        internal ObjectKey ReadKey(long position)
        {
            var buffer = new byte[ObjectKey.HEADER_SIZE];
            ReadBuffer(buffer, position, buffer.Length);
            var input = new MemoryStream(buffer);
            var reader = new BinaryReader(input);
            string text = reader.ReadString();

            var key = new ObjectKey(this, null, null);
            key.Label = text;
            key.ReadHeader(reader);

            buffer = new byte[key.headSize];
            ReadBuffer(buffer, position, buffer.Length);
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

        private FreeKey GetFreeKeyWithEnoughLength(int size)
        {
            return this.freeKeys.FirstOrDefault(k => k.size >= size);
        }

        internal void DeleteObjectKey(ObjectKey key, bool remove = true, bool recycle = true)
        {
            key.freed = true;
            WriteBuffer(new byte[] { 1 }, key.position + ObjectKey.LABEL_SIZE, 1);
            if (remove)
            {
                Keys.Remove(key.Name);
                this.oKeysCount--;
            }
            if (recycle)
            {
                this.freeKeys.Add(new FreeKey(key));
                this.freeKeys.Sort();
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
                if (this.mstream.Length > (long)key.totalSize)
                {
                    DeleteObjectKey(key, false, true);
                    key.totalSize = (int)this.mstream.Length;
                    var fKey = key == this.fKeysKey ? GetFreeKeyWithEnoughLength(key.headSize + key.contentSize - FreeKey.HEADER_SIZE) : GetFreeKeyWithEnoughLength(key.headSize + key.contentSize);
                    if (fKey != null)
                    {
                        key.position = fKey.position;
                        key.totalSize = fKey.size;
                        this.freeKeys.Remove(fKey);
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
            this.oKeysKeySize = this.oKeysKey.headSize + this.oKeysKey.contentSize;
        }

        private void SaveFKeys()
        {
            if (this.fKeysKey != null)
                DeleteObjectKey(this.fKeysKey, false, true);

            this.fKeysKey = new ObjectKey(this, "FreeKeys", new FreeKeyList(this.freeKeys));
            this.fKeysKey.CompressionLevel = 0;
            WriteObjectKey(this.fKeysKey);
            this.fKeysKeyPosition = this.fKeysKey.position;
            this.fKeysKeySize = this.fKeysKey.headSize + this.fKeysKey.contentSize;
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
                // when it is empty
                this.headSize = HEAD_SIZE;
                this.newKeyPosition = HEAD_SIZE;
                Reset();
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

            var key = ReadObjectKey(this.fKeysKeyPosition, this.fKeysKeySize);
            this.freeKeys = ((FreeKeyList)key.GetObject()).Keys;
            this.fKeysKey = key;
        }

        protected void ReadKeys()
        {
            if (this.oKeysKeySize == 0)
                return;

            var key = ReadObjectKey(this.oKeysKeyPosition, this.oKeysKeySize);
            Keys = ((ObjectKeyList)key.GetObject()).Keys;
            foreach (var k in Keys.Values)
                k.Init(this);
            this.oKeysKey = key;
        }

        // TODO: rewrite it!
        public virtual void Recover()
        {
            Keys.Clear();
            this.freeKeys.Clear();
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
                    this.freeKeys.Add(new FreeKey(objectKey));
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
                num += (long)objectKey.totalSize;
                if (objectKey.totalSize <= 0)
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
            // do nothing
        }

        public virtual void Write(string name, object obj)
        {
            ObjectKey key;
            Keys.TryGetValue(name, out key);
            if (key != null)
            {
                key.obj = obj;
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

        private FileMode mode;

        private int oKeysKeySize;

        private int fKeysKeySize;

        private int oKeysCount;

        private int fKeysCount;

        private List<FreeKey> freeKeys = new List<FreeKey>();

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

        #region Testing Functions
        public ObjectKey GetKey(string name)
        {
            ObjectKey key;
            return Keys.TryGetValue(name, out key) ? key : null;
        }
        #endregion
    }
}