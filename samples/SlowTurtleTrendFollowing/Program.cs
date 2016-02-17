using System;
using System.Drawing;
using FastQuant;
using FastQuant.Indicators;
using System.Linq;

namespace Samples.SlowTurtleTrendFollowing
{
    public class Turtles : InstrumentStrategy
    {
        private int positionInBlock;
        private bool buyOnNewBlock;
        private bool sellOnNewBlock;
        private SMA fastSMA;
        private SMA slowSMA;
        private Group barsGroup;
        private Group fillGroup;
        private Group equityGroup;
        private Group fastSmaGroup;
        private Group slowSmaGroup;

        [Parameter]
        public double AllocationPerInstrument = 100000;

        [Parameter]
        double Qty = 100;

        [Parameter]
        int BarBlockSize = 6;

        [Parameter]
        int FastSMALength = 22;

        [Parameter]
        int SlowSMALength = 55;

        public Turtles(Framework framework, string name)
            : base(framework, name)
        {
        }

        protected override void OnStrategyStart()
        {
            Portfolio.Account.Deposit(AllocationPerInstrument, CurrencyId.USD, "Initial allocation");

            // Set up indicators.
            fastSMA = new SMA(Bars, FastSMALength);
            slowSMA = new SMA(Bars, SlowSMALength);

            AddGroups();
        }

        protected override void OnBarOpen(Instrument instrument, Bar bar)
        {
            double orderQty = Qty;

            // Set order qty if position already exist.
            if (HasPosition(Instrument))
                orderQty = Math.Abs(Position.Amount) + Qty;

            // Send trading orders if needed.
            if (positionInBlock == 0)
            {
                if (buyOnNewBlock)
                {
                    Order order = BuyOrder(Instrument, orderQty, "Reverse to Long");
                    Send(order);

                    buyOnNewBlock = false;
                }

                if (sellOnNewBlock)
                {
                    Order order = SellOrder(Instrument, orderQty, "Reverse to Short");
                    Send(order);

                    sellOnNewBlock = false;
                }
            }
        }

        protected override void OnBar(Instrument instrument, Bar bar)
        {
            // Add bar to bar series.
            Bars.Add(bar);

            Log(bar, barsGroup);

            if (fastSMA.Count > 0)
                Log(fastSMA.Last, fastSmaGroup);

            if (slowSMA.Count > 0)
                Log(slowSMA.Last, slowSmaGroup);

            // Calculate performance.
            Portfolio.Performance.Update();

            Log(Portfolio.Value, equityGroup);

            // Check strategy logic.
            if (fastSMA.Count > 0 && slowSMA.Count > 0)
            {
                Cross cross = fastSMA.Crosses(slowSMA, bar.DateTime);

                if (cross == Cross.Above)
                    buyOnNewBlock = true;

                if (cross == Cross.Below)
                    sellOnNewBlock = true;
            }

            positionInBlock = (positionInBlock++) % BarBlockSize;
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
            equityGroup.Add("Pad", 1);
            equityGroup.Add("SelectorKey", Instrument.Symbol);

            // Create fast sma values group.
            fastSmaGroup = new Group("FastSMA");
            fastSmaGroup.Add("Pad", 0);
            fastSmaGroup.Add("SelectorKey", Instrument.Symbol);
            fastSmaGroup.Add("Color", Color.Green);

            // Create slow sma values group.
            slowSmaGroup = new Group("SlowSMA");
            slowSmaGroup.Add("Pad", 0);
            slowSmaGroup.Add("SelectorKey", Instrument.Symbol);
            slowSmaGroup.Add("Color", Color.Red);

            // Add groups to manager.
            GroupManager.Add(barsGroup);
            GroupManager.Add(fillGroup);
            GroupManager.Add(equityGroup);
            GroupManager.Add(fastSmaGroup);
            GroupManager.Add(slowSmaGroup);
        }
    }

    public class Backtest : Scenario
    {
        private long barSize = 300;

        public Backtest(Framework framework)
            : base(framework)
        {
        }

        public override void Run()
        {
            Instrument instrument1 = InstrumentManager.Instruments["AAPL"];
            Instrument instrument2 = InstrumentManager.Instruments["MSFT"];

            strategy = new Turtles(framework, "Turtles");

            strategy.AddInstrument(instrument1);
            strategy.AddInstrument(instrument2);

            DataSimulator.DateTime1 = new DateTime(2013, 01, 01);
            DataSimulator.DateTime2 = new DateTime(2013, 12, 31);

            BarFactory.Add(instrument1, BarType.Time, barSize);
            BarFactory.Add(instrument2, BarType.Time, barSize);

            StartStrategy();
        }
    }

    public class Realtime : Scenario
    {
        private long barSize = 300;

        public Realtime(Framework framework)
            : base(framework)
        {

        }

        public override void Run()
        {
            Instrument instrument1 = InstrumentManager.Instruments["AAPL"];
            Instrument instrument2 = InstrumentManager.Instruments["MSFT"];

            strategy = new Turtles(framework, "Turtles");

            strategy.AddInstrument(instrument1);
            strategy.AddInstrument(instrument2);

            strategy.DataProvider = ProviderManager.GetDataProvider("QuantRouter");
            strategy.ExecutionProvider = ProviderManager.GetExecutionProvider("QuantRouter");

            DataSimulator.DateTime1 = new DateTime(2013, 01, 01);
            DataSimulator.DateTime2 = new DateTime(2013, 12, 31);

            BarFactory.Add(instrument1, BarType.Time, barSize);
            BarFactory.Add(instrument2, BarType.Time, barSize);

            StartStrategy();
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

