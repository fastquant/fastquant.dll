using System;
using System.Drawing;
using SmartQuant;
using SmartQuant.Indicators;
using System.Linq;
using System.Collections.Generic;

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

    public class OrderProcessor
    {
        private SpreadSellSide strategy;
        private ExecutionCommand command;
        private Order order;
        private Instrument spreadInstrument;
        private Dictionary<Order, Leg> orders;
        private bool isDone;

        public OrderProcessor(SpreadSellSide strategy, ExecutionCommand command)
        {
            // Add current processor to SellSide processor's list. 
            strategy.processors.AddLast(this);

            // Init OrderProcessor fields.
            this.strategy = strategy;
            this.command = command;
            order = command.Order;
            spreadInstrument = order.Instrument;
            orders = new Dictionary<Order, Leg>();

            // Send leg orders if order type is market.
            if (order.Type == OrderType.Market)
                SendLegOrders();
        }

        public bool IsDone
        {
            get { return isDone; }
        }

        public Order Order
        {
            get { return order; }
        }

        public void SendLegOrders()
        {
            // Create leg orders.
            if (order.Side == OrderSide.Buy)
            {
                foreach (Leg leg in spreadInstrument.Legs)
                {
                    double qty = Math.Round(Math.Abs(order.Qty * leg.Weight), MidpointRounding.AwayFromZero);

                    if (leg.Weight > 0)
                        orders.Add(strategy.BuyOrder(leg.Instrument, qty, "Buy leg"), leg);
                    else if (leg.Weight < 0)
                        orders.Add(strategy.SellOrder(leg.Instrument, qty, "Sell leg"), leg);
                }
            }
            else
            {
                foreach (Leg leg in spreadInstrument.Legs)
                {
                    double qty = Math.Round(Math.Abs(order.Qty * leg.Weight), MidpointRounding.AwayFromZero);

                    if (leg.Weight > 0)
                        orders.Add(strategy.SellOrder(leg.Instrument, qty, "Sell leg"), leg);
                    else if (leg.Weight < 0)
                        orders.Add(strategy.BuyOrder(leg.Instrument, qty, "Buy leg"), leg);
                }
            }

            // Send leg orders.
            foreach (Order ord in orders.Keys)
                strategy.Send(ord);
        }

        public void CancelLegOrders()
        {
            isDone = true;

            // Create ExecCancelled report for spread instrument.
            ExecutionReport report = new ExecutionReport();
            report.DateTime = strategy.Framework.Clock.DateTime;
            report.Order = order;
            report.Instrument = order.Instrument;
            report.OrdQty = order.Qty;
            report.ExecType = ExecType.ExecCancelled;
            report.OrdStatus = OrderStatus.Cancelled;
            report.OrdType = order.Type;
            report.Side = order.Side;
            report.CumQty = order.CumQty;
            report.LastQty = 0;
            report.LeavesQty = order.LeavesQty;
            report.LastPx = 0;
            report.AvgPx = 0;

            // Send report to SellSide strategy.
            strategy.EmitExecutionReport(report);
        }

        public void OnExecutionReport(ExecutionReport report)
        {
            if (orders.ContainsKey(report.Order))
            {
                double avgPrice = 0;
                bool isFilled = true;
                bool isCancelled = true;

                foreach (Order ord in orders.Keys)
                {
                    avgPrice += ord.AvgPx * orders[ord].Weight;

                    if (!ord.IsFilled)
                        isFilled = false;

                    if (!ord.IsCancelled)
                        isCancelled = false;
                }

                // If leg orders are filled.
                if (isFilled)
                {
                    // Create ExecTrade report for spread instrument.
                    ExecutionReport execution = new ExecutionReport();
                    execution.AvgPx = avgPrice;
                    execution.Commission = report.Commission;
                    execution.CumQty = report.CumQty;
                    execution.DateTime = report.DateTime;
                    execution.ExecType = ExecType.ExecTrade;
                    execution.Instrument = spreadInstrument;
                    execution.LastPx = execution.AvgPx;
                    execution.LastQty = order.Qty;
                    execution.LeavesQty = 0;
                    execution.Order = command.Order;
                    execution.OrdQty = order.Qty;
                    execution.OrdStatus = OrderStatus.Filled;
                    execution.OrdType = command.Order.Type;
                    execution.Price = command.Order.Price;
                    execution.Side = command.Order.Side;
                    execution.StopPx = command.Order.StopPx;
                    execution.Text = command.Order.Text;

                    // Send report to SellSide strategy.
                    strategy.EmitExecutionReport(execution);

                    isDone = true;
                }

                // If leg orders are cancelled.
                if (isCancelled)
                {
                    // Create ExecCancelled report for spread instrument.
                    ExecutionReport execution = new ExecutionReport();
                    execution.DateTime = report.DateTime;
                    execution.Order = command.Order;
                    execution.Instrument = spreadInstrument;
                    execution.OrdQty = order.Qty;
                    execution.ExecType = ExecType.ExecCancelled;
                    execution.OrdStatus = OrderStatus.Cancelled;
                    execution.OrdType = order.Type;
                    execution.Side = order.Side;
                    execution.CumQty = report.CumQty;
                    execution.LastQty = 0;
                    execution.LeavesQty = report.LeavesQty;
                    execution.LastPx = 0;
                    execution.AvgPx = 0;

                    // Send report to SellSide strategy.
                    strategy.EmitExecutionReport(execution);

                    isDone = true;
                }
            }
        }

        public void OnBid(Bid bid)
        {
            // Check conditions for send leg orders.
            if (order.Side == OrderSide.Sell)
            {
                switch (order.Type)
                {
                    case OrderType.Limit:
                        if (bid.Price >= order.Price)
                            SendLegOrders();
                        break;

                    case OrderType.Stop:
                        if (bid.Price <= order.StopPx)
                            SendLegOrders();
                        break;
                }
            }
        }

        public void OnAsk(Ask ask)
        {
            // Check conditions for send leg orders.
            if (order.Side == OrderSide.Buy)
            {
                switch (order.Type)
                {
                    case OrderType.Limit:
                        if (ask.Price <= order.Price)
                            SendLegOrders();
                        break;

                    case OrderType.Stop:
                        if (ask.Price >= order.StopPx)
                            SendLegOrders();
                        break;
                }
            }
        }

        public void OnTrade(Trade trade)
        {
            // Check conditions for send leg orders.
            switch (order.Type)
            {
                case OrderType.Limit:
                    switch (order.Side)
                    {
                        case OrderSide.Buy:
                            if (trade.Price <= order.Price)
                                SendLegOrders();
                            break;

                        case OrderSide.Sell:
                            if (trade.Price >= order.Price)
                                SendLegOrders();
                            break;
                    }
                    break;

                case OrderType.Stop:
                    switch (order.Side)
                    {
                        case OrderSide.Buy:
                            if (trade.Price >= order.StopPx)
                                SendLegOrders();
                            break;

                        case OrderSide.Sell:
                            if (trade.Price <= order.StopPx)
                                SendLegOrders();
                            break;
                    }
                    break;
            }
        }
    }

    public class SpreadSellSide : SellSideInstrumentStrategy
    {
        public const string barSizeCode = "BarSize";

        private long barSize;
        private Instrument spread;
        private bool isAskBidReady;
        private bool isTradeReady;
        private Dictionary<Instrument, Group[]> groups;
        internal System.Collections.Generic.LinkedList<OrderProcessor> processors;

        public SpreadSellSide(Framework framework, string name)
            : base(framework, name)
        {
        }

        internal Framework Framework
        {
            get { return framework; }
        }

        #region Market Data

        protected override void OnSubscribe(Instrument instrument)
        {
            // Get size of bar.
            barSize = (long)Global[barSizeCode];

            // Get spread instrument.
            spread = instrument;

            // Add legs instruments to bar factory.
            foreach (Leg leg in spread.Legs)
                BarFactory.Add(leg.Instrument, BarType.Time, barSize);

            // Remove instruments from strategy.
            Instruments.Clear();

            // Add legs instruments to strategy.
            foreach (Leg leg in spread.Legs)
                AddInstrument(leg.Instrument);

            processors = new System.Collections.Generic.LinkedList<OrderProcessor>();
            groups = new Dictionary<Instrument, Group[]>();

            AddGroups();
        }

        protected override void OnAsk(Instrument instrument, Ask ask)
        {
            if (instrument == spread)
                return;

            if (!isAskBidReady)
            {
                isAskBidReady = true;

                foreach (Leg leg in spread.Legs)
                {
                    if (leg.Instrument.Ask == null || leg.Instrument.Bid == null)
                    {
                        isAskBidReady = false;

                        return;
                    }
                }
            }

            //
            double askPrice = 0;
            int askSize = Int32.MaxValue;

            foreach (Leg leg in spread.Legs)
            {
                if (leg.Weight > 0)
                {
                    askPrice += leg.Instrument.Ask.Price * leg.Weight;
                    askSize = Math.Min(askSize, leg.Instrument.Ask.Size);
                }
                else if (leg.Weight < 0)
                {
                    askPrice += leg.Instrument.Bid.Price * leg.Weight;
                    askSize = Math.Min(askSize, leg.Instrument.Bid.Size);
                }
            }

            // Create new ask for spread instrument.
            EmitAsk(new Ask(ask.DateTime, 0, spread.Id, askPrice, askSize));
        }

        protected override void OnBid(Instrument instrument, Bid bid)
        {
            if (instrument == spread)
                return;

            if (!isAskBidReady)
            {
                isAskBidReady = true;

                foreach (Leg leg in spread.Legs)
                {
                    if (leg.Instrument.Ask == null || leg.Instrument.Bid == null)
                    {
                        isAskBidReady = false;

                        return;
                    }
                }
            }

            //
            double bidPrice = 0;
            int bidSize = Int32.MaxValue;

            foreach (Leg leg in spread.Legs)
            {
                if (leg.Weight > 0)
                {
                    bidPrice += leg.Instrument.Bid.Price * leg.Weight;
                    bidSize = Math.Min(bidSize, leg.Instrument.Bid.Size);
                }
                else if (leg.Weight < 0)
                {
                    bidPrice += leg.Instrument.Ask.Price * leg.Weight;
                    bidSize = Math.Min(bidSize, leg.Instrument.Ask.Size);
                }
            }

            // Create new bid for spread instrument.
            EmitBid(new Bid(bid.DateTime, 0, spread.Id, bidPrice, bidSize));
        }

        protected override void OnTrade(Instrument instrument, Trade trade)
        {
            if (instrument == spread)
                return;

            if (!isTradeReady)
            {
                isTradeReady = true;

                foreach (Leg leg in spread.Legs)
                {
                    if (leg.Instrument.Trade == null)
                    {
                        isTradeReady = false;

                        return;
                    }
                }
            }

            //
            double tradePrice = 0;
            int tradeSize = Int32.MaxValue;

            foreach (Leg leg in spread.Legs)
            {
                tradePrice += leg.Instrument.Trade.Price * leg.Weight;
                tradeSize = Math.Min(tradeSize, leg.Instrument.Trade.Size);
            }

            // Create new trade for spread instrument.
            EmitTrade(new Trade(trade.DateTime, 0, spread.Id, tradePrice, tradeSize));
        }

        protected override void OnBar(Instrument instrument, Bar bar)
        {
            Group[] instrumentGroups = null;

            // Add bar to bar group (index 0).
            if (groups.TryGetValue(instrument, out instrumentGroups))
                Log(bar, instrumentGroups[0]);
        }

        protected override void OnFill(Fill fill)
        {
            Group[] instrumentGroups = null;

            // Add fill to fill group (index 1).
            if (groups.TryGetValue(fill.Instrument, out instrumentGroups))
                Log(fill, instrumentGroups[1]);
        }

        public override void EmitBid(Bid bid)
        {
            // Emit bid to BuySide strategy.
            base.EmitBid(bid);

            System.Collections.Generic.LinkedListNode<OrderProcessor> processorNode = processors.First;

            // Send bid to order processors.
            while (processorNode != null)
            {
                OrderProcessor processor = processorNode.Value;
                processor.OnBid(bid);
                processorNode = processorNode.Next;
            }
        }

        public override void EmitAsk(Ask ask)
        {
            // Emit ask to BuySide strategy.
            base.EmitAsk(ask);

            System.Collections.Generic.LinkedListNode<OrderProcessor> processorNode = processors.First;

            // Send ask to order processors.
            while (processorNode != null)
            {
                OrderProcessor processor = processorNode.Value;
                processor.OnAsk(ask);
                processorNode = processorNode.Next;
            }
        }

        public override void EmitTrade(Trade trade)
        {
            // Emit trade to BuySide strategy.
            base.EmitTrade(trade);

            System.Collections.Generic.LinkedListNode<OrderProcessor> processorNode = processors.First;

            // Send trade to order processors.
            while (processorNode != null)
            {
                OrderProcessor processor = processorNode.Value;
                processor.OnTrade(trade);
                processorNode = processorNode.Next;
            }
        }

        #endregion

        #region Execution

        protected override void OnExecutionReport(ExecutionReport report)
        {
            System.Collections.Generic.LinkedListNode<OrderProcessor> processorNode = processors.First;

            while (processorNode != null)
            {
                OrderProcessor processor = processorNode.Value;
                processor.OnExecutionReport(report);

                if (processor.IsDone)
                    processors.Remove(processor);

                processorNode = processorNode.Next;
            }
        }

        public override void OnSendCommand(ExecutionCommand command)
        {
            Order order = command.Order;

            // Create ExecNew report.
            ExecutionReport report = new ExecutionReport();
            report.DateTime = framework.Clock.DateTime;
            report.Order = order;
            report.Instrument = order.Instrument;
            report.OrdQty = order.Qty;
            report.ExecType = ExecType.ExecNew;
            report.OrdStatus = OrderStatus.New;
            report.OrdType = order.Type;
            report.Side = order.Side;

            // Send report to BuySide strategy.
            EmitExecutionReport(report);

            // Create new order processor.
            new OrderProcessor(this, command);
        }

        public override void OnCancelCommand(ExecutionCommand command)
        {
            // Search for needed order processor.
            foreach (OrderProcessor processor in processors)
            {
                if (processor.Order == command.Order)
                {
                    // Cancel leg orders.
                    processor.CancelLegOrders();

                    // Remove order processor.
                    if (processor.IsDone)
                        processors.Remove(processor);

                    return;
                }
            }
        }

        public override void OnReplaceCommand(ExecutionCommand command)
        {
            Console.WriteLine("{0} Replace command is not supported.", GetType().Name);
        }

        #endregion

        private void AddGroups()
        {
            int pad = 0;

            foreach (Leg leg in spread.Legs)
            {
                ++pad;
                groups[leg.Instrument] = new Group[2];

                // Create bars group.
                Group barsGroup = new Group("Bars");
                barsGroup.Add("Pad", DataObjectType.Int32, pad);
                barsGroup.Add("SelectorKey", DataObjectType.String, spread.Symbol);

                // Create fills group.
                Group fillGroup = new Group("Fills");
                fillGroup.Add("Pad", DataObjectType.Int32, pad);
                fillGroup.Add("SelectorKey", DataObjectType.String, spread.Symbol);

                // Add groups to manager.
                GroupManager.Add(barsGroup);
                GroupManager.Add(fillGroup);

                groups[leg.Instrument][0] = barsGroup;
                groups[leg.Instrument][1] = fillGroup;
            }
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

