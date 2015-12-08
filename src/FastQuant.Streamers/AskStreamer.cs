// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;

namespace SmartQuant
{
    public class AskStreamer : ObjectStreamer
    {
        public AskStreamer()
        {
            this.typeId = DataObjectType.Ask;
            this.type = typeof(Ask);
        }

        public override byte GetVersion(object obj)
        {
            return (obj as Tick).ExchangeDateTime.Ticks != 0 ? (byte)1 : (byte)0;
        }

        public override object Read(BinaryReader reader, byte version)
        {
            return version == 0
                ? new Ask(new DateTime(reader.ReadInt64()), reader.ReadByte(), reader.ReadInt32(), reader.ReadDouble(), reader.ReadInt32())
                : new Ask(new DateTime(reader.ReadInt64()), new DateTime(reader.ReadInt64()), reader.ReadByte(), reader.ReadInt32(), reader.ReadDouble(), reader.ReadInt32());

        }

        public override void Write(BinaryWriter writer, object obj)
        {
            var ask = obj as Ask;
            writer.Write(ask.DateTime.Ticks);
            if (ask.ExchangeDateTime.Ticks != 0)
                writer.Write(ask.ExchangeDateTime.Ticks);
            writer.Write(ask.ProviderId);
            writer.Write(ask.InstrumentId);
            writer.Write(ask.Price);
            writer.Write(ask.Size);
        }
    }
}
