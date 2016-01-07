using System;
using System.Collections.Generic;
using System.IO;

namespace SmartQuant
{
    class DataKey : ObjectKey
    {
        public DataKey(DataFile dataFile_1, object object_0 = null, long long_5 = -1L, long long_6 = -1L)
        {
            Class59.DyuZFiEzcdouj();
            this.size = 10000;
            this.int_5 = -1;
            this.prev = -1L;
            this.next = -1L;
            base..ctor(dataFile_1, "", object_0);
            this.label = "DK01";
            this.int_0 = 77;
            this.prev = long_5;
            this.next = long_6;
        }

        public void Add(DataObject dataObject_1)
        {
            if (this.count == this.size)
            {
                Console.WriteLine("DataKey::Add Can not add object. Buffer is full.");
                return;
            }
            if (this.dataObject_0 == null)
            {
                this.dataObject_0 = new DataObject[this.size];
            }
            if (this.count == 0)
            {
                this.dataObject_0[this.count++] = dataObject_1;
                this.dateTime_1 = dataObject_1.dateTime;
                this.dateTime_2 = dataObject_1.dateTime;
            }
            else if (dataObject_1.dateTime >= this.dataObject_0[this.count - 1].dateTime)
            {
                this.dataObject_0[this.count++] = dataObject_1;
                this.dateTime_2 = dataObject_1.dateTime;
            }
            else
            {
                int num = this.count;
                while (true)
                {
                    this.dataObject_0[num] = this.dataObject_0[num - 1];
                    if (dataObject_1.DateTime >= this.dataObject_0[num].DateTime)
                    {
                        break;
                    }
                    if (num == 1)
                    {
                        break;
                    }
                    num--;
                }
                this.dataObject_0[num - 1] = dataObject_1;
                if (num == 1)
                {
                    this.dateTime_1 = dataObject_1.dateTime;
                }
                this.count++;
            }
            if (this.count > 1)
            {
                this.index2 += 1L;
            }
            this.changed = true;
        }

        public void method_3(int int_6, DataObject dataObject_1)
        {
            this.dataObject_0[int_6] = dataObject_1;
            this.changed = true;
        }

        public void method_4(long long_5)
        {
            if (this.dataObject_0 == null)
            {
                this.method_5();
            }
            for (long num = long_5; num < (long)(this.count - 1); num += 1L)
            {
                checked
                {
                    this.dataObject_0[(int)((IntPtr)num)] = this.dataObject_0[(int)((IntPtr)(unchecked(num + 1L)))];
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
                this.dateTime_1 = this.dataObject_0[0].dateTime;
            }
            if (long_5 == (long)this.count)
            {
                this.dateTime_2 = this.dataObject_0[this.count - 1].dateTime;
            }
        }

        public DataObject[] method_5()
        {
            if (this.dataObject_0 != null)
            {
                return this.dataObject_0;
            }
            this.dataObject_0 = new DataObject[this.size];
            if (this.int_1 == -1)
            {
                return this.dataObject_0;
            }
            MemoryStream input = new MemoryStream(base.method_1(true));
            BinaryReader reader = new BinaryReader(input);
            for (int i = 0; i < this.count; i++)
            {
                this.dataObject_0[i] = (DataObject)this.dataFile_0.StreamerManager.Deserialize(reader);
            }
            return this.dataObject_0;
        }

        public DataObject method_6(int int_6)
        {
            return this.method_5()[int_6];
        }

        public DataObject method_7(DateTime dateTime_3)
        {
            if (this.dataObject_0 == null)
            {
                this.method_5();
            }
            for (int i = 0; i < this.count; i++)
            {
                if (this.dataObject_0[i].dateTime >= dateTime_3)
                {
                    return this.dataObject_0[i];
                }
            }
            return null;
        }

        public int GetIndex(DateTime dateTime_3, SearchOption option = SearchOption.Next)
        {
            if (this.dataObject_0 == null)
            {
                this.method_5();
            }
            for (int i = 0; i < this.count; i++)
            {
                if (this.dataObject_0[i].dateTime >= dateTime_3)
                {
                    switch (option)
                    {
                        case SearchOption.Next:
                            return i;
                        case SearchOption.Prev:
                            if (this.dataObject_0[i].dateTime == dateTime_3)
                            {
                                return i;
                            }
                            return i - 1;
                        case SearchOption.ExactFirst:
                            if (this.dataObject_0[i].dateTime != dateTime_3)
                            {
                                return -1;
                            }
                            return i;
                        default:
                            Console.WriteLine("DataKey::GetIndex Unknown search option: " + option);
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
                    Console.WriteLine("ObjectKey::ReadKey This is not DataKey! version = " + this.label);
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
            return string.Concat(new object[]
            {
            "DataKey position = ",
            this.position,
            " prev =  ",
            this.prev,
            " next = ",
            this.next,
            " number ",
            this.int_5,
            " size = ",
            this.size,
            " count = ",
            this.count,
            " ",
            this.changed,
            " index1 = ",
            this.index1,
            " index2 = ",
            this.index2
            });
        }

        internal override void Write(BinaryWriter writer)
        {
            MemoryStream output = new MemoryStream();
            new BinaryWriter(output);
            byte[] array = WriteObjectData(true);
            this.int_1 = array.Length;
            if (this.int_2 == -1)
            {
                this.int_2 = this.int_0 + this.int_1;
            }
            this.WriteKey(writer);
            writer.Write(array, 0, array.Length);
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
            MemoryStream memoryStream = new MemoryStream();
            BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
            var streamerManager_ = this.dataFile_0.StreamerManager;
            byte typeId = this.dataObject_0[0].TypeId;
            ObjectStreamer objectStreamer = streamerManager_.Get(typeId);
            for (int i = 0; i < this.count; i++)
            {
                if (this.dataObject_0[i].TypeId != typeId)
                {
                    typeId = this.dataObject_0[i].TypeId;
                    objectStreamer = streamerManager_.Get(typeId);
                }
                binaryWriter.Write(objectStreamer.typeId);
                binaryWriter.Write(objectStreamer.GetVersion(this.dataObject_0[i]));
                objectStreamer.Write(binaryWriter, this.dataObject_0[i]);
            }
            byte[] array = memoryStream.ToArray();
            if (compress && this.byte_1 != 0)
            {
                return new QuickLZ().Compress(array);
            }
            return array;
        }

        internal DataObject[] dataObject_0;

        internal DateTime dateTime_1;

        internal DateTime dateTime_2;

        internal int size;

        internal int count;

        internal int int_5;

        internal long index1;

        internal long index2;

        internal long prev;

        internal long next;
    }


    internal class Class24
    {
        internal Class24(List<FreeKey> sNSyTAzOtSrOnZ0qtS)
        {
            this.list_0 = sNSyTAzOtSrOnZ0qtS;
        }

        internal List<FreeKey> list_0;
    }


    class Class26
    {
        internal Class26(Dictionary<string, ObjectKey> zDOX6N4lLLHBLYT6hL8)
        {
            this.dictionary_0 = zDOX6N4lLLHBLYT6hL8;
        }
        internal Dictionary<string, ObjectKey> dictionary_0;
    }

    internal class FreeKey : IComparable<FreeKey>
    {
        public FreeKey()
        {
        }

        public FreeKey(ObjectKey objectKey_0):this(objectKey_0.dataFile_0, objectKey_0.position, objectKey_0.int_2)
        {
        }

        public FreeKey(DataFile dataFile_1, long long_1 = -1, int int_1 = -1)
        {
            this.dataFile_0 = dataFile_1;
            this.long_0 = long_1;
            this.int_0 = int_1;
        }

        public int CompareTo(FreeKey other)
        {
            if (other == null)
            {
                return 1;
            }
            return this.int_0.CompareTo(other.int_0);
        }

        internal void method_0(BinaryWriter binaryWriter_0)
        {
            binaryWriter_0.Write(this.string_0);
            binaryWriter_0.Write(this.long_0);
            binaryWriter_0.Write(this.int_0);
        }

        internal void ReadKey(BinaryReader binaryReader_0, bool bool_0 = true)
        {
            if (bool_0)
            {
                this.string_0 = binaryReader_0.ReadString();
                if (!this.string_0.StartsWith("FK"))
                {
                    Console.WriteLine("FreeKey::ReadKey This is not FreeKey! version = " + this.string_0);
                }
            }
            this.long_0 = binaryReader_0.ReadInt64();
            this.int_0 = binaryReader_0.ReadInt32();
        }

        internal DataFile dataFile_0;

        internal int int_0 = -1;

        internal long long_0 = -1;

        internal string string_0 = "FK01";
    }

    public class DataFile
    {
        public StreamerManager StreamerManager { get; }

        public DataFile(string name, StreamerManager streamerManager)
        {
            Class59.DyuZFiEzcdouj();
            this.label = "SmartQuant";
            this.byte_0 = 1;
            this.byte_1 = 1;
            this.byte_2 = 1;
            this.dictionary_0 = new Dictionary<string, ObjectKey>();
            this.list_0 = new List<FreeKey>();
            base..ctor();
            this.name = name;
            StreamerManager = streamerManager;
            this.memoryStream_0 = new MemoryStream();
            this.binaryWriter_0 = new BinaryWriter(this.memoryStream_0);
        }

        public virtual void Close()
        {
            if (!this.bool_0)
            {
                Console.WriteLine("DataFile::Close File is not open: " + this.name);
                return;
            }
            Flush();
            this.CloseFileStream();
            this.bool_0 = false;
        }

        protected virtual void CloseFileStream()
        {
            this.stream_0.Close();
        }

        public virtual void Delete(string name)
        {
            ObjectKey objectKey;
            this.dictionary_0.TryGetValue(name, out objectKey);
            if (objectKey != null)
            {
                this.method_5(objectKey, true, true);
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
            if (this.bool_0)
            {
                Console.WriteLine(string.Concat(new object[]
                {
                    "DataFile::Dump DataFile ",
                    this.name,
                    " is open in ",
                    this.fileMode_0,
                    " mode and contains ",
                    this.dictionary_0.Values.Count,
                    " objects:"
                }));
                foreach (ObjectKey current in this.dictionary_0.Values)
                {
                    current.Dump();
                }
                Console.WriteLine("Free objects = " + this.int_3);
                return;
            }
            Console.WriteLine("DataFile::Dump DataFile " + this.name + " is closed");
        }

        ~DataFile()
        {
            Dispose(false);
        }

        public virtual void Flush()
        {
            if (!this.bool_0)
            {
                Console.WriteLine("DataFile::Flush Can not flush file which is not open " + this.name);
                return;
            }
            if (this.bool_1)
            {
                foreach (ObjectKey current in this.dictionary_0.Values)
                {
                    if (current.typeId == 101 && current.class20_0 != null)
                    {
                        DataSeries dataSeries = (DataSeries)current.class20_0;
                        if (dataSeries.bool_3)
                        {
                            dataSeries.method_18();
                        }
                    }
                }
                this.method_8();
                this.method_9();
                this.method_1();
                this.stream_0.Flush();
            }
            this.bool_1 = false;
        }

        public virtual object Get(string name)
        {
            ObjectKey objectKey;
            this.dictionary_0.TryGetValue(name, out objectKey);
            if (objectKey != null)
            {
                return objectKey.GetObject();
            }
            return null;
        }

        internal bool ReadHeader()
        {
            byte[] buffer = new byte[62];
            this.ReadBuffer(buffer, 0L, 62);
            MemoryStream input = new MemoryStream(buffer);
            BinaryReader binaryReader = new BinaryReader(input);
            this.label = binaryReader.ReadString();
            if (this.label != "SmartQuant")
            {
                Console.WriteLine("DataFile::ReadHeader This is not SmartQuant data file!");
                return false;
            }
            this.byte_0 = binaryReader.ReadByte();
            this.long_0 = binaryReader.ReadInt64();
            this.long_1 = binaryReader.ReadInt64();
            this.long_2 = binaryReader.ReadInt64();
            this.long_3 = binaryReader.ReadInt64();
            this.int_0 = binaryReader.ReadInt32();
            this.int_1 = binaryReader.ReadInt32();
            this.int_2 = binaryReader.ReadInt32();
            this.int_3 = binaryReader.ReadInt32();
            this.byte_1 = binaryReader.ReadByte();
            this.byte_2 = binaryReader.ReadByte();
            return true;
        }

        internal void method_1()
        {
            MemoryStream memoryStream = new MemoryStream();
            BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
            binaryWriter.Write(this.label);
            binaryWriter.Write(this.byte_0);
            binaryWriter.Write(this.long_0);
            binaryWriter.Write(this.long_1);
            binaryWriter.Write(this.long_2);
            binaryWriter.Write(this.long_3);
            binaryWriter.Write(this.int_0);
            binaryWriter.Write(this.int_1);
            binaryWriter.Write(this.int_2);
            binaryWriter.Write(this.int_3);
            binaryWriter.Write(this.byte_1);
            binaryWriter.Write(this.byte_2);
            this.WriteBuffer(memoryStream.GetBuffer(), 0L, (int)memoryStream.Length);
        }

        private void Dispose(bool disposing)
        {
            if (!this.bool_2)
            {
                if (disposing)
                {
                    Close();
                }
                this.bool_2 = true;
            }
        }

        // Token: 0x06000376 RID: 886 RVA: 0x00004AA9 File Offset: 0x00002CA9
        internal void method_2()
        {
            this.bool_1 = true;
            this.long_2 = -1L;
            this.long_3 = -1L;
            this.method_1();
        }

        // Token: 0x06000377 RID: 887 RVA: 0x0001E690 File Offset: 0x0001C890
        internal ObjectKey ReadKey(long long_4)
        {
            byte[] buffer = new byte[37];
            this.ReadBuffer(buffer, long_4, 37);
            MemoryStream input = new MemoryStream(buffer);
            BinaryReader binaryReader = new BinaryReader(input);
            string text = binaryReader.ReadString();
            ObjectKey objectKey = new ObjectKey(this, null, null);
            objectKey.label = text;
            objectKey.ReadHeader(binaryReader);
            int num = objectKey.int_0;
            buffer = new byte[num];
            this.ReadBuffer(buffer, long_4, num);
            input = new MemoryStream(buffer);
            binaryReader = new BinaryReader(input);
            if (text.StartsWith("OK"))
            {
                objectKey = new ObjectKey(this, null, null);
            }
            else
            {
                if (!text.StartsWith("DK"))
                {
                    Console.WriteLine("DataFile::ReadKey This is not object or data key : " + text);
                    return null;
                }
                objectKey = new DataKey(this, null, -1L, -1L);
            }
            objectKey.Read(binaryReader, true);
            return objectKey;
        }

        // Token: 0x0600037B RID: 891 RVA: 0x0001E838 File Offset: 0x0001CA38
        private FreeKey method_4(int int_4)
        {
            foreach (FreeKey current in this.list_0)
            {
                if (current.int_0 >= int_4)
                {
                    return current;
                }
            }
            return null;
        }

        // Token: 0x0600037C RID: 892 RVA: 0x0001E898 File Offset: 0x0001CA98
        internal void method_5(ObjectKey objectKey_2, bool bool_3 = true, bool bool_4 = true)
        {
            objectKey_2.bool_0 = true;
            this.WriteBuffer(new byte[]
            {
                1
            }, objectKey_2.position + 5L, 1);
            if (bool_3)
            {
                this.dictionary_0.Remove(objectKey_2.name);
                this.int_2--;
            }
            if (bool_4)
            {
                this.list_0.Add(new FreeKey(objectKey_2));
                this.list_0.Sort();
                this.int_3++;
            }
            this.bool_1 = true;
        }

        // Token: 0x0600037D RID: 893 RVA: 0x0001E928 File Offset: 0x0001CB28
        internal ObjectKey method_6(long long_4, int int_4)
        {
            byte[] buffer = new byte[int_4];
            this.ReadBuffer(buffer, long_4, int_4);
            MemoryStream input = new MemoryStream(buffer);
            BinaryReader reader = new BinaryReader(input);
            ObjectKey objectKey = new ObjectKey(this, null, null);
            objectKey.Read(reader, true);
            objectKey.position = long_4;
            return objectKey;
        }

        // Token: 0x0600037E RID: 894 RVA: 0x0001E96C File Offset: 0x0001CB6C
        internal void method_7(ObjectKey objectKey_2)
        {
            this.memoryStream_0.SetLength(0L);
            objectKey_2.Write(this.binaryWriter_0);
            if (objectKey_2.position != -1L)
            {
                if (this.memoryStream_0.Length > (long)objectKey_2.int_2)
                {
                    this.method_5(objectKey_2, false, true);
                    objectKey_2.int_2 = (int)this.memoryStream_0.Length;
                    FreeKey @class;
                    if (objectKey_2 == this.objectKey_1)
                    {
                        @class = this.method_4(objectKey_2.int_0 + objectKey_2.int_1 - 17);
                    }
                    else
                    {
                        @class = this.method_4(objectKey_2.int_0 + objectKey_2.int_1);
                    }
                    if (@class != null)
                    {
                        objectKey_2.position = @class.long_0;
                        objectKey_2.int_2 = @class.int_0;
                        this.list_0.Remove(@class);
                        this.int_3--;
                        if (objectKey_2 == this.objectKey_1)
                        {
                            this.memoryStream_0.SetLength(0L);
                            objectKey_2.Write(this.binaryWriter_0);
                        }
                    }
                    else
                    {
                        objectKey_2.position = this.stream_0.Length;
                        this.long_1 = objectKey_2.position;
                    }
                }
            }
            else
            {
                objectKey_2.position = this.stream_0.Length;
                this.long_1 = objectKey_2.position;
            }
            this.WriteBuffer(this.memoryStream_0.GetBuffer(), objectKey_2.position, (int)this.memoryStream_0.Length);
            objectKey_2.changed = false;
            this.bool_1 = true;
        }

        private void method_8()
        {
            if (this.objectKey_0 != null)
            {
                this.method_5(this.objectKey_0, false, true);
            }
            this.objectKey_0 = new ObjectKey(this, "ObjectKeys", new Class26(this.dictionary_0));
            this.objectKey_0.byte_1 = 0;
            this.method_7(this.objectKey_0);
            this.long_2 = this.objectKey_0.position;
            this.int_0 = this.objectKey_0.int_0 + this.objectKey_0.int_1;
        }

        // Token: 0x06000382 RID: 898 RVA: 0x0001ECBC File Offset: 0x0001CEBC
        private void method_9()
        {
            if (this.objectKey_1 != null)
            {
                this.method_5(this.objectKey_1, false, true);
            }
            this.objectKey_1 = new ObjectKey(this, "FreeKeys", new Class24(this.list_0));
            this.objectKey_1.byte_1 = 0;
            this.method_7(this.objectKey_1);
            this.long_3 = this.objectKey_1.position;
            this.int_1 = this.objectKey_1.int_0 + this.objectKey_1.int_1;
        }

        public virtual void Open(FileMode mode = FileMode.OpenOrCreate)
        {
            if (mode != FileMode.OpenOrCreate && mode != FileMode.Create)
            {
                Console.WriteLine("DataFile::Open Can not open file in " + mode + " mode. DataFile suppports FileMode.OpenOrCreate and FileMode.Create modes.");
                return;
            }
            if (this.bool_0)
            {
                Console.WriteLine("DataFile::Open File is already open: " + this.name);
                return;
            }
            this.fileMode_0 = mode;
            if (!this.OpenFileStream(this.name, mode))
            {
                this.long_0 = 62L;
                this.long_1 = 62L;
                this.method_2();
            }
            else
            {
                if (!this.ReadHeader())
                {
                    Console.WriteLine("DataFile::Open Error opening file " + this.name);
                    return;
                }
                if (this.long_2 == -1L || this.long_3 == -1L)
                {
                    Console.WriteLine("DataFile::Open The file was not properly closed and needs to be recovered!");
                    this.Recover();
                }
                this.ReadKeys();
                this.ReadFree();
            }
            this.bool_0 = true;
        }

        protected virtual bool OpenFileStream(string name, FileMode mode)
        {
            this.stream_0 = new FileStream(name, mode);
            return this.stream_0.Length != 0L;
        }

        protected internal virtual void ReadBuffer(byte[] buffer, long position, int length)
        {
            lock (this)
            {
                this.stream_0.Seek(position, SeekOrigin.Begin);
                this.stream_0.Read(buffer, 0, length);
            }
        }

        protected void ReadFree()
        {
            if (this.int_1 == 0)
            {
                return;
            }
            byte[] buffer = new byte[this.int_1];
            this.ReadBuffer(buffer, this.long_3, this.int_1);
            MemoryStream input = new MemoryStream(buffer);
            BinaryReader reader = new BinaryReader(input);
            ObjectKey objectKey = new ObjectKey(this, null, null);
            objectKey.Read(reader, true);
            objectKey.position = this.long_3;
            this.list_0 = ((Class24)objectKey.GetObject()).list_0;
            this.objectKey_1 = objectKey;
        }

        protected void ReadKeys()
        {
            if (this.int_0 == 0)
            {
                return;
            }
            byte[] buffer = new byte[this.int_0];
            this.ReadBuffer(buffer, this.long_2, this.int_0);
            MemoryStream input = new MemoryStream(buffer);
            BinaryReader reader = new BinaryReader(input);
            ObjectKey objectKey = new ObjectKey(this, null, null);
            objectKey.Read(reader, true);
            objectKey.position = this.long_2;
            this.dictionary_0 = ((Class26)objectKey.GetObject()).dictionary_0;
            foreach (ObjectKey current in this.dictionary_0.Values)
            {
                current.Init(this);
            }
            this.objectKey_0 = objectKey;
        }

        public virtual void Recover()
        {
            this.dictionary_0.Clear();
            this.list_0.Clear();
            long num = this.long_0;
            new BinaryReader(this.stream_0);
            ObjectKey objectKey = null;
            while (true)
            {
                try
                {
                    objectKey = this.ReadKey(num);
                    goto IL_F6;
                }
                catch (Exception arg)
                {
                    Console.WriteLine("DataFile::Recover exception " + arg);
                    break;
                }
                goto IL_4E;
                IL_6A:
                if (objectKey.bool_0)
                {
                    this.list_0.Add(new FreeKey(objectKey));
                }
                else if (!(objectKey is DataKey) && objectKey.TypeId != 105)
                {
                    if (objectKey.typeId != 102)
                    {
                        if (objectKey.typeId != 103)
                        {
                            this.dictionary_0.Add(objectKey.name, objectKey);
                            goto IL_CA;
                        }
                    }
                    this.method_5(objectKey, false, true);
                }
                IL_CA:
                Console.WriteLine(objectKey);
                num += (long)objectKey.int_2;
                if (objectKey.int_2 <= 0)
                {
                    break;
                }
                if (num >= this.stream_0.Length)
                {
                    break;
                }
                continue;
                IL_4E:
                Console.WriteLine("DataFile::Recover Key position is -1 , setting to " + num);
                objectKey.position = num;
                goto IL_6A;
                IL_F6:
                if (objectKey.position == -1L)
                {
                    goto IL_4E;
                }
                goto IL_6A;
            }
            this.bool_1 = true;
            this.Flush();
        }

        public virtual void Refresh()
        {
        }

        public virtual void Write(string name, object obj)
        {
            ObjectKey objectKey;
            this.dictionary_0.TryGetValue(name, out objectKey);
            if (objectKey != null)
            {
                objectKey.class20_0 = (Class20)obj;
                objectKey.Init(this);
            }
            else
            {
                objectKey = new ObjectKey(this, name, obj);
                this.dictionary_0.Add(name, objectKey);
                this.int_2++;
            }
            objectKey.dateTime_0 = DateTime.Now;
            if (objectKey.typeId == 101)
            {
                ((DataSeries)obj).method_0(this, objectKey);
            }
            this.method_7(objectKey);
        }

        protected internal virtual void WriteBuffer(byte[] buffer, long position, int length)
        {
            lock (this)
            {
                this.stream_0.Seek(position, SeekOrigin.Begin);
                this.stream_0.Write(buffer, 0, length);
                if (!this.bool_1)
                {
                    this.method_2();
                }
            }
        }

        public byte CompressionLevel { get; set; } = 1;

        public byte CompressionMethod { get; set; } = 1;

        public Dictionary<string, ObjectKey> Keys { get; } = new Dictionary<string, ObjectKey>();

        private BinaryWriter binaryWriter_0;

        internal bool bool_0;

        internal bool bool_1;

        private bool bool_2;

        internal byte byte_0;

        internal byte byte_1;

        internal byte byte_2;

        internal Dictionary<string, ObjectKey> dictionary_0;

        internal FileMode fileMode_0;

        internal int int_0;

        internal int int_1;

        internal int int_2;

        internal int int_3;

        internal List<FreeKey> list_0;

        internal long long_0;

        internal long long_1;

        internal long long_2;

        internal long long_3;

        private MemoryStream memoryStream_0;

        internal ObjectKey objectKey_0;

        internal ObjectKey objectKey_1;

        internal Stream stream_0;

        internal string label;

        internal string name;
    }
}