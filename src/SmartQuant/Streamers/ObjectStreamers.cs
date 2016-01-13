using System;
using System.IO;

namespace SmartQuant
{
    public class DataSeriesStreamer : ObjectStreamer
    {
        public DataSeriesStreamer()
        {
            this.typeId = ObjectType.DataSeries;
            this.type = typeof(DataSeries);
        }

        public override object Read(BinaryReader reader, byte version)
        {
            return DataSeries.FromReader(reader, version);
        }

        public override void Write(BinaryWriter writer, object obj)
        {
            (obj as DataSeries).ToWriter(writer);
        }
    }

    public class ObjectKeyListStreamer : ObjectStreamer
    {
        public ObjectKeyListStreamer()
        {
            this.typeId = ObjectType.ObjectKeyList;
            this.type = typeof(ObjectKeyList);
        }

        public override object Read(BinaryReader reader, byte version)
        {
            return ObjectKeyList.FromReader(reader, version);
        }

        public override void Write(BinaryWriter writer, object obj)
        {
            (obj as ObjectKeyList).ToWriter(writer);
        }
    }

    public class FreeKeyListStreamer : ObjectStreamer
    {
        public FreeKeyListStreamer()
        {
            this.typeId = ObjectType.FreeKeyList;
            this.type = typeof(FreeKeyList);
        }

        public override object Read(BinaryReader reader, byte version)
        {
            return FreeKeyList.FromReader(reader, version);

        }

        public override void Write(BinaryWriter writer, object obj)
        {
            (obj as FreeKeyList).ToWriter(writer);
        }
    }

    public class DataKeyIdArrayStreamer : ObjectStreamer
    {
        public DataKeyIdArrayStreamer()
        {
            this.typeId = ObjectType.DataKeyIdArray;
            this.type = typeof(DataKeyIdArray);
        }

        public override object Read(BinaryReader reader, byte version)
        {
            return DataKeyIdArray.FromReader(reader, version);
        }

        public override void Write(BinaryWriter writer, object obj)
        {
            (obj as DataKeyIdArray).ToWriter(writer);
        }
    }

    public class AltIdStreamer : ObjectStreamer
    {
        public AltIdStreamer()
        {
            this.typeId = ObjectType.AltId;
            this.type = typeof(AltId);
            this.version = 1;
        }

        public override object Read(BinaryReader reader, byte version)
        {
            var providerId = reader.ReadByte();
            var symbol = reader.ReadString();
            var exchange = reader.ReadString();
            var altId = new AltId(providerId, symbol, exchange);
            if (version >= 1)
                altId.CurrencyId = reader.ReadByte();
            return altId;
        }

        public override void Write(BinaryWriter writer, object obj)
        {
            AltId altId = (AltId)obj;
            writer.Write(altId.ProviderId);
            writer.Write(altId.Symbol);
            writer.Write(altId.Exchange);
            if (this.version >= 1)
                writer.Write(altId.ProviderId);
        }
    }

	public class LegStreamer : ObjectStreamer
	{
		public LegStreamer()
		{
			this.typeId = ObjectType.Leg;
			this.type = typeof(Leg);
		}

		public override object Read(BinaryReader reader, byte streamer)
		{
            return Leg.FromReader(reader);
		}

		public override void Write(BinaryWriter writer, object obj)
		{
            (obj as Leg).ToWriter(writer);
		}
	}

    public class InstrumentStreamer : ObjectStreamer
    {
        public InstrumentStreamer()
        {
            this.typeId = ObjectType.Instrument;
            this.type = typeof(Instrument);
            this.version = 5;
        }

        public override object Read(BinaryReader reader, byte version)
        {
            int id = reader.ReadInt32();
            var type = (InstrumentType)reader.ReadByte();
            var symbol = reader.ReadString();
            var description = reader.ReadString();
            var currencyId = reader.ReadByte();
            var exchange = reader.ReadString();
            var instrument = new Instrument(id, type, symbol, description, currencyId, exchange);
            instrument.TickSize = reader.ReadDouble();
            instrument.Maturity = new DateTime(reader.ReadInt64());
            instrument.Factor = reader.ReadDouble();
            instrument.Strike = reader.ReadDouble();
            instrument.PutCall = (PutCall)reader.ReadByte();
            instrument.Margin = reader.ReadDouble();

            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
                instrument.AltId.Add((AltId)this.streamerManager.Deserialize(reader));

            count = reader.ReadInt32();
            for (int j = 0; j < count; j++)
                instrument.Legs.Add((Leg)this.streamerManager.Deserialize(reader));

            if (version == 0)
            {
                int cnt = reader.ReadInt32();
                for (int k = 0; k < cnt; k++)
                    instrument.Fields[k] = reader.ReadDouble();
            }

            if (version >= 1)
                instrument.PriceFormat = reader.ReadString();

            if (version >= 1 && version <= 4)
            {
                int size = reader.ReadInt32();
                if (size != -1)
                {
                    byte typeId = reader.ReadByte();
                    byte ver = reader.ReadByte();
                    var fields = new ObjectTable();
                    int index;
                    while ((index = reader.ReadInt32()) != -1)
                    {
                        typeId = reader.ReadByte();
                        var objectStreamer = this.streamerManager.Get(typeId);
                        fields[index] = objectStreamer.Read(reader, ver);
                    }
                    instrument.Fields = fields;
                }
            }
            if (version >= 2)
            {
                instrument.CCY1 = reader.ReadByte();
                instrument.CCY2 = reader.ReadByte();
            }

            if (version >= 3)
            {
                instrument.DeleteCached = reader.ReadBoolean();
            }

            if (version >= 4)
            {
                instrument.Formula = reader.ReadString();
            }

            if (version >= 5)
            {
                int eof = reader.ReadInt32();
                if (eof != -1)
                    instrument.Fields = (ObjectTable)this.streamerManager.Deserialize(reader);
            }
            return instrument;
        }

        public override void Write(BinaryWriter writer, object obj)
        {
            var instrument = (Instrument)obj;
            writer.Write(instrument.Id);
            writer.Write((byte)instrument.Type);
            writer.Write(instrument.Symbol);
            writer.Write(instrument.Description);
            writer.Write(instrument.CurrencyId);
            writer.Write(instrument.Exchange);
            writer.Write(instrument.TickSize);
            writer.Write(instrument.Maturity.Ticks);
            writer.Write(instrument.Factor);
            writer.Write(instrument.Strike);
            writer.Write((byte)instrument.PutCall);
            writer.Write(instrument.Margin);
            writer.Write(instrument.AltId.Count);
            foreach (AltId altId in instrument.AltId)
                this.streamerManager.Serialize(writer, altId);
            writer.Write(instrument.Legs.Count);
            foreach (Leg leg in instrument.Legs)
                this.streamerManager.Serialize(writer, leg);
            if (this.version == 0)
            {
                writer.Write(instrument.Fields.Size);
                for (int i = 0; i < instrument.Fields.Size; i++)
                    writer.Write((double)instrument.Fields[i]);
            }

            if (this.version >= 1)
                writer.Write(instrument.PriceFormat);

            if (this.version >= 2)
            {
                writer.Write(instrument.CCY1);
                writer.Write(instrument.CCY2);
            }

            if (this.version >= 3)
                writer.Write(instrument.DeleteCached);

            if (this.version >= 4)
                writer.Write(instrument.Formula);

            if (this.version >= 5)
            {
                if (instrument.Fields == null)
                {
                    writer.Write(-1);
                }
                else
                {
                    writer.Write(instrument.Fields.Size);
                    this.streamerManager.Serialize(writer, instrument.Fields);
                }
            }
        }
    }


}
