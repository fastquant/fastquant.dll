using System;
using System.Drawing;
using SmartQuant;
using SmartQuant.Indicators;
using System.Linq;
using SmartQuant.Component;

namespace Samples.Components
{
    class MyAlpha : AlphaComponent
    {
        int length = 55;

        double highestHigh;
        double lowestLow;

        int count = 0;

        public override void OnBar(Bar bar)
        {
            if (Bars.Count < length)
                return;

            highestHigh = Bars.HighestHigh(length);
            lowestLow = Bars.LowestLow(length);
        }

        public override void OnTrade(Trade trade)
        {
            if (Bars.Count > length)
            {
                if (trade.Price > highestHigh + Bars[Bars.Count - 1].Range)
                {
                    count++;

                    if (count >= 10)
                    {
                        Signal(+1);

                        return;
                    }
                }
                else
                    count = 0;

                if (trade.Price < lowestLow - Bars[Bars.Count - 1].Range)
                {
                    Signal(-1);

                    return;
                }
            }

            Signal(0);
        }
    }

    class MyData : DataComponent
    {
        public override void OnBar(Bar bar)
        {
            Bars.Add(bar);
        }
    }

    class MyExecution : ExecutionComponent
    {
        int OCACount = 0;

        Order stopLossOrder;

        public override void OnOrder(Order order)
        {
            if (order.Text == "TakeProfit")
                order.OCA = (++OCACount).ToString();

            if (order.Text == "StopLoss")
                order.OCA = OCACount.ToString();

            if (order.Text == "StopLoss")
                stopLossOrder = order;

            if (order.Text == "StopExit" || order.Text == "StopSession")
                framework.OrderManager.Cancel(stopLossOrder);

            base.OnOrder(order);
        }
    }

    class MyPosition : PositionComponent
    {
        bool canEntry = true;
        bool inSession = true;
        bool hold = false;
        int holdCount = 0;

        public override void OnSignal(Signal signal)
        {
            if (inSession && canEntry)
            {
                if (signal.Value == +1 && !HasLongPosition(1))
                {
                    canEntry = false;

                    Buy(1);

                    return;
                }

                if (signal.Value == -1 && !HasShortPosition(1))
                {
                    canEntry = false;

                    Sell(1);

                    return;
                }
            }
        }

        public override void OnBar(Bar bar)
        {
            if (bar.DateTime.TimeOfDay > new TimeSpan(15, 45, 00))
            {
                inSession = false;

                if (HasLongPosition(1))
                    Sell(1, "StopSession");

                if (HasShortPosition(1))
                    Buy(1, "StopSession");
            }
            else
                inSession = true;

            if (hold)
                if (++holdCount == 5)
                {
                    hold = false;
                    holdCount = 0;
                    canEntry = true;
                }
        }

        public override void OnPositionChanged(Position position)
        {
            if (HasLongPosition(1))
                SellLimit(position.Qty, position.Price + 1, "TakeProfit");

            if (HasShortPosition(1))
                BuyLimit(position.Qty, position.Price - 1, "TakeProfit");
        }

        public override void OnPositionOpened(Position position)
        {
            SetStop(position.Price * 0.995, StopType.Trailing);
        }

        public override void OnPositionClosed(Position position)
        {
            hold = true;
            canEntry = false;
        }

        public override void OnStopExecuted(Stop stop)
        {
            if (HasLongPosition(1))
                Sell(1, "StopExit");

            if (HasShortPosition(1))
                Buy(1, "StopExit");

            canEntry = true;
        }
    }

    class MyReport : ReportComponent
    {
        Group barsChartGroup;
        Group fillChartGroup;
        Group equityChartGroup;

        public override void OnStrategyStart()
        {
            barsChartGroup = new Group("Bars");

            barsChartGroup.Add("Pad", 0);
            barsChartGroup.Add("SelectorKey", Instrument.Symbol);

            // add fill group

            fillChartGroup = new Group("Fills");

            fillChartGroup.Add("Pad", 0);
            fillChartGroup.Add("SelectorKey", Instrument.Symbol);

            // add equity group

            equityChartGroup = new Group("Equity");

            equityChartGroup.Add("Pad", 1);
            equityChartGroup.Add("SelectorKey", Instrument.Symbol);

            // register groups

            GroupManager.Add(barsChartGroup);
            GroupManager.Add(fillChartGroup);
            GroupManager.Add(equityChartGroup);
        }

        public override void OnBar(Bar bar)
        {
            // log bars

            Log(bar, barsChartGroup);

            // log equity

            Log(Portfolio.Value, equityChartGroup);

            // update performance

            Portfolio.Performance.Update();
        }

        public override void OnFill(Fill fill)
        {
            Log(fill, fillChartGroup);
        }
    }

    class MyRisk : RiskComponent
    {
        public override void OnPositionChanged(Position position)
        {
            if (HasLongPosition(1))
                SellStop(position.Qty, position.Price * 0.9975, "StopLoss");

            if (HasShortPosition(1))
                BuyStop(position.Qty, position.Price * (2 - 0.9975), "StopLoss");
        }
    }
    public class MyStrategy : ComponentStrategy
    {
        public MyStrategy(Framework framework, string name)
            : base(framework, name)
        {
            DataComponent = new MyData();
            AlphaComponent = new MyAlpha();
            PositionComponent = new MyPosition();
            RiskComponent = new MyRisk();
            ExecutionComponent = new MyExecution();
            ReportComponent = new MyReport();
        }
    }


    public class MyScenario : Scenario
    {
        public MyScenario(Framework framework)
            : base(framework)
        {

        }

        public override void Run()
        {
            strategy = new MyStrategy(framework, "ComponentStrategy");

            Instrument instrument = InstrumentManager.Instruments["AAPL"];

            strategy.AddInstrument(instrument);

            //DataSimulator.DateTime1 = new DateTime(2012, 12, 03);
            //DataSimulator.DateTime2 = new DateTime(2013, 12, 04);  

            BarFactory.Add(instrument, BarType.Time, 60);

            StartStrategy();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var scenario = new MyScenario(Framework.Current);
            scenario.Run();
        }
    }
}

