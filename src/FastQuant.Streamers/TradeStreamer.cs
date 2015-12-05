// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;

namespace SmartQuant
{
    public class TradeStreamer : ObjectStreamer
    {
        public TradeStreamer()
        {
            this.typeId = DataObjectType.Trade;
            this.type = typeof(Trade);
        }

        public override object Read(BinaryReader reader, byte version)
        {
            if (version == 0)
                return new Trade(new DateTime(reader.ReadInt64()), reader.ReadByte(), reader.ReadInt32(), reader.ReadDouble(), reader.ReadInt32());
            else
                return new Trade(new DateTime(reader.ReadInt64()), new DateTime(reader.ReadInt64()), reader.ReadByte(), reader.ReadInt32(), reader.ReadDouble(), reader.ReadInt32());
        }

        public override void Write(BinaryWriter writer, object obj)
        {
            var trade = obj as Trade;
            byte version = 0;
            if (trade.ExchangeDateTime.Ticks != 0)
                version = 1;
            writer.Write(version);
            writer.Write(trade.DateTime.Ticks);
            if (version == 1)
                writer.Write(trade.ExchangeDateTime.Ticks);
            writer.Write(trade.ProviderId);
            writer.Write(trade.InstrumentId);
            writer.Write(trade.Price);
            writer.Write(trade.Size); 
        }
    }
}