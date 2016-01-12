using System;
using System.IO;

namespace SmartQuant
{
    public class ObjectStreamer
    {
        protected internal Type type;
        protected internal byte typeId;
        protected byte version;
        protected internal StreamerManager streamerManager;

        public Type Type => this.type;
        public byte TypeId => this.typeId;
        public StreamerManager StreamerManager => this.streamerManager;
        
        public ObjectStreamer()
        {
            this.type = typeof(object);
        }

        public virtual byte GetVersion(object obj) => this.version;

        public virtual object Read(BinaryReader reader, byte version) => new object();

        public virtual void Write(BinaryWriter writer, object obj)
        {
        }
    }
}