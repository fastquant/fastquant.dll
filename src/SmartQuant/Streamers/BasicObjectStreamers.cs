using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SmartQuant
{
    public class DataObjectStreamer : ObjectStreamer
    {
        public DataObjectStreamer()
        {
            this.typeId = DataObjectType.DataObject;
            this.type = typeof(DataObject);
        }

        public override object Read(BinaryReader reader, byte version)
        {
            var dateTime = new DateTime(reader.ReadInt64());
            return new DataObject(dateTime);
        }

        public override void Write(BinaryWriter writer, object obj)
        {
            writer.Write((obj as DataObject).DateTime.Ticks);
        }
    }

    public class ObjectTableStreamer : ObjectStreamer
    {
        public ObjectTableStreamer()
        {
            this.typeId = DataObjectType.ObjectTable;
            this.type = typeof(ObjectTable);
        }

        public override object Read(BinaryReader reader, byte version)
        {
            var table = new ObjectTable();
            int index;
            while ((index = reader.ReadInt32()) != -1)
                table[index] = this.streamerManager.Deserialize(reader);
            return table;
        }

        public override void Write(BinaryWriter writer, object obj)
        {
            var table = (ObjectTable)obj;
            for (int i = 0; i < table.Fields.Size; i++)
            {
                if (table[i] != null)
                {
                    writer.Write(i);
                    this.streamerManager.Serialize(writer, table[i]);
                }
            }
            writer.Write(-1);
        }
    }

    public class TimeSeriesItemStreamer : ObjectStreamer
    {
        public TimeSeriesItemStreamer()
        {
            this.typeId = DataObjectType.TimeSeriesItem;
            this.type = typeof(TimeSeriesItem);
        }

        public override object Read(BinaryReader reader, byte version)
        {
            return new TimeSeriesItem(new DateTime(reader.ReadInt64()), reader.ReadDouble());
        }

        public override void Write(BinaryWriter writer, object obj)
        {
            var item = (TimeSeriesItem)obj;
            writer.Write(item.DateTime.Ticks);
            writer.Write(item.Value);
        }
    }

    public class ExecutionReportStreamer : ObjectStreamer
    {
        public ExecutionReportStreamer()
        {
            this.typeId = DataObjectType.ExecutionReport;
            this.type = typeof(ExecutionReport);
        }

        public override object Read(BinaryReader reader, byte version)
        {
            var r = new ExecutionReport();
            r.DateTime = new DateTime(reader.ReadInt64());
            r.OrderId = reader.ReadInt32();
            r.ClOrderId = reader.ReadString();
            r.ProviderOrderId = reader.ReadString();
            r.InstrumentId = reader.ReadInt32();
            r.CurrencyId = reader.ReadByte();
            r.ClientId = reader.ReadInt32();
            r.ExecType = (ExecType)reader.ReadByte();
            r.OrdStatus = (OrderStatus)reader.ReadByte();
            r.OrdType = (OrderType)reader.ReadByte();
            r.Side = (OrderSide)reader.ReadByte();
            r.TimeInForce = (TimeInForce)reader.ReadByte();
            r.ExpireTime = new DateTime(reader.ReadInt64());
            r.Price = reader.ReadDouble();
            r.StopPx = reader.ReadDouble();
            r.OrdQty = reader.ReadDouble();
            r.CumQty = reader.ReadDouble();
            r.LeavesQty = reader.ReadDouble();
            r.LastPx = reader.ReadDouble();
            r.LastQty = reader.ReadDouble();
            r.Commission = reader.ReadDouble();
            r.Text = reader.ReadString();
            r.ExecId = reader.ReadString();
            if (reader.ReadBoolean())
                r.Fields = (ObjectTable)this.streamerManager.Deserialize(reader);
            return r;
        }

        public override void Write(BinaryWriter writer, object obj)
        {
            var r = (ExecutionReport)obj;
            writer.Write(r.DateTime.Ticks);
            writer.Write(r.OrderId);
            writer.Write(r.ClOrderId);
            writer.Write(r.ProviderOrderId);
            writer.Write(r.InstrumentId);
            writer.Write(r.CurrencyId);
            writer.Write(r.ClientId);
            writer.Write((byte)r.ExecType);
            writer.Write((byte)r.OrdStatus);
            writer.Write((byte)r.OrdType);
            writer.Write((byte)r.Side);
            writer.Write((byte)r.TimeInForce);
            writer.Write(r.ExpireTime.Ticks);
            writer.Write(r.Price);
            writer.Write(r.StopPx);
            writer.Write(r.OrdQty);
            writer.Write(r.CumQty);
            writer.Write(r.LeavesQty);
            writer.Write(r.LastPx);
            writer.Write(r.LastQty);
            writer.Write(r.Commission);
            writer.Write(r.Text);
            writer.Write(r.ExecId);
            if (r.Fields != null)
            {
                writer.Write(true);
                this.streamerManager.Serialize(writer, r.Fields);
            }
            else
            {
                writer.Write(false);
            }
        }
    }

    public class ExecutionCommandStreamer : ObjectStreamer
    {
        public ExecutionCommandStreamer()
        {
            this.typeId = DataObjectType.ExecutionCommand;
            this.type = typeof(ExecutionCommand);
        }

        public override object Read(BinaryReader reader, byte version)
        {
            var e = new ExecutionCommand();
            e.Id = reader.ReadInt32();
            e.Type = (ExecutionCommandType)reader.ReadByte();
            e.DateTime = new DateTime(reader.ReadInt64());
            e.TransactTime = new DateTime(reader.ReadInt64());
            e.OrderId = reader.ReadInt32();
            e.InstrumentId = reader.ReadInt32();
            e.ProviderId = reader.ReadByte();
            e.RouteId = reader.ReadByte();
            e.PortfolioId = reader.ReadInt32();
            e.ClientId = reader.ReadInt32();
            e.Side = (OrderSide)reader.ReadByte();
            e.OrdType = (OrderType)reader.ReadByte();
            e.TimeInForce = (TimeInForce)reader.ReadByte();
            e.ExpireTime = new DateTime(reader.ReadInt64());
            e.Price = reader.ReadDouble();
            e.StopPx = reader.ReadDouble();
            e.Qty = reader.ReadDouble();
            e.OCA = reader.ReadString();
            e.Text = reader.ReadString();
            if (reader.ReadBoolean())
                e.Account = reader.ReadString();
            if (reader.ReadBoolean())
                e.ClientID = reader.ReadString();
            if (reader.ReadBoolean())
            {
                e.Fields = (ObjectTable)this.streamerManager.Deserialize(reader);
            }
            e.AlgoId = reader.ReadInt32();
            e.ClOrderId = reader.ReadString();
            return e;
        }

        public override void Write(BinaryWriter writer, object obj)
        {
            var e = (ExecutionCommand)obj;
            writer.Write(e.Id);
            writer.Write((byte)e.Type);
            writer.Write(e.DateTime.Ticks);
            writer.Write(e.TransactTime.Ticks);
            writer.Write(e.OrderId);
            writer.Write(e.InstrumentId);
            writer.Write(e.ProviderId);
            writer.Write(e.RouteId);
            writer.Write(e.PortfolioId);
            writer.Write(e.ClientId);
            writer.Write((byte)e.Side);
            writer.Write((byte)e.OrdType);
            writer.Write((byte)e.TimeInForce);
            writer.Write(e.ExpireTime.Ticks);
            writer.Write(e.Price);
            writer.Write(e.StopPx);
            writer.Write(e.Qty);
            writer.Write(e.OCA);
            writer.Write(e.Text);
            if (e.Account != null)
            {
                writer.Write(true);
                writer.Write(e.Account);
            }
            else
            {
                writer.Write(false);
            }

            if (e.ClientID != null)
            {
                writer.Write(true);
                writer.Write(e.ClientID);
            }
            else
            {
                writer.Write(false);
            }

            if (e.Fields != null)
            {
                writer.Write(true);
                this.streamerManager.Serialize(writer, e.Fields);
            }
            else
            {
                writer.Write(false);
            }

            writer.Write(e.AlgoId);
            writer.Write(e.ClOrderId);
        }
    }

    public class TextInfoStreamer : ObjectStreamer
    {
        public TextInfoStreamer()
        {
            this.typeId = DataObjectType.Text;
            this.type = typeof(TextInfo);
        }

        public override object Read(BinaryReader reader, byte version)
        {
            return new TextInfo(new DateTime(reader.ReadInt64()), reader.ReadString());
        }

        public override void Write(BinaryWriter writer, object obj)
        {
            var t = obj as TextInfo;
            writer.Write(t.DateTime.Ticks);
            writer.Write(t.Content);
        }
    }

    public class FieldListStreamer : ObjectStreamer
    {

        public FieldListStreamer()
        {
            this.typeId = DataObjectType.FieldList;
            this.type = typeof(FieldList);
        }

        public override object Read(BinaryReader reader, byte version)
        {
            var f = new FieldList();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                int id = reader.ReadInt32();
                double value = reader.ReadDouble();
                f[id] = value;
            }
            return f;
        }

        public override void Write(BinaryWriter writer, object obj)
        {
            var f = obj as FieldList;
            int count = Enumerable.Range(0, f.Size).Count(i => f[i] != 0);
            writer.Write(count);
            for (int i = 0; i < f.Size; i++)
            {
                if (f[i] != 0)
                {
                    writer.Write(i);
                    writer.Write(f[i]);
                }
            }
        }
    }

    public class StrategyStatusStreamer : ObjectStreamer
    {
        public StrategyStatusStreamer()
        {
            this.typeId = 20;
            this.type = typeof(StrategyStatusInfo);
        }

        public override object Read(BinaryReader reader, byte version)
        {
            var dateTime = new DateTime(reader.ReadInt64());
            var type = (StrategyStatusType)reader.ReadByte();
            return new StrategyStatusInfo(dateTime, type)
            {
                Solution = reader.ReadString(),
                Mode = reader.ReadString()
            };
        }

        public override void Write(BinaryWriter writer, object obj)
        {
            var s = obj as StrategyStatusInfo;
            writer.Write(s.DateTime.Ticks);
            writer.Write((byte)s.Type);
            writer.Write(s.Solution);
            writer.Write(s.Mode);
        }
    }

    public class ProviderErrorStreamer : ObjectStreamer
    {
        public ProviderErrorStreamer()
        {
            this.typeId = DataObjectType.ProviderError;
            this.type = typeof(ProviderError);
        }

        public override object Read(BinaryReader reader, byte version)
        {
            return new ProviderError
            {
                DateTime = new DateTime(reader.ReadInt64()),
                Type = (ProviderErrorType)reader.ReadByte(),
                ProviderId = reader.ReadByte(),
                Id = reader.ReadInt32(),
                Code = reader.ReadInt32(),
                Text = reader.ReadString()
            };
        }

        public override void Write(BinaryWriter writer, object obj)
        {
            var e = (ProviderError)obj;
            writer.Write(e.DateTime.Ticks);
            writer.Write((byte)e.Type);
            writer.Write(e.ProviderId);
            writer.Write(e.Id);
            writer.Write(e.Code);
            writer.Write(e.Text);
        }
    }

    public class FundamentalStreamer : ObjectStreamer
    {
        public FundamentalStreamer()
        {
            this.typeId = DataObjectType.Fundamental;
            this.type = typeof(Fundamental);
        }

        // Token: 0x06000561 RID: 1377 RVA: 0x00025C3C File Offset: 0x00023E3C
        public override object Read(BinaryReader reader, byte version)
        {
            var f = new Fundamental(new DateTime(reader.ReadInt64()), reader.ReadInt32(), reader.ReadInt32())
            {
                Fields = (ObjectTable)this.streamerManager.Deserialize(reader)
            };

            return f;
        }

        public override void Write(BinaryWriter writer, object obj)
        {
            var f = (Fundamental)obj;
            writer.Write(f.DateTime.Ticks);
            writer.Write(f.ProviderId);
            writer.Write(f.InstrumentId);
            this.streamerManager.Serialize(writer, f.Fields);
        }
    }
}
