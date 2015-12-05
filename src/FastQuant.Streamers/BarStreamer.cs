// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;

namespace SmartQuant
{
    public class BarStreamer : ObjectStreamer
    {
        public BarStreamer()
        {
            this.typeId = DataObjectType.Bar;
            this.type = typeof(Bar);
            this.version = 1;
        }

        public override object Read(BinaryReader reader, byte version)
        {
            var bar = new Bar();
            bar.DateTime = new DateTime(reader.ReadInt64());
            bar.OpenDateTime = new DateTime(reader.ReadInt64());
            bar.InstrumentId = reader.ReadInt32();
            bar.Size = reader.ReadInt64();
            bar.High = reader.ReadDouble();
            bar.Low = reader.ReadDouble();
            bar.Open = reader.ReadDouble();
            bar.Close = reader.ReadDouble();
            bar.Volume = reader.ReadInt64();
            bar.OpenInt = reader.ReadInt64();
            bar.Status = (BarStatus)reader.ReadByte();
            if (version >= 1)
                bar.Type = (BarType)reader.ReadByte();
            int size = reader.ReadInt32();
            if (size != 0)
            {
                bar.fields = new IdArray<double>(size);
                for (int i = 0; i < size; ++i)
                    bar.fields[i] = reader.ReadDouble();
            }
            return bar;
        }

        public override void Write(BinaryWriter writer, object obj)
        {
            var bar = (Bar)obj;
            writer.Write(bar.DateTime.Ticks);
            writer.Write(bar.OpenDateTime.Ticks);
            writer.Write(bar.InstrumentId);
            writer.Write(bar.Size);
            writer.Write(bar.High);
            writer.Write(bar.Low);
            writer.Write(bar.Open);
            writer.Write(bar.Close);
            writer.Write(bar.Volume);
            writer.Write(bar.OpenInt);
            writer.Write((byte)bar.Status);
            writer.Write((byte)bar.Type);
            if (bar.Fields != null)
            {
                writer.Write(bar.fields.Size);
                for (int i = 0; i < bar.fields.Size; ++i)
                    writer.Write(bar.fields[i]);
            }
            else
                writer.Write(0);
        }
    }
}
