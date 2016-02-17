// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace FastQuant
{
    public enum StopType
    {
        Fixed,
        Trailing,
        Time
    }

    public enum StopMode
    {
        Absolute,
        Percent
    }

    public enum StopFillMode
    {
        Close,
        HighLow,
        Stop
    }

    public enum StopStatus
    {
        Active,
        Executed,
        Canceled
    }

    public class Stop
    {
        protected internal Strategy strategy;
        protected internal Position position;
        protected internal Instrument instrument;
        protected internal bool connected;

        protected internal StopType type = StopType.Trailing;
        protected internal StopMode mode = StopMode.Percent;
        protected internal StopStatus status;
        protected internal double level;
        protected internal double initPrice;
        protected internal double currPrice;
        protected internal double stopPrice;
        protected internal double fillPrice;
        protected internal double trailPrice;
        protected internal double qty;
        protected internal PositionSide side;
        protected internal DateTime creationTime;
        protected internal DateTime completionTime;
        protected internal bool traceOnQuote = true;
        protected internal bool traceOnTrade = true;
        protected internal bool traceOnBar = true;
        protected internal bool traceOnBarOpen = true;
        protected internal bool trailOnOpen = true;
        protected internal bool trailOnHighLow;
        protected internal long filterBarSize = -1;
        protected internal BarType filterBarType = BarType.Time;
        protected internal StopFillMode fillMode = StopFillMode.Stop;

        protected internal ObjectTable fields;

        public Strategy Strategy => this.strategy;

        public Position Position => this.position;

        public Instrument Instrument => this.instrument;

        public bool Connected => this.connected;

        public StopType Type => this.type;

        public StopMode Mode => this.mode;

        public StopStatus Status => this.status;

        public double Level => this.level;

        public double Qty => this.qty;

        public PositionSide Side => this.side;

        public DateTime CreationTime => this.creationTime;

        public DateTime CompletionTime => this.completionTime;

        public bool TraceOnQuote
        {
            get
            {
                return this.traceOnQuote;
            }
            set
            {
                this.traceOnQuote = value;
            }
        }

        public bool TraceOnTrade
        {
            get
            {
                return this.traceOnTrade;
            }
            set
            {
                this.traceOnTrade = value;
            }
        }

        public bool TraceOnBar
        {
            get
            {
                return this.traceOnBar;
            }
            set
            {
                this.traceOnBar = value;
            }
        }

        public bool TraceOnBarOpen
        {
            get
            {
                return this.traceOnBarOpen;
            }
            set
            {
                this.traceOnBarOpen = value;
            }
        }

        public bool TrailOnOpen
        {
            get
            {
                return this.trailOnOpen;
            }
            set
            {
                this.trailOnOpen = value;
            }
        }

        public bool TrailOnHighLow
        {
            get
            {
                return this.trailOnHighLow;
            }
            set
            {
                this.trailOnHighLow = value;
            }
        }

        public long FilterBarSize
        {
            get
            {
                return this.filterBarSize;
            }
            set
            {
                this.filterBarSize = value;
            }
        }

        public BarType FilterBarType
        {
            get
            {
                return this.filterBarType;
            }
            set
            {
                this.filterBarType = value;
            }
        }

        public StopFillMode FillMode
        {
            get
            {
                return this.fillMode;
            }
            set
            {
                this.fillMode = value;
            }
        }

        public ObjectTable Fields => this.fields = this.fields ?? new ObjectTable();

        public Stop(Strategy strategy, Position position, double level, StopType type, StopMode mode)
            :this(strategy, position, DateTime.MinValue, level, type, mode)
        {
        }

        public Stop(Strategy strategy, Position position, DateTime time)
            :this(strategy, position, time, 0, StopType.Trailing, StopMode.Percent)
        {
        }

        private Stop(Strategy strategy, Position position, DateTime time, double level = 0, StopType type = StopType.Trailing, StopMode mode = StopMode.Percent)
        {
            this.strategy = strategy;
            this.position = position;
            this.instrument = position.Instrument;
            this.qty = position.Qty;
            this.side = position.Side;
            this.type = type;
            this.mode = mode;
            this.creationTime = strategy.framework.Clock.DateTime;
            this.completionTime = time;
            this.stopPrice = GetInstrumentPrice();
            if (this.completionTime > this.creationTime)
                strategy.framework.Clock.AddReminder(new Reminder(this.method_9, this.completionTime, null));
        }

        public void Cancel()
        {
            if (Status != StopStatus.Active)
                return;
            Disconnect();
            OnStopStatusChange(StopStatus.Canceled);
        }

        protected virtual double GetPrice(double price) => price;

        protected virtual double GetInstrumentPrice()
        {
            if (this.position.Side == PositionSide.Long)
            {
                var bid = this.strategy.framework.DataManager.GetBid(this.instrument);
                if (bid != null)
                    return GetPrice(bid.Price);
            }

            if (this.position.Side == PositionSide.Short)
            {
                var ask = this.strategy.framework.DataManager.GetAsk(this.instrument);
                if (ask != null)
                    return GetPrice(ask.Price);
            }

            var trade = this.strategy.framework.DataManager.GetTrade(this.instrument);
            if (trade != null)
                return GetPrice(trade.Price);

            var bar = this.strategy.framework.DataManager.GetBar(this.instrument);
            return bar != null ? GetPrice(bar.Close) : 0;
        }

        protected virtual double GetStopPrice()
        {
            this.initPrice = this.trailPrice;
            if (Mode == StopMode.Absolute)
                return Side == PositionSide.Long ? this.trailPrice - Math.Abs(Level) : this.trailPrice + Math.Abs(Level);
            else
                return Position.Side == PositionSide.Long
                    ? this.trailPrice - Math.Abs(this.trailPrice*Level)
                    : this.trailPrice + Math.Abs(this.trailPrice*Level);
        }

        public void Disconnect()
        {
            if (Type == StopType.Time)
                this.strategy.framework.Clock.RemoveReminder(OnConnect, this.completionTime);
            else
                this.connected = false;
        }

        private void Connect()
        {
            this.connected = true;
        }

        private void OnStopStatusChange(StopStatus status)
        {
            this.status = status;
            this.completionTime = this.strategy.framework.Clock.DateTime;
           // this.strategy.OnStopStatusChanged_(this);
        }

        private void OnConnect(DateTime dateTime, object obj)
        {
            this.stopPrice = this.GetInstrumentPrice();
            OnStopStatusChange(StopStatus.Executed);
        }

        internal void OnBid(Bid bid)
        {
            if (TraceOnQuote && Side == PositionSide.Long)
            {
                this.fillPrice = this.trailPrice = this.currPrice = GetPrice(bid.Price);
                this.method_1();
            }
        }

        internal void OnAsk(Ask ask)
        {
            if (TraceOnQuote && Side == PositionSide.Short)
            {
                this.fillPrice = this.trailPrice = this.currPrice = GetPrice(ask.Price);
                this.method_1();
            }
        }

        internal void OnTrade(Trade trade)
        {
            if (TraceOnTrade)
            {
                this.fillPrice = this.trailPrice = this.currPrice = GetPrice(trade.Price);
                this.method_1();
            }
        }

        internal void OnBarOpen(Bar bar)
        {
            if (TraceOnBar && TraceOnBarOpen && (FilterBarSize < 0 || (FilterBarSize == bar.Size && FilterBarType == BarType.Time)))
            {
                this.fillPrice = this.currPrice = GetPrice(bar.Open);
                if (TrailOnOpen)
                    this.trailPrice = GetPrice(bar.Open);
                this.method_1();
            }
        }

        internal void OnBar(Bar bar)
        {
            if (TraceOnBar && (FilterBarSize < 0 || (FilterBarSize == bar.Size && FilterBarType == BarType.Time)))
            {
                this.trailPrice = GetPrice(bar.Close);
                switch (Side)
                {
                    case PositionSide.Long:
                        this.fillPrice = this.currPrice = GetPrice(bar.Low);
                        if (this.trailOnHighLow)
                            this.trailPrice = GetPrice(bar.High);
                        break;
                    case PositionSide.Short:
                        this.fillPrice = this.currPrice = GetPrice(bar.High);
                        if (this.trailOnHighLow)
                            this.trailPrice = GetPrice(bar.Low);
                        break;
                }
                switch (FillMode)
                {
                    case StopFillMode.Close:
                        this.fillPrice = GetPrice(bar.Close);
                        break;
                    case StopFillMode.Stop:
                        this.fillPrice = this.stopPrice;
                        break;
                }
                this.method_1();
            }
        }

        private void method_1()
        {
            if (this.currPrice == 0)
                return;

            switch (Side)
            {
                case PositionSide.Long:
                    if (this.currPrice <= this.stopPrice)
                    {
                        Disconnect();
                        this.method_8(StopStatus.Executed);
                        return;
                    }
                    if (Type == StopType.Trailing && this.trailPrice > this.initPrice)
                    {
                        this.stopPrice = GetStopPrice();
                        return;
                    }
                    break;
                case PositionSide.Short:
                    if (this.currPrice >= this.stopPrice)
                    {
                        this.Disconnect();
                        this.method_8(StopStatus.Executed);
                        return;
                    }
                    if (Type == StopType.Trailing && this.trailPrice < this.initPrice)
                    {
                        this.stopPrice = this.GetStopPrice();
                    }
                    break;
                default:
                    return;
            }
        }

        private void method_9(DateTime dateTime, object obj)
        {
            this.stopPrice = GetInstrumentPrice();
            this.method_8(StopStatus.Executed);
        }

        private void method_8(StopStatus status)
        {
            this.status = status;
            this.completionTime = this.strategy.framework.Clock.DateTime;
            this.strategy.EmitStopStatusChanged(this);
        }
    }
}

