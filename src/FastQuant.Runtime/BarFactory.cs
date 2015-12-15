// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        public int ProviderId { get; set; }

        protected BarFactoryItem(Instrument instrument, BarType barType, long barSize, BarInput barInput = BarInput.Trade, int providerId = -1)
        {
            this.instrument = instrument;
            this.barType = barType;
            this.barSize = barSize;
            this.barInput = barInput;
            this.providerId = providerId;
        }

        protected BarFactoryItem(Instrument instrument, BarType barType, long barSize, BarInput barInput, TimeSpan session1, TimeSpan session2, int providerId = -1)
        {
            this.factory = null;
            this.instrument = instrument;
            this.barType = barType;
            this.barSize = barSize;
            this.barInput = barInput;
            this.sessionEnabled = true;
            this.session1 = session1;
            this.session2 = session2;
            this.providerId = providerId;
        }

        //TODO: rewrite!
        protected virtual bool InSession(DateTime dateTime)
        {
            if (this.sessionEnabled)
            {
                TimeSpan timeOfDay = dateTime.TimeOfDay;
                if (timeOfDay < this.session1 || timeOfDay > this.session2)
                    return false;
            }
            return true;
        }

        protected virtual void OnData(DataObject obj)
        {
            var tick = obj as Tick;
            if (this.bar == null)
            {
                this.bar = new Bar();
                this.bar.InstrumentId = tick.InstrumentId;
                this.bar.Type = this.barType;
                this.bar.Size = this.barSize;
                this.bar.OpenDateTime = GetBarOpenDateTime(obj);
                this.bar.DateTime = this.GetDataObjectDateTime(obj, ClockType.Local);
                this.bar.Open = tick.Price;
                this.bar.High = tick.Price;
                this.bar.Low = tick.Price;
                this.bar.Close = tick.Price;
                this.bar.Volume = tick.Size;
                this.bar.Status = BarStatus.Open;
                this.factory.framework.EventServer.OnEvent(this.bar);
            }
            else
            {
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
            // do nothing
        }

        protected virtual DateTime GetBarOpenDateTime(DataObject obj)
        {
            return obj.DateTime;
        }

        protected virtual DateTime GetBarCloseDateTime(DataObject obj)
        {
            return obj.DateTime;
        }

        protected virtual DateTime GetDataObjectDateTime(DataObject obj, ClockType type)
        {
            return type == ClockType.Local ? obj.DateTime : (obj as Tick).ExchangeDateTime;
        }

        protected bool AddReminder(DateTime datetime, ClockType type)
        {
            throw new NotImplementedException();
        }

        protected void EmitBar()
        {
            this.bar.Status = BarStatus.Close;
            this.factory.framework.EventServer.OnEvent(this.bar);
            this.bar = null;
        }

        public override string ToString() => $"{this.instrument.Symbol} {this.barType} {this.barSize} {this.barInput}";
    }

    public class TimeBarFactoryItem : BarFactoryItem
    {
        private ClockType type;

        public TimeBarFactoryItem(Instrument instrument, long barSize, int providerId = -1): base(instrument, BarType.Time, barSize, BarInput.Trade, providerId)
        {
        }

        public TimeBarFactoryItem(Instrument instrument, long barSize, ClockType type = ClockType.Local, int providerId = -1)
        : base(instrument, BarType.Time, barSize, BarInput.Trade, providerId)
        {
            this.type = type;
        }

        public TimeBarFactoryItem(Instrument instrument, long barSize, BarInput barInput = BarInput.Trade, int providerId = -1): base(instrument, BarType.Time, barSize, barInput, providerId)
        {
        }

        public TimeBarFactoryItem(Instrument instrument, long barSize, BarInput barInput = BarInput.Trade, ClockType type = ClockType.Local, int providerId = -1): base(instrument, BarType.Time, barSize, barInput, providerId)
        {
            this.type = type;
        }

        public TimeBarFactoryItem(Instrument instrument, long barSize, BarInput barInput, TimeSpan session1, TimeSpan session2, int providerId = -1): base(instrument, BarType.Tick, barSize, barInput, session1, session2, providerId)
        {
        }

        // Token: 0x06000050 RID: 80 RVA: 0x00002698 File Offset: 0x00000898
        public TimeBarFactoryItem(Instrument instrument, long barSize, BarInput barInput, ClockType type, TimeSpan session1, TimeSpan session2, int providerId = -1) : base(instrument, BarType.Time, barSize, barInput, session1, session2, providerId)
        {   
            this.type = type;
        }

        protected override void OnData(DataObject obj)
        {
            base.OnData(obj);
            if (this.bar != null || AddReminder(GetBarCloseDateTime(obj), this.type))
                return;
            if (obj is Tick)
                Console.WriteLine("TimeBarFactoryItem::OnData Can not add reminder. Clock = {0} local datetime = {1} exchange dateTime = {2} object = {3} object exchange datetime = {4} reminder datetime = {5}", this.type, this.factory.framework.Clock.DateTime, this.factory.framework.ExchangeClock.DateTime, obj, (obj as Tick).ExchangeDateTime, GetBarCloseDateTime(obj));
            else
                Console.WriteLine("TimeBarFactoryItem::OnData Can not add reminder. Object is not tick! Clock = {0} local datetime = {1} exchange datetime = {2} object = {3} reminder datetime = {4}", this.type, this.factory.framework.Clock.DateTime, this.factory.framework.ExchangeClock.DateTime, obj, GetBarCloseDateTime(obj));
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
            this.bar.DateTime = this.type == ClockType.Local ? datetime : this.factory.framework.Clock.DateTime;
            EmitBar();
        }
    }

    public class TickBarFactoryItem : BarFactoryItem
    {
        public TickBarFactoryItem(Instrument instrument, long barSize, BarInput barInput = BarInput.Trade, int providerId = -1) : base(instrument, BarType.Tick, barSize, barInput, providerId)
        { 
        }

        public TickBarFactoryItem(Instrument instrument, long barSize, BarInput barInput, TimeSpan session1, TimeSpan session2, int providerId = -1): base(instrument, BarType.Tick, barSize, barInput, session1, session2, providerId)
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
        public RangeBarFactoryItem(Instrument instrument, long barSize, BarInput barInput = BarInput.Trade, int providerId = -1) : base(instrument, BarType.Range, barSize, barInput, providerId)
        {
        }

        public RangeBarFactoryItem(Instrument instrument, long barSize, BarInput barInput, TimeSpan session1, TimeSpan session2, int providerId = -1) : base(instrument, BarType.Tick, barSize, barInput, session1, session2, providerId)
        {
        }

        protected override void OnData(DataObject obj)
        {
            base.OnData(obj);
            if (this.bar.N == this.barSize)
                EmitBar();
        }
    }

    public class VolumeBarFactoryItem : BarFactoryItem
    {
        public VolumeBarFactoryItem(Instrument instrument, long barSize, BarInput barInput = BarInput.Trade, int providerId = -1) : base(instrument, BarType.Volume, barSize, barInput, providerId)
        {
        }

        public VolumeBarFactoryItem(Instrument instrument, long barSize, BarInput barInput, TimeSpan session1, TimeSpan session2, int providerId = -1) : base(instrument, BarType.Tick, barSize, barInput, session1, session2, providerId)

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

        public SessionBarFactoryItem(Instrument instrument, BarInput barInput, TimeSpan session1, TimeSpan session2, int providerId = -1) : base(instrument, BarType.Session, (long)(session2 - session1).TotalSeconds, barInput, session1, session2, providerId)
        {
        }

        public SessionBarFactoryItem(Instrument instrument, BarInput barInput, ClockType type, TimeSpan session1, TimeSpan session2, int providerId = -1) : base(instrument, BarType.Session, (long)(session2 - session1).TotalSeconds, barInput, session1, session2, providerId)
        {
            this.type = type;
        }

        protected override void OnData(DataObject obj)
        {
            bool flag = this.bar == null;
            base.OnData(obj);
            if (!flag || AddReminder(GetBarCloseDateTime(obj), this.type))
                return;
            if (!(obj is Tick))
                Console.WriteLine("SessionBarFactoryItem::OnData Can not add reminder. Object is not tick! Clock = {0} local datetime = {1} exchange datetime = {2} object = {3} reminder datetime = {4} ", this.type, this.factory.framework.Clock.DateTime, this.factory.framework.ExchangeClock.DateTime, obj, GetBarCloseDateTime(obj));
            else
                Console.WriteLine("SessionBarFactoryItem::OnData Can not add reminder. Clock = {0} local datetime = {1} exchange dateTime = {2} object = {3} object exchange datetime = {4} reminder datetime = {5}", this.type, this.factory.framework.Clock.DateTime, this.factory.framework.ExchangeClock.DateTime, obj, ((Tick)obj).ExchangeDateTime, GetBarCloseDateTime(obj));
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
            if (this.type == ClockType.Local)
                this.bar.DateTime = datetime;
            else
                this.bar.DateTime = this.factory.framework.Clock.DateTime;
            EmitBar();
        }
    }

    public class BarFactory
    {
        internal Framework framework;

        public IdArray<List<BarFactoryItem>> ItemLists { get; private set; }

        public BarFactory(Framework framework)
        {
            this.framework = framework;
            ItemLists = new IdArray<List<BarFactoryItem>>(8192);
        }

        public void Add(BarFactoryItem item)
        {
            if (item.factory != null)
                throw new InvalidOperationException("BarFactoryItem is already added to another BarFactory instance.");
            item.factory = this;
            int id = item.instrument.Id;
            var items = ItemLists[id];
            if (items == null)
            {
                items = new List<BarFactoryItem>();
                ItemLists[id] = items;
            }
            if (items.Exists(match => item.barType == match.barType && item.barSize == match.barSize && item.barInput == match.barInput))
                Console.WriteLine("{0} BarFactory::Add Item '{1}' is already added", DateTime.Now, item);
            else
                items.Add(item); 
        }

        public void Add(string symbol, BarType barType, long barSize, BarInput barInput = BarInput.Trade, ClockType type = ClockType.Local)
        {
            Add(this.framework.InstrumentManager.Get(symbol), barType, barSize, barInput, type);
        }

        public void Add(Instrument instrument, BarType barType, long barSize, BarInput barInput = BarInput.Trade, ClockType type = ClockType.Local)
        {
            BarFactoryItem item;
            switch (barType)
            {
                case BarType.Time:
                    item = new TimeBarFactoryItem(instrument, barSize, barInput, type);
                    break;
                case BarType.Tick:
                    item = new TickBarFactoryItem(instrument, barSize, barInput);
                    break;
                case BarType.Volume:
                    item = new VolumeBarFactoryItem(instrument, barSize, barInput);
                    break;
                default:
                    throw new ArgumentException(string.Format("Unknown bar type - {0}", barType));
            }
            Add(item);
        }

        public void Add(Instrument instrument, BarType barType, long barSize, BarInput barInput, ClockType type, TimeSpan session1, TimeSpan session2)
        {
            BarFactoryItem barFactoryItem;
            switch (barType)
            {
                case BarType.Time:
                    barFactoryItem = new TimeBarFactoryItem(instrument, barSize, barInput, type, session1, session2);
                    break;
                case BarType.Tick:
                    barFactoryItem = new TickBarFactoryItem(instrument, barSize, barInput, session1, session2);
                    break;
                case BarType.Volume:
                    barFactoryItem = new VolumeBarFactoryItem(instrument, barSize, barInput, session1, session2);
                    break;
                case BarType.Range:
                    barFactoryItem = new RangeBarFactoryItem(instrument, barSize, barInput, session1, session2);
                    break;
                default:
                    throw new ArgumentException(string.Format("Unknown bar type - {0}", barType));
            }
            Add(barFactoryItem);
        }

        public void Add(InstrumentList instruments, BarType barType, long barSize, BarInput barInput = BarInput.Trade, ClockType type = ClockType.Local)
        {
            foreach (var i in instruments)
                Add(i, barType, barSize, barInput, type);
        }

        public void Add(string[] symbols, BarType barType, long barSize, BarInput barInput = BarInput.Trade, ClockType type = ClockType.Local)
        {
            foreach (var s in symbols)
                Add(this.framework.InstrumentManager.Get(s), barType, barSize, barInput, type);
        }

        public void Add(Instrument instrument, BarType barType, long barSize, TimeSpan session1, TimeSpan session2)
        {
            Add(instrument, barType, barSize, BarInput.Trade, ClockType.Local, session1, session2);
        }

        public void Add(string symbol, BarType barType, long barSize, BarInput barInput, ClockType type, TimeSpan session1, TimeSpan session2)
        {
            Add(this.framework.InstrumentManager[symbol], barType, barSize, barInput, type, session1, session2);
        }

        public void Add(string symbol, BarType barType, long barSize, TimeSpan session1, TimeSpan session2)
        {
            Add(this.framework.InstrumentManager[symbol], barType, barSize, BarInput.Trade, ClockType.Local, session1, session2);
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        internal void OnData(DataObject obj)
        {
            throw new NotImplementedException();
        }
    }
}
