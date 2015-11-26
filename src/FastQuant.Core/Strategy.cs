// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace SmartQuant
{
    public class StrategyEventArgs : EventArgs
    {
    }

    public enum StrategyMode
    {
        Backtest = 1,
        Paper,
        Live
    }

    public enum StrategyPersistence
    {
        None,
        Full,
        Save,
        Load
    }

    public enum StrategyStatus
    {
        Running,
        Stopped
    }

    public enum StrategyStatusType : byte
    {
        Started,
        Stopped
    }

    public class StrategyMethodAttribute : Attribute
    {
    }

    public class Strategy
    {
        private Framework framework;
        protected internal bool raiseEvents;

        public int Id { get; }
        public int ClientId { get; set; }
        public string Name { get; set; }
        public bool Enabled { get; set; }
        public Strategy Parent { get; private set;  }
        public StrategyStatus Status { get; private set; }
        public LinkedList<Strategy> Strategies { get; private set; }
        public Portfolio Portfolio { get; private set; }
        public TickSeries Bids { get; private set; }
        public TickSeries Asks { get; private set; }
        public BarSeries Bars { get; private set; }
        public TimeSeries Equity { get; private set; }
        public InstrumentList Instruments { get; private set; }
        public Clock Clock => this.framework.Clock;
        public StrategyMode Mode => this.framework.StrategyManager.Mode;
        public Global Global => this.framework.StrategyManager.Global;
        public OrderManager OrderManager => this.framework.OrderManager;
        public InstrumentManager InstrumentManager => this.framework.InstrumentManager;
        public GroupManager GroupManager => this.framework.GroupManager;
        public DataManager DataManager => this.framework.DataManager;
        public AccountDataManager AccountDataManager => this.framework.AccountDataManager;
        public EventManager EventManager => this.framework.EventManager;
        public ProviderManager ProviderManager => this.framework.ProviderManager;
        public StrategyManager StrategyManager => this.framework.StrategyManager;

        public IDataProvider DataProvider { get; set; }
        public IExecutionProvider ExecutionProvider { get; set; }

        public virtual void Init()
        {
            
        }

        public void AddInstrument(Instrument instrument1)
        {
            throw new NotImplementedException();
        }

        public Order BuyLimitOrder(Instrument instrument, double qty, double price, string text = "")
        {
            throw new NotImplementedException();
        }

        public Order SellLimitOrder(Instrument instrument, double qty, double price, string text = "")
        {
            throw new NotImplementedException();
        }

        public Order BuyOrder(Instrument instrument, double qty, string text = "")
        {
            throw new NotImplementedException();
        }

        public void Cancel(Order order)
        {
            throw new NotImplementedException();
        }

        public void Send(Order order)
        {
            throw new NotImplementedException();
        }

        public bool HasPosition(Instrument instrument)
        {
            return Portfolio.HasPosition(instrument);
        }

        public Order SellOrder(Instrument instrument, double qty, string text = "")
        {
            throw new NotImplementedException();
        }

        public void Log(DataObject data, Group group)
        {
            throw new NotImplementedException();
        }

        public void Log(double value, Group group)
        {
            throw new NotImplementedException();
        }

        protected internal virtual void OnStrategyInit()
        {
        }

        protected internal virtual void OnStrategyStart()
        {
        }

        protected internal virtual void OnStrategyStop()
        {
        }

        protected internal virtual void OnBid(Instrument instrument, Bid bid)
        {
        }

        protected internal virtual void OnAsk(Instrument instrument, Ask ask)
        {
        }

        protected internal virtual void OnBarOpen(Instrument instrument, Bar bar)
        {
        }

        protected internal virtual void OnBar(Instrument instrument, Bar bar)
        {
        }

        protected internal virtual void OnBarSlice(BarSlice slice)
        {
        }

        protected internal virtual void OnFill(Fill fill)
        {
        }

        protected internal virtual void OnTrade(Instrument instrument, Trade trade)
        {
        }

        protected internal virtual void OnTransaction(Transaction transaction)
        {
        }

        protected internal virtual void OnPositionOpened(Position position)
        {
        }

        protected internal virtual void OnPositionChanged(Position position)
        {
        }

        protected internal virtual void OnPositionClosed(Position position)
        {
        }

    }

    public class MetaStrategy : Strategy
    {
        public MetaStrategy(Framework framework, string name)
            : base(framework, name)
        {
        }

        public void Add(Strategy strategy)
        {
        }
    }

    public class InstrumentStrategy : Strategy
    {
        public Instrument Instrument { get; }
        public bool IsInstance { get; }
        public Position Position => Portfolio.GetPosition(Instrument);

        public InstrumentStrategy(Framework framework, string name):base(framework, name)
        {
            this.raiseEvents = false;
        }
        public override void Init()
        {
        }
}