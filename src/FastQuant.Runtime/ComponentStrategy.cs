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
        public Portfolio Portfolio=> this.strategy.Portfolio;
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
            return this.strategy.HasShortPosition(this.Instrument);
        }

        public bool HasShortPosition(double qty)
        {
            return this.strategy.HasShortPosition(this.Instrument, qty);
        }

        public void Buy(double qty)
        {
            throw new NotImplementedException();
        }
        public void Buy(double qty, string text)
        {
            throw new NotImplementedException();
        }

        public void BuyLimit(double qty, double price, string text)
        {
            throw new NotImplementedException();
        }

        public void BuyStop(double qty, double stopPx, string text)
        {
            throw new NotImplementedException();
        }

        public void Sell(double qty)
        {
            throw new NotImplementedException();
        }
        public void Sell(double qty, string text)
        {
            throw new NotImplementedException();
        }

        public void SellLimit(double qty, double price, string text)
        {
            throw new NotImplementedException();
        }

        public void SellStop(double qty, double stopPx)
        {
            throw new NotImplementedException();
        }

        public void SellStop(double qty, double stopPx, string text)
        {
            throw new NotImplementedException();
        }

        public Stop SetStop(double level, StopType type = StopType.Fixed, StopMode mode = StopMode.Absolute)
        {
            throw new NotImplementedException();
        }

        public void Log(DataObject data, Group group)
        {
            this.strategy.Log(data, group);
        }

        public void Log(double value, Group group)
        {
            this.strategy.Log(value, group);
        }

        public virtual void OnReminder(DateTime dateTime, object data)
        {
        }

        public virtual void OnStrategyStart()
        {
        }
    }

    public class DataComponent : StrategyComponent
    {
        public virtual void OnAsk(Ask ask)
        {
        }

        public virtual void OnBar(Bar bar)
        {
        }

        public virtual void OnBid(Bid bid)
        {
        }

        public virtual void OnTrade(Trade trade)
        {
        }
    }

    public class AlphaComponent : StrategyComponent
    {
        public virtual void OnAsk(Ask ask)
        {
        }

        public virtual void OnBar(Bar bar)
        {
        }

        public virtual void OnBid(Bid bid)
        {
        }

        public virtual void OnTrade(Trade trade)
        {
        }
    }

    public class PositionComponent : StrategyComponent
    {
        public virtual void OnAsk(Ask ask)
        {
        }

        public virtual void OnBar(Bar bar)
        {
        }

        public virtual void OnBid(Bid bid)
        {
        }

        public virtual void OnPositionChanged(Position position)
        {
        }

        public virtual void OnPositionClosed(Position position)
        {
        }

        public virtual void OnPositionOpened(Position position)
        {
        }

        public virtual void OnSignal(Signal signal)
        {
        }

        public virtual void OnStopCancelled(Stop stop)
        {
        }

        public virtual void OnStopExecuted(Stop stop)
        {
        }

        public virtual void OnTrade(Trade trade)
        {
        }
    }

    public class RiskComponent : StrategyComponent
    {
        public virtual void OnPositionChanged(Position position)
        {
        }

        public virtual void OnPositionClosed(Position position)
        {
        }

        public virtual void OnPositionOpened(Position position)
        {
        }
    }

    public class ExecutionComponent : StrategyComponent
    {
        public virtual void OnExecutionReport(ExecutionReport report)
        {
        }

        public virtual void OnOrder(Order order)
        {
            throw new NotImplementedException();
        }

        public virtual void OnOrderFilled(Order order)
        {
        }
    }

    public class ReportComponent : StrategyComponent
    {
        public virtual void OnAsk(Ask ask)
        {
        }

        public virtual void OnBar(Bar bar)
        {
        }

        public virtual void OnBid(Bid bid)
        {
        }

        public virtual void OnFill(Fill fill)
        {
        }

        public virtual void OnTrade(Trade trade)
        {
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