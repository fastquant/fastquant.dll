using System;
using System.IO;

namespace SmartQuant
{
    public class DataKeyIdArray
    {
        public DataKeyIdArray(IdArray<DataKey> keys)
        {
            Keys = keys;
        }

        public IdArray<DataKey> Keys { get; }

        #region Extra
        public void ToWriter(BinaryWriter writer)
        {
            writer.Write(Keys.Size);
            for (int i = 0; i < Keys.Size; i++)
            {
                var key = Keys[i];
                if (key != null)
                {
                    writer.Write(i);
                    key.WriteKey(writer);
                }
            }
            writer.Write(-1);
        }

        public static object FromReader(BinaryReader reader, byte version)
        {
            int size = reader.ReadInt32();
            var keys = new IdArray<DataKey>(size);
            while (true)
            {
                int index = reader.ReadInt32();
                if (index == -1)
                    break;
                var key = new DataKey(null, null, -1, -1);
                key.Read(reader, true);
                keys.Add(index, key);
            }
            return new DataKeyIdArray(keys);
        }
        #endregion
    }

    public class ObjectKey : IComparable<ObjectKey>
    {
        private const string LABEL = "OKey";
        internal const int LABEL_SIZE = 5;
        internal const int HEADER_SIZE = 37;
        internal const int EMPTYNAME_KEY_SIZE = HEADER_SIZE + 1;  // 38

        public ObjectKey()
        {
        }

        public ObjectKey(DataFile file, string name = null, object obj = null)
        {
            Name = name;
            this.obj = obj;

            if (file != null)
            {
                CompressionMethod = file.CompressionMethod;
                CompressionLevel = file.CompressionLevel;
                Init(file);
            }
        }

        public int CompareTo(ObjectKey other) => other == null ? 1 : this.totalSize.CompareTo(other.totalSize);

        public virtual void Dump()
        {
            var streamer = this.dataFile.StreamerManager.Get(TypeId);
            var stype = streamer != null ? $"{streamer.Type}" : $"Unknown streamer, typeId = {TypeId}";
            Console.WriteLine($"{Name} of typeId {TypeId} ({stype}) position = {this.position}");
        }

        public virtual object GetObject()
        {
            if (this.obj != null)
                return this.obj;

            if (this.contentSize == -1)
                return null;

            var input = new MemoryStream(ReadObjectData(true));
            var reader = new BinaryReader(input);
            var streamer = this.dataFile.StreamerManager.Get(TypeId);
            byte version = reader.ReadByte();
            this.obj = streamer.Read(reader, version);
            if (TypeId == ObjectType.DataSeries)
                ((DataSeries)this.obj).Init(this.dataFile, this);
            return this.obj;
        }

        public void Init(DataFile file)
        {
            this.dataFile = file;
            if (this.obj != null)
            {
                ObjectStreamer streamer;
                file.StreamerManager.Get(this.obj.GetType(), out streamer);
                if (streamer != null)
                    TypeId = streamer.TypeId;
                else
                    Console.WriteLine($"ObjectKey::Init Can not find streamer for object of type {this.obj.GetType()}");
            }
        }

        internal byte[] ReadObjectData(bool compress = true)
        {
            var data = new byte[this.contentSize];
            this.dataFile.ReadBuffer(data, this.position + this.headSize, data.Length);
            return compress && CompressionLevel != 0 ? new QuickLZ().Decompress(data) : data;
        }

        internal virtual void Read(BinaryReader reader, bool readLabel = true)
        {
            if (readLabel)
            {
                Label = reader.ReadString();
                if (!Label.StartsWith("OK"))
                {
                    Console.WriteLine($"ObjectKey::Read This is not ObjectKey! label = {Label}");
                }
            }
            ReadHeader(reader);
            Name = reader.ReadString();
        }

        protected internal void ReadHeader(BinaryReader reader)
        {
            this.freed = reader.ReadBoolean();
            DateTime = new DateTime(reader.ReadInt64());
            this.position = reader.ReadInt64();
            this.headSize = reader.ReadInt32();
            this.contentSize = reader.ReadInt32();
            this.totalSize = reader.ReadInt32();
            CompressionMethod = reader.ReadByte();
            CompressionLevel = reader.ReadByte();
            TypeId = reader.ReadByte();
        }

        public override string ToString()
        {
            if (this.dataFile.StreamerManager.Get(TypeId) != null)
                return $"ObjectKey {Name} of typeId {TypeId} ({this.dataFile.StreamerManager.Get(TypeId).Type}) position = {this.position}";
            else
                return $"ObjectKey {Name} of typeId {TypeId} (Unknown streamer, typeId = {TypeId}) position = {this.position}";
        }

        internal virtual void Write(BinaryWriter writer)
        {
            var data = WriteObjectData(true);
            // TODO: string length != byte[] buffer size
            this.headSize = HEADER_SIZE + Name.Length + 1;
            this.contentSize = data.Length;
            if (this.totalSize == -1)
                this.totalSize = this.headSize + this.contentSize;
            WriteKey(writer);
            writer.Write(data, 0, data.Length);
        }

        protected internal void WriteHeader(BinaryWriter writer)
        {
            writer.Write(Label);                // 5
            writer.Write(this.freed);           // 6
            writer.Write(DateTime.Ticks);       // 14
            writer.Write(this.position);        // 22
            writer.Write(this.headSize);        // 26
            writer.Write(this.contentSize);     // 30
            writer.Write(this.totalSize);       // 34
            writer.Write(CompressionMethod);    // 35
            writer.Write(CompressionLevel);     // 36
            writer.Write(TypeId);               // 37
        }

        internal virtual void WriteKey(BinaryWriter writer)
        {
            WriteHeader(writer);
            writer.Write(Name);
        }

        internal virtual byte[] WriteObjectData(bool compress = true)
        {
            var mstream = new MemoryStream();
            var writer = new BinaryWriter(mstream);
            Type type = this.obj.GetType();
            var streamer = this.dataFile.StreamerManager.Get(type);
            writer.Write(streamer.GetVersion(this.obj));
            streamer.Write(writer, this.obj);
            var data = mstream.ToArray();
            return compress && CompressionLevel != 0 ? new QuickLZ().Compress(data) : data;
        }

        public byte CompressionLevel { get; internal set; } = 1;

        public byte CompressionMethod { get; internal set; } = 1;

        public DateTime DateTime { get; internal set; }

        public string Name { get; private set; }

        public byte TypeId { get; private set; }

        protected internal bool changed;

        internal string Label { get; set; }

        internal object obj;

        internal DataFile dataFile;

        internal bool freed;

        internal int headSize = -1;

        internal int contentSize = -1;

        internal int totalSize = -1;

        internal long position = -1;
    }
}