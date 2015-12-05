using System;
using System.Collections.Generic;
using System.IO;

namespace SmartQuant
{
    public class DataFile
    {
        public byte CompressionLevel { get; set; }

        public byte CompressionMethod { get; set; }

        public Dictionary<string, ObjectKey> Keys { get; } = new Dictionary<string, ObjectKey>();

        public DataFile(string name, StreamerManager streamerManager)
        {
        }

        ~DataFile()
        {
            Dispose(false);
        }

        public virtual void Open(FileMode mode = FileMode.OpenOrCreate)
        {
        }

        public virtual void Close()
        {
        }

        protected virtual void CloseFileStream()
        {
        }

        public virtual object Get(string name)
        {
            ObjectKey objectKey;
            return null;
        }

        public virtual void Write(string name, object obj)
        {
        }

        public virtual void Delete(string name)
        {
        }

        public virtual void Flush()
        {
        }

        public virtual void Recover()
        {
        }

        public virtual void Refresh()
        {
            // noop
        }

        public void Dispose()
        {
            Console.WriteLine("DataFile::Dispose");
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual bool OpenFileStream(string name, FileMode mode)
        {
            throw new NotImplementedException();
        }

        private void Dispose(bool disposing)
        {
        }

        protected internal virtual void ReadBuffer(byte[] buffer, long position, int length)
        {
        }

        protected internal virtual void WriteBuffer(byte[] buffer, long position, int length)
        {
        }

        protected void ReadFree()
        {
        }

        protected void ReadKeys()
        {
        }
    }
}