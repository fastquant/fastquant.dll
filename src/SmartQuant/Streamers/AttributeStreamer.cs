using System;
using System.IO;

namespace SmartQuant
{
    public class AttributeStreamer : ObjectStreamer
    {
        public AttributeStreamer()
        {
            this.typeId = DataObjectType.Attribute;
            this.type = typeof(Attribute);
        }

        public override object Read(BinaryReader reader, byte version)
        {
            throw new NotImplementedException();
        }

        public override void Write(BinaryWriter writer, object obj)
        {
            throw new NotImplementedException();
        }
    }
}
