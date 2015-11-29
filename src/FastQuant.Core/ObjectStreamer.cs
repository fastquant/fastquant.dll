using System;
using System.IO;

namespace SmartQuant
{
    public class ObjectStreamer
    {
        public ObjectStreamer()
        {
            this.type = typeof(object);
        }

        public virtual byte GetVersion(object obj) => this.version;

        public virtual object Read(BinaryReader reader, byte version) => new object();

        public virtual void Write(BinaryWriter writer, object obj)
        {
        }

        public StreamerManager StreamerManager => this.streamerManager;

        protected internal StreamerManager streamerManager;

        protected internal Type type;
        protected internal byte typeId;
        protected byte version;
    }
}