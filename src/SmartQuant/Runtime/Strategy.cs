// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Collections;

namespace SmartQuant
{
    public class StrategyList : IEnumerable<Strategy>
    {
        private GetByList<Strategy> strategies = new GetByList<Strategy>("Id", "Name");

        public void Add(Strategy strategy)
        {
            if (!Contains(strategy.Id))
                this.strategies.Add(strategy);
            else
                Console.WriteLine($"StrategyList::Add strategy {strategy.Name} with Id = {strategy.Id} is already in the list");
        }

        public void Clear() => this.strategies.Clear();

        public bool Contains(Strategy strategy) => this.strategies.Contains(strategy);

        public bool Contains(int id) => this.strategies.Contains(id);

        public Strategy GetById(int id) => this.strategies.GetById(id);

        public Strategy GetByIndex(int index) => this.strategies.GetByIndex(index);

        public IEnumerator<Strategy> GetEnumerator() => this.strategies.GetEnumerator();

        public void Remove(Strategy strategy) => this.strategies.Remove(strategy);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int Count => this.strategies.Count;

        public Strategy this[int index]
        {
            get
            {
                return this.strategies[index];
            }
            set
            {
                this.strategies[index] = value;
            }
        }
    }

    public class Strategy
    {
        private IdArray<LinkedList<Strategy>> idArray_0 = new IdArray<LinkedList<Strategy>>(10240);
        private IdArray<Strategy> idArray_1 = new IdArray<Strategy>(102400);
        private IdArray<int> idArray_2 = new IdArray<int>(10240);
        private IdArray<List<Stop>> idArray_3 = new IdArray<List<Stop>>(10240);

        protected internal Framework framework;

        private IDataProvider idataProvider_0;
        private IExecutionProvider ginterface3_0;
        private IFundamentalProvider ginterface1_0;

        protected internal bool raiseEvents;
        private ParameterHelper parameterHelper = new ParameterHelper();
        private Portfolio portfolio;

        public int Id { get; internal set; }
        public int ClientId { get; set; }
        public string Name { get; set; }
        public bool Enabled { get; set; }
        public Strategy Parent { get; private set; }
        public StrategyStatus Status { get; private set; }
        public Portfolio Portfolio { get; private set; }

        #region Datapart
        public TickSeries Bids { get; } = new TickSeries("", "");
        public TickSeries Asks { get; } = new TickSeries("", "");
        public BarSeries Bars { get; } = new BarSeries("", "", -1, -1);
        public TimeSeries Equity { get; } = new TimeSeries();
        #endregion

        public InstrumentList Instruments { get; } = new InstrumentList();
        public LinkedList<Strategy> Strategies { get; } = new LinkedList<Strategy>();

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

        #region Fetch Provider Section

        public virtual IDataProvider DataProvider
        {
            get
            {
                return this.method_6(this, null);
            }
            set
            {
                this.idataProvider_0 = value;
                for (var s = Strategies.First; s != null; s = s.Next)
                    s.Data.DataProvider = this.idataProvider_0;
            }
        }

        public virtual IExecutionProvider ExecutionProvider
        {
            get
            {
                return this.method_5(null);
            }
            set
            {
                this.ginterface3_0 = value;
                for (var s = Strategies.First; s != null; s = s.Next)
                    s.Data.ExecutionProvider = this.ginterface3_0;
            }
        }

        public virtual IFundamentalProvider FundamentalProvider
        {
            get
            {
                return this.ViqNiQdFkq(null);
            }
            set
            {
                this.ginterface1_0 = value;
                for (var s = Strategies.First; s != null; s = s.Next)
                    s.Data.FundamentalProvider = this.ginterface1_0;
            }
        }

        public IDataSimulator DataSimulator => ProviderManager.DataSimulator;

        public IExecutionSimulator ExecutionSimulator => ProviderManager.ExecutionSimulator;

        #endregion

        internal SubscriptionList SubscriptionList { get; } = new SubscriptionList();

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
            var instrument = InstrumentManager.Get(symbol);
            if (instrument != null)
                AddInstrument(instrument);
            else
                Console.WriteLine($"Strategy::AddInstrument instrument with symbol {symbol} doesn't exist in the framework");
        }

        public void AddInstrument(int id)
        {
            var instrument = InstrumentManager.GetById(id);
            if (instrument != null)
                AddInstrument(instrument);
            else
                Console.WriteLine($"Strategy::AddInstrument instrument with Id {id} doesn't exist in the framework");
        }

        public virtual void AddInstrument(Instrument instrument)
        {
            AddInstrument(instrument, this.method_6(this, instrument));
        }

        public void AddInstrument(Instrument instrument, IDataProvider provider)
        {
            if (SubscriptionList.Contains(instrument, provider))
            {
                Console.WriteLine($"Strategy::AddInstrument Strategy is already subscribed for instrument {instrument} and provider {provider}");
                return;
            }
            SubscriptionList.Add(instrument, provider);

            if (!Instruments.Contains(instrument))
            {
                Instruments.Add(instrument);
                this.EmitInstrumentAdded(instrument);
            }
            if (Status == StrategyStatus.Running)
            {
                this.method_2(instrument, provider);
            }
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
            var instrument = InstrumentManager.Get(symbol);
            if (instrument != null)
                RemoveInstrument(instrument);
            else
                Console.WriteLine($"Strategy::RemoveInstrument instrument with symbol {symbol} doesn't exist in the framework");
        }

        public void RemoveInstrument(int id)
        {
            var instrument = InstrumentManager.GetById(id);
            if (instrument != null)
                RemoveInstrument(instrument);
            else
                Console.WriteLine($"Strategy::RemoveInstrument instrument with Id {id} doesn't exist in the framework");
        }

        public virtual void RemoveInstrument(Instrument instrument)
        {
            if (!Instruments.Contains(instrument))
            {
                Console.WriteLine("Strategy::RemoveInstrument " + instrument + " doesn't exist in the strategy instrument list");
                return;
            }
            Instruments.Remove(instrument);
            var instrumentList = new InstrumentList();
            instrumentList.Add(instrument);
            StrategyManager.method_0(this.method_6(this, instrument), instrumentList);
            Parent?.method_4(this, instrumentList, Id);
            EmitInstrumentRemoved(instrument);
        }

        public void RemoveInstrument(Instrument instrument, IDataProvider dataProvider)
        {
            if (SubscriptionList.Contains(instrument, dataProvider))
                SubscriptionList.Remove(instrument, dataProvider);

            var instrumentList = new InstrumentList();
            instrumentList.Add(instrument);
            StrategyManager.method_0(dataProvider, instrumentList);
            Parent?.method_4(this, instrumentList, Id);
            EmitInstrumentRemoved(instrument);
        }

        public Strategy GetRootStrategy() => Parent?.GetRootStrategy() ?? this;

        private void method_2(Instrument instrument_0, IDataProvider idataProvider_1)
        {
            InstrumentList instrumentList = new InstrumentList();
            instrumentList.Add(instrument_0);
            IExecutionProvider gInterface = this.method_5(instrument_0);
            if (idataProvider_1 != null && idataProvider_1.Status == ProviderStatus.Disconnected)
            {
                idataProvider_1.Connect();
            }
            if (gInterface != null && gInterface.Status == ProviderStatus.Disconnected)
            {
                gInterface.Connect();
            }
            StrategyManager.RegisterMarketDataRequest(idataProvider_1, instrumentList);
            Parent?.method_3(this, instrumentList, Id);
        }

        internal virtual void UxFbinsqFw(Ask ask)
        {
            if (!Enabled)
                return;

            var iId = ask.InstrumentId;
            if (this.raiseEvents && ((Mode == StrategyMode.Backtest && SubscriptionList.Contains(iId)) || SubscriptionList.Contains(iId, ask.ProviderId)))
            {
                OnAsk(InstrumentManager.GetById(iId), ask);
                List<Stop> list = this.idArray_3[iId];
                if (list != null)
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        Stop stop = list[i];
                        if (stop.Connected)
                        {
                            stop.method_6(ask);
                        }
                    }
                }
            }
            LinkedList<Strategy> linkedList = this.idArray_0[iId];
            if (linkedList != null)
            {
                for (var node = linkedList.First; node != null; node = node.Next)
                {
                    node.Data.UxFbinsqFw(ask);
                }
            }
        }

        internal virtual void vmethod_9(Bid bid)
        {
            if (!Enabled)
                return;

            var iId = bid.InstrumentId;

            if (this.raiseEvents && ((Mode == StrategyMode.Backtest && SubscriptionList.Contains(iId)) || SubscriptionList.Contains(iId, bid.ProviderId)))
            {
                OnBid(InstrumentManager.GetById(iId), bid);
                List<Stop> list = this.idArray_3[iId];
                if (list != null)
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        Stop stop = list[i];
                        if (stop.Connected)
                        {
                            stop.method_5(bid);
                        }
                    }
                }
            }
            var linkedList = this.idArray_0[iId];
            if (linkedList != null)
            {
                for (var node = linkedList.First; node != null; node = node.Next)
                {
                    node.Data.vmethod_9(bid);
                }
            }
        }

        private void method_3(Strategy strategy, InstrumentList instruments, int strategyId)
        {
            strategy.Init();
            strategy.Portfolio.Parent = Portfolio;
            foreach (Instrument current in instruments)
            {
                LinkedList<Strategy> linkedList;
                if (this.idArray_0[current.Id] == null)
                {
                    linkedList = new LinkedList<Strategy>();
                    this.idArray_0[current.Id] = linkedList;
                }
                else
                {
                    linkedList = this.idArray_0[current.Id];
                }
                linkedList.Add(strategy);
                IdArray<int> idArray;
                int id;
                (idArray = this.idArray_2)[id = current.Id] = idArray[id] + 1;
            }
            var dictionary = new Dictionary<IDataProvider, InstrumentList>();
            foreach (Instrument current2 in instruments)
            {
                InstrumentList instrumentList = null;
                IDataProvider dataProvider = this.method_6(strategy, current2);
                IExecutionProvider gInterface = strategy.method_5(current2);
                if (dataProvider.Status == ProviderStatus.Disconnected)
                {
                    dataProvider.Connect();
                }
                if (gInterface.Status == ProviderStatus.Disconnected)
                {
                    gInterface.Connect();
                }
                if (!dictionary.TryGetValue(dataProvider, out instrumentList))
                {
                    instrumentList = new InstrumentList();
                    dictionary[dataProvider] = instrumentList;
                }
                instrumentList.Add(current2);
            }
            foreach (var current3 in dictionary)
            {
                StrategyManager.RegisterMarketDataRequest(current3.Key, current3.Value);
            }
            Strategy rootStrategy = GetRootStrategy();
            if (strategy.Id == strategyId)
                rootStrategy.idArray_1[strategyId] = strategy;

            Parent?.method_3(this, instruments, strategyId);

        }

        private IExecutionProvider method_5(Instrument instrument_0 = null)
        {
            var gInterface = instrument_0?.ExecutionProvider ?? this.ginterface3_0;
            if (Mode == StrategyMode.Live)
                return gInterface = gInterface ?? this.framework.ExecutionProvider;
            if (gInterface is SellSideStrategy)
                return gInterface;
            return this.framework.ProviderManager.ExecutionSimulator;
        }

        private IFundamentalProvider ViqNiQdFkq(Instrument instrument_0 = null) => this.framework.Mode == FrameworkMode.Simulation ? null : this.ginterface1_0;

        private IDataProvider method_6(Strategy strategy, Instrument instrument = null)
        {
            var dataProvider = instrument?.DataProvider;//?? strategy?.DataProvider;
            if (this.framework.Mode != FrameworkMode.Simulation)
                return dataProvider = dataProvider ?? this.framework.DataProvider;
            if (dataProvider is SellSideStrategy)
                return dataProvider;
            return this.framework.ProviderManager.DataSimulator;
        }

        #endregion

        #region Strategy Management

        public void AddStrategy(Strategy strategy) => AddStrategy(strategy, true);

        public void AddStrategy(Strategy strategy, bool callOnStrategyStart)
        {
            strategy.Id = StrategyManager.GetNextId();
            strategy.Parent = this;
            Strategies.Add(strategy);
            strategy.Status = Status;
            if (Status == StrategyStatus.Running)
            {
                this.method_3(strategy, strategy.Instruments, strategy.Id);
                if (callOnStrategyStart)
                    strategy.vmethod_0();
            }
            this.framework.EventServer.OnStrategyAdded(strategy);
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

        public bool HasPosition(Instrument instrument) => Portfolio.HasPosition(instrument);

        public bool HasPosition(Instrument instrument, PositionSide side, double qty) => Portfolio.HasPosition(instrument, side, qty);

        public bool HasLongPosition(Instrument instrument) => Portfolio.HasLongPosition(instrument);

        public bool HasLongPosition(Instrument instrument, double qty) => Portfolio.HasLongPosition(instrument, qty);

        public bool HasShortPosition(Instrument instrument) => Portfolio.HasShortPosition(instrument);

        public bool HasShortPosition(Instrument instrument, double qty) => Portfolio.HasShortPosition(instrument, qty);

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

        public Strategy GetStrategy(string name)
        {
            Strategy strategy = this;
            foreach (var p in name.Split(new[] { '\\', '/' }))
            {
                strategy = strategy.method_0(p);
                if (strategy == null)
                    return null;
            }
            return strategy;
        }

        public bool ExecuteMethod(string methodName)
        {
            var methods = GetType().GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
            var method = methods.FirstOrDefault(m => m.GetCustomAttributes(typeof(StrategyMethodAttribute), true).Any() && m.GetParameters().Length == 0 && m.Name == methodName);
            method?.Invoke(this, null);
            return method != null;
        }

        public virtual double Objective() => this.portfolio.Value;

        public void Deposit(double value, byte currencyId = 148, string text = null, bool updateParent = true)
        {
            Portfolio.Account.Deposit(value, currencyId, text, updateParent);
        }

        public void Deposit(DateTime dateTime, double value, byte currencyId = 148, string text = null, bool updateParent = true)
        {
            Portfolio.Account.Deposit(dateTime, value, currencyId, text, updateParent);
        }

        public void Withdraw(double value, byte currencyId = 148, string text = null, bool updateParent = true)
        {
            Portfolio.Account.Withdraw(value, currencyId, text, updateParent);
        }

        public void Withdraw(DateTime dateTime, double value, byte currencyId = 148, string text = null, bool updateParent = true)
        {
            Portfolio.Account.Withdraw(dateTime, value, currencyId, text, updateParent);
        }

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

        internal virtual void EmitInstrumentAdded(Instrument instrument)
        {
            OnInstrumentAdded(instrument);
        }

        internal virtual void EmitInstrumentRemoved(Instrument instrument)
        {
            OnInstrumentRemoved(instrument);
        }

        protected virtual void OnInstrumentAdded(Instrument instrument)
        {
        }

        protected virtual void OnInstrumentRemoved(Instrument instrument)
        {
        }

        internal void method_4(Strategy strategy, InstrumentList instrumentList_1, int int_2)
        {
            strategy.Portfolio.Parent = Portfolio;
            foreach (Instrument current in instrumentList_1)
            {
                LinkedList<Strategy> linkedList = this.idArray_0[current.Id];
                if (linkedList != null)
                {
                    linkedList.Remove(strategy);
                }
                linkedList.Add(strategy);
                this.idArray_2[current.Id] -= 1;
                if (this.idArray_2[current.Id] == 0)
                    Instruments.Remove(current);
            }
            var dictionary = new Dictionary<IDataProvider, InstrumentList>();
            foreach (Instrument current2 in instrumentList_1)
            {
                InstrumentList instrumentList = null;
                IDataProvider key = this.method_6(strategy, current2);
                if (!dictionary.TryGetValue(key, out instrumentList))
                {
                    instrumentList = new InstrumentList();
                    dictionary[key] = instrumentList;
                }
                instrumentList.Add(current2);
            }
            foreach (var current3 in dictionary)
            {
                StrategyManager.method_0(current3.Key, current3.Value);
            }
            this.idArray_1[int_2] = null;
            Parent?.method_4(this, instrumentList_1, int_2);
        }

        internal virtual void vmethod_0()
        {
            Status = StrategyStatus.Running;
            foreach (var current in SubscriptionList)
                this.method_1(current);

            if (this.raiseEvents)
                OnStrategyStart();

            for (var s = Strategies.First; s != null; s = s.Next)
            {
                s.Data.Status = StrategyStatus.Running;
                this.method_3(s.Data, s.Data.Instruments, s.Data.Id);
                s.Data.vmethod_0();
            }
        }

        internal virtual void vmethod_1()
        {
            Status = StrategyStatus.Stopped;
            if (this.raiseEvents)
                OnStrategyStop();

            for (var s = Strategies.First; s != null; s = s.Next)
                s.Data.vmethod_1();
        }

        internal virtual void vmethod_2()
        {
            if (this.raiseEvents)
            {
                if (this.DataProvider != null && (this.DataProvider.IsConnected || this.DataProvider.IsConnecting))
                {
                    this.DataProvider.Disconnect();
                }
                if (this.ExecutionProvider != null && (this.ExecutionProvider.IsConnected || this.ExecutionProvider.IsConnecting))
                {
                    this.ExecutionProvider.Disconnect();
                }
                if (this.FundamentalProvider != null && (this.FundamentalProvider.IsConnected || this.FundamentalProvider.IsConnecting))
                {
                    this.FundamentalProvider.Disconnect();
                }
            }
            for (var s = Strategies.First; s != null; s = s.Next)
                s.Data.vmethod_2();
        }

        private void method_1(Subscription subscription_0)
        {
            this.method_2(subscription_0.Instrument, ProviderManager.GetDataProvider(subscription_0.ProviderId));
        }

        private Strategy method_0(string name) => Strategies.FirstOrDefault(s => s.Name == name);
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