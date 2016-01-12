// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace SmartQuant
{
    public class Strategy
    {
        protected internal Framework framework;

        protected internal bool raiseEvents;
        private ParameterHelper parameterHelper = new ParameterHelper();
        private Portfolio portfolio;

        public int Id { get; }
        public int ClientId { get; set; }
        public string Name { get; set; }
        public bool Enabled { get; set; }
        public Strategy Parent { get; private set; }
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
        public BarFactory BarFactory => this.framework.EventManager.BarFactory;

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

        public Reminder AddReminder(DateTime dateTime, object data = null)
        {
            return this.framework.Clock.AddReminderWithHandler(OnReminder, dateTime, data);
        }

        public Reminder AddExchangeReminder(DateTime dateTime, object data = null)
        {
            return this.framework.ExchangeClock.AddReminderWithHandler(OnExchangeReminder, dateTime, data);
        }

        #region Instrument Management

        public void AddInstrument(string symbol)
        {
            var instrument = this.framework.InstrumentManager.Get(symbol);
            if (instrument != null)
                AddInstrument(instrument);
            else
                Console.WriteLine($"Strategy::AddInstrument instrument with symbol {symbol} doesn't exist in the framework");
        }

        public void AddInstrument(int id)
        {
            var instrument = this.framework.InstrumentManager.GetById(id);
            if (instrument != null)
                AddInstrument(instrument);
            else
                Console.WriteLine($"Strategy::AddInstrument instrument with Id {id} doesn't exist in the framework");
        }

        public virtual void AddInstrument(Instrument instrument)
        {
            AddInstrument(instrument, null);
        }

        public void AddInstrument(Instrument instrument, IDataProvider provider)
        {
            throw new NotImplementedException();
        }

        public void AddInstruments(string[] symbols)
        {
            foreach (var symbol in symbols)
                AddInstrument(symbol);
        }

        public void AddInstruments(InstrumentList instruments)
        {
            foreach (var instrument in instruments)
                AddInstrument(instrument);
        }

        public void RemoveInstrument(string symbol)
        {
            var instrument = this.framework.InstrumentManager.Get(symbol);
            if (instrument != null)
                RemoveInstrument(instrument);
            else
                Console.WriteLine($"Strategy::RemoveInstrument instrument with symbol {symbol} doesn't exist in the framework");
        }

        public void RemoveInstrument(int id)
        {
            var instrument = this.framework.InstrumentManager.GetById(id);
            if (instrument != null)
                RemoveInstrument(instrument);
            else
                Console.WriteLine($"Strategy::RemoveInstrument instrument with Id {id} doesn't exist in the framework");
        }

        public virtual void RemoveInstrument(Instrument instrument)
        {
            throw new NotImplementedException();
        }

        public void RemoveInstrument(Instrument instrument, IDataProvider dataProvider)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Strategy Management

        public void AddStrategy(Strategy strategy)
        {
            AddStrategy(strategy, true);
        }

        public void AddStrategy(Strategy strategy, bool callOnStrategyStart)
        {
            throw new NotImplementedException();
        }

        public void RemoveStrategy(Strategy strategy)
        {
            // noop
        }

        #endregion

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

        public string GetModeAsString() => Mode == StrategyMode.Backtest ? "Backtest" : Mode == StrategyMode.Paper ? "Paper" : Mode == StrategyMode.Live ? "Live" : "Undefined";

        public void SetParameter(string name, object value) => this.parameterHelper.SetStrategyParameter(name, this, value);

        public object GetParameter(string name) => this.parameterHelper.GetStrategyParameter(name, this);

        public ParameterList GetParameters() => this.parameterHelper.GetStrategyParameters(Name, this);

        public bool ExecuteMethod(string methodName)
        {
            var methods = GetType().GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
            var method = methods.FirstOrDefault(m => m.GetCustomAttributes(typeof(StrategyMethodAttribute), true).Any() && m.GetParameters().Length == 0 && m.Name == methodName);
            method?.Invoke(this, null);
            return method != null;
        }

        public virtual double Objective() => this.portfolio.Value;

        public Order Order(Instrument instrument, OrderType type, OrderSide side, double qty, double stopPx, double price, string text = "")
        {
            throw new NotImplementedException();
            //var order = new Order(this.method_5(instrument), this.portfolio, instrument, type, side, qty, 0.0, 0.0, TimeInForce.Day, 0, "");
            //order.StrategyId = Id;
            //order.ClientId = ClientId;
            //order.Text = text;
            //order.StopPx = stopPx;
            //order.Price = price;
            //this.framework.OrderManager.Register(order);
            //return order;
        }

        protected internal virtual void OnStrategyInit()
        {
        }

        protected internal virtual void OnStrategyStart()
        {
        }

        protected virtual void OnStrategyStop()
        {
        }

        protected virtual void OnReminder(DateTime dateTime, object data)
        {
        }

        protected virtual void OnExchangeReminder(DateTime dateTime, object data)
        {
        }

        protected virtual void OnBid(Instrument instrument, Bid bid)
        {
        }

        protected virtual void OnAsk(Instrument instrument, Ask ask)
        {
        }

        protected virtual void OnBarOpen(Instrument instrument, Bar bar)
        {
        }

        protected virtual void OnBar(Instrument instrument, Bar bar)
        {
        }

        protected virtual void OnBarSlice(BarSlice slice)
        {
        }

        protected virtual void OnExecutionReport(ExecutionReport report)
        {
        }

        protected virtual void OnFill(Fill fill)
        {
        }

        protected virtual void OnTrade(Instrument instrument, Trade trade)
        {
        }

        protected virtual void OnTransaction(Transaction transaction)
        {
        }

        protected virtual void OnPositionOpened(Position position)
        {
        }

        protected virtual void OnPositionChanged(Position position)
        {
        }

        protected virtual void OnPositionClosed(Position position)
        {
        }

        protected virtual void OnNewOrder(Order order)
        {
        }

        protected virtual void OnOrderCancelled(Order order)
        {
        }

        protected virtual void OnOrderCancelRejected(Order order)
        {
        }

        protected virtual void OnOrderDone(Order order)
        {
        }

        protected virtual void OnOrderExpired(Order order)
        {
        }

        protected virtual void OnOrderFilled(Order order)
        {
        }

        protected virtual void OnOrderPartiallyFilled(Order order)
        {
        }

        protected virtual void OnOrderRejected(Order order)
        {
        }

        protected virtual void OnOrderReplaced(Order order)
        {
        }

        protected virtual void OnOrderReplaceRejected(Order order)
        {
        }

        protected virtual void OnOrderStatusChanged(Order order)
        {
        }

        protected virtual void OnStopCancelled(Stop stop)
        {
        }

        protected virtual void OnStopExecuted(Stop stop)
        {
        }

        protected virtual void OnStopStatusChanged(Stop stop)
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

        public bool IsConnecting => false;

        public bool IsDisconnecting => false;

        public SellSideStrategy(Framework framework, string name) : base(framework, name)
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

        public virtual void EmitAsk(Ask ask)
        {
            this.framework.EventManager.OnEvent(new Ask(ask) { ProviderId = (byte)Id });
        }

        public virtual void EmitAsk(DateTime dateTime, int instrumentId, double price, int size)
        {
            this.framework.EventManager.OnEvent(new Ask(dateTime, (byte)Id, instrumentId, price, size));
        }

        public virtual void EmitBid(Bid bid)
        {
            this.framework.EventManager.OnEvent(new Bid(bid) { ProviderId = (byte)Id });
        }

        public virtual void EmitBid(DateTime dateTime, int instrumentId, double price, int size)
        {
            this.framework.EventManager.OnEvent(new Bid(dateTime, (byte)Id, instrumentId, price, size));
        }

        public virtual void EmitTrade(Trade trade)
        {
            this.framework.EventManager.OnEvent(new Trade(trade) { ProviderId = (byte)Id });
        }

        public virtual void EmitTrade(DateTime dateTime, int instrumentId, double price, int size)
        {
            this.framework.EventManager.OnEvent(new Trade(dateTime, (byte)Id, instrumentId, price, size));
        }

        public virtual void EmitBar(Bar bar)
        {
            this.framework.EventManager.OnEvent(bar);
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