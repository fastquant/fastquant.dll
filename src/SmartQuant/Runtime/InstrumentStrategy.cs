using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace SmartQuant
{
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
                return DetermineDataProvider(this, Instrument);
            }
            set
            {
                SetRawDataProvider(value);
                foreach (var s in Strategies)
                    s.DataProvider = this.rawDataProvider;
            }
        }

        public override IExecutionProvider ExecutionProvider
        {
            get
            {
                return DetermineExecutionProvider(Instrument);
            }
            set
            {
                SetRawExecutionProvider(value);
                foreach (var s in Strategies)
                    s.ExecutionProvider = this.rawExecutionProvider;
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
                    // Create a child strategy for each instrument
                    foreach (var instrument in Instruments)
                    {
                        var strategy = CreateChildInstrumentStrategy(instrument);
                        AddStrategy(strategy, false);
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

        public bool HasPosition() => base.HasPosition(Instrument);

        public bool HasPosition(PositionSide side, double qty) => base.HasPosition(Instrument, side, qty);

        public bool HasLongPosition() => base.HasLongPosition(Instrument);

        public bool HasLongPosition(double qty) => base.HasLongPosition(Instrument, qty);

        public bool HasShortPosition() => base.HasShortPosition(Instrument);

        public bool HasShortPosition(double qty) => base.HasShortPosition(Instrument, qty);


        //public void AddInstance(Instrument instrument, InstrumentStrategy strategy)
        //{
        //    strategy.Instrument = instrument;
        //    strategy.Instruments.Add(instrument);
        //    strategy.Portfolio.Add(instrument);
        //    strategy.raiseEvents = true;
        //    strategy.SetRawDataProvider(this.rawDataProvider);
        //    strategy.SetRawExecutionProvider(this.rawExecutionProvider);
        //    this.method_8(strategy);
        //    if (Instruments.GetById(instrument.Id) == null)
        //        Instruments.Add(instrument);
        //    strategy.Status = StrategyStatus.Running;
        //    strategy.OnStrategyInit();
        //    strategy.OnStrategyStart();
        //}

        internal override void EmitStrategyStart()
        {
            Status = StrategyStatus.Running;
            foreach(var s in Strategies)
            {
                s.Status = StrategyStatus.Running;
                base.method_3(s, s.Instruments, s.Id);
                s.OnStrategyStart();
            }
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
                var strategy = CreateChildInstrumentStrategy(instrument);
                AddStrategy(strategy, true);
            }
        }

        private Strategy CreateChildInstrumentStrategy(Instrument instrument)
        {
            var name = $"{Name} ({instrument.Symbol})";
            var strategy = (InstrumentStrategy)Activator.CreateInstance(GetType(), new object[] { this.framework, name });
            strategy.Instrument = instrument;
            strategy.Instruments.Add(instrument);
            strategy.SubscriptionList.Add(instrument, DetermineDataProvider(this, instrument));
            strategy.Portfolio = GetOrCreatePortfolio(strategy.Name);
            strategy.Portfolio.Add(instrument);
            strategy.raiseEvents = true;
            strategy.IsInstance = true;
            strategy.SetRawDataProvider(DataProvider);
            strategy.SetRawExecutionProvider(ExecutionProvider);

            // Set Parameters for substrategy
            var fields = strategy.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            // TODO: code1 vs code2
            // code 1
            foreach (var f in fields)
                if (f.GetCustomAttributes(typeof(ParameterAttribute), true).Any())
                    f.SetValue(strategy, f.GetValue(this));
            // code 2
            //foreach (var f in fields.TakeWhile(f => f.GetCustomAttributes(typeof(ParameterAttribute), true).Any()))
            //    f.SetValue(strategy, f.GetValue(this));

            return strategy;
        }

        private void method_8(InstrumentStrategy strategy)
        {
            AddStrategy(strategy, false);
        }
    }
}
