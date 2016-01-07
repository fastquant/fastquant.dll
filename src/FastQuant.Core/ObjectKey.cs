using System;
using System.IO;

namespace SmartQuant
{
    public class ObjectType
    {
        public const byte DataSeries = 101;
        public const byte ObjectKeyList = 102;
        public const byte FreeKeyList = 103;
        public const byte ObjectKeyIdArray = 104;
        public const byte DataKeyIdArray = 105;
        public const byte Instrument = 106;
        public const byte AltId = 107;
        public const byte Leg = 108;
    }

    class Class20
    {
        internal Class20(IdArray<DataKey> keys)
        {
            this.keys = keys;
        }

        internal IdArray<DataKey> keys;
    }


    public class ObjectKey : IComparable<ObjectKey>
    {
        public ObjectKey()
        {
        }

        public ObjectKey(DataFile file, string name = null, object obj = null)
        {
            this.name = name;
            this.class20_0 = (Class20)obj;
            if (file != null)
            {
                this.byte_1 = file.byte_2;
                this.byte_0 = file.byte_1;
                this.Init(file);
            }
        }

        public int CompareTo(ObjectKey other)
        {
            if (other == null)
            {
                return 1;
            }
            return this.int_2.CompareTo(other.int_2);
        }

        public virtual void Dump()
        {
            if (this.dataFile_0.StreamerManager.Get(this.typeId) != null)
            {
                Console.WriteLine(string.Concat(new object[]
                {
                    this.name,
                    " of typeId ",
                    this.typeId,
                    " (",
                    this.dataFile_0.StreamerManager.Get(this.typeId).type,
                    ") position = ",
                    this.position
                }));
                return;
            }
            Console.WriteLine(string.Concat(new object[]
            {
                this.name,
                " of typeId ",
                this.typeId,
                " (Unknown streamer, typeId = ",
                this.typeId,
                ") position = ",
                this.position
            }));
        }

        public virtual object GetObject()
        {
            if (this.class20_0 != null)
            {
                return this.class20_0;
            }
            if (this.int_1 == -1)
            {
                return null;
            }
            MemoryStream input = new MemoryStream(this.method_1(true));
            BinaryReader binaryReader = new BinaryReader(input);
            ObjectStreamer objectStreamer = this.dataFile_0.StreamerManager.Get(this.typeId);
            byte version = binaryReader.ReadByte();
            this.class20_0 = (Class20)objectStreamer.Read(binaryReader, version);
            if (this.typeId == 101)
            {
                ((DataSeries)this.class20_0).method_0(this.dataFile_0, this);
            }
            return this.class20_0;
        }

        public void Init(DataFile dataFile_1)
        {
            this.dataFile_0 = dataFile_1;
            if (this.class20_0 != null)
            {

                ObjectStreamer objectStreamer;
                dataFile_1.StreamerManager.Get(this.class20_0.GetType(), out objectStreamer);
                if (objectStreamer != null)
                {
                    this.typeId = objectStreamer.typeId;
                    return;
                }
                Console.WriteLine("ObjectKey::Init Can not find streamer for object of type " + this.class20_0.GetType());
            }
        }

        internal byte[] method_1(bool bool_1 = true)
        {
            byte[] array = new byte[this.int_1];
            this.dataFile_0.ReadBuffer(array, this.position + (long)this.int_0, this.int_1);
            if (bool_1 && this.byte_1 != 0)
            {
                return new QuickLZ().Decompress(array);
            }
            return array;
        }

        internal virtual void Read(BinaryReader reader, bool readLabel = true)
        {
            if (readLabel)
            {
                this.label = reader.ReadString();
                if (!this.label.StartsWith("OK"))
                {
                    Console.WriteLine("ObjectKey::Read This is not ObjectKey! label = " + this.label);
                }
            }
            this.ReadHeader(reader);
            this.name = reader.ReadString();
        }

        protected internal void ReadHeader(BinaryReader reader)
        {
            this.bool_0 = reader.ReadBoolean();
            this.dateTime_0 = new DateTime(reader.ReadInt64());
            this.position = reader.ReadInt64();
            this.int_0 = reader.ReadInt32();
            this.int_1 = reader.ReadInt32();
            this.int_2 = reader.ReadInt32();
            this.byte_0 = reader.ReadByte();
            this.byte_1 = reader.ReadByte();
            this.typeId = reader.ReadByte();
        }

        public override string ToString()
        {
            if (this.dataFile_0.StreamerManager.Get(this.typeId) != null)
            {
                return string.Concat(new object[]
                {
                    "ObjectKey ",
                    this.name,
                    " of typeId ",
                    this.typeId,
                    " (",
                    this.dataFile_0.StreamerManager.Get(this.typeId).type,
                    ") position = ",
                    this.position
                });
            }
            return string.Concat(new object[]
            {
                "ObjectKey ",
                this.name,
                " of typeId ",
                this.typeId,
                " (Unknown streamer, typeId = ",
                this.typeId,
                ") position = ",
                this.position
            });
        }

        internal virtual void Write(BinaryWriter writer)
        {
            byte[] array = this.WriteObjectData(true);
            this.int_0 = 37 + this.name.Length + 1;
            this.int_1 = array.Length;
            if (this.int_2 == -1)
            {
                this.int_2 = this.int_0 + this.int_1;
            }
            this.WriteKey(writer);
            writer.Write(array, 0, array.Length);
        }

        // Token: 0x060003B0 RID: 944 RVA: 0x0001FF5C File Offset: 0x0001E15C
        protected internal void WriteHeader(BinaryWriter writer)
        {
            writer.Write(this.label);
            writer.Write(this.bool_0);
            writer.Write(this.dateTime_0.Ticks);
            writer.Write(this.position);
            writer.Write(this.int_0);
            writer.Write(this.int_1);
            writer.Write(this.int_2);
            writer.Write(this.byte_0);
            writer.Write(this.byte_1);
            writer.Write(this.typeId);
        }

        // Token: 0x060003B1 RID: 945 RVA: 0x00004C41 File Offset: 0x00002E41
        internal virtual void WriteKey(BinaryWriter writer)
        {
            this.WriteHeader(writer);
            writer.Write(this.name);
        }

        internal virtual byte[] WriteObjectData(bool compress = true)
        {
            MemoryStream memoryStream = new MemoryStream();
            BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
            Type type = this.class20_0.GetType();
            ObjectStreamer objectStreamer = this.dataFile_0.StreamerManager.Get(type);
            binaryWriter.Write(objectStreamer.GetVersion(this.class20_0));
            objectStreamer.Write(binaryWriter, this.class20_0);
            byte[] array = memoryStream.ToArray();
            if (compress && this.byte_1 != 0)
            {
                QuickLZ quickLZ = new QuickLZ();
                return quickLZ.Compress(array);
            }
            return array;
        }

        public byte CompressionLevel { get; } = 1;

        public byte CompressionMethod { get; } = 1;

        public DateTime DateTime
        {
            // Token: 0x060003A7 RID: 935 RVA: 0x00004C29 File Offset: 0x00002E29
            get
            {
                return this.dateTime_0;
            }
        }

        public string Name
        {
            get
            {
                return this.name;
            }
        }

        public byte TypeId
        {
            // Token: 0x060003A5 RID: 933 RVA: 0x00004C19 File Offset: 0x00002E19
            get
            {
                return this.typeId;
            }
        }

        // Token: 0x040001A5 RID: 421
        internal bool bool_0;

        // Token: 0x040001AB RID: 427
        internal byte byte_0 = 1;

        // Token: 0x040001AC RID: 428
        internal byte byte_1 = 1;

        // Token: 0x040001AD RID: 429
        internal byte typeId;

        // Token: 0x040001AF RID: 431
        protected internal bool changed;

        // Token: 0x040001A3 RID: 419
        internal Class20 class20_0;

        // Token: 0x040001A2 RID: 418
        internal DataFile dataFile_0;

        // Token: 0x040001A6 RID: 422
        internal DateTime dateTime_0;

        // Token: 0x040001A8 RID: 424
        internal int int_0 = -1;

        // Token: 0x040001A9 RID: 425
        internal int int_1 = -1;

        // Token: 0x040001AA RID: 426
        internal int int_2 = -1;

        internal long position = -1;
        internal string label = "OK01";
        internal string name;
    }
}