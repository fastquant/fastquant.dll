using System;
using System.Collections.Generic;
using System.Drawing;
using FastQuant;
using FastQuant.Indicators;
using System.Linq;
using FastQuant.Optimization;

namespace Samples.SMACrossover
{
    public class SMACrossover : InstrumentStrategy
    {
        private SMA sma1;
        private SMA sma2;
        private bool entryEnabled = true;
        private int OCACount = 0;
        private Order marketOrder;
        private Order limitOrder;
        private Order stopOrder;
        private Group barsGroup;
        private Group fillGroup;
        private Group equityGroup;
        private Group sma1Group;
        private Group sma2Group;
        private Group closeMonitorGroup;
        private Group openMonitorGroup;
        private Group positionMonitorGroup;

        [Parameter]
        public double MoneyForInstrument = 100000;

        [Parameter]
        double Qty = 100;

        [Parameter]
        public int Length1 = 14;

        [Parameter]
        public int Length2 = 50;

        public double StopOCALevel = 0.98;
        public double LimitOCALevel = 1.05;
        public double StopLevel = 0.05;
        public StopType StopType = StopType.Trailing;
        public StopMode StopMode = StopMode.Percent;
        public bool CrossoverExitEnabled = true;
        public bool OCAExitEnabled = true;
        public bool StopExitEnabled = true;

        public SMACrossover(Framework framework, string name)
            : base(framework, name)
        {
        }

        protected override void OnStrategyStart()
        {
            // Add money for current trading instrument's portfolio.
            Portfolio.Account.Deposit(Clock.DateTime, MoneyForInstrument, CurrencyId.USD, "Initial allocation");

            // Set up indicators.
            sma1 = new SMA(Bars, Length1);
            sma2 = new SMA(Bars, Length2);

            AddGroups();
        }

        protected override void OnBar(Instrument instrument, Bar bar)
        {
            Bars.Add(bar);

            // Log open close and position info to Strategy Monitor.
            Log(bar.DateTime, bar.Close, closeMonitorGroup);
            Log(bar.DateTime, bar.Open, openMonitorGroup);

            if (HasPosition(instrument))
                Log(bar.DateTime, Position.Side.ToString(), positionMonitorGroup);
            else
                Log(bar.DateTime, "None", positionMonitorGroup);

            // Log bars.
            Log(bar, barsGroup);

            if (sma1.Count == 0 || sma2.Count == 0)
                return;

            // Log sma.
            Log(bar.DateTime, sma1.Last, sma1Group);
            Log(bar.DateTime, sma2.Last, sma2Group);

            // Update performance.
            Portfolio.Performance.Update();

            // Log equity.
            Log(bar.DateTime, Portfolio.Value, equityGroup);

            // Does the fast average cross over the slow average? If so, time to buy long.
            Cross cross = sma1.Crosses(sma2, bar.DateTime);

            // We only allow one active position at a time.
            if (entryEnabled)
            {
                // If price trend is moving upward, open a long position using a market order, and send it in.
                if (cross == Cross.Above)
                {
                    marketOrder = BuyOrder(instrument, Qty, "Entry");
                    Send(marketOrder);

                    // If one cancels all exit method is desired, we
                    // also issue a limit (profit target) order, and
                    // a stop loss order in case the breakout fails.
                    // The OCA exit method uses a real stop loss order.
                    // The Stop exit method uses a stop indicator.
                    // Use either the OCA or Stop method, not both at once.
                    if (OCAExitEnabled)
                    {
                        // Create and send a profit limit order.
                        double profitTarget = LimitOCALevel * bar.Close;
                        limitOrder = SellLimitOrder(instrument, Qty, profitTarget, "Limit OCA " + OCACount);
                        limitOrder.OCA = "OCA " + Instrument.Symbol + " " + OCACount;
                        Send(limitOrder);

                        // Create and send a stop loss order.
                        double lossTarget = StopOCALevel * bar.Close;
                        stopOrder = SellStopOrder(instrument, Qty, lossTarget, "Stop OCA " + OCACount);
                        stopOrder.OCA = "OCA " + Instrument.Symbol + " " + OCACount;
                        Send(stopOrder);

                        // Bump the OCA count to make OCA group strings unique.
                        OCACount++;
                    }

                    entryEnabled = false;
                }
            }
            // Else if entry is disabled on this bar, we have an open position.
            else
            {
                // If we are using the crossover exit, and if the fast
                // average just crossed below the slow average, issue a
                // market order to close the existing position.
                if (CrossoverExitEnabled)
                {
                    if (cross == Cross.Below)
                    {
                        marketOrder = SellOrder(instrument, Qty, "Crossover Exit");
                        Send(marketOrder);
                    }
                }
            }
        }

        protected override void OnPositionOpened(Position position)
        {
            // If we want to exit trades using the Stop method, set a
            // a trailing stop indicator when the position is
            // first opened. The stop indicator is not a stop loss
            // order that can be executed by a broker. Instead, the stop
            // just fires the OnStopExecuted event when it it triggered.
            if (StopExitEnabled)
                AddStop(new Stop(this, position, StopLevel, StopType, StopMode));
        }

        protected override void OnPositionClosed(Position position)
        {
            // When a position is closed, cancel the limit and stop
            // orders that might be associated with this position.
            // But only cancel if the order has not been filled or
            // not been cancelled already.
            if (OCAExitEnabled && !(limitOrder.IsFilled || limitOrder.IsCancelled))
                Cancel(limitOrder);

            // Allow entries once again, since our position is closed.
            entryEnabled = true;
        }

        protected override void OnFill(Fill fill)
        {
            // Add fill to group.
            Log(fill, fillGroup);
        }

        protected override void OnStopExecuted(Stop stop)
        {
            // If our trailing stop indicator was executed,
            // issue a market sell order to close the position.
            marketOrder = SellOrder(Instrument, Qty, "Stop Exit");
            Send(marketOrder);
        }

        private void AddGroups()
        {
            // Create bars group.
            barsGroup = new Group("Bars");
            barsGroup.Add("Pad", DataObjectType.String, 0);
            barsGroup.Add("SelectorKey", Instrument.Symbol);

            //// Create fills group.
            fillGroup = new Group("Fills");
            fillGroup.Add("Pad", 0);
            fillGroup.Add("SelectorKey", Instrument.Symbol);

            // Create equity group.
            equityGroup = new Group("Equity");
            equityGroup.Add("Pad", 1);
            equityGroup.Add("SelectorKey", Instrument.Symbol);

            // Create sma1 values group.
            sma1Group = new Group("SMA1");
            sma1Group.Add("Pad", 0);
            sma1Group.Add("SelectorKey", Instrument.Symbol);
            sma1Group.Add("Color", Color.Green);

            // Create sma2 values group.
            sma2Group = new Group("SMA2");
            sma2Group.Add("Pad", 0);
            sma2Group.Add("SelectorKey", Instrument.Symbol);
            sma2Group.Add("Color", Color.Red);

            // Create log monitor groups.
            closeMonitorGroup = new Group("Close");
            closeMonitorGroup.Add("LogName", "Close");
            closeMonitorGroup.Add("StrategyName", "MyStrategy");
            closeMonitorGroup.Add("Symbol", Instrument.Symbol);

            openMonitorGroup = new Group("Open");
            openMonitorGroup.Add("LogName", "Open");
            openMonitorGroup.Add("StrategyName", "MyStrategy");
            openMonitorGroup.Add("Symbol", Instrument.Symbol);

            positionMonitorGroup = new Group("Position");
            positionMonitorGroup.Add("LogName", "Position");
            positionMonitorGroup.Add("StrategyName", "MyStrategy");
            positionMonitorGroup.Add("Symbol", Instrument.Symbol);

            // Add groups to manager.
            GroupManager.Add(barsGroup);
            GroupManager.Add(fillGroup);
            GroupManager.Add(equityGroup);
            GroupManager.Add(sma1Group);
            GroupManager.Add(sma2Group);
            GroupManager.Add(closeMonitorGroup);
            GroupManager.Add(openMonitorGroup);
            GroupManager.Add(positionMonitorGroup);
        }
    }

    public class SMACrossoverLoadOnStart : InstrumentStrategy
    {
        private SMA fastSMA;
        private SMA slowSMA;
        private Group barsGroup;
        private Group fillGroup;
        private Group equityGroup;
        private Group fastSmaGroup;
        private Group slowSmaGroup;

        public static bool SuspendTrading = false;

        [Parameter]
        public double AllocationPerInstrument = 100000;

        [Parameter]
        public double Qty = 100;

        [Parameter]
        public int FastSMALength = 8;

        [Parameter]
        public int SlowSMALength = 21;

        public SMACrossoverLoadOnStart(Framework framework, string name)
            : base(framework, name)
        {
        }

        protected override void OnStrategyInit()
        {
            Portfolio.Account.Deposit(AllocationPerInstrument, CurrencyId.USD, "Initial allocation");

            // Set up indicators.
            fastSMA = new SMA(Bars, FastSMALength);
            slowSMA = new SMA(Bars, SlowSMALength);

            AddGroups();
        }

        protected override void OnStrategyStart()
        {
            Console.WriteLine("Starting strategy in {0} mode.", Mode);
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

            if (!SuspendTrading)
            {
                // Check strategy logic.
                if (fastSMA.Count > 0 && slowSMA.Count > 0)
                {
                    Cross cross = fastSMA.Crosses(slowSMA, bar.DateTime);

                    if (!HasPosition(instrument))
                    {
                        // Enter long/short.
                        if (cross == Cross.Above)
                        {
                            Order enterOrder = BuyOrder(Instrument, Qty, "Enter Long");
                            Send(enterOrder);
                        }
                        else if (cross == Cross.Below)
                        {
                            Order enterOrder = SellOrder(Instrument, Qty, "Enter Short");
                            Send(enterOrder);
                        }
                    }
                    else
                    {
                        // Reverse to long/short.
                        if (Position.Side == PositionSide.Long && cross == Cross.Below)
                        {
                            Order reverseOrder = SellOrder(Instrument, Math.Abs(Position.Amount) + Qty, "Reverse to Short");
                            Send(reverseOrder);
                        }
                        else if (Position.Side == PositionSide.Short && cross == Cross.Above)
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

    public class SMACrossoverWithEventFilter : InstrumentStrategy
    {
        class MyEventFilter : EventFilter
        {
            private TimeSpan filterStartTime;
            private TimeSpan filterEndTime;
            private Dictionary<int, Trade> goodTrades;

            public MyEventFilter(Framework framework, TimeSpan filterStartTime, TimeSpan filterEndTime)
                : base(framework)
            {
                this.filterStartTime = filterStartTime;
                this.filterEndTime = filterEndTime;
                goodTrades = new Dictionary<int, Trade>();
            }

            public override Event Filter(Event e)
            {
                // Filter if event is trade.
                if (e.TypeId == EventType.Trade)
                {
                    Trade trade = (Trade)e;
                    Trade lastGoodTrade = null;

                    // If filter has good trade.
                    if (goodTrades.TryGetValue(trade.InstrumentId, out lastGoodTrade))
                    {
                        // Check if trade falls in the +-7.5% interval from last good trade.
                        if (Math.Abs((1 - lastGoodTrade.Price / trade.Price) * 100) < 7.5)
                        {
                            // Save trade as good.
                            goodTrades[trade.InstrumentId] = trade;

                            // Check if trade falls in the filter time interval.
                            // If no return null, else return trade.
                            if (trade.DateTime.TimeOfDay < filterStartTime ||
                                trade.DateTime.TimeOfDay > filterEndTime)
                                return null;
                            else
                                return trade;
                        }
                        else
                            return null;
                    }
                    // If filter hasn't one trade.
                    else
                    {
                        // Save trade as good.
                        goodTrades[trade.InstrumentId] = trade;

                        // Check if trade falls in the filter time interval.
                        // If no return null, else return trade.
                        if (trade.DateTime.TimeOfDay < filterStartTime ||
                            trade.DateTime.TimeOfDay > filterEndTime)
                            return null;
                        else
                            return trade;
                    }
                }

                return e;
            }
        }

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
        int FastSMALength = 8;

        [Parameter]
        int SlowSMALength = 21;

        public SMACrossoverWithEventFilter(Framework framework, string name)
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

                if (!HasPosition(instrument))
                {
                    // Enter long/short.
                    if (cross == Cross.Above)
                    {
                        Order enterOrder = BuyOrder(Instrument, Qty, "Enter Long");
                        Send(enterOrder);
                    }
                    else if (cross == Cross.Below)
                    {
                        Order enterOrder = SellOrder(Instrument, Qty, "Enter Short");
                        Send(enterOrder);
                    }
                }
                else
                {
                    // Reverse to long/short.
                    if (Position.Side == PositionSide.Long && cross == Cross.Below)
                    {
                        Order reverseOrder = SellOrder(Instrument, Math.Abs(Position.Amount) + Qty, "Reverse to Short");
                        Send(reverseOrder);
                    }
                    else if (Position.Side == PositionSide.Short && cross == Cross.Above)
                    {
                        Order reverseOrder = BuyOrder(Instrument, Math.Abs(Position.Amount) + Qty, "Reverse to Long");
                        Send(reverseOrder);
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

    public class SMACrossoverWithTakeProfitAndStopLoss : InstrumentStrategy
    {
        private SMA fastSMA;
        private SMA slowSMA;
        private Order enterOrder;
        private Order takeProfitOrder;
        private Order stopLossOrder;
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
        int FastSMALength = 8;

        [Parameter]
        int SlowSMALength = 21;

        [Parameter]
        double TakeProfit = 0.01;

        [Parameter]
        double StopLoss = 0.01;

        public SMACrossoverWithTakeProfitAndStopLoss(Framework framework, string name)
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

            // Add equity to group.
            Log(Portfolio.Value, equityGroup);

            // Check strategy logic.
            if (fastSMA.Count > 0 && slowSMA.Count > 0)
            {
                Cross cross = fastSMA.Crosses(slowSMA, bar.DateTime);

                // Enter long/short.
                if (!HasPosition(instrument))
                {
                    if (cross == Cross.Above)
                    {
                        enterOrder = BuyOrder(Instrument, Qty, "Enter Long");
                        Send(enterOrder);
                    }
                    else if (cross == Cross.Below)
                    {
                        enterOrder = SellOrder(Instrument, Qty, "Enter Short");
                        Send(enterOrder);
                    }
                }
            }
        }

        protected override void OnFill(Fill fill)
        {
            Log(fill, fillGroup);
        }

        protected override void OnOrderPartiallyFilled(Order order)
        {
            // Update take profit order.
            if (order == stopLossOrder)
            {
                // Get take profit price and updated qty.
                double takeProfitPrice = takeProfitOrder.Price;
                double qty = Math.Abs(Position.Amount);

                // Cancel existing take profit order.
                Cancel(takeProfitOrder);

                if (Position.Side == PositionSide.Long)
                {
                    // Create updated order and send it.
                    takeProfitOrder = SellLimitOrder(Instrument, qty, takeProfitPrice, "TakeProfit");
                    Send(takeProfitOrder);
                }
                else
                {
                    // Create updated order and send it.
                    takeProfitOrder = BuyLimitOrder(Instrument, qty, takeProfitPrice, "TakeProfit");
                    Send(takeProfitOrder);
                }
            }
            // Update stop loss order.
            else if (order == takeProfitOrder)
            {
                // Get take profit price and updated qty.
                double stopLossPrice = stopLossOrder.Price;
                double qty = Math.Abs(Position.Amount);

                // Cancel existing stop loss order.
                Cancel(stopLossOrder);

                if (Position.Side == PositionSide.Long)
                {
                    // Create updated order and send it.
                    stopLossOrder = SellStopOrder(Instrument, qty, stopLossPrice, "StopLoss");
                    Send(stopLossOrder);
                }
                else
                {
                    // Create updated order and send it.
                    stopLossOrder = BuyStopOrder(Instrument, qty, stopLossPrice, "StopLoss");
                    Send(stopLossOrder);
                }
            }
        }

        protected override void OnOrderFilled(Order order)
        {
            if (order == enterOrder)
            {
                // Send take profit and stop loss orders.
                if (Position.Side == PositionSide.Long)
                {
                    // Calculate prices.
                    double takeProfitPrice = Position.EntryPrice * (1 + TakeProfit);
                    double stopLossPrice = Position.EntryPrice * (1 - StopLoss);

                    // Create orders.
                    takeProfitOrder = SellLimitOrder(Instrument, Qty, takeProfitPrice, "TakeProfit");
                    stopLossOrder = SellStopOrder(Instrument, Qty, stopLossPrice, "StopLoss");

                    // Send orders.
                    Send(stopLossOrder);
                    Send(takeProfitOrder);
                }
                else
                {
                    // Calculate prices.
                    double takeProfitPrice = Position.EntryPrice * (1 - TakeProfit);
                    double stopLossPrice = Position.EntryPrice * (1 + StopLoss);

                    // Create orders.
                    takeProfitOrder = BuyLimitOrder(Instrument, Qty, takeProfitPrice, "TakeProfit");
                    stopLossOrder = BuyStopOrder(Instrument, Qty, stopLossPrice, "StopLoss");

                    // Send orders.
                    Send(stopLossOrder);
                    Send(takeProfitOrder);
                }
            }
            else if (order == stopLossOrder)
            {
                // Cancel take profit order.
                if (!takeProfitOrder.IsDone)
                    Cancel(takeProfitOrder);
            }
            else if (order == takeProfitOrder)
            {
                // Cancel stop loss order.
                if (!stopLossOrder.IsDone)
                    Cancel(stopLossOrder);
            }
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

    public class Optimization : Scenario
    {
        public Optimization(Framework framework)
            : base(framework)
        {
        }

        public override void Run()
        {
            MulticoreOptimizer optimizer = new MulticoreOptimizer();

            OptimizationUniverse universe = new OptimizationUniverse();

            for (int length1 = 2; length1 < 14; length1++)
                for (int length2 = length1 + 1; length2 < 28; length2++)
                {
                    OptimizationParameterSet parameter = new OptimizationParameterSet();

                    parameter.Add("Length1", length1);
                    parameter.Add("Length2", length2);
                    parameter.Add("Bar", (long)60);

                    universe.Add(parameter);
                }

            strategy = new SMACrossover(framework, "strategy ");

            Instrument instrument1 = InstrumentManager.Instruments["AAPL"];
            Instrument instrument2 = InstrumentManager.Instruments["MSFT"];

            InstrumentList instruments = new InstrumentList();

            instruments.Add(instrument1);
            instruments.Add(instrument2);

            DataSimulator.DateTime1 = new DateTime(2013, 12, 01);
            DataSimulator.DateTime2 = new DateTime(2013, 12, 31);

            optimizer.Optimize(strategy, instruments, universe, 100);
        }
    }
}

