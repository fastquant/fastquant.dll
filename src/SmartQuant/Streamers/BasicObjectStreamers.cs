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

    public class NewsStreamer : ObjectStreamer
    {
        public NewsStreamer()
        {
            this.typeId = DataObjectType.News;
            this.type = typeof(News);
        }

        public override object Read(BinaryReader reader, byte version)
        {
            return new News
            {
                DateTime = new DateTime(reader.ReadInt64()),
                ProviderId = reader.ReadInt32(),
                InstrumentId = reader.ReadInt32(),
                Urgency = reader.ReadByte(),
                Url = reader.ReadString(),
                Headline = reader.ReadString(),
                Text = reader.ReadString()
            };
        }

        public override void Write(BinaryWriter writer, object obj)
        {
            News news = obj as News;
            writer.Write(news.DateTime.Ticks);
            writer.Write(news.ProviderId);
            writer.Write(news.InstrumentId);
            writer.Write(news.Urgency);
            writer.Write(news.Url);
            writer.Write(news.Headline);
            writer.Write(news.Text);
        }
    }

    public class PositionStreamer : ObjectStreamer
    {
        public PositionStreamer()
        {
            this.typeId = DataObjectType.Position;
            this.type = typeof(Position);
            this.version = 1;
        }

        public override object Read(BinaryReader reader, byte version)
        {
            var p = new Position();
            if (version >= 1)
            {
                p.PortfolioId = reader.ReadInt32();
                p.InstrumentId = reader.ReadInt32();
                p.Amount = reader.ReadDouble();
                p.QtyBought = reader.ReadDouble();
                p.QtySold = reader.ReadDouble();
            }
            return p;
        }

        public override void Write(BinaryWriter writer, object obj)
        {
            var p = (Position)obj;
            if (this.version >= 1)
            {
                writer.Write(p.PortfolioId);
                writer.Write(p.InstrumentId);
                writer.Write(p.Amount);
                writer.Write(p.QtyBought);
                writer.Write(p.QtySold);
            }
        }
    }

    public class PortfolioStreamer : ObjectStreamer
    {
        public PortfolioStreamer()
        {
            this.typeId = DataObjectType.Portfolio;
            this.type = typeof(Portfolio);
        }

        public override object Read(BinaryReader reader, byte version)
        {
            var p = new Portfolio(reader.ReadString());
            p.Description = reader.ReadString();
            p.Id = reader.ReadInt32();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
                p.Children.Add((Portfolio)this.streamerManager.Deserialize(reader));
            return p;
        }

        public override void Write(BinaryWriter writer, object obj)
        {
            Portfolio portfolio = (Portfolio)obj;
            writer.Write(portfolio.Name);
            writer.Write(portfolio.Description);
            writer.Write(portfolio.Id);
            writer.Write(portfolio.Children.Count);
            foreach (var c in portfolio.Children)
                this.streamerManager.Serialize(writer, c);
        }
    }

    public class AccountDataStreamer : ObjectStreamer
    {
        public AccountDataStreamer()
        {
            this.typeId = DataObjectType.AccountData;
            this.type = typeof(AccountData);
        }

        public override object Read(BinaryReader reader, byte version)
        {
            var datetime = new DateTime(reader.ReadInt64());
            var type = (AccountDataType)reader.ReadInt32();
            var account = reader.ReadString();
            var providerId = reader.ReadByte();
            var route = reader.ReadByte();
            var accountData = new AccountData(datetime, type, account, providerId, route);
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                var name = reader.ReadString();
                var currency = reader.ReadString();
                object value = StreamerManager.Deserialize(reader);
                accountData.Fields.Add(name, currency, value);
            }
            return accountData;
        }

        public override void Write(BinaryWriter writer, object obj)
        {
            var data = (AccountData)obj;
            writer.Write(data.DateTime.Ticks);
            writer.Write((int)data.Type);
            writer.Write(data.Account);
            writer.Write(data.ProviderId);
            writer.Write(data.Route);
            var list = new List<AccountDataField>();
            foreach (AccountDataField field in data.Fields)
            {
                var type = field.Value.GetType();
                if (StreamerManager.HasStreamer(type))
                {
                    list.Add(field);
                }
                else if (type == typeof(object[]))
                {
                    var array = (object[])field.Value;
                    for (int i = 0; i < array.Length; i++)
                        StreamerManager.HasStreamer(array[i].GetType());
                    list.Add(field);
                }
            }
            writer.Write(list.Count);
            foreach (var field in list)
            {
                writer.Write(field.Name);
                writer.Write(field.Currency);
                StreamerManager.Serialize(writer, field.Value);
            }
        }
    }

    public class AccountTransactionStreamer : ObjectStreamer
    {
        public AccountTransactionStreamer()
        {
            this.typeId = DataObjectType.AccountTransaction;
            this.type = typeof(AccountTransaction);
        }

        public override object Read(BinaryReader reader, byte version)
        {
            var dateTime = new DateTime(reader.ReadInt64());
            byte currencyId = reader.ReadByte();
            string text = reader.ReadString();
            double value = reader.ReadDouble();
            return new AccountTransaction(dateTime, value, currencyId, text);
        }

        public override void Write(BinaryWriter writer, object obj)
        {
            var t = obj as AccountTransaction;
            writer.Write(t.DateTime.Ticks);
            writer.Write(t.CurrencyId);
            writer.Write(t.Text);
            writer.Write(t.Value);
        }
    }

    public class UserStreamer : ObjectStreamer
    {
        public UserStreamer()
        {
            this.typeId = DataObjectType.User;
            this.type = typeof(User);
        }

        public override object Read(BinaryReader reader, byte version)
        {
            var user = new User();
            user.Id = reader.ReadInt32();
            user.Name = reader.ReadString();
            user.Password = reader.ReadString();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                UserItem userItem = new UserItem();
                userItem.Name = reader.ReadString();
                userItem.Value = reader.ReadString();
                user.Items.Add(userItem);
            }
            return user;
        }

        public override void Write(BinaryWriter writer, object obj)
        {
            User user = (User)obj;
            writer.Write(user.Id);
            writer.Write(user.Name);
            writer.Write(user.Password);
            writer.Write(user.Items.Count);
            if (user.Items.Count > 0)
            {
                foreach (var item in user.Items)
                {
                    writer.Write(item.Name);
                    writer.Write(item.Value);
                }
            }
        }
    }

    public class AccountReportStreamer : ObjectStreamer
    {
        public AccountReportStreamer()
        {
            this.typeId = DataObjectType.AccountReport;
            this.type = typeof(AccountReport);
        }

        public override object Read(BinaryReader reader, byte version)
        {
            var r = new AccountReport();
            r.DateTime = new DateTime(reader.ReadInt64());
            r.PortfolioId = reader.ReadInt32();
            r.CurrencyId = reader.ReadByte();
            r.Amount = reader.ReadDouble();
            r.Text = reader.ReadString();
            if (reader.ReadBoolean())
            {
                r.Fields = (ObjectTable)this.streamerManager.Deserialize(reader);
            }
            return r;
        }

        public override void Write(BinaryWriter writer, object obj)
        {
            var r = (AccountReport)obj;
            writer.Write(r.dateTime.Ticks);
            writer.Write(r.PortfolioId);
            writer.Write(r.CurrencyId);
            writer.Write(r.Amount);
            writer.Write(r.Text);
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

    public class OnSubscribeStreamer : ObjectStreamer
    {
        public OnSubscribeStreamer()
        {
            this.typeId = DataObjectType.OnSubscribe;
            this.type = typeof(OnSubscribe);
            this.version = 1;
        }

        public override object Read(BinaryReader reader, byte version)
        {
            var dateTime = new DateTime(reader.ReadInt64());
            OnSubscribe os = new OnSubscribe(reader.ReadString());
            os.dateTime = dateTime;
            if (version >= 1 && reader.ReadBoolean())
            {
                os.Subscription = new Subscription
                {
                    SourceId = reader.ReadInt32(),
                    ProviderId = reader.ReadInt32(),
                    RouteId = reader.ReadInt32(),
                    RequestId = reader.ReadInt32(),
                    Symbol = reader.ReadString(),
                    Instrument = (Instrument)this.StreamerManager.Deserialize(reader)
                };
            }
            return os;
        }

        public override void Write(BinaryWriter writer, object obj)
        {
            OnSubscribe os = (OnSubscribe)obj;
            writer.Write(os.DateTime.Ticks);
            writer.Write(os.Symbol);
            if (this.version >= 1)
            {
                writer.Write(os.Subscription != null);
                if (os.Subscription != null)
                {
                    writer.Write(os.Subscription.SourceId);
                    writer.Write(os.Subscription.ProviderId);
                    writer.Write(os.Subscription.RouteId);
                    writer.Write(os.Subscription.RequestId);
                    writer.Write(os.Subscription.Symbol);
                    this.StreamerManager.Serialize(writer, os.Subscription.Instrument);
                }
            }
        }
    }

    public class OnUnsubscribeStreamer : ObjectStreamer
    {
        public OnUnsubscribeStreamer()
        {
            this.typeId = DataObjectType.OnUnsubscribe;
            this.type = typeof(OnUnsubscribe);
            this.version = 1;
        }

        public override object Read(BinaryReader reader, byte version)
        {
            var ou = new OnUnsubscribe();
            ou.DateTime = new DateTime(reader.ReadInt64());
            ou.Symbol = reader.ReadString();
            if (version >= 1 && reader.ReadBoolean())
            {
                ou.Subscription = new Subscription
                {
                    SourceId = reader.ReadInt32(),
                    ProviderId = reader.ReadInt32(),
                    RouteId = reader.ReadInt32(),
                    RequestId = reader.ReadInt32(),
                    Symbol = reader.ReadString(),
                    Instrument = (Instrument)StreamerManager.Deserialize(reader)
                };
            }
            return ou;
        }

        // Token: 0x060008B1 RID: 2225 RVA: 0x0002EC6C File Offset: 0x0002CE6C
        public override void Write(BinaryWriter writer, object obj)
        {
            var ou = (OnUnsubscribe)obj;
            writer.Write(ou.DateTime.Ticks);
            writer.Write(ou.Symbol);
            if (this.version >= 1)
            {
                writer.Write(ou.Subscription != null);
                if (ou.Subscription != null)
                {
                    writer.Write(ou.Subscription.SourceId);
                    writer.Write(ou.Subscription.ProviderId);
                    writer.Write(ou.Subscription.RouteId);
                    writer.Write(ou.Subscription.RequestId);
                    writer.Write(ou.Subscription.Symbol);
                    StreamerManager.Serialize(writer, ou.Subscription.Instrument);
                }
            }
        }
    }

    public class ParameterStreamer : ObjectStreamer
    {
        public ParameterStreamer()
        {
            this.typeId = DataObjectType.Parameter;
            this.type = typeof(Parameter);
        }

        public override object Read(BinaryReader reader, byte version)
        {
            var name = reader.ReadString();
            var value = StreamerManager.Deserialize(reader);
            var typeName = reader.ReadString();
            int count = reader.ReadInt32();
            var attributes = new List<Attribute>();
            for (int i = 0; i < count; i++)
            {
                var attribute = StreamerManager.Deserialize(reader) as Attribute;
                if (attribute != null)
                    attributes.Add(attribute);
            }
            return new Parameter(name, value, typeName, attributes.ToArray());
        }

        public override void Write(BinaryWriter writer, object obj)
        {
            var p = (Parameter)obj;
            writer.Write(p.Name);
            StreamerManager.Serialize(writer, p.Value);
            writer.Write(p.TypeName);
            writer.Write(p.Attributes.Length);
            var attributes = p.Attributes;
            for (int i = 0; i < attributes.Length; i++)
                StreamerManager.Serialize(writer, attributes[i]);
        }
    }

    public class ParameterListStreamer : ObjectStreamer
    {
        public ParameterListStreamer()
        {
            this.typeId = DataObjectType.ParameterList;
            this.type = typeof(ParameterList);
        }

        public override object Read(BinaryReader reader, byte version)
        {
            var list = new ParameterList();
            list.Name = reader.ReadString();
            int pCount = reader.ReadInt32();
            int mCount = reader.ReadInt32();
            for (int i = 0; i < pCount; i++)
                list.Add(StreamerManager.Deserialize(reader) as Parameter);
            for (int j = 0; j < mCount; j++)
                list.Methods.Add(reader.ReadString());
            return list;
        }

        public override void Write(BinaryWriter writer, object obj)
        {
            var list = (ParameterList)obj;
            var ps = list.Parameters().Select(p => StreamerManager.HasStreamer(p.Value.GetType()));
            writer.Write(list.Name);
            writer.Write(ps.Count());
            writer.Write(list.Methods.Count);
            foreach (var p in ps)
                StreamerManager.Serialize(writer, p);
            foreach (string m in list.Methods)
                writer.Write(m);
        }
    }

    public class OnLoginStreamer : ObjectStreamer
    {
        public OnLoginStreamer()
        {
            this.typeId = DataObjectType.OnLogin;
            this.type = typeof(OnLogin);
        }

        public override object Read(BinaryReader reader, byte version)
        {
            var dateTime = new DateTime(reader.ReadInt64());
            return new OnLogin(dateTime)
            {
                ProductName = reader.ReadString(),
                UserName = reader.ReadString(),
                Password = reader.ReadString(),
                Id = reader.ReadInt32(),
                GUID = reader.ReadString()
            };
        }

        public override void Write(BinaryWriter writer, object obj)
        {
            var ol = (OnLogin)obj;
            writer.Write(ol.DateTime.Ticks);
            writer.Write(ol.ProductName);
            writer.Write(ol.UserName);
            writer.Write(ol.Password);
            writer.Write(ol.Id);
            writer.Write(ol.GUID);
        }
    }

    public class OnLogoutStreamer : ObjectStreamer
    {
        public OnLogoutStreamer()
        {
            this.typeId = DataObjectType.OnLogout;
            this.type = typeof(OnLogout);
        }

        public override object Read(BinaryReader reader, byte version)
        {
            DateTime dateTime = new DateTime(reader.ReadInt64());
            return new OnLogout(dateTime)
            {
                ProductName = reader.ReadString(),
                UserName = reader.ReadString(),
                Reason = reader.ReadString(),
                Id = reader.ReadInt32()
            };
        }

        public override void Write(BinaryWriter writer, object obj)
        {
            var ol = (OnLogout)obj;
            writer.Write(ol.DateTime.Ticks);
            writer.Write(ol.ProductName);
            writer.Write(ol.UserName);
            writer.Write(ol.Reason);
            writer.Write(ol.Id);
        }
    }

    public class OnLoggedInStreamer : ObjectStreamer
    {
        public OnLoggedInStreamer()
        {
            this.typeId = DataObjectType.OnLoggedIn;
            this.type = typeof(OnLoggedIn);
        }

        public override object Read(BinaryReader reader, byte version)
        {
            var dateTime = new DateTime(reader.ReadInt64());
            return new OnLoggedIn(dateTime)
            {
                UserId = reader.ReadInt32(),
                UserName = reader.ReadString(),
                DefaultAlgoId = reader.ReadInt32(),
                Fields = (ObjectTable)StreamerManager.Deserialize(reader)
            };
        }

        public override void Write(BinaryWriter writer, object obj)
        {
            var ol = (OnLoggedIn)obj;
            writer.Write(ol.DateTime.Ticks);
            writer.Write(ol.UserId);
            writer.Write(ol.UserName);
            writer.Write(ol.DefaultAlgoId);
            StreamerManager.Serialize(writer, ol.Fields);
        }
    }

    public class OnLoggedOutStreamer : ObjectStreamer
    {
        public OnLoggedOutStreamer()
        {
            this.typeId = DataObjectType.OnLoggedOut;
            this.type = typeof(OnLoggedOut);
        }

        public override object Read(BinaryReader reader, byte version)
        {
            return new OnLoggedOut(new DateTime(reader.ReadInt64()));
        }

        public override void Write(BinaryWriter writer, object obj)
        {
            writer.Write((obj as Event).DateTime.Ticks);
        }
    }

    public class OnHeartbeatStreamer : ObjectStreamer
    {
        public OnHeartbeatStreamer()
        {
            this.typeId = DataObjectType.OnHeartbeat;
            this.type = typeof(OnHeartbeat);
        }

        public override object Read(BinaryReader reader, byte version)
        {
            return new OnHeartbeat(new DateTime(reader.ReadInt64()));
        }

        public override void Write(BinaryWriter writer, object obj)
        {
            writer.Write((obj as OnHeartbeat).DateTime.Ticks);
        }
    }

    public class OnProviderConnectedStreamer : ObjectStreamer
    {
        public OnProviderConnectedStreamer()
        {
            this.typeId = DataObjectType.OnProviderConnected;
            this.type = typeof(OnProviderConnected);
        }

        public override object Read(BinaryReader reader, byte version)
        {
            var dateTime = new DateTime(reader.ReadInt64());
            var providerId = reader.ReadByte();
            return new OnProviderConnected(dateTime, providerId);
        }

        public override void Write(BinaryWriter writer, object obj)
        {
            var op = (OnProviderConnected)obj;
            writer.Write(op.DateTime.Ticks);
            writer.Write(op.ProviderId);
        }
    }

    public class OnProviderDisconnectedStreamer : ObjectStreamer
    {
        public OnProviderDisconnectedStreamer()
        {
            this.typeId = DataObjectType.OnProviderDisconnected;
            this.type = typeof(OnProviderDisconnected);
        }

        public override object Read(BinaryReader reader, byte version)
        {
            var dateTime = new DateTime(reader.ReadInt64());
            var providerId = reader.ReadByte();
            return new OnProviderDisconnected(dateTime, providerId);
        }

        public override void Write(BinaryWriter writer, object obj)
        {
            var op = (OnProviderDisconnected)obj;
            writer.Write(op.DateTime.Ticks);
            writer.Write(op.ProviderId);
        }
    }
}
