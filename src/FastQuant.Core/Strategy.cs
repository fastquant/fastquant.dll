// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using System.Collections.Generic;

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
        protected internal Framework framework;

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

        public Strategy(Framework framework, string name)
        {
            this.framework = framework;
            Name = name;
        }

        public virtual void Init()
        {
            
        }

        public void AddInstruments(InstrumentList instruments)
        {
            throw new NotImplementedException();
        }

        public void AddInstrument(Instrument instrument1)
        {
            throw new NotImplementedException();
        }

        public void AddStop(Stop stop)
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

        public Order Buy(Instrument instrument, double qty, string text = "")
        {
            throw new NotImplementedException();
        }

        public Order BuyOrder(Instrument instrument, double qty, string text = "")
        {
            throw new NotImplementedException();
        }

        public Order BuyStopOrder(Instrument instrument, double qty, double stopPx, string text = "")
        {
            throw new NotImplementedException();
        }

        public Order Sell(Instrument instrument, double qty, string text = "")
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

        public bool HasPosition(Instrument instrument, PositionSide side, double qty)
        {
            return Portfolio.HasPosition(instrument, side, qty);
        }

        public bool HasLongPosition(Instrument instrument)
        {
            return Portfolio.HasLongPosition(instrument);
        }

        public bool HasLongPosition(Instrument instrument, double qty)
        {
            return Portfolio.HasLongPosition(instrument, qty);
        }

        public bool HasShortPosition(Instrument instrument)
        {
            return Portfolio.HasShortPosition(instrument);
        }

        public bool HasShortPosition(Instrument instrument, double qty)
        {
            return Portfolio.HasShortPosition(instrument, qty);
        }

        public Order SellOrder(Instrument instrument, double qty, string text = "")
        {
            throw new NotImplementedException();
        }

        public Order SellStopOrder(Instrument instrument, double qty, double stopPx, string text = "")
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

        public void Log(DateTime dateTime, double value, Group group)
        {
            throw new NotImplementedException();
        }

        public void Log(DateTime dateTime, string text, Group group)
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

        protected internal virtual void OnNewOrder(Order order)
        {
        }

        protected internal virtual void OnOrderCancelled(Order order)
        {
        }

        protected internal virtual void OnOrderCancelRejected(Order order)
        {
        }

        protected internal virtual void OnOrderDone(Order order)
        {
        }

        protected internal virtual void OnOrderExpired(Order order)
        {
        }

        protected internal virtual void OnOrderFilled(Order order)
        {
        }

        protected internal virtual void OnOrderPartiallyFilled(Order order)
        {
        }

        protected internal virtual void OnOrderRejected(Order order)
        {
        }

        protected internal virtual void OnOrderReplaced(Order order)
        {
        }

        internal double Objective()
        {
            throw new NotImplementedException();
        }

        protected internal virtual void OnOrderReplaceRejected(Order order)
        {
        }

        protected internal virtual void OnOrderStatusChanged(Order order)
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

        public InstrumentStrategy(Framework framework, string name) : base(framework, name)
        {
            this.raiseEvents = false;
        }

        public override void Init()
        {
        }
    }

    public class SellSideStrategy : Strategy, IDataProvider, IExecutionProvider
    {
        [ReadOnly(true)]
        public virtual int AlgoId => -1;

        public bool IsConnected => true;
        public bool IsDisconnected => false;

        public new ProviderStatus Status { get; set; }
        public bool IsInstance { get; }

        byte IProvider.Id
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public SellSideStrategy(Framework framework, string name): base(framework, name)
        {
        }

        public virtual void Connect()
        {
            Console.WriteLine("SellSideStrategy::Connect");
        }

        public virtual bool Connect(int timeout)
        {
            throw new NotImplementedException();
        }

        public virtual void Disconnect()
        {
            Console.WriteLine("SellSideStrategy::Disconnect");
        }

        public virtual void EmitExecutionReport(ExecutionReport report)
        {
            throw new NotImplementedException();
        }

        public virtual void OnCancelCommand(ExecutionCommand command)
        {
        }

        public virtual void OnReplaceCommand(ExecutionCommand command)
        {
        }

        public virtual void OnSendCommand(ExecutionCommand command)
        {
        }

        protected virtual void OnSubscribe(InstrumentList instruments)
        {
        }

        protected virtual void OnSubscribe(Instrument instrument)
        {
        }

        protected virtual void OnUnsubscribe(InstrumentList instruments)
        {
        }

        protected virtual void OnUnsubscribe(Instrument instrument)
        {
        }

        public void Subscribe(Instrument instrument)
        {
            throw new NotImplementedException();
        }

        public void Subscribe(InstrumentList instrument)
        {
            throw new NotImplementedException();
        }

        public void Unsubscribe(Instrument instrument)
        {
            throw new NotImplementedException();
        }

        public void Unsubscribe(InstrumentList instrument)
        {
            throw new NotImplementedException();
        }

        public void Send(ExecutionCommand command)
        {
            throw new NotImplementedException();
        }
    }

    public class SellSideInstrumentStrategy : SellSideStrategy
    {
        public Instrument Instrument { get; set; }

        public SellSideInstrumentStrategy(Framework framework, string name) : base(framework, name)
        {
            this.raiseEvents = false;
        }
    }
}