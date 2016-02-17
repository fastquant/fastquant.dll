using System;
using System.Drawing;
using FastQuant;
using FastQuant.Indicators;
using System.Linq;

namespace Samples.ChannelBreakout
{
    public class MyStrategy : InstrumentStrategy
    {
        private double highest;
        private double lowest;
        private Group barsGroup;
        private Group fillGroup;
        private Group equityGroup;
        private Group highestGroup;
        private Group lowestGroup;

        [Parameter]
        public double AllocationPerInstrument = 100000;

        [Parameter]
        double Qty = 100;

        [Parameter]
        int Length = 8;

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

            // Add bar to group.
            Log(bar, barsGroup);

            // Add highest value to group.
            if (highest != 0)
                Log(highest, highestGroup);

            // Add lowest value to group.
            if (lowest != 0)
                Log(lowest, lowestGroup);

            // Calculate performance.
            Portfolio.Performance.Update();

            // Add equity to group.
            Log(Portfolio.Value, equityGroup);

            // Check strategy logic.
            if (highest != 0 && lowest != 0)
            {
                if (!HasPosition(instrument))
                {
                    // Enter long/short.
                    if (bar.Close > highest)
                    {
                        Order enterOrder = BuyOrder(Instrument, Qty, "Enter Long");
                        Send(enterOrder);
                    }
                    else if (bar.Close < lowest)
                    {
                        Order enterOrder = SellOrder(Instrument, Qty, "Enter Short");
                        Send(enterOrder);
                    }
                }
                else
                {
                    // Reverse to long/short.
                    if (Position.Side == PositionSide.Long && bar.Close < lowest)
                    {
                        Order reverseOrder = SellOrder(Instrument, Math.Abs(Position.Amount) + Qty, "Reverse to Short");
                        Send(reverseOrder);
                    }
                    else if (Position.Side == PositionSide.Short && bar.Close > highest)
                    {
                        Order reverseOrder = BuyOrder(Instrument, Math.Abs(Position.Amount) + Qty, "Reverse to Long");
                        Send(reverseOrder);
                    }
                }
            }

            // Calculate channel's highest/lowest values.
            if (Bars.Count > Length)
            {
                highest = Bars.HighestHigh(Length);
                lowest = Bars.LowestLow(Length);
            }
        }

        protected override void OnFill(Fill fill)
        {
            // Add fill to group.
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

            // Create channel's highest values group.
            highestGroup = new Group("Highest");
            highestGroup.Add("Pad", 0);
            highestGroup.Add("SelectorKey", Instrument.Symbol);
            highestGroup.Add("Color", Color.Green);

            // Create channel's lowest values group.
            lowestGroup = new Group("Lowest");
            lowestGroup.Add("Pad", 0);
            lowestGroup.Add("SelectorKey", Instrument.Symbol);
            lowestGroup.Add("Color", Color.Red);

            // Add groups to manager.
            GroupManager.Add(barsGroup);
            GroupManager.Add(fillGroup);
            GroupManager.Add(equityGroup);
            GroupManager.Add(highestGroup);
            GroupManager.Add(lowestGroup);
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

            strategy = new MyStrategy(framework, "ChannelBreakout");

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

            strategy = new MyStrategy(framework, "BollingerBands");

            strategy.AddInstrument(instrument1);
            strategy.AddInstrument(instrument2);

            strategy.DataProvider = ProviderManager.GetDataProvider("QuantRouter");
            strategy.ExecutionProvider = ProviderManager.GetExecutionProvider("QuantRouter");

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

