using System;
using System.Collections.Generic;

namespace SmartQuant
{
    public class ExecutionSimulator : Provider, IExecutionSimulator
    {
        private class Class43
        {
            public Class43(ExecutionReport report)
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

        private IdArray<ExecutionSimulator.Class43> idArray_1 = new IdArray<ExecutionSimulator.Class43>(10240);

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
            this.url = "fastquant.org";
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
            var iId = bid.InstrumentId;
            if (this.idArray_0[iId] == null)
                return;

            if (FillOnQuote)
            {
                for (int i = 0; i < this.idArray_0[iId].Count; i++)
                {
                    Order order_ = this.idArray_0[iId][i];
                    this.method_12(order_, bid);
                }
                this.method_11();
            }
        }

        public void OnAsk(Ask ask)
        {
            var iId = ask.InstrumentId;
            if (this.idArray_0[iId] == null)
                return;

            if (FillOnQuote)
            {
                for (int i = 0; i < this.idArray_0[iId].Count; i++)
                {
                    Order order_ = this.idArray_0[iId][i];
                    this.method_13(order_, ask);
                }
                this.method_11();
            }
        }

        public void OnTrade(Trade trade)
        {
            var iId = trade.InstrumentId;
            if (this.idArray_0[iId] == null)
                return;

            if (FillOnTrade)
            {
                for (int i = 0; i < this.idArray_0[iId].Count; i++)
                {
                    var order_ = this.idArray_0[iId][i];
                    this.method_14(order_, trade);
                }
                this.method_11();
            }
        }

        public void OnBar(Bar bar)
        {
            var iId = bar.InstrumentId;
            if (this.idArray_0[iId] == null)
                return;

            if (FillOnBar)
            {
                if (BarFilter.Count != 0 && !BarFilter.Contains(bar.Type, bar.Size))
                {
                    return;
                }
                for (int i = 0; i < this.idArray_0[iId].Count; i++)
                {
                    Order order_ = this.idArray_0[iId][i];
                    this.method_15(order_, bar);
                }
                this.method_11();
            }
        }

        public void OnBarOpen(Bar bar)
        {
            var iId = bar.InstrumentId;
            if (this.idArray_0[iId] == null)
                return;

            if (FillOnBarOpen)
            {
                if (BarFilter.Count != 0 && !BarFilter.Contains(bar.Type, bar.Size))
                {
                    return;
                }
                int i = 0;
                while (i < this.idArray_0[iId].Count)
                {
                    Order order = this.idArray_0[iId][i];
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

        private bool method_13(Order order_0, Ask ask_0)
        {
            if (order_0.Side == OrderSide.Buy)
            {
                while (true)
                {
                    switch (order_0.Type)
                    {
                        case OrderType.Market:
                        case OrderType.Pegged:
                            goto IL_C3;
                        case OrderType.Stop:
                            if (ask_0.Price >= order_0.StopPx)
                            {
                                if (!FillAtStopPrice)
                                {
                                    order_0.Type = OrderType.Market;
                                    continue;
                                }
                                goto IL_6E;
                            }
                            break;
                        case OrderType.Limit:
                            goto IL_83;
                        case OrderType.StopLimit:
                            if (ask_0.Price >= order_0.StopPx)
                            {
                                order_0.Type = OrderType.Limit;
                                continue;
                            }
                            break;
                    }
                    break;
                }
                return false;
                IL_6E:
                this.Fill(order_0, order_0.StopPx, ask_0.Size);
                return true;
                IL_83:
                if (ask_0.Price <= order_0.Price)
                {
                    if (FillAtLimitPrice)
                    {
                        this.Fill(order_0, order_0.Price, ask_0.Size);
                    }
                    else
                    {
                        this.Fill(order_0, ask_0.Price, ask_0.Size);
                    }
                    return true;
                }
                return false;
                IL_C3:
                this.Fill(order_0, ask_0.Price, ask_0.Size);
                return true;
            }
            return false;
        }

        private void method_16(DateTime dateTime_0, object object_0)
        {
            for (int i = this.list_1.Count - 1; i >= 0; i--)
            {
                Order order = this.list_1[i];
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
                foreach (Order current in this.list_0)
                {
                    this.idArray_0[current.InstrumentId].Remove(current);
                }
                this.list_0.Clear();
            }
        }

        private void method_10(Order order_0, string string_0)
        {
            Class43 @class = this.idArray_1[order_0.Id];
            base.EmitExecutionReport(new ExecutionReport
            {
                dateTime = this.framework.Clock.DateTime,
                Order = order_0,
                OrderId = order_0.Id,
                Instrument = order_0.Instrument,
                InstrumentId = order_0.InstrumentId,
                OrdQty = order_0.Qty,
                Price = order_0.Price,
                StopPx = order_0.StopPx,
                TimeInForce = order_0.TimeInForce,
                ExecType = ExecType.ExecReplaceReject,
                OrdStatus = @class.OrdStatus,
                CurrencyId = order_0.Instrument.CurrencyId,
                OrdType = order_0.Type,
                Side = order_0.Side,
                CumQty = @class.CumQty,
                LeavesQty = @class.LeavesQty,
                AvgPx = @class.AvgPx,
                Text = string_0
            }, Queued);
        }

        private void method_5(Order order_0)
        {
            if (order_0.Qty == 0)
            {
                this.method_6(order_0, "Order amount can not be zero");
                return;
            }
            ExecutionReport executionReport = new ExecutionReport();
            executionReport.dateTime = this.framework.Clock.DateTime;
            executionReport.Order = order_0;
            executionReport.OrderId = order_0.Id;
            executionReport.Instrument = order_0.Instrument;
            executionReport.InstrumentId = order_0.InstrumentId;
            executionReport.ExecType = ExecType.ExecNew;
            executionReport.OrdStatus = OrderStatus.New;
            executionReport.CurrencyId = order_0.Instrument.CurrencyId;
            executionReport.OrdType = order_0.Type;
            executionReport.Side = order_0.Side;
            executionReport.OrdQty = order_0.Qty;
            executionReport.Price = order_0.Price;
            executionReport.StopPx = order_0.StopPx;
            executionReport.TimeInForce = order_0.TimeInForce;
            executionReport.CumQty = 0.0;
            executionReport.LastQty = 0.0;
            executionReport.OrdQty = order_0.Qty;
            executionReport.LastPx = 0.0;
            executionReport.AvgPx = 0.0;
            executionReport.Text = order_0.Text;
            order_0.LeavesQty = executionReport.LeavesQty;
            this.idArray_1[order_0.Id] = new ExecutionSimulator.Class43(executionReport);
            base.EmitExecutionReport(executionReport, Queued);
            if (order_0.TimeInForce == TimeInForce.AUC)
            {
                this.list_1.Add(order_0);
                if (this.list_1.Count == 1)
                {
                    this.framework.Clock.AddReminder(new ReminderCallback(this.method_16), this.framework.Clock.DateTime.Date.Add(Auction1), null);
                    this.framework.Clock.AddReminder(new ReminderCallback(this.method_17), this.framework.Clock.DateTime.Date.Add(Auction2), null);
                }
                return;
            }
            int int_ = order_0.InstrumentId;
            if (this.idArray_0[int_] == null)
            {
                this.idArray_0[int_] = new List<Order>();
            }
            this.idArray_0[int_].Add(order_0);
            if (((order_0.Type == OrderType.Market || order_0.Type == OrderType.Pegged) && !FillMarketOnNext) || (order_0.Type == OrderType.Limit && !FillLimitOnNext) || (order_0.Type == OrderType.Stop && !FillStopOnNext) || (order_0.Type == OrderType.StopLimit && !FillStopLimitOnNext))
            {
                if (FillOnQuote)
                {
                    Ask ask = this.framework.DataManager.GetAsk(int_);
                    if (ask != null && this.method_13(order_0, ask))
                    {
                        this.method_11();
                        return;
                    }
                    Bid bid = this.framework.DataManager.GetBid(int_);
                    if (bid != null && this.method_12(order_0, bid))
                    {
                        this.method_11();
                        return;
                    }
                }
                if (FillOnTrade)
                {
                    Trade trade = this.framework.DataManager.GetTrade(int_);
                    if (trade != null && this.method_14(order_0, trade))
                    {
                        this.method_11();
                        return;
                    }
                }
                if (FillOnBar)
                {
                    Bar bar = this.framework.DataManager.GetBar(int_);
                    if (BarFilter.Count != 0 && !BarFilter.Contains(bar.Type, bar.Size))
                    {
                        return;
                    }
                    if (bar != null && this.method_15(order_0, bar))
                    {
                        this.method_11();
                    }
                }
            }
        }

        private void method_9(ExecutionCommand executionCommand_0)
        {
            Order order_ = executionCommand_0.Order;
            if (this.idArray_1[order_.Id] != null && this.method_19(this.idArray_1[order_.Id].OrdStatus))
            {
                this.method_10(order_, "Order already done");
                return;
            }
            ExecutionReport executionReport = new ExecutionReport();
            executionReport.dateTime = this.framework.Clock.DateTime;
            executionReport.Order = order_;
            executionReport.OrderId = order_.Id;
            executionReport.Instrument = order_.Instrument;
            executionReport.InstrumentId = order_.InstrumentId;
            executionReport.ExecType = ExecType.ExecReplace;
            executionReport.OrdStatus = OrderStatus.Replaced;
            executionReport.CurrencyId = order_.Instrument.CurrencyId;
            executionReport.OrdType = order_.Type;
            executionReport.Side = order_.Side;
            executionReport.CumQty = order_.CumQty;
            executionReport.LastQty = 0.0;
            executionReport.LeavesQty = executionCommand_0.Qty - order_.CumQty;
            executionReport.LastPx = 0.0;
            executionReport.AvgPx = 0.0;
            executionReport.OrdType = order_.Type;
            executionReport.Price = executionCommand_0.Price;
            executionReport.StopPx = executionCommand_0.StopPx;
            executionReport.OrdQty = executionCommand_0.Qty;
            executionReport.TimeInForce = order_.TimeInForce;
            executionReport.Text = order_.Text;
            this.idArray_1[order_.Id] = new ExecutionSimulator.Class43(executionReport);
            base.EmitExecutionReport(executionReport, Queued);
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
                        if (FillAtLimitPrice)
                        {
                            this.Fill(order_0, order_0.Price, trade_0.Size);
                        }
                        else
                        {
                            this.Fill(order_0, trade_0.Price, trade_0.Size);
                        }
                        return true;
                    }
                    break;
                case OrderSide.Sell:
                    if (trade_0.Price >= order_0.Price)
                    {
                        if (FillAtLimitPrice)
                        {
                            this.Fill(order_0, order_0.Price, trade_0.Size);
                        }
                        else
                        {
                            this.Fill(order_0, trade_0.Price, trade_0.Size);
                        }
                        return true;
                    }
                    break;
            }
            return false;
            IL_1A0:
            this.Fill(order_0, trade_0.Price, trade_0.Size);
            return true;
        }

        public void Fill(Order order, double price, int size)
        {
            if (!PartialFills)
            {
                this.list_0.Add(order);
                ExecutionReport executionReport = new ExecutionReport();
                executionReport.dateTime = this.framework.Clock.DateTime;
                executionReport.Order = order;
                executionReport.OrderId = order.Id;
                executionReport.OrdType = order.Type;
                executionReport.Side = order.Side;
                executionReport.Instrument = order.Instrument;
                executionReport.InstrumentId = order.InstrumentId;
                executionReport.OrdQty = order.Qty;
                executionReport.Price = order.Price;
                executionReport.StopPx = order.StopPx;
                executionReport.TimeInForce = order.TimeInForce;
                executionReport.ExecType = ExecType.ExecTrade;
                executionReport.OrdStatus = OrderStatus.Filled;
                executionReport.CurrencyId = order.Instrument.CurrencyId;
                executionReport.CumQty = order.LeavesQty;
                executionReport.LastQty = order.LeavesQty;
                executionReport.LeavesQty = 0.0;
                executionReport.LastPx = price;
                executionReport.Text = order.Text;
                executionReport.Commission = this.CommissionProvider.GetCommission(executionReport);
                executionReport.LastPx = this.SlippageProvider.GetPrice(executionReport);
                this.idArray_1[order.Id] = new ExecutionSimulator.Class43(executionReport);
                base.EmitExecutionReport(executionReport, this.Queued);
                return;
            }
            if (size <= 0)
            {
                Console.WriteLine("ExecutionSimulator::Fill Error - using partial fills, size can not be zero");
                return;
            }
            ExecutionReport executionReport2 = new ExecutionReport();
            executionReport2.dateTime = this.framework.Clock.DateTime;
            executionReport2.Order = order;
            executionReport2.OrderId = order.Id;
            executionReport2.OrdType = order.Type;
            executionReport2.Side = order.Side;
            executionReport2.Instrument = order.Instrument;
            executionReport2.InstrumentId = order.InstrumentId;
            executionReport2.OrdQty = order.Qty;
            executionReport2.Price = order.Price;
            executionReport2.StopPx = order.StopPx;
            executionReport2.TimeInForce = order.TimeInForce;
            executionReport2.ExecType = ExecType.ExecTrade;
            executionReport2.CurrencyId = order.Instrument.CurrencyId;
            if ((double)size >= order.LeavesQty)
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
            else if ((double)size < order.LeavesQty)
            {
                executionReport2.OrdStatus = OrderStatus.PartiallyFilled;
                executionReport2.CumQty = order.CumQty + (double)size;
                executionReport2.LastQty = (double)size;
                executionReport2.LeavesQty = order.LeavesQty - (double)size;
                executionReport2.LastPx = price;
                executionReport2.Text = order.Text;
                order.LeavesQty = executionReport2.LeavesQty;
            }
            executionReport2.Commission = CommissionProvider.GetCommission(executionReport2);
            executionReport2.LastPx = SlippageProvider.GetPrice(executionReport2);
            this.idArray_1[order.Id] = new Class43(executionReport2);
            EmitExecutionReport(executionReport2, Queued);
        }


        private bool method_18(Order order_0)
        {
            if (order_0.Type == OrderType.Limit)
            {
                int int_ = order_0.InstrumentId;
                if (FillOnQuote)
                {
                    Ask ask = this.framework.DataManager.GetAsk(int_);
                    if (ask != null && this.method_13(order_0, ask))
                    {
                        return true;
                    }
                    Bid bid = this.framework.DataManager.GetBid(int_);
                    if (bid != null && this.method_12(order_0, bid))
                    {
                        return true;
                    }
                }
                if (FillOnTrade)
                {
                    Trade trade = this.framework.DataManager.GetTrade(int_);
                    if (trade != null && this.method_14(order_0, trade))
                    {
                        return true;
                    }
                }
                if (FillOnBar)
                {
                    Bar bar = this.framework.DataManager.GetBar(int_);
                    if (this.BarFilter.Count != 0 && !BarFilter.Contains(bar.Type, bar.Size))
                    {
                        return false;
                    }
                    if (bar != null && this.method_15(order_0, bar))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void Send(Order order)
        {
            if (order.Qty == 0)
            {
                this.method_6(order, "Order amount can not be zero");
                return;
            }
            var report = new ExecutionReport();
            report.DateTime = this.framework.Clock.DateTime;
            report.Order = order;
            report.OrderId = order.Id;
            report.Instrument = order.Instrument;
            report.InstrumentId = order.InstrumentId;
            report.ExecType = ExecType.ExecNew;
            report.OrdStatus = OrderStatus.New;
            report.CurrencyId = order.Instrument.CurrencyId;
            report.OrdType = order.Type;
            report.Side = order.Side;
            report.OrdQty = order.Qty;
            report.Price = order.Price;
            report.StopPx = order.StopPx;
            report.TimeInForce = order.TimeInForce;
            report.CumQty = 0;
            report.LastQty = 0;
            report.LeavesQty = order.Qty;
            report.LastPx = 0;
            report.AvgPx = 0;
            report.Text = order.Text;
            order.LeavesQty = report.LeavesQty;
            this.idArray_1[order.Id] = new ExecutionSimulator.Class43(report);
            EmitExecutionReport(report, Queued);
            if (order.TimeInForce == TimeInForce.AUC)
            {
                this.list_1.Add(order);
                if (this.list_1.Count == 1)
                {
                    this.framework.Clock.AddReminder(new ReminderCallback(this.method_16), this.framework.Clock.DateTime.Date.Add(Auction1), null);
                    this.framework.Clock.AddReminder(new ReminderCallback(this.method_17), this.framework.Clock.DateTime.Date.Add(Auction2), null);
                }
                return;
            }
            int int_ = order.InstrumentId;
            if (this.idArray_0[int_] == null)
            {
                this.idArray_0[int_] = new List<Order>();
            }
            this.idArray_0[int_].Add(order);
            if (((order.Type == OrderType.Market || order.Type == OrderType.Pegged) && !FillMarketOnNext) || (order.Type == OrderType.Limit && !FillLimitOnNext) || (order.Type == OrderType.Stop && !FillStopOnNext) || (order.Type == OrderType.StopLimit && !FillStopLimitOnNext))
            {
                if (FillOnQuote)
                {
                    Ask ask = this.framework.DataManager.GetAsk(int_);
                    if (ask != null && this.method_13(order, ask))
                    {
                        this.method_11();
                        return;
                    }
                    Bid bid = this.framework.DataManager.GetBid(int_);
                    if (bid != null && this.method_12(order, bid))
                    {
                        this.method_11();
                        return;
                    }
                }
                if (FillOnTrade)
                {
                    Trade trade = this.framework.DataManager.GetTrade(int_);
                    if (trade != null && this.method_14(order, trade))
                    {
                        this.method_11();
                        return;
                    }
                }
                if (FillOnBar)
                {
                    Bar bar = this.framework.DataManager.GetBar(int_);
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

        private void method_6(Order order, string text)
        {
            base.EmitExecutionReport(new ExecutionReport
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
        private bool method_12(Order order_0, Bid bid_0)
        {
            if (order_0.Side == OrderSide.Sell)
            {
                while (true)
                {
                    switch (order_0.Type)
                    {
                        case OrderType.Market:
                        case OrderType.Pegged:
                            goto IL_C4;
                        case OrderType.Stop:
                            if (bid_0.Price <= order_0.StopPx)
                            {
                                if (!FillAtStopPrice)
                                {
                                    order_0.Type = OrderType.Market;
                                    continue;
                                }
                                goto IL_6F;
                            }
                            break;
                        case OrderType.Limit:
                            goto IL_84;
                        case OrderType.StopLimit:
                            if (bid_0.Price <= order_0.StopPx)
                            {
                                order_0.Type = OrderType.Limit;
                                continue;
                            }
                            break;
                    }
                    break;
                }
                return false;
                IL_6F:
                this.Fill(order_0, order_0.StopPx, bid_0.Size);
                return true;
                IL_84:
                if (bid_0.Price >= order_0.Price)
                {
                    if (FillAtLimitPrice)
                    {
                        this.Fill(order_0, order_0.Price, bid_0.Size);
                    }
                    else
                    {
                        this.Fill(order_0, bid_0.Price, bid_0.Size);
                    }
                    return true;
                }
                return false;
                IL_C4:
                this.Fill(order_0, bid_0.Price, bid_0.Size);
                return true;
            }
            return false;
        }

        private bool method_15(Order order_0, Bar bar_0)
        {
            while (true)
            {
                switch (order_0.Type)
                {
                    case OrderType.Market:
                    case OrderType.Pegged:
                        goto IL_1A6;
                    case OrderType.Stop:
                        switch (order_0.Side)
                        {
                            case OrderSide.Buy:
                                if (bar_0.High >= order_0.StopPx)
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
                                if (bar_0.Low <= order_0.StopPx)
                                {
                                    if (!FillAtStopPrice)
                                    {
                                        order_0.Type = OrderType.Market;
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
                        switch (order_0.Side)
                        {
                            case OrderSide.Buy:
                                if (bar_0.High >= order_0.StopPx)
                                {
                                    order_0.Type = OrderType.Limit;
                                    continue;
                                }
                                break;
                            case OrderSide.Sell:
                                if (bar_0.Low <= order_0.StopPx)
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
            this.Fill(order_0, order_0.StopPx, (int)bar_0.Volume);
            return true;
            IL_F0:
            this.Fill(order_0, order_0.StopPx, (int)bar_0.Volume);
            return true;
            IL_106:
            switch (order_0.Side)
            {
                case OrderSide.Buy:
                    if (bar_0.Low <= order_0.Price)
                    {
                        if (FillAtLimitPrice)
                        {
                            this.Fill(order_0, order_0.Price, (int)bar_0.Volume);
                        }
                        else
                        {
                            this.Fill(order_0, bar_0.Close, (int)bar_0.Volume);
                        }
                        return true;
                    }
                    break;
                case OrderSide.Sell:
                    if (bar_0.High >= order_0.Price)
                    {
                        if (FillAtLimitPrice)
                        {
                            this.Fill(order_0, order_0.Price, (int)bar_0.Volume);
                        }
                        else
                        {
                            this.Fill(order_0, bar_0.Close, (int)bar_0.Volume);
                        }
                        return true;
                    }
                    break;
            }
            return false;
            IL_1A6:
            this.Fill(order_0, bar_0.Close, (int)bar_0.Volume);
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
        private bool method_19(OrderStatus orderStatus_0)
        {
            bool result = false;
            switch (orderStatus_0)
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

        private void method_8(Order order_0, string string_0)
        {
            ExecutionSimulator.Class43 @class = this.idArray_1[order_0.Id];
            base.EmitExecutionReport(new ExecutionReport
            {
                dateTime = this.framework.Clock.DateTime,
                Order = order_0,
                OrderId = order_0.Id,
                Instrument = order_0.Instrument,
                InstrumentId = order_0.InstrumentId,
                OrdQty = order_0.Qty,
                Price = order_0.Price,
                StopPx = order_0.StopPx,
                TimeInForce = order_0.TimeInForce,
                ExecType = ExecType.ExecCancelReject,
                OrdStatus = @class.OrdStatus,
                CurrencyId = order_0.Instrument.CurrencyId,
                OrdType = order_0.Type,
                Side = order_0.Side,
                CumQty = @class.CumQty,
                LeavesQty = @class.LeavesQty,
                AvgPx = @class.AvgPx,
                Text = string_0
            }, Queued);
        }



        private void method_7(Order order_0)
        {
            if (this.idArray_1[order_0.Id] != null && this.method_19(this.idArray_1[order_0.Id].OrdStatus))
            {
                this.method_8(order_0, "Order already done");
                return;
            }
            this.idArray_0[order_0.Instrument.Id].Remove(order_0);
            ExecutionReport executionReport = new ExecutionReport();
            executionReport.dateTime = this.framework.Clock.DateTime;
            executionReport.Order = order_0;
            executionReport.OrderId = order_0.Id;
            executionReport.Instrument = order_0.Instrument;
            executionReport.InstrumentId = order_0.InstrumentId;
            executionReport.OrdQty = order_0.Qty;
            executionReport.Price = order_0.Price;
            executionReport.StopPx = order_0.StopPx;
            executionReport.TimeInForce = order_0.TimeInForce;
            executionReport.ExecType = ExecType.ExecCancelled;
            executionReport.OrdStatus = OrderStatus.Cancelled;
            executionReport.CurrencyId = order_0.Instrument.CurrencyId;
            executionReport.OrdType = order_0.Type;
            executionReport.Side = order_0.Side;
            executionReport.CumQty = order_0.CumQty;
            executionReport.LastQty = 0.0;
            executionReport.LeavesQty = order_0.LeavesQty;
            executionReport.LastPx = 0.0;
            executionReport.AvgPx = 0.0;
            executionReport.Text = order_0.Text;
            this.idArray_1[order_0.Id] = new Class43(executionReport);
            EmitExecutionReport(executionReport, Queued);
        }
      }
}