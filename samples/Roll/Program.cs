using System;
using System.Collections.Generic;
using System.Drawing;
using SmartQuant;
using SmartQuant.Indicators;
using System.Linq;

namespace Samples.Roll
{
    public class MyStrategy : InstrumentStrategy
    {
        private Group barsGroup;
        private Group fillGroup;
        private Group equityGroup;

        [Parameter]
        public double AllocationPerInstrument = 100000;

        [Parameter]
        public double Qty = 100;

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

            // Calculate performance.
            Portfolio.Performance.Update();

            // Add equity to group.
            Log(Portfolio.Value, equityGroup);

            // Check strategy logic.
            if (Bars.Count > 2)
            {
                if (!HasPosition(Instrument))
                {
                    if (Bars[Bars.Count - 1].Close > Bars[Bars.Count - 2].Close &&
                        Bars[Bars.Count - 2].Close > Bars[Bars.Count - 3].Close)
                    {
                        Order enterOrder = BuyOrder(Instrument, Qty, "Enter Long");
                        Send(enterOrder);
                    }
                    else if (Bars[Bars.Count - 1].Close < Bars[Bars.Count - 2].Close &&
                        Bars[Bars.Count - 2].Close < Bars[Bars.Count - 3].Close)
                    {
                        Order enterOrder = SellOrder(Instrument, Qty, "Enter Short");
                        Send(enterOrder);
                    }
                }
                else
                {
                    if (Position.Side == PositionSide.Short &&
                        Bars[Bars.Count - 1].Close > Bars[Bars.Count - 2].Close &&
                        Bars[Bars.Count - 2].Close > Bars[Bars.Count - 3].Close)
                    {
                        Order reverseOrder = BuyOrder(Instrument, Math.Abs(Position.Amount) + Qty, "Reverse to Long");
                        Send(reverseOrder);
                    }
                    else if (Position.Side == PositionSide.Long &&
                        Bars[Bars.Count - 1].Close < Bars[Bars.Count - 2].Close &&
                        Bars[Bars.Count - 2].Close < Bars[Bars.Count - 3].Close)
                    {
                        Order reverseOrder = SellOrder(Instrument, Math.Abs(Position.Amount) + Qty, "Reverse to Short");
                        Send(reverseOrder);
                    }
                }
            }
        }

        protected override void OnFill(Fill fill)
        {
            // Add fill to fill group.
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

            // Add groups to manager.
            GroupManager.Add(barsGroup);
            GroupManager.Add(fillGroup);
            GroupManager.Add(equityGroup);
        }
    }

    public class Backtest : Scenario
    {
        // Bar size in seconds. 14400 seconds is 4 hour.
        private long barSize = 14400;

        public Backtest(Framework framework)
            : base(framework)
        {
        }

        public override void Run()
        {
            // Get synthetic trading instrument.
            Instrument instrument1 = InstrumentManager.Instruments["NQ"];

            // Init roll info - leg index, symbol and maturity date.
            List<RollInfo> rollInfo = new List<RollInfo>()
            {
                new RollInfo(0, "NQZ3", new DateTime(2013, 12, 20)),
                new RollInfo(1, "NQH4", new DateTime(2014, 03, 21)),
            };

            // Add legs.
            for (var i = 0; i < rollInfo.Count; i++)
                instrument1.Legs.Add(new Leg(InstrumentManager.Instruments[rollInfo[i].Symbol]));

            // Main strategy.
            strategy = new Strategy(framework, "Roll");

            // Create BuySide strategy and add trading instrument.
            MyStrategy buySide = new MyStrategy(framework, "BuySide");
            buySide.Instruments.Add(instrument1);

            // Create SellSide strategy.
            RollSellSide sellSide = new RollSellSide(framework, "SellSide");
            sellSide.Global[RollSellSide.barSizeCode] = barSize;
            sellSide.Global[RollSellSide.rollInfoCode] = rollInfo;

            // Set SellSide as data and execution provider for BuySide strategy.
            buySide.DataProvider = sellSide;
            buySide.ExecutionProvider = sellSide;

            // Add strategies to main.
            strategy.AddStrategy(buySide);
            strategy.AddStrategy(sellSide);

            // Set DataSimulator's dates.
            DataSimulator.DateTime1 = new DateTime(2013, 01, 01);
            DataSimulator.DateTime2 = new DateTime(2013, 12, 31);

            // Add 4 hours bars (14400 seconds) for ins1.
            BarFactory.Add(instrument1, BarType.Time, barSize);

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
            // Set bar size in seconds. 14400 seconds is 4 hour.
            barSize = 14400;
        }

        public override void Run()
        {
            // Synthetic instrument.
            Instrument instrument1 = InstrumentManager.Instruments["NQ"];

            // Init roll info - leg index, symbol and maturity date.
            List<RollInfo> rollInfo = new List<RollInfo>()
            {
                new RollInfo(0, "NQZ3", new DateTime(2013, 12, 20)),
                new RollInfo(1, "NQH4", new DateTime(2014, 03, 21)),
            };

            // Add legs.
            for (var i = 0; i < rollInfo.Count; i++)
                instrument1.Legs.Add(new Leg(InstrumentManager.Instruments[rollInfo[i].Symbol]));

            // Main strategy.
            strategy = new Strategy(framework, "Roll");

            // Create BuySide strategy and add trading instrument.
            MyStrategy buySide = new MyStrategy(framework, "BuySide");
            buySide.Instruments.Add(instrument1);

            // Create SellSide strategy.
            RollSellSide sellSide = new RollSellSide(framework, "SellSide");
            sellSide.Global[RollSellSide.barSizeCode] = barSize;
            sellSide.Global[RollSellSide.rollInfoCode] = rollInfo;

            // Set SellSide as data and execution provider for BuySide strategy.
            buySide.DataProvider = sellSide;
            buySide.ExecutionProvider = sellSide;

            // Add strategies to main.
            strategy.AddStrategy(buySide);
            strategy.AddStrategy(sellSide);

            // Get provider for realtime.
            Provider quantRouter = framework.ProviderManager.Providers["QuantRouter"] as Provider;

            if (quantRouter.Status == ProviderStatus.Disconnected)
                quantRouter.Connect();

            if (StrategyManager.Mode == StrategyMode.Paper)
            {
                // Set QuantRouter as data provider.
                strategy.DataProvider = quantRouter as IDataProvider;
            }
            else if (StrategyManager.Mode == StrategyMode.Live)
            {
                // Set QuantRouter as data and execution provider.
                strategy.DataProvider = quantRouter as IDataProvider;
                strategy.ExecutionProvider = quantRouter as IExecutionProvider;
            }

            BarFactory.Add(instrument1, BarType.Time, barSize);

            // Run.
            StartStrategy();
        }
    }

    public class RollSellSide : SellSideStrategy
    {
        class RollInfo
        {
            public int LegIndex { get; private set; }
            public string Symbol { get; private set; }
            public DateTime Maturity { get; private set; }

            public RollInfo(int legIndex, string symbol, DateTime maturity)
            {
                LegIndex = legIndex;
                Symbol = symbol;
                Maturity = maturity;
            }
        }
        public const string rollInfoCode = "RollInfo";
        public const string barSizeCode = "BarSize";

        private long barSize;
        private List<RollInfo> rollInfo;
        private int legIndex;
        private Instrument rootInstrument;
        private Instrument currentFuturesContract;
        private Dictionary<Instrument, Group> barsGroups;
        private Dictionary<Instrument, Group> fillGroups;
        private Dictionary<Order, ExecutionCommand> orderTable;
        private List<Order> rollOrders;

        #region Parameters

        [Parameter]
        public TimeSpan TimeOfRoll = new TimeSpan(09, 00, 05);

        #endregion

        public RollSellSide(Framework framework, string name)
            : base(framework, name)
        {
            barsGroups = new Dictionary<Instrument, Group>();
            fillGroups = new Dictionary<Instrument, Group>();
            orderTable = new Dictionary<Order, ExecutionCommand>();
            rollOrders = new List<Order>();
        }

        protected override void OnSubscribe(InstrumentList instruments)
        {
            // Get size of bar.
            barSize = (long)Global[barSizeCode];

            // Get roll info.
            rollInfo = (List<RollInfo>)Global[rollInfoCode];

            // Get root instrument.
            rootInstrument = instruments.GetByIndex(0);

            // Get current futures contract.
            currentFuturesContract = rootInstrument.Legs[legIndex].Instrument;

            // Add current futures contract to bar factory.
            BarFactory.Add(currentFuturesContract, BarType.Time, barSize);

            // Add current futures contract to strategy.
            AddInstrument(currentFuturesContract);

            // Add reminder to maturity date and roll time.
            AddReminder(rollInfo[legIndex].Maturity.Date + TimeOfRoll);

            AddGroups();
        }

        public override void OnSendCommand(ExecutionCommand command)
        {
            // Logic for send command.
            if (command.Type == ExecutionCommandType.Send)
            {
                Order order;

                // Create and send order to current futures contract.
                switch (command.Order.Type)
                {
                    case OrderType.Market:
                        if (command.Side == OrderSide.Buy)
                            order = BuyOrder(currentFuturesContract, command.Qty, command.Text);
                        else
                            order = SellOrder(currentFuturesContract, command.Qty, command.Text);

                        orderTable[order] = command;
                        Send(order);
                        break;

                    case OrderType.Limit:
                        if (command.Side == OrderSide.Buy)
                            order = BuyLimitOrder(currentFuturesContract, command.Qty, command.Price, command.Text);
                        else
                            order = SellLimitOrder(currentFuturesContract, command.Qty, command.Price, command.Text);

                        orderTable[order] = command;
                        Send(order);
                        break;

                    default:
                        break;
                }
            }
        }

        protected override void OnOrderFilled(Order order)
        {
            // Emit fill for BuySide strategy's order.
            if (!rollOrders.Contains(order))
                EmitFilled(order, orderTable[order]);
        }

        private void EmitFilled(Order order, ExecutionCommand command)
        {
            // Create execution report for BuySide strategy.

            Instrument instrument = command.Instrument;

            ExecutionReport execution = new ExecutionReport();

            execution.AvgPx = order.AvgPx;
            execution.Commission = 0;
            execution.CumQty = order.CumQty;
            execution.DateTime = framework.Clock.DateTime;
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

            // Emit execution report to BuySide strategy.
            EmitExecutionReport(execution);
        }

        protected override void OnAsk(Instrument instrument, Ask ask)
        {
            if (instrument.Id == currentFuturesContract.Id)
            {
                Ask rootAsk = new Ask(ask.DateTime, 0, rootInstrument.Id, ask.Price, ask.Size);

                // Emit ask to to BuySide strategy.
                EmitAsk(rootAsk);
            }
        }

        protected override void OnBid(Instrument instrument, Bid bid)
        {
            if (instrument.Id == currentFuturesContract.Id)
            {
                Bid rootBid = new Bid(bid.DateTime, 0, rootInstrument.Id, bid.Price, bid.Size);

                // Emit bid to to BuySide strategy.
                EmitBid(rootBid);
            }
        }

        protected override void OnTrade(Instrument instrument, Trade trade)
        {
            if (instrument.Id == currentFuturesContract.Id)
            {
                Trade rootTrade = new Trade(trade.DateTime, 0, rootInstrument.Id, trade.Price, trade.Size);

                // Emit trade to to BuySide strategy.
                EmitTrade(rootTrade);
            }
        }

        protected override void OnBar(Instrument instrument, Bar bar)
        {
            // Add bar to bar group.
            Log(bar, barsGroups[instrument]);
        }

        protected override void OnFill(Fill fill)
        {
            // Add fill to fill group.
            Log(fill, fillGroups[fill.Instrument]);
        }

        protected override void OnReminder(DateTime dateTime, object data)
        {
            legIndex++;

            Position position = Portfolio.GetPosition(currentFuturesContract);

            double rollAmount = 0;

            if (position != null)
                rollAmount = position.Amount;

            if (legIndex > rollInfo.Count - 1)
                return;

            Instrument prevFuturesContract = currentFuturesContract;

            // Get new current futures contract.
            currentFuturesContract = rootInstrument.Legs[legIndex].Instrument;

            // Add current futures contract to bar factory.
            BarFactory.Add(currentFuturesContract, BarType.Time, barSize);

            // Add current futures contract to strategy.
            AddInstrument(currentFuturesContract);

            // Add reminder to maturity date and roll time.
            AddReminder(rollInfo[legIndex].Maturity.AddDays(-1).Date + TimeOfRoll);

            AddGroups();

            // Roll from previous contract to current contract if needed.
            if (rollAmount > 0)
            {
                Order order1 = SellOrder(prevFuturesContract, Math.Abs(rollAmount), "Roll");
                Order order2 = BuyOrder(currentFuturesContract, Math.Abs(rollAmount), "Roll");

                rollOrders.Add(order1);
                rollOrders.Add(order2);

                Send(order1);
                Send(order2);
            }
            else if (rollAmount < 0)
            {
                Order order1 = BuyOrder(prevFuturesContract, Math.Abs(rollAmount), "Roll");
                Order order2 = SellOrder(currentFuturesContract, Math.Abs(rollAmount), "Roll");

                rollOrders.Add(order1);
                rollOrders.Add(order2);

                Send(order1);
                Send(order2);
            }
        }

        private void AddGroups()
        {
            // Create bars group.
            Group barGroup = new Group("Bars");
            barGroup.Add("Pad", DataObjectType.Int, legIndex + 2);
            barGroup.Add("SelectorKey", DataObjectType.String, rootInstrument.Symbol);

            // Create fills group.
            Group fillGroup = new Group("Fills");
            fillGroup.Add("Pad", DataObjectType.Int, legIndex + 2);
            fillGroup.Add("SelectorKey", DataObjectType.String, rootInstrument.Symbol);

            // Add groups to manager.
            GroupManager.Add(fillGroup);
            GroupManager.Add(barGroup);

            // Add groups to dictionary.
            fillGroups[currentFuturesContract] = fillGroup;
            barsGroups[currentFuturesContract] = barGroup;
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

