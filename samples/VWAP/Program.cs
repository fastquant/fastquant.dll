using System;
using System.Collections.Generic;
using System.Drawing;
using FastQuant;
using FastQuant.Indicators;
using System.Linq;

namespace Samples.VWAP
{
    public class MyStrategy : InstrumentStrategy
    {
        Group barChartGroup;
        Group fillChartGroup;

        public MyStrategy(Framework framework, string name)
            : base(framework, name)
        {

        }

        protected override void OnStrategyStart()
        {
            //
            barChartGroup = new Group("Bars");

            barChartGroup.Add("Pad", 0);
            barChartGroup.Add("SelectorKey", Instrument.Symbol);

            //
            fillChartGroup = new Group("Fills");

            fillChartGroup.Add("Pad", 0);
            fillChartGroup.Add("SelectorKey", Instrument.Symbol);

            //
            GroupManager.Add(barChartGroup);
            GroupManager.Add(fillChartGroup);
        }

        protected override void OnBar(Instrument instrument, Bar bar)
        {
            Log(bar, barChartGroup);

            Bars.Add(bar);

            if (Bars.Count % 20 == 0)
            {
                if (!HasPosition(instrument))
                    Buy(instrument, 5, "Buy");
                else
                    Sell(instrument, 5, "Sell");
            }

            Portfolio.Performance.Update();
        }

        protected override void OnFill(Fill fill)
        {
            Log(fill, fillChartGroup);
        }
    }

    public class VWAP_SellSide : SellSideInstrumentStrategy
    {
        class OrderInfo
        {
            public ExecutionCommand Command;
            public double QtyFilled;

            public OrderInfo(ExecutionCommand command)
            {
                Command = command;
            }

            public void AddOrder(Order order)
            {
                QtyFilled += order.Qty;
            }
        }
        Dictionary<Order, OrderInfo> orderTable = new Dictionary<Order, OrderInfo>();

        Group barChartGroup;
        Group fillChartGroup;

        public VWAP_SellSide(Framework framework, string name)
            : base(framework, name)
        {
        }

        protected override void OnStrategyStart()
        {
            //
            barChartGroup = new Group("Bars");

            barChartGroup.Add("Pad", DataObjectType.Int, 0);
            barChartGroup.Add("SelectorKey", DataObjectType.String, "VWAP " + Instrument.Symbol);

            //
            fillChartGroup = new Group("Fills");

            fillChartGroup.Add("Pad", DataObjectType.Int, 0);
            fillChartGroup.Add("SelectorKey", DataObjectType.String, "VWAP " + Instrument.Symbol);

            //
            GroupManager.Add(barChartGroup);
            GroupManager.Add(fillChartGroup);
        }

        protected override void OnSubscribe(Instrument instrument)
        {
            AddInstrument(instrument);
        }

        protected override void OnUnsubscribe(InstrumentList instruments)
        {
        }

        public override void OnSendCommand(ExecutionCommand command)
        {
            Order order;

            OrderInfo orderInfo = new OrderInfo(command);

            for (int i = 0; i < command.Qty; i++)
            {
                if (command.Side == OrderSide.Buy)
                {
                    order = BuyOrder(command.Instrument, 1, "VWAP Buy");
                }
                else
                {
                    order = SellOrder(command.Instrument, 1, "VWAP Sell");
                }

                orderTable[order] = orderInfo;

                Send(order);
            }
        }

        protected override void OnOrderFilled(Order order)
        {
            OrderInfo orderInfo = orderTable[order];

            orderInfo.QtyFilled += order.Qty;

            ExecutionCommand command = orderInfo.Command;

            if ((int)Math.Round(orderInfo.QtyFilled - command.Qty, 5) == 0)
                EmitFilled(order, orderInfo);
        }

        private void EmitFilled(Order order, OrderInfo orderInfo)
        {
            ExecutionCommand command = orderInfo.Command;
            Instrument instrument = command.Instrument;

            ExecutionReport execution = new ExecutionReport();

            execution.AvgPx = order.AvgPx;

            execution.Commission = 0;
            execution.CumQty = orderInfo.QtyFilled;
            execution.DateTime = Clock.DateTime;
            execution.ExecType = ExecType.ExecTrade;
            execution.Instrument = instrument;
            execution.LastPx = order.AvgPx;
            execution.LastQty = command.Qty;
            execution.LeavesQty = 0;
            execution.Order = command.Order;
            execution.OrdQty = command.Qty;
            execution.OrdStatus = OrderStatus.Filled;
            execution.OrdType = command.Order.Type;
            execution.Price = command.Order.Price;
            execution.Side = command.Order.Side;
            execution.StopPx = command.Order.StopPx;
            execution.Text = command.Order.Text;

            EmitExecutionReport(execution);
        }

        private void EmitPartiallyFilled(Order order, OrderInfo orderInfo)
        {
            ExecutionCommand command = orderInfo.Command;
            Instrument instrument = command.Instrument;

            ExecutionReport execution = new ExecutionReport();

            execution.AvgPx = order.AvgPx;

            execution.Commission = 0;
            execution.CumQty = orderInfo.QtyFilled;
            execution.DateTime = Clock.DateTime;
            execution.ExecType = ExecType.ExecTrade;
            execution.Instrument = instrument;
            execution.LastPx = execution.AvgPx;
            execution.LastQty = order.Qty;
            execution.LeavesQty = command.Qty - orderInfo.QtyFilled;
            execution.Order = command.Order;
            execution.OrdQty = command.Qty;
            execution.OrdStatus = OrderStatus.PartiallyFilled;
            execution.OrdType = command.Order.Type;
            execution.Price = command.Order.Price;
            execution.Side = command.Order.Side;
            execution.StopPx = command.Order.StopPx;
            execution.Text = command.Order.Text;

            EmitExecutionReport(execution);
        }

        protected override void OnFill(Fill fill)
        {
            Log(fill, fillChartGroup);
        }

        protected override void OnBar(Instrument instrument, Bar bar)
        {
            Log(bar, barChartGroup);
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
            Strategy metaStrategy = new Strategy(framework, "MetaStrategy");

            Instrument instrument1 = InstrumentManager.Instruments["CSCO"];
            Instrument instrument2 = InstrumentManager.Instruments["MSFT"];

            MyStrategy strategy = new MyStrategy(framework, "BuySell");

            strategy.Instruments.Add(instrument1);
            strategy.Instruments.Add(instrument2);

            VWAP_SellSide sellSideStrategy = new VWAP_SellSide(framework, "VWAP SellSide");

            strategy.ExecutionProvider = sellSideStrategy;
            strategy.DataProvider = sellSideStrategy;

            DataSimulator.DateTime1 = new DateTime(2013, 12, 01);
            DataSimulator.DateTime2 = new DateTime(2013, 12, 31);

            BarFactory.Add(instrument1, BarType.Time, 60);
            BarFactory.Add(instrument2, BarType.Time, 60);

            metaStrategy.AddStrategy(strategy);
            metaStrategy.AddStrategy(sellSideStrategy);

            this.strategy = metaStrategy;

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

