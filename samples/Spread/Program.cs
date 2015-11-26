using System;
using System.Drawing;
using SmartQuant;
using SmartQuant.Indicators;
using System.Linq;

namespace Samples.Spread
{
    public class MyStrategy : InstrumentStrategy
    {
        private Group barsGroup;
        private Group fillGroup;
        private Group equityGroup;

        [Parameter]
        public double AllocationPerInstrument = 100000;

        [Parameter]
        double Qty = 2;

        public MyStrategy(Framework framework, string name)
            : base(framework, name)
        {
        }

        protected override void OnStrategyStart()
        {
            Portfolio.Account.Deposit(AllocationPerInstrument, CurrencyId.USD, "Initial allocation");

            AddGroups();
        }

        protected override void OnBar(Instrument instrument, Bar bar)
        {
            // Add bar to bar series.
            Bars.Add(bar);

            Log(bar, barsGroup);

            // Calculate performance.
            Portfolio.Performance.Update();

            Log(Portfolio.Value, equityGroup);

            if (Bars.Count % 10 == 0)
            {
                if (!HasPosition(instrument))
                    Buy(instrument, Qty, "Buy spread");
                else
                    Sell(instrument, Qty, "Sell spread");
            }
        }

        protected override void OnFill(Fill fill)
        {
            Log(fill, fillGroup);
        }

        private void AddGroups()
        {
            // Create bars group.
            barsGroup = new Group("Bars");
            barsGroup.Add("Pad", DataObjectType.String, 0);
            barsGroup.Add("SelectorKey", Instrument.Symbol);

            // Create fills group.
            fillGroup = new Group("Fills");
            fillGroup.Add("Pad", 0);
            fillGroup.Add("SelectorKey", Instrument.Symbol);

            // Create equity group.
            equityGroup = new Group("Equity");
            equityGroup.Add("Pad", Instrument.Legs.Count + 1);
            equityGroup.Add("SelectorKey", Instrument.Symbol);

            // Add groups to manager.
            GroupManager.Add(barsGroup);
            GroupManager.Add(fillGroup);
            GroupManager.Add(equityGroup);
        }
    }

    public class Backtest : Scenario
    {
        private long barSize = 60;

        public Backtest(Framework framework)
            : base(framework)
        {
        }

        public override void Run()
        {
            Instrument spreadInsturment = InstrumentManager.Get("AAPL vs MSFT");

            // Add spread instrument if needed.
            if (spreadInsturment == null)
            {
                spreadInsturment = new Instrument(InstrumentType.Stock, "AAPL vs MSFT");

                InstrumentManager.Add(spreadInsturment);
            }

            spreadInsturment.Legs.Clear();

            // Add legs for spread instrument if needed.
            if (spreadInsturment.Legs.Count == 0)
            {
                spreadInsturment.Legs.Add(new Leg(InstrumentManager.Get("AAPL"), 1));
                spreadInsturment.Legs.Add(new Leg(InstrumentManager.Get("MSFT"), -12));
            }

            // Main strategy.
            strategy = new Strategy(framework, "SpreadTrading");

            // Create BuySide strategy and add trading instrument.
            MyStrategy buySide = new MyStrategy(framework, "BuySide");
            buySide.Instruments.Add(spreadInsturment);

            // Create SellSide strategy.
            SpreadSellSide sellSide = new SpreadSellSide(framework, "SellSide");
            sellSide.Global[SpreadSellSide.barSizeCode] = barSize;

            // Set SellSide as data and execution provider for BuySide strategy.
            buySide.DataProvider = sellSide;
            buySide.ExecutionProvider = sellSide;

            // Add strategies to main.
            strategy.AddStrategy(buySide);
            strategy.AddStrategy(sellSide);

            // Set DataSimulator's dates.
            DataSimulator.DateTime1 = new DateTime(2013, 01, 01);
            DataSimulator.DateTime2 = new DateTime(2013, 12, 31);

            // Add 1 minute bars (60 seconds) for spread instrument.
            BarFactory.Add(spreadInsturment, BarType.Time, barSize);

            // Run.
            StartStrategy();
        }
    }

    public class Realtime : Scenario
    {
        private long barSize;

        public Realtime(Framework framework)
            : base(framework)
        {
            // Set bar size in seconds. 60 seconds is 1 minute.
            barSize = 60;
        }

        public override void Run()
        {
            // Prepare running.
            Console.WriteLine("Prepare running in {0} mode...", StrategyManager.Mode);

            // Get spread instrument.
            Instrument spreadInsturment = InstrumentManager.Get("AAPL vs MSFT");

            // Add spread instrument if needed.
            if (spreadInsturment == null)
            {
                spreadInsturment = new Instrument(InstrumentType.Stock, "AAPL vs MSFT");
                InstrumentManager.Add(spreadInsturment);
            }

            // Add legs for spread instrument if needed.
            if (spreadInsturment.Legs.Count == 0)
            {
                spreadInsturment.Legs.Add(new Leg(InstrumentManager.Get("AAPL"), 1));
                spreadInsturment.Legs.Add(new Leg(InstrumentManager.Get("MSFT"), -1));
            }

            // Main strategy.
            strategy = new Strategy(framework, "SpreadTrading");

            // Create BuySide strategy and add trading instrument.
            MyStrategy buySide = new MyStrategy(framework, "BuySide");
            buySide.Instruments.Add(spreadInsturment);

            // Create SellSide strategy.
            SpreadSellSide sellSide = new SpreadSellSide(framework, "SellSide");
            sellSide.Global[SpreadSellSide.barSizeCode] = barSize;

            // Set SellSide as data and execution provider for BuySide strategy.
            buySide.DataProvider = sellSide;
            buySide.ExecutionProvider = sellSide;

            // Add strategies to main.
            strategy.AddStrategy(buySide);
            strategy.AddStrategy(sellSide);

            // Get provider for realtime.
            Provider quantRouter = ProviderManager.Providers["QuantRouter"] as Provider;

            if (quantRouter.Status == ProviderStatus.Disconnected)
                quantRouter.Connect();

            if (StrategyManager.Mode == StrategyMode.Paper)
            {
                // Set QuantRouter as data provider.
                sellSide.DataProvider = quantRouter as IDataProvider;
            }
            else if (StrategyManager.Mode == StrategyMode.Live)
            {
                // Set QuantRouter as data and execution provider.
                sellSide.DataProvider = quantRouter as IDataProvider;
                sellSide.ExecutionProvider = quantRouter as IExecutionProvider;
            }

            // Set null for event filter.
            EventManager.Filter = null;

            // Add 1 minute bars (60 seconds) for spread instrument.
            BarFactory.Add(spreadInsturment, BarType.Time, barSize);

            // Run.
            Console.WriteLine("Run in {0} mode.", StrategyManager.Mode);
            StartStrategy(StrategyManager.Mode);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var scenario = args.Contains("--realtime") ?  (Scenario)new Realtime(Framework.Current) : (Scenario)new Backtest(Framework.Current);
            scenario.Run();
        }
    }
}

