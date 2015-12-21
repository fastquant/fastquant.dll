using System;
using System.ComponentModel;
using System.IO;
using SmartQuant;

namespace FastQuant.Private.DataFile
{
    public class ObjectKeyImpl : IComparable<ObjectKeyImpl>
    {
        public ObjectKeyImpl()
        {
            Class59.DyuZFiEzcdouj();
            this.string_0 = "OK01";
            this.long_0 = -1L;
            this.int_0 = -1;
            this.int_1 = -1;
            this.int_2 = -1;
            this.byte_0 = 1;
            this.byte_1 = 1;
            base..ctor();
        }

        public ObjectKeyImpl(DataFileImpl file, string name = null, object obj = null)
        {
            Class59.DyuZFiEzcdouj();
            this.string_0 = "OK01";
            this.long_0 = -1L;
            this.int_0 = -1;
            this.int_1 = -1;
            this.int_2 = -1;
            this.byte_0 = 1;
            this.byte_1 = 1;
            base..ctor();
            this.string_1 = name;
            this.class20_0 = obj;
            if (file != null)
            {
                this.byte_1 = file.byte_2;
                this.byte_0 = file.byte_1;
                this.method_0(file);
            }
        }

        public int CompareTo(ObjectKeyImpl other)
        {
            if (other == null)
            {
                return 1;
            }
            return this.int_2.CompareTo(other.int_2);
        }

        public virtual void Dump()
        {
            if (this.dataFile_0.streamerManager_0.idArray_0[(int)this.byte_2] != null)
            {
                Console.WriteLine(string.Concat(new object[]
                {
                    this.string_1,
                    " of typeId ",
                    this.byte_2,
                    " (",
                    this.dataFile_0.streamerManager_0.idArray_0[(int)this.byte_2].type,
                    ") position = ",
                    this.long_0
                }));
                return;
            }
            Console.WriteLine(string.Concat(new object[]
            {
                this.string_1,
                " of typeId ",
                this.byte_2,
                " (Unknown streamer, typeId = ",
                this.byte_2,
                ") position = ",
                this.long_0
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
            ObjectStreamer objectStreamer = this.dataFile_0.streamerManager_0.idArray_0[(int)this.byte_2];
            byte version = binaryReader.ReadByte();
            this.class20_0 = objectStreamer.Read(binaryReader, version);
            if (this.byte_2 == 101)
            {
                ((DataSeries)this.class20_0).method_0(this.dataFile_0, this);
            }
            return this.class20_0;
        }

        internal void method_0(DataFileImpl dataFile_1)
        {
            this.dataFile_0 = dataFile_1;
            if (this.class20_0 != null)
            {
                ObjectStreamer objectStreamer;
                dataFile_1.streamerManager_0.dictionary_0.TryGetValue(this.class20_0.GetType(), out objectStreamer);
                if (objectStreamer != null)
                {
                    this.byte_2 = objectStreamer.typeId;
                    return;
                }
                Console.WriteLine("ObjectKey::Init Can not find streamer for object of type " + this.class20_0.GetType());
            }
        }

        internal byte[] method_1(bool bool_1 = true)
        {
            byte[] array = new byte[this.int_1];
            this.dataFile_0.ReadBuffer(array, this.long_0 + (long)this.int_0, this.int_1);
            if (bool_1 && this.byte_1 != 0)
            {
                QuickLZ quickLZ = new QuickLZ();
                return quickLZ.Decompress(array);
            }
            return array;
        }

        internal virtual void Read(BinaryReader reader, bool readLabel = true)
        {
            if (readLabel)
            {
                this.string_0 = reader.ReadString();
                if (!this.string_0.StartsWith("OK"))
                {
                    Console.WriteLine("ObjectKey::Read This is not ObjectKey! label = " + this.string_0);
                }
            }
            this.ReadHeader(reader);
            this.string_1 = reader.ReadString();
        }

        // Token: 0x060003B3 RID: 947 RVA: 0x0002004C File Offset: 0x0001E24C
        protected internal void ReadHeader(BinaryReader reader)
        {
            this.bool_0 = reader.ReadBoolean();
            this.dateTime_0 = new DateTime(reader.ReadInt64());
            this.long_0 = reader.ReadInt64();
            this.int_0 = reader.ReadInt32();
            this.int_1 = reader.ReadInt32();
            this.int_2 = reader.ReadInt32();
            this.byte_0 = reader.ReadByte();
            this.byte_1 = reader.ReadByte();
            this.byte_2 = reader.ReadByte();
        }

        // Token: 0x060003B7 RID: 951 RVA: 0x00020220 File Offset: 0x0001E420
        public override string ToString()
        {
            if (this.dataFile_0.streamerManager_0.idArray_0[(int)this.byte_2] != null)
            {
                return string.Concat(new object[]
                {
                    "ObjectKey ",
                    this.string_1,
                    " of typeId ",
                    this.byte_2,
                    " (",
                    this.dataFile_0.streamerManager_0.idArray_0[(int)this.byte_2].type,
                    ") position = ",
                    this.long_0
                });
            }
            return string.Concat(new object[]
            {
                "ObjectKey ",
                this.string_1,
                " of typeId ",
                this.byte_2,
                " (Unknown streamer, typeId = ",
                this.byte_2,
                ") position = ",
                this.long_0
            });
        }

        internal virtual void Write(BinaryWriter writer)
        {
            byte[] array = this.WriteObjectData(true);
            this.int_0 = 37 + this.string_1.Length + 1;
            this.int_1 = array.Length;
            if (this.int_2 == -1)
            {
                this.int_2 = this.int_0 + this.int_1;
            }
            this.WriteKey(writer);
            writer.Write(array, 0, array.Length);
        }

        protected internal void WriteHeader(BinaryWriter writer)
        {
            writer.Write(this.string_0);
            writer.Write(this.bool_0);
            writer.Write(this.dateTime_0.Ticks);
            writer.Write(this.long_0);
            writer.Write(this.int_0);
            writer.Write(this.int_1);
            writer.Write(this.int_2);
            writer.Write(this.byte_0);
            writer.Write(this.byte_1);
            writer.Write(this.byte_2);
        }

        internal virtual void WriteKey(BinaryWriter writer)
        {
            this.WriteHeader(writer);
            writer.Write(this.string_1);
        }

        internal virtual byte[] WriteObjectData(bool compress = true)
        {
            MemoryStream memoryStream = new MemoryStream();
            BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
            Type type = this.class20_0.GetType();
            ObjectStreamer objectStreamer = this.dataFile_0.streamerManager_0.dictionary_0[type];
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

        public byte CompressionLevel
        {
            get
            {
                return this.byte_1;
            }
        }

        public byte CompressionMethod
        {
            get
            {
                return this.byte_0;
            }
        }

        public DateTime DateTime
        {
            get
            {
                return this.dateTime_0;
            }
        }

        public string Name { get; }

        public byte TypeId 
        {
            get
            {
                return this.byte_2;
            }
        }

        // Token: 0x040001A5 RID: 421
        internal bool bool_0;

        // Token: 0x040001AB RID: 427
        internal byte byte_0;

        // Token: 0x040001AC RID: 428
        internal byte byte_1;

        // Token: 0x040001AD RID: 429
        internal byte byte_2;

        // Token: 0x040001AF RID: 431
        protected internal bool changed;

        // Token: 0x040001A3 RID: 419
        internal Class20 class20_0;

        // Token: 0x040001A2 RID: 418
        internal DataFileImpl dataFile_0;

        // Token: 0x040001A6 RID: 422
        internal DateTime dateTime_0;

        // Token: 0x040001A8 RID: 424
        internal int int_0;

        // Token: 0x040001A9 RID: 425
        internal int int_1;

        // Token: 0x040001AA RID: 426
        internal int int_2;

        // Token: 0x040001A7 RID: 423
        internal long long_0;

        // Token: 0x040001A4 RID: 420
        internal string string_0;

        // Token: 0x040001AE RID: 430
        internal string string_1;
    }
}
