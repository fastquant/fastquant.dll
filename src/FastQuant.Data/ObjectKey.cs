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
        public ObjectKey()
        {
        }

        public ObjectKey(DataFile file, string name = null, object obj = null)
        {
            Name = name;
            this.dkeyIdArray = (DataKeyIdArray)obj;
            if (file != null)
            {
                CompressionMethod = file.CompressionMethod;
                CompressionLevel = file.CompressionLevel;
                Init(file);
            }
        }

        public int CompareTo(ObjectKey other) => other == null ? 1 : this.int_2.CompareTo(other.int_2);

        public virtual void Dump()
        {
            var streamer = this.dataFile.StreamerManager.Get(TypeId);
            var stype = streamer != null ? $"{streamer.Type}" : $"Unknown streamer, typeId = {TypeId}";
            Console.WriteLine($"{Name} of typeId {TypeId} ({stype}) position = {this.position}");
        }

        public virtual object GetObject()
        {
            if (this.dkeyIdArray != null)
                return this.dkeyIdArray;

            if (this.contentSize == -1)
                return null;

            var input = new MemoryStream(ReadObjectData(true));
            var reader = new BinaryReader(input);
            var streamer = this.dataFile.StreamerManager.Get(TypeId);
            byte version = reader.ReadByte();
            var obj = streamer.Read(reader, version);
            this.dkeyIdArray = (DataKeyIdArray)obj;
            if (TypeId == ObjectType.DataSeries)
                ((DataSeries)obj).Init(this.dataFile, this);

            return this.dkeyIdArray;
        }

        public void Init(DataFile dataFile)
        {
            this.dataFile = dataFile;
            if (this.dkeyIdArray != null)
            {
                ObjectStreamer streamer;
                dataFile.StreamerManager.Get(this.dkeyIdArray.GetType(), out streamer);
                if (streamer != null)
                    TypeId = streamer.TypeId;
                else
                    Console.WriteLine($"ObjectKey::Init Can not find streamer for object of type {this.dkeyIdArray.GetType()}");
            }
        }

        internal byte[] ReadObjectData(bool compress = true)
        {
            var data = new byte[this.contentSize];
            this.dataFile.ReadBuffer(data, this.position + this.size, this.contentSize);
            return compress && CompressionLevel != 0 ? new QuickLZ().Decompress(data) : data;
        }

        internal virtual void Read(BinaryReader reader, bool readLabel = true)
        {
            if (readLabel)
            {
                this.label = reader.ReadString();
                if (!this.label.StartsWith("OK"))
                {
                    Console.WriteLine($"ObjectKey::Read This is not ObjectKey! label = {this.label}");
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
            this.size = reader.ReadInt32();
            this.contentSize = reader.ReadInt32();
            this.int_2 = reader.ReadInt32();
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
            this.size = 37 + Name.Length + 1;
            this.contentSize = data.Length;
            if (this.int_2 == -1)
            {
                this.int_2 = this.size + this.contentSize;
            }
            WriteKey(writer);
            writer.Write(data, 0, data.Length);
        }

        protected internal void WriteHeader(BinaryWriter writer)
        {
            writer.Write(this.label);
            writer.Write(this.freed);
            writer.Write(DateTime.Ticks);
            writer.Write(this.position);
            writer.Write(this.size);
            writer.Write(this.contentSize);
            writer.Write(this.int_2);
            writer.Write(CompressionMethod);
            writer.Write(CompressionLevel);
            writer.Write(TypeId);
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
            Type type = this.dkeyIdArray.GetType();
            var streamer = this.dataFile.StreamerManager.Get(type);
            writer.Write(streamer.GetVersion(this.dkeyIdArray));
            streamer.Write(writer, this.dkeyIdArray);
            var data = mstream.ToArray();
            return compress && CompressionLevel != 0 ? new QuickLZ().Compress(data) : data;
        }

        public byte CompressionLevel { get; set; } = 1;

        public byte CompressionMethod { get; set; } = 1;

        public DateTime DateTime { get; set; }

        public string Name { get; private set; }

        public byte TypeId { get; private set; }

        internal bool freed;

        protected internal bool changed;

        internal DataKeyIdArray dkeyIdArray;

        internal DataFile dataFile;

        internal int size = -1;

        internal int contentSize = -1;

        internal int int_2 = -1;

        internal long position = -1;

        internal string label = "OK01";

        internal object obj;
    }
}