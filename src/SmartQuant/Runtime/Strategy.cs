// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Collections;
using System.Threading;

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
        internal IdArray<LinkedList<Strategy>> idArray_0 = new IdArray<LinkedList<Strategy>>(10240);
        private IdArray<Strategy> idArray_1 = new IdArray<Strategy>(102400);
        private IdArray<int> idArray_2 = new IdArray<int>(10240);
        private IdArray<List<Stop>> idArray_3 = new IdArray<List<Stop>>(10240);

        private List<Stop> list_0 = new List<Stop>();
        private IdArray<List<Stop>>  efBkyXtSiO = new IdArray<List<Stop>>(10240);

        protected internal bool initialized;
        protected internal Framework framework;

        internal IDataProvider idataProvider_0;
        internal IExecutionProvider ginterface3_0;
        private IFundamentalProvider ginterface1_0;

        protected internal bool raiseEvents;
        private ParameterHelper parameterHelper = new ParameterHelper();

        [Parameter, Category("Information"), ReadOnly(true)]
        public int Id { get; internal set; }
        [Parameter, Category("Information"), ReadOnly(true)]
        public virtual string Type => "Strategy";

        public int ClientId { get; set; }
        public string Name { get; set; }
        public bool Enabled { get; set; }
        public Strategy Parent { get; private set; }
        public StrategyStatus Status { get; internal set; }
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
            if (!this.initialized)
            {
                Portfolio = GetOrCreatePortfolio(Name);
                foreach (var instrument in Instruments)
                    Portfolio.Add(instrument);

                OnStrategyInit();

                for (var s = Strategies.First; s != null; s = s.Next)
                    s.Data.Init();

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
                    strategy.vmethod_0();
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
            StrategyManager.UnregisterMarketDataRequest(this.method_6(this, instrument), instrumentList);
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
            var order = new Order(this.method_5(instrument), Portfolio, instrument, type, side, qty, 0, 0, TimeInForce.Day, 0, "");
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
            var order = new Order(this.method_5(instrument), Portfolio, instrument, OrderType.Market, OrderSide.Buy, qty, 0.0, 0.0, TimeInForce.Day, 0, "");
            order.StrategyId = Id;
            order.ClientId = ClientId;
            order.Text = text;
            this.framework.OrderManager.Register(order);
            return order;
        }

        public Order SellOrder(Instrument instrument, double qty, string text = "")
        {
            var order = new Order(this.method_5(instrument), Portfolio, instrument, OrderType.Market, OrderSide.Sell, qty, 0.0, 0.0, TimeInForce.Day, 0, "");
            order.StrategyId = Id;
            order.ClientId = ClientId;
            order.Text = text;
            this.framework.OrderManager.Register(order);
            return order;
        }

        public Order Buy(Instrument instrument, double qty, string text = "")
        {
            return Buy(this.method_5(instrument), instrument, qty, text);
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
            return Buy(this.method_5(instrument), instrument, type, qty, price, stopPx, text);
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
            return Sell(this.method_5(instrument), instrument, qty, text);
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
            return Sell(this.method_5(instrument), instrument, type, qty, price, stopPx, text);
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
            var order = new Order(this.method_5(instrument), Portfolio, instrument, OrderType.Limit, OrderSide.Buy, qty, 0.0, 0.0, TimeInForce.Day, 0, "");
            order.StrategyId = Id;
            order.ClientId = ClientId;
            order.Text = text;
            order.Price = price;
            this.framework.OrderManager.Register(order);
            return order;
        }

        public Order SellLimitOrder(Instrument instrument, double qty, double price, string text = "")
        {
            Order order = new Order(this.method_5(instrument), Portfolio, instrument, OrderType.Limit, OrderSide.Sell, qty, 0.0, 0.0, TimeInForce.Day, 0, "");
            order.StrategyId = Id;
            order.ClientId = ClientId;
            order.Text = text;
            order.Price = price;
            this.framework.OrderManager.Register(order);
            return order;
        }

        public Order BuyLimit(Instrument instrument, double qty, double price, string text = "")
        {
            return this.BuyLimit(this.method_5(instrument), instrument, qty, price, text);
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
            return this.SellLimit(this.method_5(instrument), instrument, qty, price, text);
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
            Order order = new Order(this.method_5(instrument), Portfolio, instrument, OrderType.StopLimit, OrderSide.Buy, qty, 0.0, 0.0, TimeInForce.Day, 0, "");
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
            Order order = new Order(this.method_5(instrument), Portfolio, instrument, OrderType.StopLimit, OrderSide.Sell, qty, 0.0, 0.0, TimeInForce.Day, 0, "");
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
            return BuyStopLimit(this.method_5(instrument), instrument, qty, stopPx, price, text);
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
            return SellStopLimit(this.method_5(instrument), instrument, qty, stopPx, price, text);
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
            var order = new Order(this.method_5(instrument), Portfolio, instrument, OrderType.Stop, OrderSide.Buy, qty, 0.0, 0.0, TimeInForce.Day, 0, "");
            order.StrategyId = Id;
            order.ClientId = ClientId;
            order.Text = text;
            order.StopPx = stopPx;
            this.framework.OrderManager.Register(order);
            return order;
        }

        public Order SellStopOrder(Instrument instrument, double qty, double stopPx, string text = "")
        {
            Order order = new Order(this.method_5(instrument), Portfolio, instrument, OrderType.Stop, OrderSide.Sell, qty, 0.0, 0.0, TimeInForce.Day, 0, "");
            order.StrategyId = Id;
            order.ClientId = ClientId;
            order.Text = text;
            order.StopPx = stopPx;
            this.framework.OrderManager.Register(order);
            return order;
        }

        public Order BuyStop(Instrument instrument, double qty, double stopPx, string text = "")
        {
            return BuyStop(this.method_5(instrument), instrument, qty, stopPx, text);
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
            return this.SellStop(this.method_5(instrument), instrument, qty, stopPx, text);
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
            return BuyPegged(this.method_5(instrument), instrument, qty, offset, text);
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
            return SellPegged(this.method_5(instrument), instrument, qty, offset, text);
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

        internal IExecutionProvider method_5(Instrument instrument = null)
        {
            var gInterface = instrument?.ExecutionProvider ?? this.ginterface3_0;
            if (Mode == StrategyMode.Live)
                return gInterface = gInterface ?? this.framework.ExecutionProvider;
            if (gInterface is SellSideStrategy)
                return gInterface;
            return this.framework.ProviderManager.ExecutionSimulator;
        }

        private IFundamentalProvider ViqNiQdFkq(Instrument instrument_0 = null) => this.framework.Mode == FrameworkMode.Simulation ? null : this.ginterface1_0;

        internal IDataProvider method_6(Strategy strategy, Instrument instrument = null)
        {
            var dataProvider = instrument?.DataProvider;//?? strategy?.DataProvider;
            if (this.framework.Mode != FrameworkMode.Simulation)
                return dataProvider = dataProvider ?? this.framework.DataProvider;
            if (dataProvider is SellSideStrategy)
                return dataProvider;
            return this.framework.ProviderManager.DataSimulator;
        }

        private void method_2(Instrument instrument_0, IDataProvider idataProvider_1)
        {
            InstrumentList instrumentList = new InstrumentList();
            instrumentList.Add(instrument_0);
            IExecutionProvider gInterface = this.method_5(instrument_0);
            if (idataProvider_1 != null && idataProvider_1.Status == ProviderStatus.Disconnected)
                idataProvider_1.Connect();
            if (gInterface != null && gInterface.Status == ProviderStatus.Disconnected)
                gInterface.Connect();
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

        internal void vmethod_17(BarSlice barSlice)
        {
            throw new NotImplementedException();
        }

        internal void vmethod_13(Bar bar_0)
        {
            //throw new NotImplementedException();
        }

        internal void vmethod_42(string source, Event ev, Exception exception)
        {
            if (this.raiseEvents)
                OnException(source, ev, exception);
            for (var s = Strategies.First; s != null; s = s.Next)
                s.Data.vmethod_42(source, ev, exception);
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

        internal void vmethod_34(Order order)
        {
            throw new NotImplementedException();
        }

        internal void vmethod_40(Command command)
        {
            throw new NotImplementedException();
        }

        internal void vmethod_20(ExecutionReport report)
        {
            throw new NotImplementedException();
        }

        internal void vmethod_21(AccountReport report)
        {
            throw new NotImplementedException();
        }

        internal void vmethod_36(OnTransaction e)
        {
            throw new NotImplementedException();
        }

        internal void vmethod_35(OnFill e)
        {
            throw new NotImplementedException();
        }

        internal void vmethod_45(AccountData data)
        {
            throw new NotImplementedException();
        }

        internal virtual void EmitOnTrade(Trade trade)
        {
        }

        internal void vmethod_12(Level2Update level2Update_0)
        {
            throw new NotImplementedException();
        }

        internal void vmethod_11(Level2Snapshot level2Snapshot_0)
        {
            throw new NotImplementedException();
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

        internal void vmethod_622(Portfolio portfolio)
        {
            throw new NotImplementedException();
        }

        internal void vmethod_46(OnPropertyChanged e)
        {
            throw new NotImplementedException();
        }

        internal void vmethod_100(ProviderError error)
        {
            throw new NotImplementedException();
        }

        internal void vmethod_19(Fundamental fundamental)
        {
            throw new NotImplementedException();
        }

        internal void vmethod_18(News news)
        {
            throw new NotImplementedException();
        }

        internal void vmethod_62(Position position)
        {
            throw new NotImplementedException();
        }

        internal void vmethod_43(object data)
        {
            throw new NotImplementedException();
        }



        internal void vmethod_6(Provider provider)
        {
            if (this.raiseEvents)
                OnProviderConnected(provider);

            for (var s = Strategies.First; s != null; s = s.Next)
                s.Data.vmethod_6(provider);
        }






        #region EventHandlers
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

        protected virtual void OnException(string source, Event ev, Exception exception)
        {
        }

        protected virtual void OnProviderConnected(Provider provider)
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

        internal virtual void EmitInstrumentAdded(Instrument instrument)
        {
            OnInstrumentAdded(instrument);
        }

        internal virtual void EmitInstrumentRemoved(Instrument instrument)
        {
            OnInstrumentRemoved(instrument);
        }

        #endregion

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
                StrategyManager.UnregisterMarketDataRequest(current3.Key, current3.Value);

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

        #region Extra Helper Function

        protected Portfolio GetOrCreatePortfolio(string name)
        {
            Portfolio portfolio;
            if (this.framework.PortfolioManager.Portfolios.Contains(name))
            {
                portfolio = this.framework.PortfolioManager.Portfolios.GetByName(name);
            }
            else
            {
                portfolio = new Portfolio(this.framework, name);
                this.framework.PortfolioManager.Add(portfolio, true);
            }
            return portfolio;
        }

        public void SetRawDataProvider(IDataProvider provider)
        {
            this.idataProvider_0 = provider;
        }

        public void SetRawExecutionProvider(IExecutionProvider provider)
        {
            this.ginterface3_0 = provider;
        }

        #endregion
    }

    public class MetaStrategy : Strategy
    {
        internal List<Strategy> list_1 = new List<Strategy>();

        private IdArray<List<Strategy>> idArray_3 = new IdArray<List<Strategy>>();

        private IdArray<Strategy> idArray_4 = new IdArray<Strategy>();

        private IdArray<Strategy> idArray_5 = new IdArray<Strategy>();

        public MetaStrategy(Framework framework, string name) : base(framework, name)
        {
        }

        public void Add(Strategy strategy)
        {
            this.list_1.Add(strategy);
            strategy.Portfolio.Parent = Portfolio;
            foreach (Instrument current in strategy.Instruments)
            {
                List<Strategy> list;
                if (this.idArray_3[current.Id] == null)
                {
                    list = new List<Strategy>();
                    this.idArray_3[current.Id] = list;
                }
                else
                {
                    list = this.idArray_3[current.Id];
                }
                list.Add(strategy);
                if (!Instruments.Contains(current))
                {
                    Instruments.Add(current);
                }
            }
        }
    }

    public class InstrumentStrategy : Strategy
    {
        public Instrument Instrument { get; private set; }

        public bool IsInstance { get; private set; }

        public Position Position => Portfolio.GetPosition(Instrument);

        [Parameter, Category("Information"), ReadOnly(true)]
        public override string Type => "InstrumentStrategy";

        public override IDataProvider DataProvider
        {
            get
            {
                return base.method_6(this, Instrument);
            }
            set
            {
                SetRawDataProvider(value);
                for (var s = Strategies.First; s != null; s = s.Next)
                    s.Data.DataProvider = this.idataProvider_0;
            }
        }

        public override IExecutionProvider ExecutionProvider
        {
            get
            {
                return base.method_5(Instrument);
            }
            set
            {
                SetRawExecutionProvider(value);
                for (var s = Strategies.First; s != null; s = s.Next)
                    s.Data.ExecutionProvider = this.ginterface3_0;
            }
        }

        public InstrumentStrategy(Framework framework, string name) : base(framework, name)
        {
            this.raiseEvents = false;
        }

        public override void Init()
        {
            if (!this.initialized)
            {
                Portfolio = GetOrCreatePortfolio(Name);

                if (!IsInstance)
                {
                    foreach (Instrument current in Instruments)
                    {
                        var strategy = this.method_9(current);
                        base.AddStrategy(strategy, false);
                        if (Strategies.Count == 1)
                        {
                            Bars = strategy.Bars;
                            Equity = strategy.Equity;
                        }
                        strategy.OnStrategyInit();
                    }
                }
                this.initialized = true;
            }

        }

        public void AddInstance(Instrument instrument, InstrumentStrategy strategy)
        {
            strategy.Instrument = instrument;
            strategy.Instruments.Add(instrument);
            strategy.Portfolio.Add(instrument);
            strategy.raiseEvents = true;
            strategy.SetRawDataProvider(this.idataProvider_0);
            strategy.SetRawExecutionProvider(this.ginterface3_0);
            this.method_8(strategy);
            if (Instruments.GetById(instrument.Id) == null)
                Instruments.Add(instrument);
            strategy.Status = StrategyStatus.Running;
            strategy.OnStrategyInit();
            strategy.OnStrategyStart();
        }

        public override void AddInstrument(Instrument instrument)
        {
            if (IsInstance)
            {
                Parent.AddInstrument(instrument);
                return;
            }

            base.AddInstrument(instrument);
            if (this.initialized)
            {
                var strategy = this.method_9(instrument);
                base.AddStrategy(strategy, true);
            }
        }

        private Strategy method_9(Instrument instrument_1)
        {
            var strategy = (InstrumentStrategy)Activator.CreateInstance(GetType(), new object[] { this.framework, $"{Name} ({instrument_1.Symbol})" });
            strategy.Instrument = instrument_1;
            strategy.Instruments.Add(instrument_1);
            strategy.SubscriptionList.Add(instrument_1, base.method_6(this, instrument_1));
            if (this.framework.PortfolioManager.Portfolios.Contains(strategy.Name))
            {
                strategy.Portfolio = this.framework.PortfolioManager.Portfolios.GetByName(strategy.Name);
            }
            else
            {
                strategy.Portfolio = new Portfolio(this.framework, strategy.Name);
                this.framework.PortfolioManager.Add(strategy.Portfolio, true);
            }
            strategy.Portfolio.Add(instrument_1);
            strategy.raiseEvents = true;
            strategy.IsInstance = true;
            strategy.SetRawDataProvider(DataProvider);
            strategy.SetRawExecutionProvider(ExecutionProvider);
            var fields = strategy.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach(var f in fields.TakeWhile(f=> f.GetCustomAttributes(typeof(ParameterAttribute), true).Any()))
                f.SetValue(strategy, f.GetValue(this));
            return strategy;
        }

        private void method_8(InstrumentStrategy strategy)
        {
            base.AddStrategy(strategy, false);
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
                return (byte)Id;
            }
            set
            {
                Id = (byte)value;
            }
        }

        public bool IsConnecting => false;

        public bool IsDisconnecting => false;

        [Parameter, Category("Information"), ReadOnly(true)]
        public override string Type => "SellSideStrategy";

        public int RouteId { get; set; }

        public SellSideStrategy(Framework framework, string name) : base(framework, name)
        {
        }

        public virtual void Connect()
        {
            Console.WriteLine("SellSideStrategy::Connect");
            Status = ProviderStatus.Connected;
        }

        public virtual bool Connect(int timeout)
        {
            long ticks = DateTime.Now.Ticks;
            Connect();
            while (!IsConnected)
            {
                Thread.Sleep(1);
                double ms = TimeSpan.FromTicks(DateTime.Now.Ticks - ticks).TotalMilliseconds;
                if (ms >= timeout)
                {
                    Console.WriteLine($"SellSideStrategy::Connect timed out : {Name}");
                    return false;
                }
            }
            return true;
        }

        public virtual void Disconnect()
        {
            Console.WriteLine("SellSideStrategy::Disconnect");
            Status = ProviderStatus.Disconnected;
        }

        public virtual void OnCancelCommand(ExecutionCommand command)
        {
            // noop
        }

        public virtual void OnReplaceCommand(ExecutionCommand command)
        {
            // noop
        }

        public virtual void OnSendCommand(ExecutionCommand command)
        {
            // noop
        }

        protected virtual void OnSubscribe(InstrumentList instruments)
        {
            // noop
        }

        protected virtual void OnSubscribe(Instrument instrument)
        {
            // noop
        }

        protected virtual void OnUnsubscribe(InstrumentList instruments)
        {
            // noop
        }

        protected virtual void OnUnsubscribe(Instrument instrument)
        {
            // noop
        }

        public virtual void Subscribe(Instrument instrument)
        {
            OnSubscribe(instrument);
        }

        public virtual void Subscribe(InstrumentList instruments)
        {
            OnSubscribe(instruments);
        }

        public virtual void Unsubscribe(Instrument instrument)
        {
            OnUnsubscribe(instrument);
        }

        public virtual void Unsubscribe(InstrumentList instruments)
        {
            OnUnsubscribe(instruments);
        }

        public virtual void Send(ExecutionCommand command)
        {
            switch (command.Type)
            {
                case ExecutionCommandType.Send:
                    OnSendCommand(command);
                    return;
                case ExecutionCommandType.Cancel:
                    OnCancelCommand(command);
                    return;
                case ExecutionCommandType.Replace:
                    OnReplaceCommand(command);
                    return;
                default:
                    return;
            }
        }

        public virtual void EmitBid(Bid bid)
        {
            this.framework.EventManager.OnEvent(new Bid(bid) { ProviderId = (byte)Id });
        }

        public virtual void EmitBid(DateTime dateTime, int instrumentId, double price, int size)
        {
            this.framework.EventManager.OnEvent(new Bid(dateTime, (byte)Id, instrumentId, price, size));
        }

        public virtual void EmitAsk(Ask ask)
        {
            this.framework.EventManager.OnEvent(new Ask(ask) { ProviderId = (byte)Id });
        }

        public virtual void EmitAsk(DateTime dateTime, int instrumentId, double price, int size)
        {
            this.framework.EventManager.OnEvent(new Ask(dateTime, (byte)Id, instrumentId, price, size));
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

        public virtual void EmitExecutionReport(ExecutionReport report)
        {
            this.framework.EventManager.OnEvent(report);
        }

        public virtual void EmitLevel2Snapshot(Level2Snapshot snapshot)
        {
            this.framework.EventManager.OnEvent(new Level2Snapshot(snapshot) { ProviderId = (byte)Id });
        }
    }

    public class SellSideInstrumentStrategy : SellSideStrategy
    {
        public Instrument Instrument { get; internal set; }

        // FIXME: is this wrong?
        [Parameter, Category("Information"), ReadOnly(true)]
        public override string Type => "SellSideInstrumentStrategy";

        public new bool IsInstance { get; private set; }

        public SellSideInstrumentStrategy(Framework framework, string name) : base(framework, name)
        {
            this.raiseEvents = false;
        }

        public override void Init()
        {
            if (!this.initialized)
            {
                Portfolio = GetOrCreatePortfolio(Name);
                if (!IsInstance)
                {
                    foreach (Instrument instrument in Instruments)
                        if (this.idArray_0[instrument.Id] == null)
                            this.method_8(instrument, true, false);
                }
                this.initialized = true;
            }
        }

        public override void Subscribe(InstrumentList instruments)
        {
            foreach (Instrument current in instruments)
                Subscribe(current);
        }
        public override void Subscribe(Instrument instrument)
        {
            if (this.idArray_0[instrument.Id] == null)
            {
                SellSideInstrumentStrategy sellSideInstrumentStrategy = this.method_8(instrument, false, true);
                sellSideInstrumentStrategy.OnStrategyStart();
            }
        }
        public override void Unsubscribe(InstrumentList instruments)
        {
            foreach (Instrument current in instruments)
            {
                this.Unsubscribe(current);
            }
        }
        public override void Unsubscribe(Instrument instrument)
        {
            if (this.idArray_0[instrument.Id] != null)
            {
                SellSideInstrumentStrategy sellSideInstrumentStrategy = this.idArray_0[instrument.Id].First.Data as SellSideInstrumentStrategy;
                sellSideInstrumentStrategy.OnUnsubscribe(instrument);
            }
        }

        public override void Send(ExecutionCommand command)
        {
            var strategy = this.idArray_0[command.Order.Instrument.Id].First.Data as SellSideInstrumentStrategy;
            switch (command.Type)
            {
                case ExecutionCommandType.Send:
                    strategy.OnSendCommand(command);
                    return;
                case ExecutionCommandType.Cancel:
                    strategy.OnCancelCommand(command);
                    return;
                case ExecutionCommandType.Replace:
                    strategy.OnReplaceCommand(command);
                    return;
                default:
                    return;
            }
        }

        #region Emitters

        public override void EmitBid(Bid bid)
        {
            var providerId = IsInstance ? (byte)Parent.Id : (byte)Id;
            this.framework.EventManager.OnEvent(new Bid(bid) { ProviderId = providerId });
        }

        public override void EmitBid(DateTime dateTime, int instrumentId, double price, int size)
        {
            var providerId = IsInstance ? (byte)Parent.Id : (byte)Id;
            this.framework.EventManager.OnEvent(new Bid(dateTime, providerId, instrumentId, price, size));
        }

        public override void EmitAsk(Ask ask)
        {
            var providerId = IsInstance ? (byte)Parent.Id : (byte)Id;
            this.framework.EventManager.OnEvent(new Ask(ask) { ProviderId = providerId });
        }

        public override void EmitAsk(DateTime dateTime, int instrumentId, double price, int size)
        {
            var providerId = IsInstance ? (byte)Parent.Id : (byte)Id;
            this.framework.EventManager.OnEvent(new Ask(dateTime, providerId, instrumentId, price, size));
        }

        public override void EmitTrade(Trade trade)
        {
            var providerId = IsInstance ? (byte)Parent.Id : (byte)Id;
            this.framework.EventManager.OnEvent(new Trade(trade) { ProviderId = providerId });
        }

        public override void EmitTrade(DateTime dateTime, int instrumentId, double price, int size)
        {
            var providerId = IsInstance ? (byte)Parent.Id : (byte)Id;
            this.framework.EventManager.OnEvent(new Trade(dateTime, providerId, instrumentId, price, size));
        }

        #endregion

        private SellSideInstrumentStrategy method_8(Instrument instrument, bool bool_4, bool bool_5)
        {
            var strategy = (SellSideInstrumentStrategy)Activator.CreateInstance(GetType(), new object[] { this.framework, $"{Name}({instrument}" });
            strategy.Instrument = instrument;
            if (bool_4)
                strategy.Instruments.Add(instrument);
            strategy.ClientId = ClientId;
            strategy.SetRawDataProvider(DataProvider);
            strategy.SetRawExecutionProvider(ExecutionProvider);
            strategy.raiseEvents = true;
            strategy.IsInstance = true;
            this.method_10(strategy);
            this.method_9(strategy, instrument);
            if (bool_5)
                strategy.OnSubscribe(instrument);
            AddStrategy(strategy, false);
            strategy.OnStrategyInit();
            return strategy;
        }

        private void method_9(Strategy strategy, Instrument instrument)
        {
            LinkedList<Strategy> linkedList;
            if (this.idArray_0[instrument.Id] == null)
            {
                linkedList = new LinkedList<Strategy>();
                this.idArray_0[instrument.Id] = linkedList;
            }
            else
            {
                linkedList = this.idArray_0[instrument.Id];
            }
            linkedList.Add(strategy);
        }

        private void method_10(SellSideInstrumentStrategy strategy)
        {
            var fields = strategy.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var f in fields.TakeWhile(f => f.GetCustomAttributes(typeof(ParameterAttribute), true).Any()))
                f.SetValue(strategy, f.GetValue(this));
        }
    }
}