using System;
using System.Drawing;
using SmartQuant;
using SmartQuant.Indicators;
using System.Linq;

namespace Samples.RSIDemo
{
    public class MyStrategy : InstrumentStrategy
    {
        private RSI rsi;
        private Group barsGroup;
        private Group fillGroup;
        private Group equityGroup;
        private Group rsiGroup;
        private Group buyLevelGroup;
        private Group sellLevelGroup;

        [Parameter]
        public double AllocationPerInstrument = 100000;

        [Parameter]
        public int RSILength = 10;

        [Parameter]
        public double BuyLevel = 20;

        [Parameter]
        public double SellLevel = 80;

        [Parameter]
        double Qty = 100;

        public MyStrategy(Framework framework, string name)
            : base(framework, name)
        {
        }

        protected override void OnStrategyStart()
        {
            Portfolio.Account.Deposit(AllocationPerInstrument, CurrencyId.USD, "Initial allocation");

            rsi = new RSI(Bars, RSILength);

            AddGroups();
        }

        protected override void OnBar(Instrument instrument, Bar bar)
        {
            // Add bar to bar series.
            Bars.Add(bar);

            Log(bar, barsGroup);

            // Log RSI, BuyLevel and SellLevel values.
            if (rsi.Count > 0)
            {
                Log(BuyLevel, buyLevelGroup);
                Log(SellLevel, sellLevelGroup);
                Log(rsi.Last, rsiGroup);
            }

            // Calculate performance.
            Portfolio.Performance.Update();

            Log(Portfolio.Value, equityGroup);

            // Check strategy logic.
            if (rsi.Count > 1)
            {
                if (!HasPosition(Instrument))
                {
                    if (rsi[rsi.Count - 1] < BuyLevel && rsi[rsi.Count - 2] > BuyLevel)
                    {
                        Order enterOrder = BuyOrder(Instrument, Qty, "Enter Long");
                        Send(enterOrder);
                    }
                    else if (rsi[rsi.Count - 1] > SellLevel && rsi[rsi.Count - 2] < SellLevel)
                    {
                        Order enterOrder = SellOrder(Instrument, Qty, "Enter Short");
                        Send(enterOrder);
                    }
                }
                else
                {
                    if (Position.Side == PositionSide.Long)
                    {
                        if (rsi[rsi.Count - 1] < BuyLevel && rsi[rsi.Count - 2] > BuyLevel)
                        {
                            Order enterOrder = BuyOrder(Instrument, Qty, "Add to Long");
                            Send(enterOrder);
                        }
                        else if (rsi[rsi.Count - 1] > SellLevel && rsi[rsi.Count - 2] < SellLevel)
                        {
                            Order reverseOrder = SellOrder(Instrument, Math.Abs(Position.Amount) + Qty, "Reverse to Short");
                            Send(reverseOrder);
                        }
                    }
                    else if (Position.Side == PositionSide.Short)
                    {
                        if (rsi[rsi.Count - 1] > SellLevel && rsi[rsi.Count - 2] < SellLevel)
                        {
                            Order enterOrder = SellOrder(Instrument, Qty, "Add to Short");
                            Send(enterOrder);
                        }
                        else if (rsi[rsi.Count - 1] < BuyLevel && rsi[rsi.Count - 2] > BuyLevel)
                        {
                            Order reverseOrder = BuyOrder(Instrument, Math.Abs(Position.Amount) + Qty, "Reverse to Long");
                            Send(reverseOrder);
                        }
                    }
                }
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
            equityGroup.Add("Pad", 2);
            equityGroup.Add("SelectorKey", Instrument.Symbol);

            // Create RSI values group.
            rsiGroup = new Group("RSI");
            rsiGroup.Add("Pad", 1);
            rsiGroup.Add("SelectorKey", Instrument.Symbol);
            rsiGroup.Add("Color", Color.Blue);

            // Create BuyLevel values group.
            buyLevelGroup = new Group("BuyLevel");
            buyLevelGroup.Add("Pad", 1);
            buyLevelGroup.Add("SelectorKey", Instrument.Symbol);
            buyLevelGroup.Add("Color", Color.Red);

            // Create SellLevel values group.
            sellLevelGroup = new Group("SellLevel");
            sellLevelGroup.Add("Pad", 1);
            sellLevelGroup.Add("SelectorKey", Instrument.Symbol);
            sellLevelGroup.Add("Color", Color.Red);

            // Add groups to manager.
            GroupManager.Add(barsGroup);
            GroupManager.Add(fillGroup);
            GroupManager.Add(equityGroup);
            GroupManager.Add(rsiGroup);
            GroupManager.Add(buyLevelGroup);
            GroupManager.Add(sellLevelGroup);
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

            strategy = new MyStrategy(framework, "RSI");

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

            strategy = new MyStrategy(framework, "RSI");

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

