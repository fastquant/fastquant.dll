// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace SmartQuant
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
        protected internal StopType type;
        protected internal StopMode mode;
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
        protected internal bool traceOnQuote;
        protected internal bool traceOnTrade;
        protected internal bool traceOnBar;
        protected internal bool traceOnBarOpen;
        protected internal bool trailOnOpen;
        protected internal bool trailOnHighLow;
        protected internal long filterBarSize;
        protected internal BarType filterBarType;
        protected internal StopFillMode fillMode;
        protected internal ObjectTable fields;

        public Strategy Strategy
        {
            get
            {
                return this.strategy;
            }
        }

        public Position Position
        {
            get
            {
                return this.position;
            }
        }

        public Instrument Instrument
        {
            get
            {
                return this.instrument;
            }
        }

        public bool Connected
        {
            get
            {
                return this.connected;
            }
        }

        public StopType Type
        {
            get
            {
                return this.type;
            }
        }

        public StopMode Mode
        {
            get
            {
                return this.mode;
            }
        }

        public StopStatus Status
        {
            get
            {
                return this.status;
            }
        }

        public double Level
        {
            get
            {
                return this.level;
            }
        }

        public double Qty
        {
            get
            {
                return this.qty;
            }
        }

        public PositionSide Side
        {
            get
            {
                return this.side;
            }
        }

        public DateTime CreationTime
        {
            get
            {
                return this.creationTime;
            }
        }

        public DateTime CompletionTime
        {
            get
            {
                return this.completionTime;
            }
        }

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

        public ObjectTable Fields
        {
            get
            {
                if (this.fields == null)
                    this.fields = new ObjectTable();
                return this.fields;
            }
        }

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
            TraceOnQuote = true;
            TraceOnTrade = true;
            TraceOnBar = true;
            TraceOnBarOpen = true;
            TrailOnOpen = true;
            FilterBarSize = -1;
            FilterBarType = BarType.Time;
            FillMode = StopFillMode.Stop;

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
            if (time == null)
            {
            }
        }

        public void Cancel()
        {
            if (Status != StopStatus.Active)
                return;
            Disconnect();
            OnStopStatusChange(StopStatus.Canceled);
        }

        protected virtual double GetPrice(double price)
        {
            return price;
        }

        protected virtual double GetInstrumentPrice()
        {
            throw new NotImplementedException(); 
        }

        protected virtual double GetStopPrice()
        {
            throw new NotImplementedException(); 
        }

        public void Disconnect()
        {
            if (Type == StopType.Time)
                this.strategy.framework.Clock.RemoveReminder(new ReminderCallback(OnConnect), this.completionTime);
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

        internal void method_6(Ask ask)
        {
            if (TraceOnQuote && this.side == PositionSide.Short)
            {
                this.currPrice = GetPrice(ask.Price);
                this.fillPrice = this.currPrice;
                this.trailPrice = this.currPrice;
                this.method_1();
            }
        }

        private void method_1()
        {
            throw new NotImplementedException();
        }

        internal void method_5(Bid bid)
        {
            if (this.traceOnQuote && this.side == PositionSide.Long)
            {
                this.currPrice = this.GetPrice(bid.Price);
                this.fillPrice = this.currPrice;
                this.trailPrice = this.currPrice;
                this.method_1();
            }
        }
    }
}

