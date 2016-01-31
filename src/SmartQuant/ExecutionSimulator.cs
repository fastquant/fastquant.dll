using System;
using System.Collections.Generic;

namespace SmartQuant
{
    public class ExecutionSimulator : Provider, IExecutionSimulator
    {
        private class ReportSummary
        {
            public ReportSummary(ExecutionReport report)
            {
                OrdStatus = report.OrdStatus;
                AvgPx = report.AvgPx;
                CumQty = report.CumQty;
                LeavesQty = report.LeavesQty;
                Commission = report.Commission;
            }

            public double AvgPx { get; }

            public double LeavesQty { get; }

            public double Commission { get; }

            public OrderStatus OrdStatus { get; }

            public double CumQty { get; }
        }

        private List<Order> orders = new List<Order>();

        private List<Order> stops = new List<Order>();

        private IdArray<List<Order>> ordersByInstrumentId = new IdArray<List<Order>>(10240);

        private IdArray<ReportSummary> summariesByOrderId = new IdArray<ReportSummary>(10240);

        public TimeSpan Auction1 { get; set; }

        public TimeSpan Auction2 { get; set; }

        public BarFilter BarFilter { get; } = new BarFilter();

        public ISlippageProvider SlippageProvider { get; set; } = new SlippageProvider();

        public ICommissionProvider CommissionProvider { get; set; } = new CommissionProvider();

        public bool FillOnQuote { get; set; } = true;
        public bool FillOnTrade { get; set; } = true;
        public bool FillOnLevel2 { get; set; } = true;
        public bool FillLimitOnNext { get; set; } = true;
        public bool FillStopOnNext { get; set; } = true;
        public bool FillStopLimitOnNext { get; set; } = true;
        public bool FillAtLimitPrice { get; set; } = true;
        public bool FillAtStopPrice { get; set; }
        public bool FillMarketOnNext { get; set; }
        public bool FillOnBar { get; set; }
        public bool FillOnBarOpen { get; set; }

        public bool Queued { get; set; } = true;
        public bool PartialFills { get; set; }

        public ExecutionSimulator(Framework framework) : base(framework)
        {
            this.framework = framework;
            this.id = ProviderId.ExecutionSimulator;
            this.name = "ExecutionSimulator";
            this.description = "Default execution simulator";
            this.url = $"www.{nameof(SmartQuant).ToLower()}.com";
        }

        public override void Clear()
        {
            this.ordersByInstrumentId.Clear();
            this.summariesByOrderId.Clear();
        }

        public override void Send(ExecutionCommand command)
        {
            if (IsDisconnected)
                Connect();

            switch (command.Type)
            {
                case ExecutionCommandType.Send:
                    HandleSend(command.Order);
                    return;
                case ExecutionCommandType.Cancel:
                    HandleCancel(command.Order);
                    return;
                case ExecutionCommandType.Replace:
                    HandleReplace(command);
                    return;
                default:
                    return;
            }
        }

        public void OnBid(Bid bid)
        {
            var orders = this.ordersByInstrumentId[bid.InstrumentId];
            if (orders == null)
                return;

            if (FillOnQuote)
            {
                foreach (var order in orders)
                    FillWithBid(order, bid);
                ClearOrders();
            }
        }

        public void OnAsk(Ask ask)
        {
            var orders = this.ordersByInstrumentId[ask.InstrumentId];
            if (orders == null)
                return;

            if (FillOnQuote)
            {
                foreach (var order in orders)
                    FillWithAsk(order, ask);
                ClearOrders();
            }
        }

        public void OnTrade(Trade trade)
        {
            var orders = this.ordersByInstrumentId[trade.InstrumentId];
            if (orders == null)
                return;

            if (FillOnTrade)
            {
                foreach (var order in orders)
                    this.FillWithTrade(order, trade);
                this.ClearOrders();
            }
        }

        public void OnBar(Bar bar)
        {
            var orders = this.ordersByInstrumentId[bar.InstrumentId];
            if (orders == null)
                return;

            if (FillOnBar && (BarFilter.Count == 0 || BarFilter.Contains(bar.Type, bar.Size)))
            {
                foreach (var order in orders)
                    FillWithBar(order, bar);
                ClearOrders();
            }
        }

        public void OnBarOpen(Bar bar)
        {
            var orders = this.ordersByInstrumentId[bar.InstrumentId];
            if (orders == null)
                return;

            if (FillOnBarOpen)
            {
                if (BarFilter.Count != 0 && !BarFilter.Contains(bar.Type, bar.Size))
                {
                    return;
                }
                int i = 0;
                while (i < orders.Count)
                {
                    Order order = orders[i];
                    while (true)
                    {
                        switch (order.Type)
                        {
                            case OrderType.Market:
                            case OrderType.Pegged:
                                goto IL_215;
                            case OrderType.Stop:
                                switch (order.Side)
                                {
                                    case OrderSide.Buy:
                                        if (bar.Open >= order.StopPx)
                                        {
                                            if (!FillAtStopPrice)
                                            {
                                                order.Type = OrderType.Market;
                                                continue;
                                            }
                                            goto IL_142;
                                        }
                                        break;
                                    case OrderSide.Sell:
                                        if (bar.Open <= order.StopPx)
                                        {
                                            if (!FillAtStopPrice)
                                            {
                                                order.Type = OrderType.Market;
                                                continue;
                                            }
                                            goto IL_15B;
                                        }
                                        break;
                                }
                                break;
                            case OrderType.Limit:
                                goto IL_174;
                            case OrderType.StopLimit:
                                switch (order.Side)
                                {
                                    case OrderSide.Buy:
                                        if (bar.Open >= order.StopPx)
                                        {
                                            order.Type = OrderType.Limit;
                                            continue;
                                        }
                                        break;
                                    case OrderSide.Sell:
                                        if (bar.Open <= order.StopPx)
                                        {
                                            order.Type = OrderType.Limit;
                                            continue;
                                        }
                                        break;
                                }
                                break;
                        }
                        break;
                    }
                    IL_229:
                    i++;
                    continue;
                    goto IL_229;
                    IL_142:
                    this.Fill(order, order.StopPx, (int) bar.Volume);
                    goto IL_229;
                    IL_15B:
                    this.Fill(order, order.StopPx, (int) bar.Volume);
                    goto IL_229;
                    IL_174:
                    switch (order.Side)
                    {
                        case OrderSide.Buy:
                            if (bar.Open > order.Price)
                            {
                                goto IL_229;
                            }
                            if (FillAtLimitPrice)
                            {
                                this.Fill(order, order.Price, (int) bar.Volume);
                                goto IL_229;
                            }
                            this.Fill(order, bar.Open, (int) bar.Volume);
                            goto IL_229;
                        case OrderSide.Sell:
                            if (bar.Open < order.Price)
                            {
                                goto IL_229;
                            }
                            if (FillAtLimitPrice)
                            {
                                this.Fill(order, order.Price, (int) bar.Volume);
                                goto IL_229;
                            }
                            this.Fill(order, bar.Open, (int) bar.Volume);
                            goto IL_229;
                        default:
                            goto IL_229;
                    }
                    IL_215:
                    this.Fill(order, bar.Open, (int) bar.Volume);
                    goto IL_229;
                }
                this.ClearOrders();
            }
        }

        public void OnLevel2(Level2Snapshot snapshot)
        {
            // noop
        }

        public void OnLevel2(Level2Update update)
        {
            // noop
        }

        public void Fill(Order order, double price, int size)
        {
            if (!PartialFills)
            {
                this.orders.Add(order);
                var report = new ExecutionReport
                {
                    DateTime = this.framework.Clock.DateTime,
                    Order = order,
                    OrderId = order.Id,
                    OrdType = order.Type,
                    Side = order.Side,
                    Instrument = order.Instrument,
                    InstrumentId = order.InstrumentId,
                    OrdQty = order.Qty,
                    Price = order.Price,
                    StopPx = order.StopPx,
                    TimeInForce = order.TimeInForce,
                    ExecType = ExecType.ExecTrade,
                    OrdStatus = OrderStatus.Filled,
                    CurrencyId = order.Instrument.CurrencyId,
                    CumQty = order.LeavesQty,
                    LastQty = order.LeavesQty,
                    LeavesQty = 0.0,
                    LastPx = price,
                    Text = order.Text
                };
                report.Commission = CommissionProvider.GetCommission(report);
                report.LastPx = SlippageProvider.GetPrice(report);
                this.summariesByOrderId[order.Id] = new ReportSummary(report);
                EmitExecutionReport(report, Queued);
                return;
            }
            if (size <= 0)
            {
                Console.WriteLine("ExecutionSimulator::Fill Error - using partial fills, size can not be zero");
                return;
            }
            var report2 = new ExecutionReport
            {
                DateTime = this.framework.Clock.DateTime,
                Order = order,
                OrderId = order.Id,
                OrdType = order.Type,
                Side = order.Side,
                Instrument = order.Instrument,
                InstrumentId = order.InstrumentId,
                OrdQty = order.Qty,
                Price = order.Price,
                StopPx = order.StopPx,
                TimeInForce = order.TimeInForce,
                ExecType = ExecType.ExecTrade,
                CurrencyId = order.Instrument.CurrencyId
            };
            if (size >= order.LeavesQty)
            {
                this.orders.Add(order);
                report2.OrdStatus = OrderStatus.Filled;
                report2.CumQty = order.CumQty + order.LeavesQty;
                report2.LastQty = order.LeavesQty;
                report2.LeavesQty = 0.0;
                report2.LastPx = price;
                report2.Text = order.Text;
                order.LeavesQty = report2.LeavesQty;
            }
            else if (size < order.LeavesQty)
            {
                report2.OrdStatus = OrderStatus.PartiallyFilled;
                report2.CumQty = order.CumQty + size;
                report2.LastQty = size;
                report2.LeavesQty = order.LeavesQty - size;
                report2.LastPx = price;
                report2.Text = order.Text;
                order.LeavesQty = report2.LeavesQty;
            }
            report2.Commission = CommissionProvider.GetCommission(report2);
            report2.LastPx = SlippageProvider.GetPrice(report2);
            this.summariesByOrderId[order.Id] = new ReportSummary(report2);
            EmitExecutionReport(report2, Queued);
        }

        #region ActionHandlers

        private void HandleSend(Order order)
        {
            if (order.Qty == 0)
            {
                ExecOrderRejected(order, "Order amount can not be zero");
                return;
            }

            var report = new ExecutionReport
            {
                DateTime = this.framework.Clock.DateTime,
                Order = order,
                OrderId = order.Id,
                Instrument = order.Instrument,
                InstrumentId = order.InstrumentId,
                ExecType = ExecType.ExecNew,
                OrdStatus = OrderStatus.New,
                CurrencyId = order.Instrument.CurrencyId,
                OrdType = order.Type,
                Side = order.Side,
                OrdQty = order.Qty,
                Price = order.Price,
                StopPx = order.StopPx,
                TimeInForce = order.TimeInForce,
                CumQty = 0,
                LastQty = 0
            };
            report.OrdQty = order.Qty;
            report.LastPx = 0;
            report.AvgPx = 0;
            report.Text = order.Text;
            order.LeavesQty = report.LeavesQty;
            this.summariesByOrderId[order.Id] = new ReportSummary(report);
            EmitExecutionReport(report, Queued);

            if (order.TimeInForce == TimeInForce.AUC)
            {
                this.stops.Add(order);
                if (this.stops.Count == 1)
                {
                    this.framework.Clock.AddReminder(OnAuction1, this.framework.Clock.DateTime.Date.Add(Auction1));
                    this.framework.Clock.AddReminder(OnAuction2, this.framework.Clock.DateTime.Date.Add(Auction2));
                }
                return;
            }

            int iId = order.InstrumentId;
            GetOrdersBy(iId, true).Add(order);
            if (((order.Type == OrderType.Market || order.Type == OrderType.Pegged) && !FillMarketOnNext) ||
                (order.Type == OrderType.Limit && !FillLimitOnNext) || (order.Type == OrderType.Stop && !FillStopOnNext) ||
                (order.Type == OrderType.StopLimit && !FillStopLimitOnNext))
            {
                if (FillOnQuote)
                {
                    var ask = this.framework.DataManager.GetAsk(iId);
                    if (ask != null && FillWithAsk(order, ask))
                    {
                        ClearOrders();
                        return;
                    }
                    var bid = this.framework.DataManager.GetBid(iId);
                    if (bid != null && FillWithBid(order, bid))
                    {
                        ClearOrders();
                        return;
                    }
                }
                if (FillOnTrade)
                {
                    var trade = this.framework.DataManager.GetTrade(iId);
                    if (trade != null && FillWithTrade(order, trade))
                    {
                        ClearOrders();
                        return;
                    }
                }
                if (FillOnBar)
                {
                    var bar = this.framework.DataManager.GetBar(iId);
                    if (bar != null && BarFilter.Contains(bar.Type, bar.Size) && FillWithBar(order, bar))
                    {
                        ClearOrders();
                        return;
                    }
                }
            }
        }

        private void HandleCancel(Order order)
        {
            var summary = this.summariesByOrderId[order.Id];
            if (summary != null && IsOrderDone(summary.OrdStatus))
            {
                ExecOrderCancelReject(order, "Order already done");
                return;
            }

            GetOrdersBy(order.Instrument.Id)?.Remove(order);
            var report = new ExecutionReport
            {
                DateTime = this.framework.Clock.DateTime,
                Order = order,
                OrderId = order.Id,
                Instrument = order.Instrument,
                InstrumentId = order.InstrumentId,
                OrdQty = order.Qty,
                Price = order.Price,
                StopPx = order.StopPx,
                TimeInForce = order.TimeInForce,
                ExecType = ExecType.ExecCancelled,
                OrdStatus = OrderStatus.Cancelled,
                CurrencyId = order.Instrument.CurrencyId,
                OrdType = order.Type,
                Side = order.Side,
                CumQty = order.CumQty,
                LastQty = 0.0,
                LeavesQty = order.LeavesQty,
                LastPx = 0.0,
                AvgPx = 0.0,
                Text = order.Text
            };
            this.summariesByOrderId[order.Id] = new ReportSummary(report);
            EmitExecutionReport(report, Queued);
        }

        private void HandleReplace(ExecutionCommand command)
        {
            var order = command.Order;
            var summary = this.summariesByOrderId[order.Id];
            if (summary != null && IsOrderDone(summary.OrdStatus))
            {
                ExecOrderReplaceReject(order, "Order already done");
                return;
            }

            var report = new ExecutionReport
            {
                DateTime = this.framework.Clock.DateTime,
                Order = order,
                OrderId = order.Id,
                Instrument = order.Instrument,
                InstrumentId = order.InstrumentId,
                ExecType = ExecType.ExecReplace,
                OrdStatus = OrderStatus.Replaced,
                CurrencyId = order.Instrument.CurrencyId,
                OrdType = order.Type,
                Side = order.Side,
                CumQty = order.CumQty,
                LastQty = 0.0,
                LeavesQty = command.Qty - order.CumQty,
                LastPx = 0.0,
                AvgPx = 0.0
            };
            report.OrdType = order.Type;
            report.Price = command.Price;
            report.StopPx = command.StopPx;
            report.OrdQty = command.Qty;
            report.TimeInForce = order.TimeInForce;
            report.Text = order.Text;
            this.summariesByOrderId[order.Id] = new ReportSummary(report);
            EmitExecutionReport(report, Queued);
        }

        #endregion

        private bool FillWithAsk(Order order, Ask ask)
        {
            if (order.Side != OrderSide.Buy)
                return false;

            while (true)
            {
                switch (order.Type)
                {
                    case OrderType.Market:
                    case OrderType.Pegged:
                        Fill(order, ask.Price, ask.Size);
                        return true;
                    case OrderType.Stop:
                        if (ask.Price >= order.StopPx)
                        {
                            if (!FillAtStopPrice)
                            {
                                order.Type = OrderType.Market;
                                continue;
                            }
                            Fill(order, order.StopPx, ask.Size);
                            return true;
                        }
                        break;
                    case OrderType.Limit:
                        if (ask.Price <= order.Price)
                        {
                            Fill(order, FillAtLimitPrice ? order.Price : ask.Price, ask.Size);
                            return true;
                        }
                        return false;
                    case OrderType.StopLimit:
                        if (ask.Price >= order.StopPx)
                        {
                            order.Type = OrderType.Limit;
                            continue;
                        }
                        break;
                }
                break;
            }
            return false;
        }

        private bool FillWithBid(Order order, Bid bid)
        {
            if (order.Side != OrderSide.Sell)
                return false;

            while (true)
            {
                switch (order.Type)
                {
                    case OrderType.Market:
                    case OrderType.Pegged:
                        Fill(order, bid.Price, bid.Size);
                        return true;
                    case OrderType.Stop:
                        if (bid.Price <= order.StopPx)
                        {
                            if (!FillAtStopPrice)
                            {
                                order.Type = OrderType.Market;
                                continue;
                            }
                            Fill(order, order.StopPx, bid.Size);
                            return true;
                        }
                        break;
                    case OrderType.Limit:
                        if (bid.Price >= order.Price)
                        {
                            Fill(order, FillAtLimitPrice ? order.Price : bid.Price, bid.Size);
                            return true;
                        }
                        return false;
                    case OrderType.StopLimit:
                        if (bid.Price <= order.StopPx)
                        {
                            order.Type = OrderType.Limit;
                            continue;
                        }
                        break;
                }
                break;
            }
            return false;
        }

        private bool FillWithBar(Order order, Bar bar)
        {
            while (true)
            {
                switch (order.Type)
                {
                    case OrderType.Market:
                    case OrderType.Pegged:
                        Fill(order, bar.Close, (int)bar.Volume);
                        return true;
                    case OrderType.Stop:
                        switch (order.Side)
                        {
                            case OrderSide.Buy:
                                if (bar.High >= order.StopPx)
                                {
                                    if (!FillAtStopPrice)
                                    {
                                        order.Type = OrderType.Market;
                                        continue;
                                    }
                                    Fill(order, order.StopPx, (int)bar.Volume);
                                    return true;
                                }
                                break;
                            case OrderSide.Sell:
                                if (bar.Low <= order.StopPx)
                                {
                                    if (!FillAtStopPrice)
                                    {
                                        order.Type = OrderType.Market;
                                        continue;
                                    }
                                    Fill(order, order.StopPx, (int)bar.Volume);
                                    return true;
                                }
                                break;
                        }
                        break;
                    case OrderType.Limit:
                        switch (order.Side)
                        {
                            case OrderSide.Buy:
                                if (bar.Low <= order.Price)
                                {
                                    Fill(order, FillAtLimitPrice ? order.Price : bar.Close, (int) bar.Volume);
                                    return true;
                                }
                                break;
                            case OrderSide.Sell:
                                if (bar.High >= order.Price)
                                {
                                    Fill(order, FillAtLimitPrice ? order.Price : bar.Close, (int)bar.Volume);
                                    return true;
                                }
                                break;
                        }
                        return false;
                    case OrderType.StopLimit:
                        switch (order.Side)
                        {
                            case OrderSide.Buy:
                                if (bar.High >= order.StopPx)
                                {
                                    order.Type = OrderType.Limit;
                                    continue;
                                }
                                break;
                            case OrderSide.Sell:
                                if (bar.Low <= order.StopPx)
                                {
                                    order.Type = OrderType.Limit;
                                    continue;
                                }
                                break;
                        }
                        break;
                }
                break;
            }
            return false;
        }

        private bool FillWithTrade(Order order, Trade trade)
        {
            while (true)
            {
                switch (order.Type)
                {
                    case OrderType.Market:
                    case OrderType.Pegged:
                        Fill(order, trade.Price, trade.Size);
                        return true;
                    case OrderType.Stop:
                        switch (order.Side)
                        {
                            case OrderSide.Buy:
                                if (trade.Price >= order.StopPx)
                                {
                                    if (!FillAtStopPrice)
                                    {
                                        order.Type = OrderType.Market;
                                        continue;
                                    }
                                    Fill(order, order.StopPx, trade.Size);
                                    return true;
                                }
                                break;
                            case OrderSide.Sell:
                                if (trade.Price <= order.StopPx)
                                {
                                    if (!FillAtStopPrice)
                                    {
                                        order.Type = OrderType.Market;
                                        continue;
                                    }
                                    Fill(order, order.StopPx, trade.Size);
                                    return true;
                                }
                                break;
                        }
                        break;
                    case OrderType.Limit:
                        switch (order.Side)
                        {
                            case OrderSide.Buy:
                                if (trade.Price <= order.Price)
                                {
                                    Fill(order, FillAtLimitPrice ? order.Price : trade.Price, trade.Size);
                                    return true;
                                }
                                break;
                            case OrderSide.Sell:
                                if (trade.Price >= order.Price)
                                {
                                    Fill(order, FillAtLimitPrice ? order.Price : trade.Price, trade.Size);
                                    return true;
                                }
                                break;
                        }
                        return false;
                    case OrderType.StopLimit:
                        switch (order.Side)
                        {
                            case OrderSide.Buy:
                                if (trade.Price >= order.StopPx)
                                {
                                    order.Type = OrderType.Limit;
                                    continue;
                                }
                                break;
                            case OrderSide.Sell:
                                if (trade.Price <= order.StopPx)
                                {
                                    order.Type = OrderType.Limit;
                                    continue;
                                }
                                break;
                        }
                        break;
                }
                break;
            }
            return false;
        }

        private void OnAuction1(DateTime dateTime, object obj)
        {
            for (var i = this.stops.Count - 1; i >= 0; i--)
            {
                var order = this.stops[i];
                var list = GetOrdersBy(order.InstrumentId, true);
                list.Add(order);
                FillLimitOrder(order);
                this.stops.RemoveAt(i);
            }
        }

        private void OnAuction2(DateTime dateTime, object obj)
        {
            for (var i = this.stops.Count - 1; i >= 0; i--)
            {
                var order = this.stops[i];
                var list = GetOrdersBy(order.InstrumentId, true);
                list.Add(order);
                if (!FillLimitOrder(order))
                    HandleCancel(order);
                this.stops.RemoveAt(i);
            }
        }

        private void ClearOrders()
        {
            if (this.orders.Count > 0)
            {
                foreach (var o in this.orders)
                    GetOrdersBy(o.InstrumentId)?.Remove(o);
                this.orders.Clear();
            }
        }

        private void ExecOrderReplaceReject(Order order, string text)
        {
            var summary = this.summariesByOrderId[order.Id];
            EmitExecutionReport(new ExecutionReport
            {
                dateTime = this.framework.Clock.DateTime,
                Order = order,
                OrderId = order.Id,
                Instrument = order.Instrument,
                InstrumentId = order.InstrumentId,
                OrdQty = order.Qty,
                Price = order.Price,
                StopPx = order.StopPx,
                TimeInForce = order.TimeInForce,
                ExecType = ExecType.ExecReplaceReject,
                OrdStatus = summary.OrdStatus,
                CurrencyId = order.Instrument.CurrencyId,
                OrdType = order.Type,
                Side = order.Side,
                CumQty = summary.CumQty,
                LeavesQty = summary.LeavesQty,
                AvgPx = summary.AvgPx,
                Text = text
            }, Queued);
        }

        private bool FillLimitOrder(Order order)
        {
            if (order.Type != OrderType.Limit)
                return false;

            int iId = order.InstrumentId;
            if (FillOnQuote)
            {
                var ask = this.framework.DataManager.GetAsk(iId);
                if (ask != null)
                    return FillWithAsk(order, ask);

                var bid = this.framework.DataManager.GetBid(iId);
                if (bid != null)
                    return FillWithBid(order, bid);
            }

            if (FillOnTrade)
            {
                var trade = this.framework.DataManager.GetTrade(iId);
                if (trade != null)
                    return FillWithTrade(order, trade);
            }

            if (FillOnBar)
            {
                var bar = this.framework.DataManager.GetBar(iId);
                if (bar != null && BarFilter.Contains(bar.Type, bar.Size))
                    return FillWithBar(order, bar);
            }
            return false;
        }

        private void ExecOrderRejected(Order order, string text)
        {
            EmitExecutionReport(new ExecutionReport
            {
                DateTime = this.framework.Clock.DateTime,
                Order = order,
                OrderId = order.Id,
                Instrument = order.Instrument,
                InstrumentId = order.InstrumentId,
                OrdQty = order.Qty,
                Price = order.Price,
                StopPx = order.StopPx,
                TimeInForce = order.TimeInForce,
                ExecType = ExecType.ExecRejected,
                OrdStatus = OrderStatus.Rejected,
                CurrencyId = order.Instrument.CurrencyId,
                OrdType = order.Type,
                Side = order.Side,
                Text = text
            }, Queued);
        }

        private static bool IsOrderDone(OrderStatus status)
        {
            switch (status)
            {
                case OrderStatus.Rejected:
                case OrderStatus.Filled:
                case OrderStatus.Cancelled:
                case OrderStatus.Expired:
                    return true;
            }
            return false;
        }

        private void ExecOrderCancelReject(Order order, string text)
        {
            var summary = this.summariesByOrderId[order.Id];
            EmitExecutionReport(new ExecutionReport
            {
                DateTime = this.framework.Clock.DateTime,
                Order = order,
                OrderId = order.Id,
                Instrument = order.Instrument,
                InstrumentId = order.InstrumentId,
                OrdQty = order.Qty,
                Price = order.Price,
                StopPx = order.StopPx,
                TimeInForce = order.TimeInForce,
                ExecType = ExecType.ExecCancelReject,
                OrdStatus = summary.OrdStatus,
                CurrencyId = order.Instrument.CurrencyId,
                OrdType = order.Type,
                Side = order.Side,
                CumQty = summary.CumQty,
                LeavesQty = summary.LeavesQty,
                AvgPx = summary.AvgPx,
                Text = text
            }, Queued);
        }

        [NotOriginal]
        private List<Order> GetOrdersBy(int instrumentId, bool create = false)
        {
            if (this.ordersByInstrumentId[instrumentId] == null && create)
                this.ordersByInstrumentId[instrumentId] = new List<Order>();
            return this.ordersByInstrumentId[instrumentId];
        } 
    }
}