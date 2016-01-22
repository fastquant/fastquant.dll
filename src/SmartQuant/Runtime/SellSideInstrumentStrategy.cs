using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace SmartQuant
{
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

        public new virtual void EmitBid(Bid bid)
        {
            this.framework.EventManager.OnEvent(new Bid(bid) { ProviderId = (byte)Id });
        }

        public virtual void EmitBid(DateTime dateTime, int instrumentId, double price, int size)
        {
            this.framework.EventManager.OnEvent(new Bid(dateTime, (byte)Id, instrumentId, price, size));
        }

        public new virtual void EmitAsk(Ask ask)
        {
            this.framework.EventManager.OnEvent(new Ask(ask) { ProviderId = (byte)Id });
        }

        public virtual void EmitAsk(DateTime dateTime, int instrumentId, double price, int size)
        {
            this.framework.EventManager.OnEvent(new Ask(dateTime, (byte)Id, instrumentId, price, size));
        }

        public new virtual void EmitTrade(Trade trade)
        {
            this.framework.EventManager.OnEvent(new Trade(trade) { ProviderId = (byte)Id });
        }

        public virtual void EmitTrade(DateTime dateTime, int instrumentId, double price, int size)
        {
            this.framework.EventManager.OnEvent(new Trade(dateTime, (byte)Id, instrumentId, price, size));
        }

        public new virtual void EmitBar(Bar bar)
        {
            this.framework.EventManager.OnEvent(bar);
        }

        public new virtual void EmitExecutionReport(ExecutionReport report)
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
                        if (this.subStrategiesByInstrument[instrument.Id] == null)
                            CreateSubSellSideInstrumentStrategy(instrument, true, false);
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
            if (this.subStrategiesByInstrument[instrument.Id] == null)
            {
                var subStrategy = CreateSubSellSideInstrumentStrategy(instrument, false, true);
                subStrategy.OnStrategyStart();
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
            if (this.subStrategiesByInstrument[instrument.Id] != null)
            {
                var subStrategy = this.subStrategiesByInstrument[instrument.Id].First.Data as SellSideInstrumentStrategy;
                subStrategy.OnUnsubscribe(instrument);
            }
        }

        public override void Send(ExecutionCommand command)
        {
            var strategy = this.subStrategiesByInstrument[command.Order.Instrument.Id].First.Data as SellSideInstrumentStrategy;
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

        private SellSideInstrumentStrategy CreateSubSellSideInstrumentStrategy(Instrument instrument, bool bool_4, bool bool_5)
        {
            var name = $"{Name}({instrument}";
            var strategy = (SellSideInstrumentStrategy)Activator.CreateInstance(GetType(), new object[] { this.framework, name });
            strategy.Instrument = instrument;
            if (bool_4)
                strategy.Instruments.Add(instrument);
            strategy.ClientId = ClientId;
            strategy.SetRawDataProvider(DataProvider);
            strategy.SetRawExecutionProvider(ExecutionProvider);
            strategy.raiseEvents = true;
            strategy.IsInstance = true;
            SetSubStrategyParameters(strategy);
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
            if (this.subStrategiesByInstrument[instrument.Id] == null)
            {
                linkedList = new LinkedList<Strategy>();
                this.subStrategiesByInstrument[instrument.Id] = linkedList;
            }
            else
            {
                linkedList = this.subStrategiesByInstrument[instrument.Id];
            }
            linkedList.Add(strategy);
        }

        private void SetSubStrategyParameters(SellSideInstrumentStrategy strategy)
        {
            var fields = strategy.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var f in fields)
                if (f.GetCustomAttributes(typeof(ParameterAttribute), true).Any())
                    f.SetValue(strategy, f.GetValue(this));
        }
    }
}
