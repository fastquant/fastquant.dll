using System;
using System.Drawing;
#if USE_FASTQUANT
using FastQuant;
using FastQuant.Indicators;
#else
using SmartQuant;
using SmartQuant.Indicators;
#endif
namespace Demo
{
    public class MyStrategy : InstrumentStrategy
    {
        private BBU bbu;
        private BBL bbl;
        private SMA sma;
        private Order exitOrder;
        private Group barsGroup;
        private Group fillGroup;
        private Group equityGroup;
        private Group bbuGroup;
        private Group bblGroup;
        private Group smaGroup;

        [Parameter]
        public double AllocationPerInstrument = 100000;

        [Parameter]
        public double Qty = 100;

        [Parameter]
        public int Length = 10;

        [Parameter]
        public double K = 2;

        public MyStrategy(Framework framework, string name)
            : base(framework, name)
        {
        }
#if USE_SOURCE
        protected internal override void OnStrategyStart()
#else
        protected override void OnStrategyStart()
#endif
        {
            Portfolio.Account.Deposit(AllocationPerInstrument, CurrencyId.USD, "Initial allocation");

            bbu = new BBU(Bars, Length, K);
            bbl = new BBL(Bars, Length, K);
            sma = new SMA(Bars, Length);

            AddGroups();
        }

        protected override void OnExecutionReport(ExecutionReport report)
        {
            Console.WriteLine(report);
        }

        protected override void OnBar(Instrument instrument, Bar bar)
        {
            // Add bar to bar series.
            Bars.Add(bar);

            // Add bar to group.
            Log(bar, barsGroup);

            // Add upper bollinger band value to group.
            if (bbu.Count > 0)
                Log(bbu.Last, bbuGroup);

            // Add lower bollinger band value to group.
            if (bbl.Count > 0)
                Log(bbl.Last, bblGroup);

            // Add simple moving average value bands to group.
            if (sma.Count > 0)
                Log(sma.Last, smaGroup);

            // Calculate performance.
            Portfolio.Performance.Update();

            // Add equity to group.
            Log(Portfolio.Value, equityGroup);

            // Check strategy logic.
            if (!HasPosition(instrument))
            {
                if (bbu.Count > 0 && bar.Close >= bbu.Last)
                {
                    Order enterOrder = SellOrder(Instrument, Qty, "Enter");
                    Send(enterOrder);
                }
                else if (bbl.Count > 0 && bar.Close <= bbl.Last)
                {
                    Order enterOrder = BuyOrder(Instrument, Qty, "Enter");
                    Send(enterOrder);
                }
            }
            else
                UpdateExitLimit();
        }

        protected override void OnFill(Fill fill)
        {
            // Add fill to group.
            Log(fill, fillGroup);
        }

        protected override void OnPositionOpened(Position position)
        {
            UpdateExitLimit();
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

            // Create BBU group.
            bbuGroup = new Group("BBU");
            bbuGroup.Add("Pad", 0);
            bbuGroup.Add("SelectorKey", Instrument.Symbol);
            bbuGroup.Add("Color", Color.Blue);

            // Create BBL group.
            bblGroup = new Group("BBL");
            bblGroup.Add("Pad", 0);
            bblGroup.Add("SelectorKey", Instrument.Symbol);
            bblGroup.Add("Color", Color.Blue);

            // Create SMA group.
            smaGroup = new Group("SMA");
            smaGroup.Add("Pad", 0);
            smaGroup.Add("SelectorKey", Instrument.Symbol);
            smaGroup.Add("Color", Color.Yellow);

            // Add groups to manager.
            GroupManager.Add(barsGroup);
            GroupManager.Add(fillGroup);
            GroupManager.Add(equityGroup);
            GroupManager.Add(bbuGroup);
            GroupManager.Add(bblGroup);
            GroupManager.Add(smaGroup);
        }

        private void UpdateExitLimit()
        {
            if (exitOrder != null && !exitOrder.IsDone)
                Cancel(exitOrder);

            if (HasPosition(Instrument))
            {
                if (Position.Side == PositionSide.Long)
                    exitOrder = SellLimitOrder(Instrument, Qty, sma.Last, "Exit");
                else
                    exitOrder = BuyLimitOrder(Instrument, Qty, sma.Last, "Exit");

                Send(exitOrder);
            }
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
           // Instrument instrument1 = InstrumentManager.Instruments["AAPL"];
            Instrument instrument2 = InstrumentManager.Instruments["MSFT"];

            strategy = new MyStrategy(framework, "BollingerBands");

          //  strategy.AddInstrument(instrument1);
            strategy.AddInstrument(instrument2);

            DataSimulator.DateTime1 = new DateTime(2013, 01, 01);
            DataSimulator.DateTime2 = new DateTime(2013, 12, 17);

         //   BarFactory.Add(instrument1, BarType.Time, barSize);
            BarFactory.Add(instrument2, BarType.Time, barSize);

            StartStrategy();
        }
    }
}

