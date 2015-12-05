// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;

namespace SmartQuant
{
    public class BidStreamer : ObjectStreamer
    {
        public BidStreamer()
        {
            this.typeId = DataObjectType.Bid;
            this.type = typeof(Bid);
        }

        public override object Read(BinaryReader reader, byte version)
        {
            if (version == 0)
                return new Bid(new DateTime(reader.ReadInt64()), reader.ReadByte(), reader.ReadInt32(), reader.ReadDouble(), reader.ReadInt32());
            else
                return new Bid(new DateTime(reader.ReadInt64()), new DateTime(reader.ReadInt64()), reader.ReadByte(), reader.ReadInt32(), reader.ReadDouble(), reader.ReadInt32());
        }

        public override void Write(BinaryWriter writer, object obj)
        {
            var bid = obj as Bid;
            writer.Write(bid.DateTime.Ticks);

            if (bid.ExchangeDateTime.Ticks != 0)
                writer.Write(bid.ExchangeDateTime.Ticks);
            writer.Write(bid.ProviderId);
            writer.Write(bid.InstrumentId);
            writer.Write(bid.Price);
            writer.Write(bid.Size); 
        }
    }
}
