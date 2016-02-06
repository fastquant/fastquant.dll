// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartQuant
{
    public class BarFactoryItem
    {
        protected internal BarFactory factory;
        protected internal Instrument instrument;
        protected internal BarType barType;
        protected internal long barSize;
        protected internal BarInput barInput;
        protected internal bool sessionEnabled;
        protected internal TimeSpan session1;
        protected internal TimeSpan session2;
        protected internal int providerId;
        protected Bar bar;

        public Instrument Instrument => this.instrument;

        public BarType BarType => this.barType;

        public long BarSize => this.barSize;

        public int ProviderId { get; set; }

        public bool SessionEnabled
        {
            get
            {
                return this.sessionEnabled;
            }
            set
            {
                this.sessionEnabled = value;
            }
        }

        public TimeSpan Session1
        {
            get
            {
                return this.session1;
            }
            set
            {
                this.session1 = value;
            }
        }

        public TimeSpan Session2
        {
            get
            {
                return this.session2;
            }
            set
            {
                this.session2 = value;
            }
        }

        protected BarFactoryItem(Instrument instrument, BarType barType, long barSize, BarInput barInput = BarInput.Trade, int providerId = -1)
        {
            this.instrument = instrument;
            this.barType = barType;
            this.barSize = barSize;
            this.barInput = barInput;
            this.providerId = providerId;
        }

        protected BarFactoryItem(Instrument instrument, BarType barType, long barSize, BarInput barInput, TimeSpan session1, TimeSpan session2, int providerId = -1)
            : this(instrument, barType, barSize, barInput, providerId)
        {
            this.sessionEnabled = true;
            this.session1 = session1;
            this.session2 = session2;
        }

        protected virtual bool InSession(DateTime dateTime) => !this.sessionEnabled || this.session1 <= dateTime.TimeOfDay && dateTime.TimeOfDay <= this.session2;

        internal void Process(DataObject obj)
        {
            if (this.providerId != -1 && ((Tick)obj).ProviderId != this.providerId)
                return;

            if (!InSession(obj.DateTime))
                return;

            OnData(obj);
        }

        protected virtual void OnData(DataObject obj)
        {
            var tick = obj as Tick;
            if (this.bar == null)
            {
                // new bar begins!
                this.bar = new Bar
                {
                    InstrumentId = tick.InstrumentId,
                    Type = this.barType,
                    Size = this.barSize,
                    OpenDateTime = GetBarOpenDateTime(obj),
                    DateTime = this.GetDataObjectDateTime(obj, ClockType.Local),
                    Open = tick.Price,
                    High = tick.Price,
                    Low = tick.Price,
                    Close = tick.Price,
                    Volume = tick.Size,
                    Status = BarStatus.Open
                };
                this.factory.Framework.EventServer.OnEvent(this.bar);
            }
            else
            {
                // update it!
                if (tick.Price > this.bar.High)
                    this.bar.High = tick.Price;
                if (tick.Price < this.bar.Low)
                    this.bar.Low = tick.Price;
                this.bar.Close = tick.Price;
                this.bar.Volume += tick.Size;
                this.bar.DateTime = GetDataObjectDateTime(obj, ClockType.Local);
            }
            ++this.bar.N;
        }

        protected internal virtual void OnReminder(DateTime datetime)
        {
            // noop
        }

        protected virtual DateTime GetBarOpenDateTime(DataObject obj) => obj.DateTime;

        protected virtual DateTime GetBarCloseDateTime(DataObject obj) => obj.DateTime;

        protected virtual DateTime GetDataObjectDateTime(DataObject obj, ClockType type)
        {
            return type == ClockType.Local ? obj.DateTime : ((Tick)obj).ExchangeDateTime;
        }

        protected bool AddReminder(DateTime datetime, ClockType type) => this.factory.AddReminder(this, datetime, type);

        protected void EmitBar()
        {
            this.bar.Status = BarStatus.Close;
            this.factory.Framework.EventServer.OnEvent(this.bar);
            this.bar = null;
        }

        public override string ToString() => $"{Instrument.Symbol} {BarType} {BarSize} {this.barInput}";
    }

    public class TimeBarFactoryItem : BarFactoryItem
    {
        private ClockType type;

        public TimeBarFactoryItem(Instrument instrument, long barSize, int providerId = -1)
            : base(instrument, BarType.Time, barSize, BarInput.Trade, providerId)
        {
        }

        public TimeBarFactoryItem(Instrument instrument, long barSize, ClockType type = ClockType.Local, int providerId = -1)
            : base(instrument, BarType.Time, barSize, BarInput.Trade, providerId)
        {
            this.type = type;
        }

        public TimeBarFactoryItem(Instrument instrument, long barSize, BarInput barInput = BarInput.Trade, int providerId = -1)
            : base(instrument, BarType.Time, barSize, barInput, providerId)
        {
        }

        public TimeBarFactoryItem(Instrument instrument, long barSize, BarInput barInput = BarInput.Trade, ClockType type = ClockType.Local, int providerId = -1)
            : base(instrument, BarType.Time, barSize, barInput, providerId)
        {
            this.type = type;
        }

        public TimeBarFactoryItem(Instrument instrument, long barSize, BarInput barInput, TimeSpan session1, TimeSpan session2, int providerId = -1)
            : base(instrument, BarType.Tick, barSize, barInput, session1, session2, providerId)
        {
        }

        public TimeBarFactoryItem(Instrument instrument, long barSize, BarInput barInput, ClockType type, TimeSpan session1, TimeSpan session2, int providerId = -1)
            : base(instrument, BarType.Time, barSize, barInput, session1, session2, providerId)
        {
            this.type = type;
        }

        protected override void OnData(DataObject obj)
        {
            bool barOpen = this.bar == null;
            base.OnData(obj);
            if (barOpen)
            {
                var success = AddReminder(GetBarCloseDateTime(obj), this.type);
                if (!success)
                {
                    if (obj is Tick)
                        Console.WriteLine($"TimeBarFactoryItem::OnData Can not add reminder. Clock = {this.type} local datetime = {this.factory.Framework.Clock.DateTime} exchange dateTime = {this.factory.Framework.ExchangeClock.DateTime} object = {obj} object exchange datetime = {(obj as Tick).ExchangeDateTime} reminder datetime = {GetBarCloseDateTime(obj)}");
                    else
                        Console.WriteLine($"TimeBarFactoryItem::OnData Can not add reminder. Object is not tick! Clock = {this.type} local datetime = {this.factory.Framework.Clock.DateTime} exchange datetime = {this.factory.Framework.ExchangeClock.DateTime} object = {obj} reminder datetime = {GetBarCloseDateTime(obj)}");
                }
            }


            //bool flag = this.bar == null;
            //base.OnData(obj);
            //if (!flag || AddReminder(GetBarCloseDateTime(obj), this.type))
            //    return;
            //if (obj is Tick)
            //    Console.WriteLine($"TimeBarFactoryItem::OnData Can not add reminder. Clock = {this.type} local datetime = {this.factory.Framework.Clock.DateTime} exchange dateTime = {this.factory.Framework.ExchangeClock.DateTime} object = {obj} object exchange datetime = {(obj as Tick).ExchangeDateTime} reminder datetime = {GetBarCloseDateTime(obj)}");
            //else
            //    Console.WriteLine($"TimeBarFactoryItem::OnData Can not add reminder. Object is not tick! Clock = {this.type} local datetime = {this.factory.Framework.Clock.DateTime} exchange datetime = {this.factory.Framework.ExchangeClock.DateTime} object = {obj} reminder datetime = {GetBarCloseDateTime(obj)}");
        }

        protected override DateTime GetBarOpenDateTime(DataObject obj)
        {
            var t = GetDataObjectDateTime(obj, this.type);
            long seconds = (long)t.TimeOfDay.TotalSeconds / this.barSize * this.barSize;
            return t.Date.AddSeconds(seconds);
        }

        protected override DateTime GetBarCloseDateTime(DataObject obj)
        {
            return GetBarOpenDateTime(obj).AddSeconds(this.barSize);
        }

        protected internal override void OnReminder(DateTime datetime)
        {
            this.bar.CloseDateTime = this.type == ClockType.Local ? datetime : this.factory.Framework.Clock.DateTime;
            EmitBar();
        }
    }

    public class TickBarFactoryItem : BarFactoryItem
    {
        public TickBarFactoryItem(Instrument instrument, long barSize, BarInput barInput = BarInput.Trade, int providerId = -1)
            : base(instrument, BarType.Tick, barSize, barInput, providerId)
        {
        }

        public TickBarFactoryItem(Instrument instrument, long barSize, BarInput barInput, TimeSpan session1, TimeSpan session2, int providerId = -1)
            : base(instrument, BarType.Tick, barSize, barInput, session1, session2, providerId)
        {
        }

        protected override void OnData(DataObject obj)
        {
            base.OnData(obj);
            if (this.bar.N == this.barSize)
                EmitBar();
        }
    }

    public class RangeBarFactoryItem : BarFactoryItem
    {
        public RangeBarFactoryItem(Instrument instrument, long barSize, BarInput barInput = BarInput.Trade, int providerId = -1)
            : base(instrument, BarType.Range, barSize, barInput, providerId)
        {
        }

        public RangeBarFactoryItem(Instrument instrument, long barSize, BarInput barInput, TimeSpan session1, TimeSpan session2, int providerId = -1)
            : base(instrument, BarType.Tick, barSize, barInput, session1, session2, providerId)
        {
        }

        protected override void OnData(DataObject obj)
        {
            var tick = (Tick)obj;
            if (this.bar != null) // when the bar is not new
            {
                if ((tick.Price > this.bar.High || tick.Price < this.bar.Low) && 10000 * (this.bar.High - this.bar.Low) >= this.barSize)
                {
                    this.bar.DateTime = tick.DateTime;
                    EmitBar();
                }
            }
            base.OnData(obj);
        }
    }

    public class VolumeBarFactoryItem : BarFactoryItem
    {
        public VolumeBarFactoryItem(Instrument instrument, long barSize, BarInput barInput = BarInput.Trade, int providerId = -1)
            : base(instrument, BarType.Volume, barSize, barInput, providerId)
        {
        }

        public VolumeBarFactoryItem(Instrument instrument, long barSize, BarInput barInput, TimeSpan session1, TimeSpan session2, int providerId = -1)
            : base(instrument, BarType.Tick, barSize, barInput, session1, session2, providerId)
        {
        }

        protected override void OnData(DataObject obj)
        {
            base.OnData(obj);
            if (this.bar.Volume >= this.barSize)
                EmitBar();
        }
    }

    public class SessionBarFactoryItem : BarFactoryItem
    {
        private ClockType type;

        public SessionBarFactoryItem(Instrument instrument, BarInput barInput, TimeSpan session1, TimeSpan session2, int providerId = -1)
            : base(instrument, BarType.Session, (long)(session2 - session1).TotalSeconds, barInput, session1, session2, providerId)
        {
        }

        public SessionBarFactoryItem(Instrument instrument, BarInput barInput, ClockType type, TimeSpan session1, TimeSpan session2, int providerId = -1)
            : base(instrument, BarType.Session, (long)(session2 - session1).TotalSeconds, barInput, session1, session2, providerId)
        {
            this.type = type;
        }

        protected override void OnData(DataObject obj)
        {
            bool barOpen = this.bar == null;
            base.OnData(obj);
            if (barOpen)
            {
                if (!AddReminder(GetBarCloseDateTime(obj), this.type))
                {
                    if (!(obj is Tick))
                        Console.WriteLine($"SessionBarFactoryItem::OnData Can not add reminder. Object is not tick! Clock = {this.type} local datetime = {this.factory.Framework.Clock.DateTime} exchange datetime = {this.factory.Framework.ExchangeClock.DateTime} object = {obj} reminder datetime = {GetBarCloseDateTime(obj)}");
                    else
                        Console.WriteLine($"SessionBarFactoryItem::OnData Can not add reminder. Clock = {this.type} local datetime = {this.factory.Framework.Clock.DateTime} exchange dateTime = {this.factory.Framework.ExchangeClock.DateTime} object = {obj} object exchange datetime = {((Tick)obj).ExchangeDateTime} reminder datetime = {GetBarCloseDateTime(obj)}");
                }
            }

            //bool barOpen = this.bar == null;
            //base.OnData(obj);
            //if (!barOpen || AddReminder(GetBarCloseDateTime(obj), this.type))
            //    return;
            //if (!(obj is Tick))
            //    Console.WriteLine($"SessionBarFactoryItem::OnData Can not add reminder. Object is not tick! Clock = {this.type} local datetime = {this.factory.Framework.Clock.DateTime} exchange datetime = {this.factory.Framework.ExchangeClock.DateTime} object = {obj} reminder datetime = {GetBarCloseDateTime(obj)}");
            //else
            //    Console.WriteLine($"SessionBarFactoryItem::OnData Can not add reminder. Clock = {this.type} local datetime = {this.factory.Framework.Clock.DateTime} exchange dateTime = {this.factory.Framework.ExchangeClock.DateTime} object = {obj} object exchange datetime = {((Tick)obj).ExchangeDateTime} reminder datetime = {GetBarCloseDateTime(obj)}");
        }

        protected override DateTime GetBarOpenDateTime(DataObject obj)
        {
            return GetDataObjectDateTime(obj, this.type).Date.Add(this.session1);
        }

        protected override DateTime GetBarCloseDateTime(DataObject obj)
        {
            return GetDataObjectDateTime(obj, this.type).Date.Add(this.session2);
        }

        protected internal override void OnReminder(DateTime datetime)
        {
            this.bar.CloseDateTime = this.type == ClockType.Local ? datetime : this.factory.Framework.Clock.DateTime;
            EmitBar();
        }
    }

    public class BarFactory
    {
        private Framework framework;

        private readonly SortedList<DateTime, SortedList<long, List<BarFactoryItem>>> reminderTable =
            new SortedList<DateTime, SortedList<long, List<BarFactoryItem>>>();

        public IdArray<List<BarFactoryItem>> ItemLists { get; } = new IdArray<List<BarFactoryItem>>(256);

        internal Framework Framework => this.framework;

        public BarFactory(Framework framework)
        {
            this.framework = framework;
        }

        // The most important one of all 'Add's
        public void Add(BarFactoryItem item)
        {
            if (item.factory != null)
                throw new InvalidOperationException("BarFactoryItem is already added to another BarFactory instance.");

            item.factory = this;
            int id = item.Instrument.Id;
            var items = ItemLists[id] = ItemLists[id] ?? new List<BarFactoryItem>();
            if (
                items.Exists(
                    match =>
                        item.barType == match.barType && item.barSize == match.barSize &&
                        item.barInput == match.barInput && item.providerId == match.providerId))
                Console.WriteLine($"{DateTime.Now} BarFactory::Add Item '{item}' is already added");
            else
                items.Add(item);
        }

        public void Add(Instrument instrument, TimeSpan session1, TimeSpan session2, int providerId = -1)
        {
            this.Add(instrument, BarType.Session, (session2 - session1).Seconds, BarInput.Trade, ClockType.Local,
                session1, session2, providerId);
        }

        public void Add(string symbol, TimeSpan session1, TimeSpan session2, int providerId = -1)
        {
            Add(this.framework.InstrumentManager[symbol], BarType.Session, (session2 - session1).Seconds, BarInput.Trade,
                ClockType.Local, session1, session2, providerId);
        }

        public void Add(string symbol, BarType barType, long barSize, BarInput barInput = BarInput.Trade,
            int providerId = -1)
        {
            Add(this.framework.InstrumentManager[symbol], barType, barSize, barInput, ClockType.Local, providerId);
        }

        public void Add(InstrumentList instruments, BarType barType, long barSize, BarInput barInput = BarInput.Trade,
            int providerId = -1)
        {
            foreach (var i in instruments)
                Add(i, barType, barSize, barInput, ClockType.Local, providerId);
        }

        public void Add(string[] symbols, BarType barType, long barSize, BarInput barInput = BarInput.Trade,
            int providerId = -1)
        {
            foreach (var symbol in symbols)
                Add(this.framework.InstrumentManager.Get(symbol), barType, barSize, barInput, ClockType.Local,
                    providerId);
        }

        public void Add(Instrument instrument, BarType barType, long barSize, BarInput barInput = BarInput.Trade,
            ClockType type = ClockType.Local, int providerId = -1)
        {
            BarFactoryItem item;
            switch (barType)
            {
                case BarType.Time:
                    item = new TimeBarFactoryItem(instrument, barSize, barInput, type, providerId);
                    break;
                case BarType.Tick:
                    item = new TickBarFactoryItem(instrument, barSize, barInput, providerId);
                    break;
                case BarType.Volume:
                    item = new VolumeBarFactoryItem(instrument, barSize, barInput, providerId);
                    break;
                case BarType.Range:
                    item = new RangeBarFactoryItem(instrument, barSize, barInput, providerId);
                    break;
                case BarType.Session:
                    throw new ArgumentException(
                        "BarFactory::Add Can not create SessionBarFactoryItem without session parameters");
                default:
                    throw new ArgumentException($"Unknown bar type - {barType}");
            }
            Add(item);
        }

        public void Add(string symbol, BarType barType, long barSize, BarInput barInput = BarInput.Trade,
            ClockType type = ClockType.Local, int providerId = -1)
        {
            Add(this.framework.InstrumentManager[symbol], barType, barSize, barInput, type, providerId);
        }

        public void Add(InstrumentList instruments, BarType barType, long barSize, BarInput barInput = BarInput.Trade,
            ClockType type = ClockType.Local, int providerId = -1)
        {
            foreach (var i in instruments)
                Add(i, barType, barSize, barInput, type, providerId);
        }

        public void Add(string[] symbols, BarType barType, long barSize, BarInput barInput = BarInput.Trade,
            ClockType type = ClockType.Local, int providerId = -1)
        {
            foreach (var symbol in symbols)
                Add(this.framework.InstrumentManager.Get(symbol), barType, barSize, barInput, type, providerId);
        }

        public void Add(Instrument instrument, BarType barType, long barSize, TimeSpan session1, TimeSpan session2,
            int providerId = -1)
        {
            Add(instrument, barType, barSize, BarInput.Trade, ClockType.Local, session1, session2, providerId);
        }

        public void Add(string symbol, BarType barType, long barSize, TimeSpan session1, TimeSpan session2,
            int providerId = -1)
        {
            Add(this.framework.InstrumentManager[symbol], barType, barSize, BarInput.Trade, ClockType.Local, session1,
                session2, providerId);
        }

        public void Add(string symbol, BarInput barInput, ClockType type, TimeSpan session1, TimeSpan session2,
            int providerId = -1)
        {
            Add(this.framework.InstrumentManager[symbol], BarType.Session, (session2 - session1).Seconds, barInput, type,
                session1, session2, providerId);
        }

        public void Add(Instrument instrument, BarType barType, long barSize, BarInput barInput, ClockType type,
            TimeSpan session1, TimeSpan session2, int providerId = -1)
        {
            BarFactoryItem item;
            switch (barType)
            {
                case BarType.Time:
                    item = new TimeBarFactoryItem(instrument, barSize, barInput, type, session1, session2, providerId);
                    break;
                case BarType.Tick:
                    item = new TickBarFactoryItem(instrument, barSize, barInput, session1, session2, providerId);
                    break;
                case BarType.Volume:
                    item = new VolumeBarFactoryItem(instrument, barSize, barInput, session1, session2, providerId);
                    break;
                case BarType.Range:
                    item = new RangeBarFactoryItem(instrument, barSize, barInput, session1, session2, providerId);
                    break;
                case BarType.Session:
                    item = new SessionBarFactoryItem(instrument, barInput, type, session1, session2, providerId);
                    break;
                default:
                    throw new ArgumentException($"Unknown bar type - {barType}");
            }
            Add(item);
        }

        public void Add(string symbol, BarType barType, long barSize, BarInput barInput, ClockType type,
            TimeSpan session1, TimeSpan session2, int providerId = -1)
        {
            Add(this.framework.InstrumentManager[symbol], barType, barSize, barInput, type, session1, session2,
                providerId);
        }

        public void Remove(BarFactoryItem item)
        {
            var list = ItemLists[item.Instrument.Id];
            if (list == null)
                return;

            var found =
                list.Find(x => x.barType == item.barType && x.barSize == item.barSize && x.barInput == item.barInput);
            if (found != null)
                list.Remove(found);
            else
                Console.WriteLine($"{DateTime.Now} BarFactory::Remove Item '{item}' is already removed");
        }

        public void Remove(Instrument instrument, BarType barType, long barSize, BarInput barInput = BarInput.Trade,
            ClockType type = ClockType.Local)
        {
            BarFactoryItem item;
            switch (barType)
            {
                case BarType.Time:
                    item = new TimeBarFactoryItem(instrument, barSize, barInput, type, -1);
                    break;
                case BarType.Tick:
                    item = new TickBarFactoryItem(instrument, barSize, barInput, -1);
                    break;
                case BarType.Volume:
                    item = new VolumeBarFactoryItem(instrument, barSize, barInput, -1);
                    break;
                case BarType.Range:
                    item = new RangeBarFactoryItem(instrument, barSize, barInput, -1);
                    break;
                case BarType.Session:
                    throw new ArgumentException(
                        "BarFactory::Remove Can not create SessionBarFactoryItem without session parameters");
                default:
                    throw new ArgumentException($"Unknown bar type - {barType}");
            }
            Remove(item);
        }

        public void Clear()
        {
            ItemLists.Clear();
            this.reminderTable.Clear();
        }

        // TODO: rewrite it using goto statement!!!
        internal void OnData(DataObject obj)
        {
            /*
                        var tick = (Tick)obj;
                        var items = ItemLists[tick.InstrumentId];
                        if (items == null)
                            return;

                        int i = 0;
                        while (i < items.Count)
                        {
                            var item = items[i];
                            switch (item.barInput)
                            {
                                case BarInput.Trade:
                                    if (tick.TypeId == DataObjectType.Trade)
                                    {
                                        item.Process(tick);
                                        i++;
                                        continue;
                                    }
                                    break;
                                case BarInput.Bid:
                                    if (tick.TypeId == DataObjectType.Bid)
                                    {
                                        item.Process(tick);
                                        i++;
                                        continue;
                                    }
                                    break;
                                case BarInput.Ask:
                                    if (tick.TypeId == DataObjectType.Ask)
                                    {
                                        item.Process(tick);
                                        i++;
                                        continue;
                                    }
                                    break;
                                case BarInput.Middle:
                                    switch (tick.TypeId)
                                    {
                                        case DataObjectType.Bid:
                                            {
                                                var ask = this.framework.DataManager.GetAsk(tick.InstrumentId);
                                                if (ask == null)
                                                {
                                                    i++;
                                                    continue;
                                                }
                                                tick = new Tick(obj.DateTime, tick.ProviderId, tick.InstrumentId, (ask.Price + tick.Price) / 2.0, (ask.Size + tick.Size) / 2);
                                                break;
                                            }
                                        case DataObjectType.Ask:
                                            {
                                                Bid bid = this.framework.DataManager.GetBid(tick.InstrumentId);
                                                if (bid == null)
                                                {
                                                    i++;
                                                    continue;
                                                }
                                                tick = new Tick(obj.dateTime, tick.ProviderId, tick.InstrumentId, (bid.Price + tick.Price) / 2.0, (bid.Size + tick.Size) / 2);
                                                break;
                                            }
                                        case DataObjectType.Trade:
                                            i++;
                                            continue;
                                    }
                                    if (obj.TypeId != DataObjectType.Ask)
                                    {
                                        item.Process(tick);
                                        i++;
                                        continue;
                                    }
                                    break;
                                case BarInput.Tick:
                                    {
                                        item.Process(tick);
                                        i++;
                                        continue;
                                    }
                                case BarInput.BidAsk:
                                    if (tick.TypeId != DataObjectType.Trade)
                                    {
                                        item.Process(tick);
                                        i++;
                                        continue;
                                    }
                                    break;
                                default:
                                    Console.WriteLine($"BarFactory::OnData BarInput is not supported : {item.barInput}");
                                    break;
                            }
                            i++;
                        }
            */
            var tick = (Tick) obj;
            var items = ItemLists[tick.InstrumentId];
            if (items != null)
            {
                int i = 0;
                while (i < items.Count)
                {
                    var item = items[i];
                    switch (item.barInput)
                    {
                        case BarInput.Trade:
                            if (tick.TypeId == DataObjectType.Trade)
                            {
                                goto IL_19C;
                            }
                            break;
                        case BarInput.Bid:
                            if (tick.TypeId == DataObjectType.Bid)
                            {
                                goto IL_19C;
                            }
                            break;
                        case BarInput.Ask:
                            if (tick.TypeId == DataObjectType.Ask)
                            {
                                goto IL_19C;
                            }
                            break;
                        case BarInput.Middle:
                            switch (tick.TypeId)
                            {
                                case DataObjectType.Bid:
                                {
                                    var ask = this.framework.DataManager.GetAsk(tick.InstrumentId);
                                    if (ask == null)
                                    {
                                        goto IL_1A3;
                                    }
                                    tick = new Tick(obj.dateTime, tick.ProviderId, tick.InstrumentId,
                                        (ask.Price + tick.Price)/2.0, (ask.Size + tick.Size)/2);
                                    break;
                                }
                                case DataObjectType.Ask:
                                {
                                    var bid = this.framework.DataManager.GetBid(tick.InstrumentId);
                                    if (bid == null)
                                    {
                                        goto IL_1A3;
                                    }
                                    tick = new Tick(obj.dateTime, tick.ProviderId, tick.InstrumentId,
                                        (bid.Price + tick.Price)/2.0, (bid.Size + tick.Size)/2);
                                    break;
                                }
                                case DataObjectType.Trade:
                                    goto IL_1A3;
                            }
                            if (obj.TypeId != DataObjectType.Ask)
                            {
                                goto IL_19C;
                            }
                            break;
                        case BarInput.Tick:
                            goto IL_19C;
                        case BarInput.BidAsk:
                            if (tick.TypeId != DataObjectType.Trade)
                            {
                                goto IL_19C;
                            }
                            break;
                        default:
                            Console.WriteLine($"BarFactory::OnData BarInput is not supported : {item.barInput}");
                            break;
                    }
                    IL_1A3:
                    i++;
                    continue;
                    IL_19C:
                    item.Process(tick);
                    goto IL_1A3;
                }
            }

        }

        internal bool AddReminder(BarFactoryItem item, DateTime dateTime, ClockType type)
        {
            bool created = false;
            SortedList<long, List<BarFactoryItem>> slistByBarSize;
            if (!this.reminderTable.TryGetValue(dateTime, out slistByBarSize))
            {
                slistByBarSize = new SortedList<long, List<BarFactoryItem>>();
                this.reminderTable.Add(dateTime, slistByBarSize);
                created = true;
            }

            List<BarFactoryItem> list;
            if (!slistByBarSize.TryGetValue(item.barSize, out list))
            {
                list = new List<BarFactoryItem>();
                slistByBarSize.Add(item.barSize, list);
            }

            list.Add(item);

            if (created)
            {
                var clock = type == ClockType.Exchange ? this.framework.ExchangeClock : this.framework.Clock;
                return clock.AddReminder(OnReminder, dateTime);
            }
            return true;
        }

        private void OnReminder(DateTime datetime, object data)
        {
            SortedList<long, List<BarFactoryItem>> sList;
            if (this.reminderTable.TryGetValue(datetime, out sList))
            {
                this.reminderTable.Remove(datetime);
                foreach (var item in sList.Values.SelectMany(list => list))
                    item.OnReminder(datetime);
            }
        }
    }
}
