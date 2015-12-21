using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using SmartQuant;

namespace FastQuant.Private.DataFile
{
    public class DataFileImpl
    {
        public DataFileImpl(string name, StreamerManager streamerManager)
        {
            Class59.DyuZFiEzcdouj();
            this.string_0 = "SmartQuant";
            this.byte_0 = 1;
            this.byte_1 = 1;
            this.byte_2 = 1;
            this.dictionary_0 = new Dictionary<string, ObjectKey>();
            this.list_0 = new List<Class23>();
            base..ctor();
            this.string_1 = name;
            this.streamerManager_0 = streamerManager;
            this.memoryStream_0 = new MemoryStream();
            this.binaryWriter_0 = new BinaryWriter(this.memoryStream_0);
        }

        public virtual void Close()
        {
            if (!this.bool_0)
            {
                Console.WriteLine("DataFileImpl::Close File is not open: " + this.string_1);
                return;
            }
            this.Flush();
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
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Dump()
        {
            if (this.bool_0)
            {
                Console.WriteLine(string.Concat(new object[]
                {
                    "DataFile::Dump DataFile ",
                    this.string_1,
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
            Console.WriteLine("DataFile::Dump DataFile " + this.string_1 + " is closed");
        }

        ~DataFileImpl()
        {
            Dispose(false);
        }

        public virtual void Flush()
        {
            if (!this.bool_0)
            {
                Console.WriteLine("DataFile::Flush Can not flush file which is not open " + this.string_1);
                return;
            }
            if (this.bool_1)
            {
                foreach (ObjectKey current in this.dictionary_0.Values)
                {
                    if (current.byte_2 == 101 && current.class20_0 != null)
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

        // Token: 0x06000379 RID: 889 RVA: 0x0001E7E8 File Offset: 0x0001C9E8
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

        internal bool method_0()
        {
            byte[] buffer = new byte[62];
            this.ReadBuffer(buffer, 0, 62);
            MemoryStream input = new MemoryStream(buffer);
            BinaryReader binaryReader = new BinaryReader(input);
            this.string_0 = binaryReader.ReadString();
            if (this.string_0 != "SmartQuant")
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
            binaryWriter.Write(this.string_0);
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

        private void Dispose(bool bool_3)
        {
            if (!this.bool_2)
            {
                if (bool_3)
                {
                    this.Close();
                }
                this.bool_2 = true;
            }
        }

        internal void method_2()
        {
            this.bool_1 = true;
            this.long_2 = -1L;
            this.long_3 = -1L;
            this.method_1();
        }

        // Token: 0x06000377 RID: 887 RVA: 0x0001E690 File Offset: 0x0001C890
        internal ObjectKey method_3(long long_4)
        {
            byte[] buffer = new byte[37];
            this.ReadBuffer(buffer, long_4, 37);
            MemoryStream input = new MemoryStream(buffer);
            BinaryReader binaryReader = new BinaryReader(input);
            string text = binaryReader.ReadString();
            ObjectKey objectKey = new ObjectKey(this, null, null);
            objectKey.string_0 = text;
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
                objectKey = new Class19(this, null, -1L, -1L);
            }
            objectKey.Read(binaryReader, true);
            return objectKey;
        }

        private Class23 method_4(int int_4)
        {
            foreach (Class23 current in this.list_0)
            {
                if (current.int_0 >= int_4)
                {
                    return current;
                }
            }
            return null;
        }

        internal void method_5(ObjectKey objectKey_2, bool bool_3 = true, bool bool_4 = true)
        {
            objectKey_2.bool_0 = true;
            this.WriteBuffer(new byte[]
            {
                1
            }, objectKey_2.long_0 + 5L, 1);
            if (bool_3)
            {
                this.dictionary_0.Remove(objectKey_2.string_1);
                this.int_2--;
            }
            if (bool_4)
            {
                this.list_0.Add(new Class23(objectKey_2));
                this.list_0.Sort();
                this.int_3++;
            }
            this.bool_1 = true;
        }

        internal ObjectKey method_6(long long_4, int int_4)
        {
            byte[] buffer = new byte[int_4];
            this.ReadBuffer(buffer, long_4, int_4);
            MemoryStream input = new MemoryStream(buffer);
            BinaryReader reader = new BinaryReader(input);
            ObjectKey objectKey = new ObjectKey(this, null, null);
            objectKey.Read(reader, true);
            objectKey.long_0 = long_4;
            return objectKey;
        }

        internal void method_7(ObjectKey objectKey_2)
        {
            this.memoryStream_0.SetLength(0L);
            objectKey_2.Write(this.binaryWriter_0);
            if (objectKey_2.long_0 != -1L)
            {
                if (this.memoryStream_0.Length > (long)objectKey_2.int_2)
                {
                    this.method_5(objectKey_2, false, true);
                    objectKey_2.int_2 = (int)this.memoryStream_0.Length;
                    Class23 @class;
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
                        objectKey_2.long_0 = @class.long_0;
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
                        objectKey_2.long_0 = this.stream_0.Length;
                        this.long_1 = objectKey_2.long_0;
                    }
                }
            }
            else
            {
                objectKey_2.long_0 = this.stream_0.Length;
                this.long_1 = objectKey_2.long_0;
            }
            this.WriteBuffer(this.memoryStream_0.GetBuffer(), objectKey_2.long_0, (int)this.memoryStream_0.Length);
            objectKey_2.changed = false;
            this.bool_1 = true;
        }

        // Token: 0x06000381 RID: 897 RVA: 0x0001EC34 File Offset: 0x0001CE34
        private void method_8()
        {
            if (this.objectKey_0 != null)
            {
                this.method_5(this.objectKey_0, false, true);
            }
            this.objectKey_0 = new ObjectKey(this, "ObjectKeys", new Class26(this.dictionary_0));
            this.objectKey_0.byte_1 = 0;
            this.method_7(this.objectKey_0);
            this.long_2 = this.objectKey_0.long_0;
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
            this.long_3 = this.objectKey_1.long_0;
            this.int_1 = this.objectKey_1.int_0 + this.objectKey_1.int_1;
        }

        // Token: 0x06000371 RID: 881 RVA: 0x0001E340 File Offset: 0x0001C540
        public virtual void Open(FileMode mode = FileMode.OpenOrCreate)
        {
            if (mode != FileMode.OpenOrCreate && mode != FileMode.Create)
            {
                Console.WriteLine("DataFile::Open Can not open file in " + mode + " mode. DataFile suppports FileMode.OpenOrCreate and FileMode.Create modes.");
                return;
            }
            if (this.bool_0)
            {
                Console.WriteLine("DataFile::Open File is already open: " + this.string_1);
                return;
            }
            this.fileMode_0 = mode;
            if (!this.OpenFileStream(this.string_1, mode))
            {
                this.long_0 = 62L;
                this.long_1 = 62L;
                this.method_2();
            }
            else
            {
                if (!this.method_0())
                {
                    Console.WriteLine("DataFile::Open Error opening file " + this.string_1);
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

        // Token: 0x0600036F RID: 879 RVA: 0x00004A74 File Offset: 0x00002C74
        protected virtual bool OpenFileStream(string name, FileMode mode)
        {
            this.stream_0 = new FileStream(name, mode);
            return this.stream_0.Length != 0L;
        }

        // Token: 0x06000374 RID: 884 RVA: 0x0001E5DC File Offset: 0x0001C7DC
        protected internal virtual void ReadBuffer(byte[] buffer, long position, int length)
        {
            lock (this)
            {
                this.stream_0.Seek(position, SeekOrigin.Begin);
                this.stream_0.Read(buffer, 0, length);
            }
        }

        // Token: 0x06000380 RID: 896 RVA: 0x0001EBB4 File Offset: 0x0001CDB4
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
            objectKey.long_0 = this.long_3;
            this.list_0 = ((Class24)objectKey.GetObject()).list_0;
            this.objectKey_1 = objectKey;
        }

        // Token: 0x0600037F RID: 895 RVA: 0x0001EAE8 File Offset: 0x0001CCE8
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
            objectKey.long_0 = this.long_2;
            this.dictionary_0 = ((Class26)objectKey.GetObject()).dictionary_0;
            foreach (ObjectKey current in this.dictionary_0.Values)
            {
                current.method_0(this);
            }
            this.objectKey_0 = objectKey;
        }

        // Token: 0x06000384 RID: 900 RVA: 0x0001EE40 File Offset: 0x0001D040
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
                    objectKey = this.method_3(num);
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
                    this.list_0.Add(new Class23(objectKey));
                }
                else if (!(objectKey is Class19) && objectKey.TypeId != 105)
                {
                    if (objectKey.byte_2 != 102)
                    {
                        if (objectKey.byte_2 != 103)
                        {
                            this.dictionary_0.Add(objectKey.string_1, objectKey);
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
                objectKey.long_0 = num;
                goto IL_6A;
                IL_F6:
                if (objectKey.long_0 == -1L)
                {
                    goto IL_4E;
                }
                goto IL_6A;
            }
            this.bool_1 = true;
            this.Flush();
        }

        // Token: 0x06000385 RID: 901 RVA: 0x000024A4 File Offset: 0x000006A4
        public virtual void Refresh()
        {
        }

        // Token: 0x06000378 RID: 888 RVA: 0x0001E76C File Offset: 0x0001C96C
        public virtual void Write(string name, object obj)
        {
            ObjectKey objectKey;
            this.dictionary_0.TryGetValue(name, out objectKey);
            if (objectKey != null)
            {
                objectKey.class20_0 = obj;
                objectKey.method_0(this);
            }
            else
            {
                objectKey = new ObjectKey(this, name, obj);
                this.dictionary_0.Add(name, objectKey);
                this.int_2++;
            }
            objectKey.dateTime_0 = DateTime.Now;
            if (objectKey.byte_2 == 101)
            {
                ((DataSeries)obj).method_0(this, objectKey);
            }
            this.method_7(objectKey);
        }

        // Token: 0x06000375 RID: 885 RVA: 0x0001E630 File Offset: 0x0001C830
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

        // Token: 0x170000CB RID: 203
        public byte CompressionLevel
        {
            // Token: 0x0600036C RID: 876 RVA: 0x00004A63 File Offset: 0x00002C63
            get
            {
                return this.byte_2;
            }
            // Token: 0x0600036D RID: 877 RVA: 0x00004A6B File Offset: 0x00002C6B
            set
            {
                this.byte_2 = value;
            }
        }

        // Token: 0x170000CA RID: 202
        public byte CompressionMethod
        {
            // Token: 0x0600036A RID: 874 RVA: 0x00004A52 File Offset: 0x00002C52
            get
            {
                return this.byte_1;
            }
            // Token: 0x0600036B RID: 875 RVA: 0x00004A5A File Offset: 0x00002C5A
            set
            {
                this.byte_1 = value;
            }
        }

        // Token: 0x170000C9 RID: 201
        public Dictionary<string, ObjectKey> Keys
        {
            // Token: 0x06000369 RID: 873 RVA: 0x00004A4A File Offset: 0x00002C4A
            get
            {
                return this.dictionary_0;
            }
        }

        // Token: 0x0400018C RID: 396
        private BinaryWriter binaryWriter_0;

        // Token: 0x04000183 RID: 387
        internal bool bool_0;

        // Token: 0x04000184 RID: 388
        internal bool bool_1;

        // Token: 0x0400018A RID: 394
        private bool bool_2;

        // Token: 0x04000175 RID: 373
        internal byte byte_0;

        // Token: 0x0400017E RID: 382
        internal byte byte_1;

        // Token: 0x0400017F RID: 383
        internal byte byte_2;

        // Token: 0x04000185 RID: 389
        internal Dictionary<string, ObjectKey> dictionary_0;

        // Token: 0x04000182 RID: 386
        internal FileMode fileMode_0;

        // Token: 0x0400017A RID: 378
        internal int int_0;

        // Token: 0x0400017B RID: 379
        internal int int_1;

        // Token: 0x0400017C RID: 380
        internal int int_2;

        // Token: 0x0400017D RID: 381
        internal int int_3;

        // Token: 0x04000186 RID: 390
        internal List<Class23> list_0;

        // Token: 0x04000176 RID: 374
        internal long long_0;

        // Token: 0x04000177 RID: 375
        internal long long_1;

        // Token: 0x04000178 RID: 376
        internal long long_2;

        // Token: 0x04000179 RID: 377
        internal long long_3;

        // Token: 0x0400018B RID: 395
        private MemoryStream memoryStream_0;

        // Token: 0x04000187 RID: 391
        internal ObjectKey objectKey_0;

        // Token: 0x04000188 RID: 392
        internal ObjectKey objectKey_1;

        // Token: 0x04000189 RID: 393
        internal StreamerManager streamerManager_0;

        // Token: 0x04000181 RID: 385
        internal Stream stream_0;

        // Token: 0x04000174 RID: 372
        internal string string_0;

        // Token: 0x04000180 RID: 384
        internal string string_1;
    }
}
