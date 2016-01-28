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

        private List<Order> list_0 = new List<Order>();

        private List<Order> list_1 = new List<Order>();

        private IdArray<List<Order>> idArray_0= new IdArray<List<Order>>(10240);

        private IdArray<ReportSummary> idArray_1 = new IdArray<ReportSummary>(10240);

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

        public ExecutionSimulator(Framework framework):base(framework)
        {
            this.framework = framework;
            this.id = ProviderId.ExecutionSimulator;
            this.name = "ExecutionSimulator";
            this.description = "Default execution simulator";
            this.url = $"www.{nameof(SmartQuant).ToLower()}.com";
        }

        public override void Clear()
        {
            this.idArray_0.Clear();
            this.idArray_1.Clear();
        }

        public override void Send(ExecutionCommand command)
        {
            if (IsDisconnected)
                Connect();

            switch (command.Type)
            {
                case ExecutionCommandType.Send:
                    method_5(command.Order);
                    return;
                case ExecutionCommandType.Cancel:
                    method_7(command.Order);
                    return;
                case ExecutionCommandType.Replace:
                    method_9(command);
                    return;
                default:
                    return;
            }
        }

        public void OnBid(Bid bid)
        {
            var orders = this.idArray_0[bid.InstrumentId];
            if (orders == null)
                return;

            if (FillOnQuote)
            {
                foreach (var order in orders)
                    this.method_12(order, bid);
                this.method_11();
            }
        }

        public void OnAsk(Ask ask)
        {
            var orders = this.idArray_0[ask.InstrumentId];
            if (orders == null)
                return;

            if (FillOnQuote)
            {
                foreach (var order in orders)
                    this.method_13(order, ask);
                this.method_11();
            }
        }

        public void OnTrade(Trade trade)
        {
            var orders = this.idArray_0[trade.InstrumentId];
            if (orders == null)
                return;

            if (FillOnTrade)
            {
                foreach (var order in orders)
                    this.method_14(order, trade);
                this.method_11();
            }
        }

        public void OnBar(Bar bar)
        {
            var orders = this.idArray_0[bar.InstrumentId];
            if (orders == null)
                return;

            if (FillOnBar && (BarFilter.Count == 0 || BarFilter.Contains(bar.Type, bar.Size)))
            {
                foreach (var order in orders)
                    this.method_15(order, bar);
                this.method_11();
            }
        }

        public void OnBarOpen(Bar bar)
        {
            var orders = this.idArray_0[bar.InstrumentId];
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
                this.method_11();
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

        private bool method_13(Order order, Ask ask)
        {
            if (order.Side == OrderSide.Buy)
            {
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
            return false;
        }

        private void method_16(DateTime dateTime, object obj)
        {
            for (int i = this.list_1.Count - 1; i >= 0; i--)
            {
                var order = this.list_1[i];
                if (this.idArray_0[order.InstrumentId] == null)
                {
                    this.idArray_0[order.InstrumentId] = new List<Order>();
                }
                this.idArray_0[order.InstrumentId].Add(order);
                this.method_18(order);
                this.list_1.RemoveAt(i);
            }
        }

        private void method_11()
        {
            if (this.list_0.Count > 0)
            {
                foreach (var current in this.list_0)
                    this.idArray_0[current.InstrumentId].Remove(current);
                this.list_0.Clear();
            }
        }

        private void method_10(Order order, string text)
        {
            var summary = this.idArray_1[order.Id];
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

        private void method_5(Order order)
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
            this.idArray_1[order.Id] = new ReportSummary(report);
            EmitExecutionReport(report, Queued);
            if (order.TimeInForce == TimeInForce.AUC)
            {
                this.list_1.Add(order);
                if (this.list_1.Count == 1)
                {
                    this.framework.Clock.AddReminder(method_16, this.framework.Clock.DateTime.Date.Add(Auction1));
                    this.framework.Clock.AddReminder(method_17, this.framework.Clock.DateTime.Date.Add(Auction2));
                }
                return;
            }
            int iId = order.InstrumentId;
            if (this.idArray_0[iId] == null)
            {
                this.idArray_0[iId] = new List<Order>();
            }
            this.idArray_0[iId].Add(order);
            if (((order.Type == OrderType.Market || order.Type == OrderType.Pegged) && !FillMarketOnNext) || (order.Type == OrderType.Limit && !FillLimitOnNext) || (order.Type == OrderType.Stop && !FillStopOnNext) || (order.Type == OrderType.StopLimit && !FillStopLimitOnNext))
            {
                if (FillOnQuote)
                {
                    var ask = this.framework.DataManager.GetAsk(iId);
                    if (ask != null && this.method_13(order, ask))
                    {
                        this.method_11();
                        return;
                    }
                    var bid = this.framework.DataManager.GetBid(iId);
                    if (bid != null && this.method_12(order, bid))
                    {
                        this.method_11();
                        return;
                    }
                }
                if (FillOnTrade)
                {
                    var trade = this.framework.DataManager.GetTrade(iId);
                    if (trade != null && this.method_14(order, trade))
                    {
                        this.method_11();
                        return;
                    }
                }
                if (FillOnBar)
                {
                    var bar = this.framework.DataManager.GetBar(iId);
                    if (BarFilter.Count != 0 && !BarFilter.Contains(bar.Type, bar.Size))
                    {
                        return;
                    }
                    if (bar != null && this.method_15(order, bar))
                    {
                        this.method_11();
                    }
                }
            }
        }

        private void method_9(ExecutionCommand command)
        {
            var order = command.Order;
            if (this.idArray_1[order.Id] != null && IsOrderDone(this.idArray_1[order.Id].OrdStatus))
            {
                this.method_10(order, "Order already done");
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
            this.idArray_1[order.Id] = new ReportSummary(report);
            EmitExecutionReport(report, Queued);
        }

        private bool method_14(Order order_0, Trade trade_0)
        {
            while (true)
            {
                switch (order_0.Type)
                {
                    case OrderType.Market:
                    case OrderType.Pegged:
                        goto IL_1A0;
                    case OrderType.Stop:
                        switch (order_0.Side)
                        {
                            case OrderSide.Buy:
                                if (trade_0.Price >= order_0.StopPx)
                                {
                                    if (!FillAtStopPrice)
                                    {
                                        order_0.Type = OrderType.Market;
                                        continue;
                                    }
                                    goto IL_DA;
                                }
                                break;
                            case OrderSide.Sell:
                                if (trade_0.Price <= order_0.StopPx)
                                {
                                    if (!FillAtStopPrice)
                                    {
                                        order_0.Type = OrderType.Market;
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
                        switch (order_0.Side)
                        {
                            case OrderSide.Buy:
                                if (trade_0.Price >= order_0.StopPx)
                                {
                                    order_0.Type = OrderType.Limit;
                                    continue;
                                }
                                break;
                            case OrderSide.Sell:
                                if (trade_0.Price <= order_0.StopPx)
                                {
                                    order_0.Type = OrderType.Limit;
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
            this.Fill(order_0, order_0.StopPx, trade_0.Size);
            return true;
            IL_EF:
            this.Fill(order_0, order_0.StopPx, trade_0.Size);
            return true;
            IL_104:
            switch (order_0.Side)
            {
                case OrderSide.Buy:
                    if (trade_0.Price <= order_0.Price)
                    {
                        Fill(order_0, FillAtLimitPrice ? order_0.Price : trade_0.Price, trade_0.Size);
                        return true;
                    }
                    break;
                case OrderSide.Sell:
                    if (trade_0.Price >= order_0.Price)
                    {
                        Fill(order_0, FillAtLimitPrice ? order_0.Price : trade_0.Price, trade_0.Size);
                        return true;
                    }
                    break;
            }
            return false;
            IL_1A0:
            Fill(order_0, trade_0.Price, trade_0.Size);
            return true;
        }

        public void Fill(Order order, double price, int size)
        {
            if (!PartialFills)
            {
                this.list_0.Add(order);
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
                this.idArray_1[order.Id] = new ReportSummary(report);
                EmitExecutionReport(report, Queued);
                return;
            }
            if (size <= 0)
            {
                Console.WriteLine("ExecutionSimulator::Fill Error - using partial fills, size can not be zero");
                return;
            }
            var executionReport2 = new ExecutionReport
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
                this.list_0.Add(order);
                executionReport2.OrdStatus = OrderStatus.Filled;
                executionReport2.CumQty = order.CumQty + order.LeavesQty;
                executionReport2.LastQty = order.LeavesQty;
                executionReport2.LeavesQty = 0.0;
                executionReport2.LastPx = price;
                executionReport2.Text = order.Text;
                order.LeavesQty = executionReport2.LeavesQty;
            }
            else if (size < order.LeavesQty)
            {
                executionReport2.OrdStatus = OrderStatus.PartiallyFilled;
                executionReport2.CumQty = order.CumQty + size;
                executionReport2.LastQty = size;
                executionReport2.LeavesQty = order.LeavesQty - size;
                executionReport2.LastPx = price;
                executionReport2.Text = order.Text;
                order.LeavesQty = executionReport2.LeavesQty;
            }
            executionReport2.Commission = CommissionProvider.GetCommission(executionReport2);
            executionReport2.LastPx = SlippageProvider.GetPrice(executionReport2);
            this.idArray_1[order.Id] = new ReportSummary(executionReport2);
            EmitExecutionReport(executionReport2, Queued);
        }

        private bool method_18(Order order)
        {
            if (order.Type == OrderType.Limit)
            {
                int int_ = order.InstrumentId;
                if (FillOnQuote)
                {
                    var ask = this.framework.DataManager.GetAsk(int_);
                    if (ask != null && this.method_13(order, ask))
                    {
                        return true;
                    }
                    var bid = this.framework.DataManager.GetBid(int_);
                    if (bid != null && this.method_12(order, bid))
                    {
                        return true;
                    }
                }
                if (FillOnTrade)
                {
                    var trade = this.framework.DataManager.GetTrade(int_);
                    if (trade != null && this.method_14(order, trade))
                    {
                        return true;
                    }
                }
                if (FillOnBar)
                {
                    var bar = this.framework.DataManager.GetBar(int_);
                    if (this.BarFilter.Count != 0 && !BarFilter.Contains(bar.Type, bar.Size))
                    {
                        return false;
                    }
                    if (bar != null && this.method_15(order, bar))
                    {
                        return true;
                    }
                }
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

        private bool method_12(Order order, Bid bid)
        {
            if (order.Side == OrderSide.Sell)
            {
                while (true)
                {
                    switch (order.Type)
                    {
                        case OrderType.Market:
                        case OrderType.Pegged:
                            goto IL_C4;
                        case OrderType.Stop:
                            if (bid.Price <= order.StopPx)
                            {
                                if (!FillAtStopPrice)
                                {
                                    order.Type = OrderType.Market;
                                    continue;
                                }
                                goto IL_6F;
                            }
                            break;
                        case OrderType.Limit:
                            goto IL_84;
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
                IL_6F:
                Fill(order, order.StopPx, bid.Size);
                return true;
                IL_84:
                if (bid.Price >= order.Price)
                {
                    Fill(order, FillAtLimitPrice ? order.Price : bid.Price, bid.Size);
                    return true;
                }
                return false;
                IL_C4:
                Fill(order, bid.Price, bid.Size);
                return true;
            }
            return false;
        }

        private bool method_15(Order order, Bar bar)
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
                        if (FillAtLimitPrice)
                        {
                            this.Fill(order, order.Price, (int)bar.Volume);
                        }
                        else
                        {
                            this.Fill(order, bar.Close, (int)bar.Volume);
                        }
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

        private void method_17(DateTime dateTime_0, object object_0)
        {
            for (int i = this.list_1.Count - 1; i >= 0; i--)
            {
                Order order = this.list_1[i];
                if (this.idArray_0[order.InstrumentId] == null)
                {
                    this.idArray_0[order.InstrumentId] = new List<Order>();
                }
                this.idArray_0[order.InstrumentId].Add(order);
                if (!this.method_18(order))
                {
                    this.method_7(order);
                }
                this.list_1.RemoveAt(i);
            }
        }

        private static bool IsOrderDone(OrderStatus status)
        {
            bool result = false;
            switch (status)
            {
                case OrderStatus.Rejected:
                case OrderStatus.Filled:
                case OrderStatus.Cancelled:
                case OrderStatus.Expired:
                    result = true;
                    break;
            }
            return result;
        }

        private void method_8(Order order, string text)
        {
            ReportSummary @class = this.idArray_1[order.Id];
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
                OrdStatus = @class.OrdStatus,
                CurrencyId = order.Instrument.CurrencyId,
                OrdType = order.Type,
                Side = order.Side,
                CumQty = @class.CumQty,
                LeavesQty = @class.LeavesQty,
                AvgPx = @class.AvgPx,
                Text = text
            }, Queued);
        }

        private void method_7(Order order)
        {
            if (this.idArray_1[order.Id] != null && IsOrderDone(this.idArray_1[order.Id].OrdStatus))
            {
                this.method_8(order, "Order already done");
                return;
            }
            this.idArray_0[order.Instrument.Id].Remove(order);
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
            this.idArray_1[order.Id] = new ReportSummary(report);
            EmitExecutionReport(report, Queued);
        }
      }
}