using System;

namespace SmartQuant.Component
{
    public class Signal
    {
        public Signal(double value)
        {
            Value = value;
        }

        public ObjectTable Fields { get; } = new ObjectTable();

        public double Value { get; }
    }

    public class StrategyComponent
    {
        protected internal Framework framework;
        protected internal ComponentStrategy strategy;
        public BarSeries Bars => this.strategy.Bars;
        public Instrument Instrument => this.strategy.Instrument;
        public InstrumentManager InstrumentManager => this.framework.InstrumentManager;
        public OrderManager OrderManager => this.framework.OrderManager;
        public GroupManager GroupManager => this.framework.GroupManager;
        public Portfolio Portfolio => this.strategy.Portfolio;
        public Position Position => this.strategy.Position;

        public void Signal(double value)
        {
            this.strategy.PositionComponent.OnSignal(new Signal(value));
        }

        public bool HasPosition()
        {
            return this.strategy.HasPosition(Instrument);
        }

        public bool HasPosition(PositionSide side, double qty)
        {
            return this.strategy.HasPosition(Instrument, side, qty);
        }

        public bool HasLongPosition()
        {
            return this.strategy.HasLongPosition(Instrument);
        }

        public bool HasLongPosition(double qty)
        {
            return this.strategy.HasLongPosition(Instrument, qty);
        }

        public bool HasShortPosition()
        {
            return this.strategy.HasShortPosition(Instrument);
        }

        public bool HasShortPosition(double qty)
        {
            return this.strategy.HasShortPosition(Instrument, qty);
        }

        public void Buy(double qty)
        {
            var order = new Order(this.strategy.ExecutionProvider, this.strategy.Portfolio, this.strategy.Instrument, OrderType.Market, OrderSide.Buy, qty, 0, 0, TimeInForce.Day, 0, "");
            order.StrategyId = this.strategy.Id;
            this.strategy.ExecutionComponent.OnOrder(order);
        }

        public void Buy(double qty, string text)
        {
            var order = new Order(this.strategy.ExecutionProvider, this.strategy.Portfolio, this.strategy.Instrument, OrderType.Market, OrderSide.Buy, qty, 0.0, 0.0, TimeInForce.Day, 0, "");
            order.Text = text;
            order.StrategyId = this.strategy.Id;
            this.strategy.ExecutionComponent.OnOrder(order);
        }

        public void BuyLimit(double qty, double price)
        {
            var order = new Order(this.strategy.ExecutionProvider, this.strategy.Portfolio, this.strategy.Instrument, OrderType.Limit, OrderSide.Buy, qty, price, 0, TimeInForce.Day, 0, "");
            order.StrategyId = this.strategy.Id;
            this.strategy.ExecutionComponent.OnOrder(order);
        }

        public void BuyLimit(double qty, double price, string text)
        {
            var order = new Order(this.strategy.ExecutionProvider, this.strategy.Portfolio, this.strategy.Instrument, OrderType.Limit, OrderSide.Buy, qty, price, 0, TimeInForce.Day, 0, text);
            order.StrategyId = this.strategy.Id;
            this.strategy.ExecutionComponent.OnOrder(order);
        }

        public void BuyStop(double qty, double stopPx)
        {
            var order = new Order(this.strategy.ExecutionProvider, this.strategy.Portfolio, this.strategy.Instrument, OrderType.Stop, OrderSide.Buy, qty, 0.0, stopPx, TimeInForce.Day, 0, "");
            order.StrategyId = this.strategy.Id;
            this.strategy.ExecutionComponent.OnOrder(order);
        }

        public void BuyStop(double qty, double stopPx, string text)
        {
            var order = new Order(this.strategy.ExecutionProvider, this.strategy.Portfolio, this.strategy.Instrument, OrderType.Stop, OrderSide.Buy, qty, 0, stopPx, TimeInForce.Day, 0, text);
            order.StrategyId = this.strategy.Id;
            this.strategy.ExecutionComponent.OnOrder(order);
        }

        public void Sell(double qty)
        {
            var order = new Order(this.strategy.ExecutionProvider, this.strategy.Portfolio, this.strategy.Instrument, OrderType.Market, OrderSide.Sell, qty, 0, 0, TimeInForce.Day, 0, "");
            order.StrategyId = this.strategy.Id;
            this.strategy.ExecutionComponent.OnOrder(order);
        }

        public void Sell(double qty, string text)
        {
            var order = new Order(this.strategy.ExecutionProvider, this.strategy.Portfolio, this.strategy.Instrument, OrderType.Market, OrderSide.Sell, qty, 0, 0, TimeInForce.Day, 0, "");
            order.Text = text;
            order.StrategyId = this.strategy.Id;
            this.strategy.ExecutionComponent.OnOrder(order);
        }

        public void SellLimit(double qty, double price)
        {
            var order = new Order(this.strategy.ExecutionProvider, this.strategy.Portfolio, this.strategy.Instrument, OrderType.Limit, OrderSide.Sell, qty, price, 0, TimeInForce.Day, 0, "");
            order.StrategyId = this.strategy.Id;
            this.strategy.ExecutionComponent.OnOrder(order);
        }

        public void SellLimit(double qty, double price, string text)
        {
            var order = new Order(this.strategy.ExecutionProvider, this.strategy.Portfolio, this.strategy.Instrument, OrderType.Limit, OrderSide.Sell, qty, price, 0, TimeInForce.Day, 0, text);
            order.StrategyId = this.strategy.Id;
            this.strategy.ExecutionComponent.OnOrder(order);
        }

        public void SellStop(double qty, double stopPx)
        {
            var order = new Order(this.strategy.ExecutionProvider, this.strategy.Portfolio, this.strategy.Instrument, OrderType.Stop, OrderSide.Sell, qty, 0, stopPx, TimeInForce.Day, 0, "");
            order.StrategyId = this.strategy.Id;
            this.strategy.ExecutionComponent.OnOrder(order);
        }

        public void SellStop(double qty, double stopPx, string text)
        {
            var order = new Order(this.strategy.ExecutionProvider, this.strategy.Portfolio, this.strategy.Instrument, OrderType.Stop, OrderSide.Sell, qty, 0, stopPx, TimeInForce.Day, 0, text);
            order.StrategyId = this.strategy.Id;
            this.strategy.ExecutionComponent.OnOrder(order);
        }

        public Stop SetStop(double level, StopType type = StopType.Fixed, StopMode mode = StopMode.Absolute)
        {
            var stop = new Stop(this.strategy, this.Position, level, type, mode);
            this.strategy.AddStop(stop);
            return stop;
        }

        public void Log(DataObject data, Group group)
        {
            this.strategy.Log(data, group);
        }

        public void Log(double value, Group group)
        {
            this.strategy.Log(value, group);
        }

        public void Log(DateTime dateTime, double value, Group group)
        {
            this.strategy.Log(dateTime, value, group);
        }

        public void Log(DateTime dateTime, double value, int groupId)
        {
            this.strategy.Log(dateTime, value, groupId);
        }

        public void Log(DateTime dateTime, string text, int groupId)
        {
            this.strategy.Log(dateTime, text, groupId);
        }

        public void Log(DateTime dateTime, string text, Group group)
        {
            this.strategy.Log(dateTime, text, group);
        }

        public void Log(string text, int groupId)
        {
            this.strategy.Log(text, groupId);
        }

        public void Log(string text, Group group)
        {
            this.strategy.Log(text, group);
        }

        public void Log(double value, int groupId)
        {
            this.strategy.Log(value, groupId);
        }

        public void Log(DataObject data, int groupId)
        {
            this.strategy.Log(data, groupId);
        }

        public virtual void OnReminder(DateTime dateTime, object data)
        {
            // noop
        }

        public virtual void OnStrategyStart()
        {
            // noop
        }
    }

    public class DataComponent : StrategyComponent
    {
        public virtual void OnAsk(Ask ask)
        {
            // noop
        }

        public virtual void OnBar(Bar bar)
        {
            // noop
        }

        public virtual void OnBid(Bid bid)
        {
            // noop
        }

        public virtual void OnTrade(Trade trade)
        {
            // noop
        }
    }

    public class AlphaComponent : StrategyComponent
    {
        public virtual void OnAsk(Ask ask)
        {
            // noop
        }

        public virtual void OnBar(Bar bar)
        {
            // noop
        }

        public virtual void OnBid(Bid bid)
        {
            // noop
        }

        public virtual void OnTrade(Trade trade)
        {
            // noop
        }
    }

    public class PositionComponent : StrategyComponent
    {
        public virtual void OnAsk(Ask ask)
        {
            // noop
        }

        public virtual void OnBar(Bar bar)
        {
            // noop
        }

        public virtual void OnBid(Bid bid)
        {
            // noop
        }

        public virtual void OnPositionChanged(Position position)
        {
            // noop
        }

        public virtual void OnPositionClosed(Position position)
        {
            // noop
        }

        public virtual void OnPositionOpened(Position position)
        {
            // noop
        }

        public virtual void OnSignal(Signal signal)
        {
            // noop
        }

        public virtual void OnStopCancelled(Stop stop)
        {
            // noop
        }

        public virtual void OnStopExecuted(Stop stop)
        {
            // noop
        }

        public virtual void OnTrade(Trade trade)
        {
            // noop
        }
    }

    public class RiskComponent : StrategyComponent
    {
        public virtual void OnPositionChanged(Position position)
        {
            // noop
        }

        public virtual void OnPositionClosed(Position position)
        {
            // noop
        }

        public virtual void OnPositionOpened(Position position)
        {
            // noop
        }
    }

    public class ExecutionComponent : StrategyComponent
    {
        public virtual void OnExecutionReport(ExecutionReport report)
        {
            // noop
        }

        public virtual void OnOrder(Order order)
        {
            order.Provider = this.strategy.DetermineExecutionProvider(Instrument);
            order.StrategyId = this.strategy.Id;
            this.strategy.framework.OrderManager.Send(order);
        }

        public virtual void OnOrderFilled(Order order)
        {
            // noop
        }
    }

    public class ReportComponent : StrategyComponent
    {
        public virtual void OnAsk(Ask ask)
        {
            // noop
        }

        public virtual void OnBar(Bar bar)
        {
            // noop
        }

        public virtual void OnBid(Bid bid)
        {
            // noop
        }

        public virtual void OnFill(Fill fill)
        {
            // noop
        }

        public virtual void OnTrade(Trade trade)
        {
            // noop
        }
    }

    public class ComponentStrategy : InstrumentStrategy
    {
        private AlphaComponent alphaComponent;
        private DataComponent dataComponent;
        private ExecutionComponent executionComponent;
        private PositionComponent positionComponent;
        private ReportComponent reportComponent;
        private RiskComponent riskComponent;

        public AlphaComponent AlphaComponent
        {
            get
            {
                return this.alphaComponent;
            }
            set
            {
                SetComponent(this.alphaComponent, value);
            }
        }

        public DataComponent DataComponent
        {
            get
            {
                return this.dataComponent;
            }
            set
            {
                SetComponent(this.dataComponent, value);
            }
        }

        public ExecutionComponent ExecutionComponent
        {
            get
            {
                return this.executionComponent;
            }
            set
            {
                SetComponent(this.executionComponent, value);
            }
        }

        public PositionComponent PositionComponent
        {
            get
            {
                return this.positionComponent;
            }
            set
            {
                SetComponent(this.positionComponent, value);
            }
        }

        public ReportComponent ReportComponent
        {
            get
            {
                return this.reportComponent;
            }
            set
            {
                SetComponent(this.reportComponent, value);
            }
        }
        public RiskComponent RiskComponent
        {
            get
            {
                return this.riskComponent;
            }
            set
            {
                SetComponent(this.riskComponent, value);
            }
        }

        public ComponentStrategy(Framework framework, string name) : base(framework, name)
        {
            DataComponent = new DataComponent();
            AlphaComponent = new AlphaComponent();
            PositionComponent = new PositionComponent();
            RiskComponent = new RiskComponent();
            ExecutionComponent = new ExecutionComponent();
            ReportComponent = new ReportComponent();
        }

        private void SetComponent(StrategyComponent oldValue, StrategyComponent value)
        {
            oldValue = value;
            oldValue.strategy = this;
            oldValue.framework = this.framework;
        }
    }
}