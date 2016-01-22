// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Collections;
using System.Threading;
using CId = SmartQuant.CurrencyId;

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
        // FIXME: visibility
        internal IdArray<LinkedList<Strategy>> subStrategiesByInstrument = new IdArray<LinkedList<Strategy>>(10240);
        private IdArray<Strategy> idArray_1 = new IdArray<Strategy>(102400);
        private IdArray<int> idArray_2 = new IdArray<int>(10240);
        private IdArray<List<Stop>> idArray_3 = new IdArray<List<Stop>>(10240);

        private List<Stop> list_0 = new List<Stop>();
        private IdArray<List<Stop>>  efBkyXtSiO = new IdArray<List<Stop>>(10240);

        protected internal bool initialized;
        protected internal Framework framework;

        protected internal IDataProvider rawDataProvider;
        protected internal IExecutionProvider rawExecutionProvider;
        private IFundamentalProvider ginterface1_0;

        protected internal bool raiseEvents = true;
        private ParameterHelper parameterHelper = new ParameterHelper();

        [Parameter, Category("Information"), ReadOnly(true)]
        public int Id { get; internal set; }
        [Parameter, Category("Information"), ReadOnly(true)]
        public virtual string Type => "Strategy";

        public int ClientId { get; set; }
        public string Name { get; set; }
        public bool Enabled { get; set; } = true;
        public Strategy Parent { get; private set; }
        public StrategyStatus Status { get; internal set; } = StrategyStatus.Stopped;
        public Portfolio Portfolio { get; internal set; }

        #region Datapart
        public TickSeries Bids { get; } = new TickSeries("", "");
        public TickSeries Asks { get; } = new TickSeries("", "");
        public BarSeries Bars { get; internal set; } = new BarSeries("", "", -1, -1);
        public TimeSeries Equity { get; internal set; } = new TimeSeries();
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
        public PortfolioManager PortfolioManager => this.framework.PortfolioManager;

        public BarFactory BarFactory => this.framework.EventManager.BarFactory;

        #region Fetch Provider Section

        public virtual IDataProvider DataProvider
        {
            get
            {
                return DetermineDataProvider(this, null);
            }
            set
            {
                this.rawDataProvider = value;
                foreach (var s in Strategies)
                    s.DataProvider = this.rawDataProvider;
            }
        }

        public virtual IExecutionProvider ExecutionProvider
        {
            get
            {
                return this.DetermineExecutionProvider(null);
            }
            set
            {
                this.rawExecutionProvider = value;
                for (var s = Strategies.First; s != null; s = s.Next)
                    s.Data.ExecutionProvider = this.rawExecutionProvider;
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
            if (!this.initialized)
            {
                Portfolio = GetOrCreatePortfolio(Name);
                foreach (var instrument in Instruments)
                    Portfolio.Add(instrument);
                OnStrategyInit();
                foreach (var s in Strategies)
                    s.Init();
                this.initialized = true;
            }
        }

        public Reminder AddReminder(DateTime dateTime, object data = null)
        {
            return this.framework.Clock.AddReminderWithHandler(OnReminder, dateTime, data);
        }

        public Reminder AddExchangeReminder(DateTime dateTime, object data = null)
        {
            return this.framework.ExchangeClock.AddReminderWithHandler(OnExchangeReminder, dateTime, data);
        }

        public string GetStatusAsString() => Status == StrategyStatus.Running ? "Running" : Status == StrategyStatus.Stopped ? "Stopped" : "Undefined";

        public string GetModeAsString() => Mode == StrategyMode.Backtest ? "Backtest" : Mode == StrategyMode.Paper ? "Paper" : Mode == StrategyMode.Live ? "Live" : "Undefined";

        public ParameterList GetParameters() => this.parameterHelper.GetStrategyParameters(Name, this);

        public object GetParameter(string name) => this.parameterHelper.GetStrategyParameter(name, this);

        public void SetParameter(string name, object value) => this.parameterHelper.SetStrategyParameter(name, this, value);

        public void SendStrategyEvent(object data) => this.framework.EventServer.OnEvent(new OnStrategyEvent(data));

        public bool ExecuteMethod(string methodName)
        {
            var methods = GetType().GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
            var method = methods.FirstOrDefault(m => m.GetCustomAttributes(typeof(StrategyMethodAttribute), true).Any() && m.GetParameters().Length == 0 && m.Name == methodName);
            method?.Invoke(this, null);
            return method != null;
        }

        public virtual double Objective() => Portfolio.Value;

        public void Respond(DataObject data) => Respond(data, -1);

        public void Respond(DataObject data, int commandId)
        {
            // noop
        }

        #region Strategy Management

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

        public Strategy GetRootStrategy() => Parent?.GetRootStrategy() ?? this;

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
                    strategy.EmitStrategyStart();
            }
            this.framework.EventServer.OnStrategyAdded(strategy);
        }

        public void RemoveStrategy(Strategy strategy)
        {
            // noop
        }

        #endregion

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
            AddInstrument(instrument, this.DetermineDataProvider(this, instrument));
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
                EmitInstrumentAdded(instrument);
            }
            if (Status == StrategyStatus.Running)
                this.method_2(instrument, provider);
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
            StrategyManager.UnregisterMarketDataRequest(this.DetermineDataProvider(this, instrument), instrumentList);
            Parent?.method_4(this, instrumentList, Id);
            EmitInstrumentRemoved(instrument);
        }

        public void RemoveInstrument(Instrument instrument, IDataProvider dataProvider)
        {
            if (SubscriptionList.Contains(instrument, dataProvider))
                SubscriptionList.Remove(instrument, dataProvider);

            var instrumentList = new InstrumentList();
            instrumentList.Add(instrument);
            StrategyManager.UnregisterMarketDataRequest(dataProvider, instrumentList);
            Parent?.method_4(this, instrumentList, Id);
            EmitInstrumentRemoved(instrument);
        }

        #endregion

        #region Postions
        public bool HasPosition(Instrument instrument) => Portfolio.HasPosition(instrument);

        public bool HasPosition(Instrument instrument, PositionSide side, double qty) => Portfolio.HasPosition(instrument, side, qty);

        public bool HasLongPosition(Instrument instrument) => Portfolio.HasLongPosition(instrument);

        public bool HasLongPosition(Instrument instrument, double qty) => Portfolio.HasLongPosition(instrument, qty);

        public bool HasShortPosition(Instrument instrument) => Portfolio.HasShortPosition(instrument);

        public bool HasShortPosition(Instrument instrument, double qty) => Portfolio.HasShortPosition(instrument, qty);
        #endregion

        #region Money Management

        public void Deposit(double value, byte currencyId = CId.USD, string text = null, bool updateParent = true)
        {
            Portfolio.Account.Deposit(value, currencyId, text, updateParent);
        }

        public void Deposit(DateTime dateTime, double value, byte currencyId = CId.USD, string text = null, bool updateParent = true)
        {
            Portfolio.Account.Deposit(dateTime, value, currencyId, text, updateParent);
        }

        public void Withdraw(double value, byte currencyId = CId.USD, string text = null, bool updateParent = true)
        {
            Portfolio.Account.Withdraw(value, currencyId, text, updateParent);
        }

        public void Withdraw(DateTime dateTime, double value, byte currencyId = 148, string text = null, bool updateParent = true)
        {
            Portfolio.Account.Withdraw(dateTime, value, currencyId, text, updateParent);
        }

        #endregion

        #region Log Functions

        public void Log(Event e, Group group)
        {
            this.framework.EventServer.OnLog(new GroupEvent(e, group));
        }

        public void Log(Event e, int groupId)
        {
            this.framework.EventServer.OnLog(new GroupEvent(e, this.framework.GroupManager.Groups[groupId]));
        }

        public void Log(DataObject data, Group group)
        {
            this.framework.EventServer.OnLog(new GroupEvent(data, group));
        }

        public void Log(DataObject data, int groupId)
        {
            this.framework.EventServer.OnLog(new GroupEvent(data, this.framework.GroupManager.Groups[groupId]));
        }

        public void Log(double value, int groupId)
        {
            this.framework.EventServer.OnLog(new GroupEvent(new TimeSeriesItem(this.framework.Clock.DateTime, value), groupId));
        }

        public void Log(double value, Group group)
        {
            this.framework.EventServer.OnLog(new GroupEvent(new TimeSeriesItem(this.framework.Clock.DateTime, value), group));
        }

        public void Log(string text, int groupId)
        {
            this.framework.EventServer.OnLog(new GroupEvent(new TextInfo(this.framework.Clock.DateTime, text), groupId));
        }

        public void Log(string text, Group group)
        {
            this.framework.EventServer.OnLog(new GroupEvent(new TextInfo(this.framework.Clock.DateTime, text), group));
        }

        public void Log(DateTime dateTime, string text, int groupId)
        {
            this.framework.EventServer.OnLog(new GroupEvent(new TextInfo(dateTime, text), groupId));
        }

        public void Log(DateTime dateTime, double value, int groupId)
        {
            this.framework.EventServer.OnLog(new GroupEvent(new TimeSeriesItem(dateTime, value), groupId));
        }

        public void Log(DateTime dateTime, double value, Group group)
        {
            this.framework.EventServer.OnLog(new GroupEvent(new TimeSeriesItem(dateTime, value), group));
        }

        public void Log(DateTime dateTime, string text, Group group)
        {
            this.framework.EventServer.OnLog(new GroupEvent(new TextInfo(dateTime, text), group));
        }

        #endregion

        #region Order Management

        public void AddStop(Stop stop)
        {
            var iId = stop.Instrument.Id;
            this.list_0.Add(stop);
            this.efBkyXtSiO[iId] = this.efBkyXtSiO[iId] ?? new List<Stop>();
            this.efBkyXtSiO[iId].Add(stop);
        }

        public Order Order(Instrument instrument, OrderType type, OrderSide side, double qty, double stopPx, double price, string text = "")
        {
            var order = new Order(this.DetermineExecutionProvider(instrument), Portfolio, instrument, type, side, qty, 0, 0, TimeInForce.Day, 0, "");
            order.StrategyId = Id;
            order.ClientId = ClientId;
            order.Text = text;
            order.StopPx = stopPx;
            order.Price = price;
            this.framework.OrderManager.Register(order);
            return order;
        }

        public void Send(Order order)
        {
            this.framework.OrderManager.Send(order);
        }

        public void Cancel(Order order)
        {
            this.framework.OrderManager.Cancel(order);
        }

        public void CancelAll()
        {
            List<Order> list = new List<Order>();
            foreach (Order current in this.framework.OrderManager.Orders)
            {
                if (current.StrategyId == Id && !current.IsDone)
                {
                    list.Add(current);
                }
            }
            foreach (Order current2 in list)
            {
                Cancel(current2);
            }
        }

        public void CancelAll(OrderSide side)
        {
            List<Order> list = new List<Order>();
            foreach (Order current in this.framework.OrderManager.Orders)
            {
                if (current.StrategyId == Id && current.Side == side && !current.IsDone)
                {
                    list.Add(current);
                }
            }
            foreach (Order current2 in list)
            {
                this.Cancel(current2);
            }
        }

        public void CancelAll(Instrument instrument)
        {
            List<Order> list = new List<Order>();
            foreach (Order current in this.framework.OrderManager.Orders)
            {
                if (current.StrategyId == Id && current.Instrument == instrument && !current.IsDone)
                {
                    list.Add(current);
                }
            }
            foreach (Order current2 in list)
            {
                this.Cancel(current2);
            }
        }

        public void CancelAll(Instrument instrument, OrderSide side)
        {
            List<Order> list = new List<Order>();
            foreach (Order current in this.framework.OrderManager.Orders)
            {
                if (current.StrategyId == this.Id && current.Instrument == instrument && current.Side == side && !current.IsDone)
                {
                    list.Add(current);
                }
            }
            foreach (Order current2 in list)
            {
                this.Cancel(current2);
            }
        }

        public void Reject(Order order)
        {
            this.framework.OrderManager.Reject(order);
        }

        public void Replace(Order order, double price)
        {
            this.framework.OrderManager.Replace(order, price);
        }

        public void Replace(Order order, double price, double stopPx, double qty)
        {
            this.framework.OrderManager.Replace(order, price, stopPx, qty);
        }

        public Order BuyOrder(Instrument instrument, double qty, string text = "")
        {
            var order = new Order(this.DetermineExecutionProvider(instrument), Portfolio, instrument, OrderType.Market, OrderSide.Buy, qty, 0.0, 0.0, TimeInForce.Day, 0, "");
            order.StrategyId = Id;
            order.ClientId = ClientId;
            order.Text = text;
            this.framework.OrderManager.Register(order);
            return order;
        }

        public Order SellOrder(Instrument instrument, double qty, string text = "")
        {
            var order = new Order(this.DetermineExecutionProvider(instrument), Portfolio, instrument, OrderType.Market, OrderSide.Sell, qty, 0.0, 0.0, TimeInForce.Day, 0, "");
            order.StrategyId = Id;
            order.ClientId = ClientId;
            order.Text = text;
            this.framework.OrderManager.Register(order);
            return order;
        }

        public Order Buy(Instrument instrument, double qty, string text = "")
        {
            return Buy(this.DetermineExecutionProvider(instrument), instrument, qty, text);
        }

        public Order Buy(IExecutionProvider provider, Instrument instrument, double qty, string text = "")
        {
            var order = new Order(provider, Portfolio, instrument, OrderType.Market, OrderSide.Buy, qty, 0.0, 0.0, TimeInForce.Day, 0, "");
            order.StrategyId = Id;
            order.ClientId = ClientId;
            order.Text = text;
            this.framework.OrderManager.Send(order);
            return order;
        }

        public Order Buy(short providerId, Instrument instrument, double qty, string text = "")
        {
            return Buy(this.framework.ProviderManager.GetExecutionProvider(providerId), instrument, qty, text);
        }

        public Order Buy(Instrument instrument, OrderType type, double qty, double price, double stopPx, string text = "")
        {
            return Buy(this.DetermineExecutionProvider(instrument), instrument, type, qty, price, stopPx, text);
        }

        public Order Buy(short providerId, Instrument instrument, OrderType type, double qty, double price, double stopPx, string text = "")
        {
            return Buy(this.framework.ProviderManager.GetExecutionProvider(providerId), instrument, type, qty, price, stopPx, text);
        }

        public Order Buy(IExecutionProvider provider, Instrument instrument, OrderType type, double qty, double price, double stopPx, string text = "")
        {
            var order = new Order(provider, Portfolio, instrument, type, OrderSide.Buy, qty, price, stopPx, TimeInForce.Day, 0, "");
            order.StrategyId =Id;
            order.ClientId = ClientId;
            order.Text = text;
            this.framework.OrderManager.Send(order);
            return order;
        }

        public Order Sell(Instrument instrument, double qty, string text = "")
        {
            return Sell(this.DetermineExecutionProvider(instrument), instrument, qty, text);
        }

        public Order Sell(IExecutionProvider provider, Instrument instrument, double qty, string text = "")
        {
            var order = new Order(provider, Portfolio, instrument, OrderType.Market, OrderSide.Sell, qty, 0, 0, TimeInForce.Day, 0, "");
            order.StrategyId = Id;
            order.ClientId = ClientId;
            order.Text = text;
            this.framework.OrderManager.Send(order);
            return order;
        }

        public Order Sell(short providerId, Instrument instrument, double qty, string text = "")
        {
            return Sell(this.framework.ProviderManager.GetExecutionProvider(providerId), instrument, qty, text);
        }

        public Order Sell(Instrument instrument, OrderType type, double qty, double price, double stopPx, string text = "")
        {
            return Sell(this.DetermineExecutionProvider(instrument), instrument, type, qty, price, stopPx, text);
        }

        public Order Sell(IExecutionProvider provider, Instrument instrument, OrderType type, double qty, double price, double stopPx, string text = "")
        {
            var order = new Order(provider, Portfolio, instrument, type, OrderSide.Sell, qty, price, stopPx, TimeInForce.Day, 0, "");
            order.StrategyId = Id;
            order.ClientId = ClientId;
            order.Text = text;
            this.framework.OrderManager.Send(order);
            return order;
        }

        public Order Sell(short providerId, Instrument instrument, OrderType type, double qty, double price, double stopPx, string text = "")
        {
            return Sell(this.framework.ProviderManager.GetExecutionProvider(providerId), instrument, type, qty, price, stopPx, text);
        }

        public Order BuyLimitOrder(Instrument instrument, double qty, double price, string text = "")
        {
            var order = new Order(this.DetermineExecutionProvider(instrument), Portfolio, instrument, OrderType.Limit, OrderSide.Buy, qty, 0.0, 0.0, TimeInForce.Day, 0, "");
            order.StrategyId = Id;
            order.ClientId = ClientId;
            order.Text = text;
            order.Price = price;
            this.framework.OrderManager.Register(order);
            return order;
        }

        public Order SellLimitOrder(Instrument instrument, double qty, double price, string text = "")
        {
            Order order = new Order(this.DetermineExecutionProvider(instrument), Portfolio, instrument, OrderType.Limit, OrderSide.Sell, qty, 0.0, 0.0, TimeInForce.Day, 0, "");
            order.StrategyId = Id;
            order.ClientId = ClientId;
            order.Text = text;
            order.Price = price;
            this.framework.OrderManager.Register(order);
            return order;
        }

        public Order BuyLimit(Instrument instrument, double qty, double price, string text = "")
        {
            return this.BuyLimit(this.DetermineExecutionProvider(instrument), instrument, qty, price, text);
        }

        public Order BuyLimit(IExecutionProvider provider, Instrument instrument, double qty, double price, string text = "")
        {
            var order = new Order(provider, Portfolio, instrument, OrderType.Limit, OrderSide.Buy, qty, 0.0, 0.0, TimeInForce.Day, 0, "");
            order.StrategyId = Id;
            order.ClientId = ClientId;
            order.Text = text;
            order.Price = price;
            this.framework.OrderManager.Send(order);
            return order;
        }

        public Order BuyLimit(short providerId, Instrument instrument, double qty, double price, string text = "")
        {
            return BuyLimit(this.framework.ProviderManager.GetExecutionProvider(providerId), instrument, qty, price, text);
        }

        public Order SellLimit(Instrument instrument, double qty, double price, string text = "")
        {
            return this.SellLimit(this.DetermineExecutionProvider(instrument), instrument, qty, price, text);
        }

        public Order SellLimit(IExecutionProvider provider, Instrument instrument, double qty, double price, string text = "")
        {
            var order = new Order(provider, Portfolio, instrument, OrderType.Limit, OrderSide.Sell, qty, 0.0, 0.0, TimeInForce.Day, 0, "");
            order.StrategyId = Id;
            order.ClientId = ClientId;
            order.Text = text;
            order.Price = price;
            this.framework.OrderManager.Send(order);
            return order;
        }

        public Order SellLimit(short providerId, Instrument instrument, double qty, double price, string text = "")
        {
            return SellLimit(this.framework.ProviderManager.GetExecutionProvider(providerId), instrument, qty, price, text);
        }

        public Order BuyStopLimitOrder(Instrument instrument, double qty, double stopPx, double price, string text = "")
        {
            Order order = new Order(this.DetermineExecutionProvider(instrument), Portfolio, instrument, OrderType.StopLimit, OrderSide.Buy, qty, 0.0, 0.0, TimeInForce.Day, 0, "");
            order.StrategyId = Id;
            order.ClientId = ClientId;
            order.Text = text;
            order.Price = price;
            order.StopPx = stopPx;
            this.framework.OrderManager.Register(order);
            return order;
        }

        public Order SellStopLimitOrder(Instrument instrument, double qty, double stopPx, double price, string text = "")
        {
            Order order = new Order(this.DetermineExecutionProvider(instrument), Portfolio, instrument, OrderType.StopLimit, OrderSide.Sell, qty, 0.0, 0.0, TimeInForce.Day, 0, "");
            order.StrategyId = Id;
            order.ClientId = ClientId;
            order.Text = text;
            order.Price = price;
            order.StopPx = stopPx;
            this.framework.OrderManager.Register(order);
            return order;
        }

        public Order BuyStopLimit(Instrument instrument, double qty, double stopPx, double price, string text = "")
        {
            return BuyStopLimit(this.DetermineExecutionProvider(instrument), instrument, qty, stopPx, price, text);
        }

        public Order BuyStopLimit(IExecutionProvider provider, Instrument instrument, double qty, double stopPx, double price, string text = "")
        {
            Order order = new Order(provider, Portfolio, instrument, OrderType.StopLimit, OrderSide.Buy, qty, 0.0, 0.0, TimeInForce.Day, 0, "");
            order.StrategyId = Id;
            order.ClientId = ClientId;
            order.Text = text;
            order.Price = price;
            order.StopPx = stopPx;
            this.framework.OrderManager.Send(order);
            return order;
        }

        public Order BuyStopLimit(short providerId, Instrument instrument, double qty, double stopPx, double price, string text = "")
        {
            return BuyStopLimit(this.framework.ProviderManager.GetExecutionProvider(providerId), instrument, qty, stopPx, price, text);
        }

        public Order SellStopLimit(Instrument instrument, double qty, double stopPx, double price, string text = "")
        {
            return SellStopLimit(this.DetermineExecutionProvider(instrument), instrument, qty, stopPx, price, text);
        }

        public Order SellStopLimit(IExecutionProvider provider, Instrument instrument, double qty, double stopPx, double price, string text = "")
        {
            Order order = new Order(provider, Portfolio, instrument, OrderType.StopLimit, OrderSide.Sell, qty, 0.0, 0.0, TimeInForce.Day, 0, "");
            order.StrategyId = Id;
            order.ClientId = ClientId;
            order.Text = text;
            order.Price = price;
            order.StopPx = stopPx;
            this.framework.OrderManager.Send(order);
            return order;
        }

        public Order SellStopLimit(short providerId, Instrument instrument, double qty, double stopPx, double price, string text = "")
        {
            return SellStopLimit(this.framework.ProviderManager.GetExecutionProvider(providerId), instrument, qty, stopPx, price, text);
        }

        public Order BuyStopOrder(Instrument instrument, double qty, double stopPx, string text = "")
        {
            var order = new Order(this.DetermineExecutionProvider(instrument), Portfolio, instrument, OrderType.Stop, OrderSide.Buy, qty, 0.0, 0.0, TimeInForce.Day, 0, "");
            order.StrategyId = Id;
            order.ClientId = ClientId;
            order.Text = text;
            order.StopPx = stopPx;
            this.framework.OrderManager.Register(order);
            return order;
        }

        public Order SellStopOrder(Instrument instrument, double qty, double stopPx, string text = "")
        {
            Order order = new Order(this.DetermineExecutionProvider(instrument), Portfolio, instrument, OrderType.Stop, OrderSide.Sell, qty, 0.0, 0.0, TimeInForce.Day, 0, "");
            order.StrategyId = Id;
            order.ClientId = ClientId;
            order.Text = text;
            order.StopPx = stopPx;
            this.framework.OrderManager.Register(order);
            return order;
        }

        public Order BuyStop(Instrument instrument, double qty, double stopPx, string text = "")
        {
            return BuyStop(this.DetermineExecutionProvider(instrument), instrument, qty, stopPx, text);
        }

        public Order BuyStop(IExecutionProvider provider, Instrument instrument, double qty, double stopPx, string text = "")
        {
            var order = new Order(provider, Portfolio, instrument, OrderType.Stop, OrderSide.Buy, qty, 0.0, 0.0, TimeInForce.Day, 0, "");
            order.StrategyId = Id;
            order.ClientId = ClientId;
            order.Text = text;
            order.StopPx = stopPx;
            this.framework.OrderManager.Send(order);
            return order;
        }

        public Order BuyStop(short providerId, Instrument instrument, double qty, double stopPx, string text = "")
        {
            return BuyStop(this.framework.ProviderManager.GetExecutionProvider(providerId), instrument, qty, stopPx, text);
        }

        public Order SellStop(Instrument instrument, double qty, double stopPx, string text = "")
        {
            return this.SellStop(this.DetermineExecutionProvider(instrument), instrument, qty, stopPx, text);
        }

        public Order SellStop(IExecutionProvider provider, Instrument instrument, double qty, double stopPx, string text = "")
        {
            var order = new Order(provider, Portfolio, instrument, OrderType.Stop, OrderSide.Sell, qty, 0.0, 0.0, TimeInForce.Day, 0, "");
            order.StrategyId = Id;
            order.ClientId = ClientId;
            order.Text = text;
            order.StopPx = stopPx;
            this.framework.OrderManager.Send(order);
            return order;
        }

        public Order SellStop(short providerId, Instrument instrument, double qty, double stopPx, string text = "")
        {
            return SellStop(this.framework.ProviderManager.GetExecutionProvider(providerId), instrument, qty, stopPx, text);
        }

        public Order BuyPegged(Instrument instrument, double qty, double offset, string text = "")
        {
            return BuyPegged(this.DetermineExecutionProvider(instrument), instrument, qty, offset, text);
        }

        public Order BuyPegged(IExecutionProvider provider, Instrument instrument, double qty, double offset, string text = "")
        {
            Order order = new Order(provider, Portfolio, instrument, OrderType.Pegged, OrderSide.Buy, qty, 0.0, 0.0, TimeInForce.Day, 0, "");
            order.StrategyId = Id;
            order.ClientId = ClientId;
            order.StopPx = offset;
            order.Text = text;
            this.framework.OrderManager.Send(order);
            return order;
        }

        public Order BuyPegged(short providerId, Instrument instrument, double qty, double offset, string text = "")
        {
            return BuyPegged(this.framework.ProviderManager.GetExecutionProvider((int)providerId), instrument, qty, offset, text);
        }

        public Order SellPegged(Instrument instrument, double qty, double offset, string text = "")
        {
            return SellPegged(this.DetermineExecutionProvider(instrument), instrument, qty, offset, text);
        }

        public Order SellPegged(IExecutionProvider provider, Instrument instrument, double qty, double offset, string text = "")
        {
            Order order = new Order(provider, this.Portfolio, instrument, OrderType.Pegged, OrderSide.Sell, qty, 0.0, 0.0, TimeInForce.Day, 0, "");
            order.StrategyId = Id;
            order.ClientId = ClientId;
            order.StopPx = offset;
            order.Text = text;
            this.framework.OrderManager.Send(order);
            return order;
        }

        public Order SellPegged(short providerId, Instrument instrument, double qty, double offset, string text = "")
        {
            return SellPegged(this.framework.ProviderManager.GetExecutionProvider(providerId), instrument, qty, offset, text);
        }

        #endregion
        // TODO: reduce it!
        internal IDataProvider DetermineDataProvider(Strategy strategy, Instrument instrument = null)
        {
            IDataProvider dataProvider = null;
            if (instrument != null && instrument.DataProvider != null)
                dataProvider = instrument.DataProvider;
            if (dataProvider == null && strategy != null && strategy.rawDataProvider != null)
                dataProvider = strategy.rawDataProvider;
            if (this.framework.Mode != FrameworkMode.Simulation)
            {
                if (dataProvider == null)
                    dataProvider = this.framework.DataProvider;
                return dataProvider;
            }
            if (dataProvider is SellSideStrategy)
                return dataProvider;
            return ProviderManager.DataSimulator;
        }

        // TODO: reduce it!
        internal IExecutionProvider DetermineExecutionProvider(Instrument instrument = null)
        {
            IExecutionProvider executionProvider = null;
            if (instrument != null && instrument.ExecutionProvider != null)
                executionProvider = instrument.ExecutionProvider;
            if (executionProvider == null && this.rawExecutionProvider != null)
                executionProvider = this.rawExecutionProvider;
            if (Mode == StrategyMode.Live)
            {
                if (executionProvider == null)
                    executionProvider = this.framework.ExecutionProvider;
                return executionProvider;
            }
            if (executionProvider is SellSideStrategy)
                return executionProvider;
            return ProviderManager.ExecutionSimulator;
        }

        private IFundamentalProvider ViqNiQdFkq(Instrument instrument_0 = null) => this.framework.Mode == FrameworkMode.Simulation ? null : this.ginterface1_0;


        private void method_2(Instrument instrument, IDataProvider dataProvider)
        {
            InstrumentList instrumentList = new InstrumentList();
            instrumentList.Add(instrument);
            IExecutionProvider gInterface = this.DetermineExecutionProvider(instrument);
            if (dataProvider != null && dataProvider.Status == ProviderStatus.Disconnected)
                dataProvider.Connect();
            if (gInterface != null && gInterface.Status == ProviderStatus.Disconnected)
                gInterface.Connect();
            StrategyManager.RegisterMarketDataRequest(dataProvider, instrumentList);
            Parent?.method_3(this, instrumentList, Id);
        }

        internal void method_3(Strategy strategy, InstrumentList instruments, int strategyId)
        {
            strategy.Init();
            strategy.Portfolio.Parent = Portfolio;
            foreach (var instrument in instruments)
            {
                var list = this.subStrategiesByInstrument[instrument.Id] = this.subStrategiesByInstrument[instrument.Id] ?? new LinkedList<Strategy>();
                list.Add(strategy);
                this.idArray_2[instrument.Id] += 1; // count of subStrategiesby
            }

            var subscriptions = new Dictionary<IDataProvider, InstrumentList>();
            foreach (var instrument in instruments)
            {
                // connect its provider for each instrument
                var dataProvider = DetermineDataProvider(strategy, instrument);
                var executionProvider = strategy.DetermineExecutionProvider(instrument);
                if (dataProvider.Status == ProviderStatus.Disconnected)
                    dataProvider.Connect();
                if (executionProvider.Status == ProviderStatus.Disconnected)
                    executionProvider.Connect();

                InstrumentList instrumentList = null;
                if (!subscriptions.TryGetValue(dataProvider, out instrumentList))
                {
                    instrumentList = new InstrumentList();
                    subscriptions[dataProvider] = instrumentList;
                }
                instrumentList.Add(instrument);
            }
            foreach (var sub in subscriptions)
                StrategyManager.RegisterMarketDataRequest(sub.Key, sub.Value);

            var rootStrategy = GetRootStrategy();
            if (strategy.Id == strategyId)
                rootStrategy.idArray_1[strategyId] = strategy;

            Parent?.method_3(this, instruments, strategyId);
        }

        #region EventHandlers

        protected virtual void OnStrategyEvent(object data)
        {
        }

        protected internal virtual void OnStrategyInit()
        {
        }

        protected internal virtual void OnStrategyStart()
        {
        }

        protected virtual void OnAccountData(AccountData accountData)
        {
        }

        protected virtual void OnAccountReport(AccountReport report)
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

        protected virtual void OnNews(Instrument instrument, News news)
        {
        }

        protected virtual void OnFundamental(Instrument instrument, Fundamental fundamental)
        {
        }

        protected virtual void OnLevel2(Instrument instrument, Level2Snapshot snapshot)
        {
        }

        protected virtual void OnLevel2(Instrument instrument, Level2Update update)
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

        protected virtual void OnCommand(Command command)
        {
        }

        protected virtual void OnPropertyChanged(OnPropertyChanged onPropertyChanged)
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

        protected virtual void OnSendOrder(Order order)
        {
        }

        protected virtual void OnPendingNewOrder(Order order)
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

        protected virtual void OnException(string source, Event ev, Exception exception)
        {
        }

        protected virtual void OnProviderError(ProviderError error)
        {
        }

        protected virtual void OnProviderConnected(Provider provider)
        {
        }

        protected virtual void OnProviderDisconnected(Provider provider)
        {
        }

        protected virtual void OnInstrumentAdded(Instrument instrument)
        {
        }

        protected virtual void OnInstrumentRemoved(Instrument instrument)
        {
        }

        #endregion

        #region Event Emitters

        internal virtual void EmitException(string source, Event ev, Exception exception)
        {
            if (this.raiseEvents)
                OnException(source, ev, exception);
            for (var s = Strategies.First; s != null; s = s.Next)
                s.Data.EmitException(source, ev, exception);
        }

        internal virtual void EmitBid(Bid bid)
        {
            if (!Enabled)
                return;

            var iId = bid.InstrumentId;

            if (this.raiseEvents && ((Mode == StrategyMode.Backtest && SubscriptionList.Contains(iId)) || SubscriptionList.Contains(iId, bid.ProviderId)))
            {
                OnBid(InstrumentManager.GetById(iId), bid);
                var list = this.idArray_3[iId];
                list?.ForEach(stop =>
                {
                    if (stop.Connected)
                        stop.OnBid(bid);
                });
            }
            var linkedList = this.subStrategiesByInstrument[iId];
            if (linkedList != null)
            {
                for (var s = linkedList.First; s != null; s = s.Next)
                    s.Data.EmitBid(bid);
            }
        }

        internal virtual void EmitAsk(Ask ask)
        {
            if (!Enabled)
                return;

            var iId = ask.InstrumentId;
            if (this.raiseEvents && ((Mode == StrategyMode.Backtest && SubscriptionList.Contains(iId)) || SubscriptionList.Contains(iId, ask.ProviderId)))
            {
                OnAsk(InstrumentManager.GetById(iId), ask);
                var list = this.idArray_3[iId];
                list?.ForEach(stop =>
                {
                    if (stop.Connected)
                        stop.OnAsk(ask);
                });
            }
            LinkedList<Strategy> linkedList = this.subStrategiesByInstrument[iId];
            if (linkedList != null)
            {
                for (var s = linkedList.First; s != null; s = s.Next)
                    s.Data.EmitAsk(ask);
            }
        }

        internal virtual void EmitTrade(Trade trade)
        {
            if (!Enabled)
                return;

            if (this.raiseEvents && ((Mode == StrategyMode.Backtest && SubscriptionList.Contains(trade.InstrumentId)) || SubscriptionList.Contains(trade.InstrumentId, (int)trade.ProviderId)))
            {
                OnTrade(this.framework.InstrumentManager.GetById(trade.InstrumentId), trade);
                var list = this.efBkyXtSiO[trade.InstrumentId];
                list?.ForEach(stop =>
                {
                    if (stop.Connected)
                        stop.OnTrade(trade);
                });
            }

            var linkedList = this.subStrategiesByInstrument[trade.InstrumentId];
            if (linkedList != null)
            {
                for (var s = linkedList.First; s != null; s = s.Next)
                    s.Data.EmitTrade(trade);
            }
        }

        internal virtual void EmitBarOpen(Bar bar)
        {
            if (!Enabled)
                return;

            var iId = bar.InstrumentId;
            if (this.raiseEvents && Instruments.Contains(iId))
            {
                OnBarOpen(this.framework.InstrumentManager.GetById(iId), bar);
                var list = this.efBkyXtSiO[iId];
                list?.ForEach(stop =>
                {
                    if (stop.Connected)
                        stop.OnBarOpen(bar);
                });
            }
            var linkedList = this.subStrategiesByInstrument[iId];
            if (linkedList != null)
            {
                for (var s = linkedList.First; s != null; s = s.Next)
                    s.Data.EmitBarOpen(bar);
            }
        }

        internal virtual void EmitBar(Bar bar)
        {
            if (!Enabled)
                return;

            var iId = bar.InstrumentId;

            if (this.raiseEvents && Instruments.Contains(iId))
            {
                OnBar(this.framework.InstrumentManager.GetById(iId), bar);
                var list = this.efBkyXtSiO[iId];
                list?.ForEach(stop =>
                {
                    if (stop.Connected)
                        stop.OnBar(bar);
                });
            }
            var linkedList = this.subStrategiesByInstrument[iId];
            if (linkedList != null)
            {
                for (var s = linkedList.First; s != null; s = s.Next)
                    s.Data.EmitBar(bar);
            }
        }

        internal virtual void EmitFundamental(Fundamental fundamental)
        {
            if (!Enabled)
                return;

            var iId = fundamental.InstrumentId;
            if (iId == -1)
            {
                if (this.raiseEvents)
                    OnFundamental(null, fundamental);
                for (var s = Strategies.First; s != null; s = s.Next)
                    s.Data.EmitFundamental(fundamental);
                return;
            }

            if (this.raiseEvents && Instruments.Contains(iId))
                OnFundamental(this.framework.InstrumentManager.GetById(iId), fundamental);

            var linkedList = this.subStrategiesByInstrument[iId];
            if (linkedList != null)
                foreach (var s in linkedList)
                    s.EmitFundamental(fundamental);
        }

        internal virtual void EmitNews(News news)
        {
            var iId = news.InstrumentId;
            if (this.raiseEvents && Instruments.Contains(news.InstrumentId))
                OnNews(this.framework.InstrumentManager.GetById(iId), news);

            var list = this.subStrategiesByInstrument[iId];
            if (list != null)
                foreach (var s in list)
                    s.EmitNews(news);
        }

        internal virtual void EmitBarSlice(BarSlice slice)
        {
            if (!Enabled)
                return;

            if (this.raiseEvents)
                OnBarSlice(slice);
            foreach (var s in Strategies)
                s.EmitBarSlice(slice);
        }

        internal virtual void EmitLevel2(Level2Snapshot snapshot)
        {
            if (!Enabled)
                return;

            var iId = snapshot.InstrumentId;
            if (this.raiseEvents && ((this.Mode == StrategyMode.Backtest && SubscriptionList.Contains(iId)) || SubscriptionList.Contains(iId, snapshot.ProviderId)))
                OnLevel2(this.framework.InstrumentManager.GetById(iId), snapshot);

            var linkedList = this.subStrategiesByInstrument[iId];
            if (linkedList != null)
            {
                foreach (var s in linkedList)
                    s.EmitLevel2(snapshot);
            }
        }

        internal virtual void EmitLevel2(Level2Update update)
        {
            if (!Enabled)
                return;
            var iId = update.InstrumentId;

            if (this.raiseEvents && ((this.Mode == StrategyMode.Backtest && SubscriptionList.Contains(iId)) || SubscriptionList.Contains(iId, update.ProviderId)))
                this.OnLevel2(this.framework.InstrumentManager.GetById(iId), update);

            var linkedList = this.subStrategiesByInstrument[iId];
            if (linkedList != null)
            {
                foreach (var s in linkedList)
                    s.EmitLevel2(update);
            }
        }

        internal virtual void EmitFill(OnFill fill)
        {
            if (this.raiseEvents && fill.Portfolio == Portfolio)
                OnFill(fill.Fill);

            foreach (var s in Strategies)
                s.EmitFill(fill);
        }

        internal virtual void EmitTransaction(OnTransaction transaction)
        {
            if (this.raiseEvents && transaction.Portfolio == Portfolio)
                OnTransaction(transaction.Transaction);
            foreach (var s in Strategies)
                s.EmitTransaction(transaction);
        }

        internal virtual void EmitAccountReport(AccountReport report)
        {
            if (this.raiseEvents && Portfolio != null && Portfolio.Id == report.PortfolioId)
                OnAccountReport(report);

            foreach (var s in Strategies)
                s.EmitAccountReport(report);
        }

        internal virtual void EmitAccountData(AccountData data)
        {
            if (this.raiseEvents)
                this.OnAccountData(data);

            foreach (var s in Strategies)
                s.EmitAccountData(data);
        }

        internal virtual void EmitExecutionReport(ExecutionReport report)
        {
            if (report.Order.StrategyId == -1)
                return;

            if (this.raiseEvents)
                OnExecutionReport(report);

            var strategy = this.idArray_1[report.Order.StrategyId];
            strategy?.EmitExecutionReport(report);
        }

        internal virtual void EmitCommand(Command command)
        {
            if (this.raiseEvents)
                OnCommand(command);

            foreach (var s in Strategies)
                s.EmitCommand(command);
        }

        internal virtual void EmitPropertyChanged(OnPropertyChanged e)
        {
            if (this.raiseEvents)
                OnPropertyChanged(e);

            foreach (var s in Strategies)
                s.EmitPropertyChanged(e);
        }

        internal virtual void EmitPositionOpened(Position position)
        {
            if (this.raiseEvents && position.Portfolio == Portfolio)
                OnPositionOpened(position);

            foreach (var s in Strategies)
                s.EmitPositionOpened(position);
        }

        internal void EmitPositionClosed(Position position)
        {
            if (this.raiseEvents && position.Portfolio == Portfolio)
            {
                OnPositionClosed(position);
                var list = this.efBkyXtSiO[position.Instrument.Id];
                if (list != null)
                    foreach (var stop in list.TakeWhile(s => s.Position == position))
                        stop.Cancel();
            }
            foreach (var s in Strategies)
                s.EmitPositionClosed(position);
        }

        internal void EmitPositionChanged(Position position)
        {
            if (this.raiseEvents && position.Portfolio == Portfolio)
                OnPositionChanged(position);

            foreach (var s in Strategies)
                s.EmitPositionChanged(position);
        }

        internal virtual void EmitOrderDone(Order order)
        {
            if (order.StrategyId == -1)
                return;

            if (this.raiseEvents)
                OnOrderDone(order);

            var strategy = this.idArray_1[order.StrategyId];
            strategy?.EmitOrderDone(order);
        }

        internal virtual void EmitOrderReplaceRejected(Order order)
        {
            if (order.StrategyId == -1)
                return;

            if (this.raiseEvents)
                OnOrderReplaceRejected(order);

            var strategy = this.idArray_1[order.StrategyId];
            strategy?.EmitOrderReplaceRejected(order);
        }

        internal virtual void EmitOrderCancelRejected(Order order)
        {
            if (order.StrategyId == -1)
                return;

            if (this.raiseEvents)
                OnOrderCancelRejected(order);

            var strategy = this.idArray_1[order.StrategyId];
            strategy?.EmitOrderCancelRejected(order);
        }


        internal virtual void EmitOrderRejected(Order order)
        {
            if (order.StrategyId == -1)
                return;

            if (this.raiseEvents)
                OnOrderRejected(order);

            var strategy = this.idArray_1[order.StrategyId];
            strategy?.EmitOrderRejected(order);
        }

        internal virtual void EmitOrderExpired(Order order)
        {
            if (order.StrategyId == -1)
                return;

            if (this.raiseEvents)
                OnOrderExpired(order);

            var strategy = this.idArray_1[order.StrategyId];
            strategy?.EmitOrderExpired(order);
        }

        internal virtual void EmitOrderCancelled(Order order)
        {
            if (order.StrategyId == -1)
                return;

            if (this.raiseEvents)
                OnOrderCancelled(order);

            var strategy = this.idArray_1[order.StrategyId];
            strategy?.EmitOrderCancelled(order);
        }

        internal virtual void EmitOrderFilled(Order order)
        {
            if (order.StrategyId == -1)
                return;

            if (this.raiseEvents)
                OnOrderFilled(order);

            var strategy = this.idArray_1[order.StrategyId];
            strategy?.EmitOrderFilled(order);
        }

        internal virtual void EmitOrderReplaced(Order order)
        {
            if (order.StrategyId == -1)
                return;

            if (this.raiseEvents)
                OnOrderReplaced(order);

            var strategy = this.idArray_1[order.StrategyId];
            strategy?.EmitOrderReplaced(order);
        }

        internal virtual void EmitOrderPartiallyFilled(Order order)
        {
            if (order.StrategyId == -1)
                return;

            if (this.raiseEvents)
                OnOrderPartiallyFilled(order);

            var strategy = this.idArray_1[order.StrategyId];
            strategy?.EmitOrderPartiallyFilled(order);
        }

        internal virtual void EmitOrderStatusChanged(Order order)
        {
            if (order.StrategyId == -1)
                return;

            if (this.raiseEvents)
                OnOrderStatusChanged(order);

            var strategy = this.idArray_1[order.StrategyId];
            strategy?.EmitOrderStatusChanged(order);
        }

        internal virtual void EmitNewOrder(Order order)
        {
            if (order.StrategyId == -1)
                return;

            if (this.raiseEvents)
                OnNewOrder(order);

            var strategy = this.idArray_1[order.StrategyId];
            strategy?.EmitNewOrder(order);
        }

        internal virtual void EmitPendingNewOrder(Order order)
        {
            if (order.StrategyId == -1)
                return;

            if (this.raiseEvents)
                OnPendingNewOrder(order);

            var strategy = this.idArray_1[order.StrategyId];
            strategy?.EmitPendingNewOrder(order);
        }

        internal virtual void EmitSendOrder(Order order)
        {
            if (order.StrategyId == -1)
                return;

            if (this.raiseEvents)
                OnSendOrder(order);

            var strategy = this.idArray_1[order.StrategyId];
            strategy?.EmitSendOrder(order);
        }

        // We use this function name to naming other functions. Clever!
        protected internal virtual void EmitStopStatusChanged(Stop stop)
        {
            if (this.raiseEvents)
            {
                switch (stop.status)
                {
                    case StopStatus.Executed:
                        OnStopExecuted(stop);
                        break;
                    case StopStatus.Canceled:
                        OnStopCancelled(stop);
                        break;
                }
                OnStopStatusChanged(stop);
                this.list_0.Remove(stop);
                this.efBkyXtSiO[stop.Instrument.Id].Remove(stop);
            }
        }

        internal virtual void EmitInstrumentAdded(Instrument instrument)
        {
            OnInstrumentAdded(instrument);
        }

        internal virtual void EmitInstrumentRemoved(Instrument instrument)
        {
            OnInstrumentRemoved(instrument);
        }

        internal virtual void EmitProviderError(ProviderError error)
        {
            if (this.raiseEvents)
                OnProviderError(error);

            foreach (var s in Strategies)
                s.EmitProviderError(error);
        }

        internal virtual void EmitProviderDisconnected(Provider provider)
        {
            if (this.raiseEvents)
                OnProviderConnected(provider);

            foreach (var s in Strategies)
                s.EmitProviderDisconnected(provider);
        }

        internal virtual void EmitProviderConnected(Provider provider)
        {
            if (this.raiseEvents)
                OnProviderDisconnected(provider);

            foreach (var s in Strategies)
                s.EmitProviderConnected(provider);
        }

        internal virtual void EmitStrategyEvent(object data)
        {
            if (this.raiseEvents)
                OnStrategyEvent(data);

            foreach (var s in Strategies)
                s.EmitStrategyEvent(data);
        }

        internal virtual void EmitStrategyStart()
        {
            Status = StrategyStatus.Running;
            foreach (var current in SubscriptionList)
                this.method_1(current);

            if (this.raiseEvents)
                OnStrategyStart();

            foreach (var s in Strategies)
            {
                s.Status = StrategyStatus.Running;
                this.method_3(s, s.Instruments, s.Id);
                s.EmitStrategyStart();
            }
        }

        internal virtual void EmitStrategyStop()
        {
            Status = StrategyStatus.Stopped;
            if (this.raiseEvents)
                OnStrategyStop();
            foreach (var s in Strategies)
                s.EmitStrategyStop();
        }

        #endregion

        internal void method_4(Strategy strategy, InstrumentList instrumentList_1, int int_2)
        {
            strategy.Portfolio.Parent = Portfolio;
            foreach (Instrument current in instrumentList_1)
            {
                LinkedList<Strategy> linkedList = this.subStrategiesByInstrument[current.Id];
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
                IDataProvider key = this.DetermineDataProvider(strategy, current2);
                if (!dictionary.TryGetValue(key, out instrumentList))
                {
                    instrumentList = new InstrumentList();
                    dictionary[key] = instrumentList;
                }
                instrumentList.Add(current2);
            }

            foreach (var current3 in dictionary)
                StrategyManager.UnregisterMarketDataRequest(current3.Key, current3.Value);

            this.idArray_1[int_2] = null;
            Parent?.method_4(this, instrumentList_1, int_2);
        }


        internal virtual void vmethod_2()
        {
            if (this.raiseEvents)
            {
                DisconnectProvider(DataProvider);
                DisconnectProvider(ExecutionProvider);
                DisconnectProvider(FundamentalProvider);
            }

            foreach(var s in Strategies)
                s.vmethod_2();
        }

        private void method_1(Subscription subscription_0)
        {
            this.method_2(subscription_0.Instrument, ProviderManager.GetDataProvider(subscription_0.ProviderId));
        }

        private Strategy method_0(string name) => Strategies.FirstOrDefault(s => s.Name == name);

        #region Extra Helper Function

        protected Portfolio GetOrCreatePortfolio(string name)
        {
            Portfolio portfolio;
            if (PortfolioManager.Portfolios.Contains(name))
                portfolio = PortfolioManager.Portfolios.GetByName(name);
            else
            {
                portfolio = new Portfolio(this.framework, name);
                PortfolioManager.Add(portfolio, true);
            }
            return portfolio;
        }

        protected void SetRawDataProvider(IDataProvider provider)
        {
            this.rawDataProvider = provider;
        }

        protected void SetRawExecutionProvider(IExecutionProvider provider)
        {
            this.rawExecutionProvider = provider;
        }

        private void DisconnectProvider(IProvider provider)
        {
            if (provider != null && (provider.IsConnected || provider.IsConnecting))
                provider.Disconnect();
        }

        #endregion
    }
}