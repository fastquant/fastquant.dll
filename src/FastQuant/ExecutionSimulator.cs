using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace FastQuant
{
    public class ExecutionSimulator : Provider,  IExecutionSimulator
    {
        class ReportSummary
        {
            public ReportSummary(ExecutionReport report)
            {
                OrdStatus = report.OrdStatus;
                AvgPx = report.AvgPx;
                CumQty = report.CumQty;
                LeavesQty = report.LeavesQty;
                Commission = report.Commission;
            }

            internal double AvgPx { get; }

            internal double CumQty { get; }

            internal double LeavesQty { get; }

            internal double Commission { get; }

            internal OrderStatus OrdStatus { get; }
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
            this.id = ProviderId.ExecutionSimulator;
            this.name = "ExecutionSimulator";
            this.description = "Default execution simulator";
            this.url = $"www.{nameof(FastQuant).ToLower()}.com";
        }

        public override void Clear()
        {
            this.ordersByInstrumentId.Clear();
            this.summariesByOrderId.Clear();
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

        private void ExecOrderReplaceReject(Order order, string text)
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

        private void ClearOrders()
        {
            if (this.orders.Count > 0)
            {
                foreach (var current in this.orders)
                    this.ordersByInstrumentId[current.InstrumentId].Remove(current);
                this.orders.Clear();
            }
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

        private bool FillWithTrade(Order order, Trade trade)
        {
            while (true)
            {
                switch (order.Type)
                {
                    case OrderType.Market:
                    case OrderType.Pegged:
                        this.Fill(order, trade.Price, trade.Size);
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
                                    goto IL_DA;
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
                                    goto IL_EF;
                                }
                                break;
                        }
                        break;
                    case OrderType.Limit:
                        goto IL_104;
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
            IL_DA:
            this.Fill(order, order.StopPx, trade.Size);
            return true;
            IL_EF:
            this.Fill(order, order.StopPx, trade.Size);
            return true;
            IL_104:
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
            IL_1A0:
            this.Fill(order, trade.Price, trade.Size);
            return true;
        }

        private bool FillWithBar(Order order, Bar bar)
        {
            while (true)
            {
                switch (order.Type)
                {
                    case OrderType.Market:
                    case OrderType.Pegged:
                        goto IL_1A6;
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
                                    goto IL_DA;
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
                                    goto IL_F0;
                                }
                                break;
                        }
                        break;
                    case OrderType.Limit:
                        goto IL_106;
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
            IL_DA:
            this.Fill(order, order.StopPx, (int)bar.Volume);
            return true;
            IL_F0:
            this.Fill(order, order.StopPx, (int)bar.Volume);
            return true;
            IL_106:
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
                        Fill(order, FillAtLimitPrice ? order.Price : bar.Close, (int) bar.Volume);
                        return true;
                    }
                    break;
            }
            return false;
            IL_1A6:
            this.Fill(order, bar.Close, (int)bar.Volume);
            return true;
        }

        private void OnAuction1(DateTime dateTime, object obj)
        {
            for (var i = this.stops.Count - 1; i >= 0; i--)
            {
                var order = this.stops[i];
                GetOrdersBy(order.InstrumentId,true).Add(order);
                this.FillLimitOrder(order);
                this.stops.RemoveAt(i);
            }
        }

        private void OnAuction2(DateTime dateTime, object obj)
        {
            for (int i = this.stops.Count - 1; i >= 0; i--)
            {
                var order = this.stops[i];
                GetOrdersBy(order.InstrumentId, true).Add(order);
                if (!FillLimitOrder(order))
                    HandleCancel(order);
                this.stops.RemoveAt(i);
            }
        }

        private bool FillLimitOrder(Order order)
        {
            if (order.Type != OrderType.Limit)
                return false;

            int iId = order.InstrumentId;
            if (FillOnQuote)
            {
                var ask = this.framework.DataManager.GetAsk(iId);
                if (ask != null && this.FillWithAsk(order, ask))
                {
                    return true;
                }
                var bid = this.framework.DataManager.GetBid(iId);
                if (bid != null && this.FillWithBid(order, bid))
                {
                    return true;
                }
            }
            if (FillOnTrade)
            {
                Trade trade = this.framework.DataManager.GetTrade(iId);
                if (trade != null && this.FillWithTrade(order, trade))
                {
                    return true;
                }
            }
            if (FillOnBar)
            {
                var bar = this.framework.DataManager.GetBar(iId);
                if (BarFilter.Count != 0 && !BarFilter.Contains(bar.Type, bar.Size))
                {
                    return false;
                }
                if (bar != null && this.FillWithBar(order, bar))
                {
                    return true;
                }
            }
            return false;
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

        private void HandleSend(Order order)
        {
            if (order.Qty == 0.0)
            {
                this.ExecOrderRejected(order, "Order amount can not be zero");
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
                LastQty = 0,
                LeavesQty = order.Qty,
                LastPx = 0,
                AvgPx = 0,
                Text = order.Text
            };
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
            int int_ = order.InstrumentId;
            GetOrdersBy(int_, true).Add(order);
            if (((order.Type == OrderType.Market || order.Type == OrderType.Pegged) && !FillMarketOnNext) || (order.Type == OrderType.Limit && !FillLimitOnNext) || (order.Type == OrderType.Stop && !FillStopOnNext) || (order.Type == OrderType.StopLimit && !FillStopLimitOnNext))
            {
                if (FillOnQuote)
                {
                    Ask ask = this.framework.DataManager.GetAsk(int_);
                    if (ask != null && FillWithAsk(order, ask))
                    {
                        this.ClearOrders();
                        return;
                    }
                    Bid bid = this.framework.DataManager.GetBid(int_);
                    if (bid != null && this.FillWithBid(order, bid))
                    {
                        this.ClearOrders();
                        return;
                    }
                }
                if (FillOnTrade)
                {
                    var trade = this.framework.DataManager.GetTrade(int_);
                    if (trade != null && this.FillWithTrade(order, trade))
                    {
                        this.ClearOrders();
                        return;
                    }
                }
                if (FillOnBar)
                {
                    var bar = this.framework.DataManager.GetBar(int_);
                    if (BarFilter.Count != 0 && !BarFilter.Contains(bar.Type, bar.Size))
                    {
                        return;
                    }
                    if (bar != null && this.FillWithBar(order, bar))
                    {
                        ClearOrders();
                    }
                }
            }
        }

        private void ExecOrderRejected(Order order, string text)
        {
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
                ExecType = ExecType.ExecRejected,
                OrdStatus = OrderStatus.Rejected,
                CurrencyId = order.Instrument.CurrencyId,
                OrdType = order.Type,
                Side = order.Side,
                Text = text
            }, Queued);
        }

        private void HandleCancel(Order order)
        {
            if (this.summariesByOrderId[order.Id] != null && IsOrderDone(this.summariesByOrderId[order.Id].OrdStatus))
            {
                this.ExecOrderCancelReject(order, "Order already done");
                return;
            }
            this.ordersByInstrumentId[order.Instrument.Id].Remove(order);
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

        private void HandleReplace(ExecutionCommand command)
        {
            var order = command.Order;
            if (this.summariesByOrderId[order.Id] != null && IsOrderDone(this.summariesByOrderId[order.Id].OrdStatus))
            {
                this.ExecOrderReplaceReject(order, "Order already done");
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

        public void OnAsk(Ask ask)
        {
            var orders = GetOrdersBy(ask.InstrumentId);
            if (orders == null)
                return;

            if (FillOnQuote)
            {
                foreach (var order in orders)
                    FillWithAsk(order, ask);
                ClearOrders();
            }
        }

        public void OnBar(Bar bar)
        {
            var orders = GetOrdersBy(bar.InstrumentId);
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
            if (this.ordersByInstrumentId[bar.InstrumentId] == null)
            {
                return;
            }
            if (FillOnBarOpen)
            {
                if (BarFilter.Count != 0 && !BarFilter.Contains(bar.Type, bar.Size))
                {
                    return;
                }
                int i = 0;
                while (i < this.ordersByInstrumentId[bar.InstrumentId].Count)
                {
                    Order order = this.ordersByInstrumentId[bar.InstrumentId][i];
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
                    this.Fill(order, order.StopPx, (int)bar.Volume);
                    goto IL_229;
                    IL_15B:
                    this.Fill(order, order.StopPx, (int)bar.Volume);
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
                                this.Fill(order, order.Price, (int)bar.Volume);
                                goto IL_229;
                            }
                            this.Fill(order, bar.Open, (int)bar.Volume);
                            goto IL_229;
                        case OrderSide.Sell:
                            if (bar.Open < order.Price)
                            {
                                goto IL_229;
                            }
                            if (FillAtLimitPrice)
                            {
                                this.Fill(order, order.Price, (int)bar.Volume);
                                goto IL_229;
                            }
                            this.Fill(order, bar.Open, (int)bar.Volume);
                            goto IL_229;
                        default:
                            goto IL_229;
                    }
                    IL_215:
                    this.Fill(order, bar.Open, (int)bar.Volume);
                    goto IL_229;
                }
                this.ClearOrders();
            }
        }

        public void OnBid(Bid bid)
        {
            var orders = GetOrdersBy(bid.InstrumentId);
            if (orders == null)
                return;

            if (FillOnQuote)
            {
                foreach (var order in orders)
                    FillWithBid(order, bid);
                ClearOrders();
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

        public void OnTrade(Trade trade)
        {
            var orders = GetOrdersBy(trade.InstrumentId);
            if (orders == null)
                return;

            if (FillOnTrade)
            {
                foreach (var order in orders)
                    FillWithTrade(order, trade);
                ClearOrders();
            }
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

        [NotOriginal]
        private List<Order> GetOrdersBy(int instrumentId, bool create = false)
        {
            if (this.ordersByInstrumentId[instrumentId] == null && create)
                this.ordersByInstrumentId[instrumentId] = new List<Order>();
            return this.ordersByInstrumentId[instrumentId];
        }
    }
}