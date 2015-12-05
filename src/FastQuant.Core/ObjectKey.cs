using System;

namespace SmartQuant
{
    public class ObjectKey
    {
        public string Name { get; }

        public byte TypeId { get; }

        public DateTime DateTime { get; }

        public byte CompressionLevel { get; }

        public byte CompressionMethod { get; }

        public ObjectKey(DataFile file, string name = null, object obj = null)
        {
            Name = name;
        }

        public int CompareTo(ObjectKey other)
        {
            throw new NotImplementedException();
        }
    }
}