using System;

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

    public class ObjectKey
    {
        private long position = -1;
        protected internal bool changed;

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

        public override string ToString()
        {
            return "";
        }
    }
}