using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace SmartQuant
{
    public class ExecutionSimulator : Provider,  IExecutionSimulator
    {
        public ExecutionSimulator(Framework framework):base(framework)
        {
            this.UcPswEhbh7 = true;
            this.bool_0 = true;
            this.bool_3 = true;
            this.bool_5 = true;
            this.bool_6 = true;
            this.bool_7 = true;
            this.bool_8 = true;
            this.bool_11 = true;
            this.barFilter_0 = new BarFilter();
            this.ginterface2_0 = new CommissionProvider();
            this.islippageProvider_0 = new SlippageProvider();
            this.list_1 = new List<Order>();
            this.id = 2;
            this.name = "ExecutionSimulator";
            this.description = "Default execution simulator";
            this.url = "www.smartquant.com";
            this.idArray_0 = new IdArray<List<Order>>(10000);
            this.idArray_1 = new IdArray<Class43>(10000);
            this.list_0 = new List<Order>();
        }

        public override void Clear()
        {
            this.idArray_0.Clear();
            this.idArray_1.Clear();
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
                    LeavesQty = 0.0
                };
                report.LastPx = price;
                report.Text = order.Text;
                report.Commission = CommissionProvider.GetCommission(report);
                report.LastPx = SlippageProvider.GetPrice(report);
                this.idArray_1[order.Id] = new Class43(report);
                EmitExecutionReport(report, Queued);
                return;
            }
            if (size <= 0)
            {
                Console.WriteLine("ExecutionSimulator::Fill Error - using partial fills, size can not be zero");
                return;
            }
            ExecutionReport executionReport2 = new ExecutionReport();
            executionReport2.DateTime = this.framework.Clock.DateTime;
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
            executionReport2.Commission = this.ginterface2_0.GetCommission(executionReport2);
            executionReport2.LastPx = this.islippageProvider_0.GetPrice(executionReport2);
            this.idArray_1[order.Id] = new Class43(executionReport2);
            base.EmitExecutionReport(executionReport2, this.bool_11);
        }

        private void method_10(Order order_0, string string_0)
        {
            Class43 @class = this.idArray_1[order_0.Id];
            base.EmitExecutionReport(new ExecutionReport
            {
                DateTime = this.framework.Clock.DateTime,
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
            }, this.bool_11);
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
                                if (!this.bool_9)
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
                    if (this.bool_8)
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
                            if (!this.bool_9)
                            {
                                order.Type = OrderType.Market;
                                continue;
                            }
                            goto IL_6E;
                        }
                        break;
                    case OrderType.Limit:
                        goto IL_83;
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
            IL_6E:
            this.Fill(order, order.StopPx, ask.Size);
            return true;
            IL_83:
            if (ask.Price <= order.Price)
            {
                if (this.bool_8)
                {
                    this.Fill(order, order.Price, ask.Size);
                }
                else
                {
                    this.Fill(order, ask.Price, ask.Size);
                }
                return true;
            }
            return false;
            IL_C3:
            this.Fill(order, ask.Price, ask.Size);
            return true;
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
                                    if (!this.bool_9)
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
                                    if (!this.bool_9)
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
                        if (this.bool_8)
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
                        if (this.bool_8)
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
                                    if (!this.bool_9)
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
                                    if (!this.bool_9)
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
                        if (this.bool_8)
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
                        if (this.bool_8)
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

        private bool method_18(Order order_0)
        {
            if (order_0.Type == OrderType.Limit)
            {
                int int_ = order_0.InstrumentId;
                if (this.UcPswEhbh7)
                {
                    Ask ask = this.framework.DataManager.GetAsk(int_);
                    if (ask != null && this.FillWithAsk(order_0, ask))
                    {
                        return true;
                    }
                    Bid bid = this.framework.DataManager.GetBid(int_);
                    if (bid != null && this.method_12(order_0, bid))
                    {
                        return true;
                    }
                }
                if (this.bool_0)
                {
                    Trade trade = this.framework.DataManager.GetTrade(int_);
                    if (trade != null && this.method_14(order_0, trade))
                    {
                        return true;
                    }
                }
                if (this.bool_1)
                {
                    Bar bar = this.framework.DataManager.GetBar(int_);
                    if (this.barFilter_0.Count != 0 && !this.barFilter_0.Contains(bar.Type, bar.Size))
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

        private void HandleSend(Order order)
        {
            if (order.Qty == 0.0)
            {
                this.method_6(order, "Order amount can not be zero");
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
            this.idArray_1[order.Id] = new Class43(report);
            EmitExecutionReport(report, Queued);
            if (order.TimeInForce == TimeInForce.AUC)
            {
                this.list_1.Add(order);
                if (this.list_1.Count == 1)
                {
                    this.framework.Clock.AddReminder(method_16, this.framework.Clock.DateTime.Date.Add(this.timeSpan_0));
                    this.framework.Clock.AddReminder(method_17, this.framework.Clock.DateTime.Date.Add(this.timeSpan_1));
                }
                return;
            }
            int int_ = order.InstrumentId;
            GetOrdersBy(int_, true).Add(order);
            //if (this.idArray_0[int_] == null)
            //{
            //    this.idArray_0[int_] = new List<Order>();
            //}
            //this.idArray_0[int_].Add(order);
            if (((order.Type == OrderType.Market || order.Type == OrderType.Pegged) && !this.bool_4) || (order.Type == OrderType.Limit && !this.bool_5) || (order.Type == OrderType.Stop && !this.bool_6) || (order.Type == OrderType.StopLimit && !this.bool_7))
            {
                if (this.UcPswEhbh7)
                {
                    Ask ask = this.framework.DataManager.GetAsk(int_);
                    if (ask != null && FillWithAsk(order, ask))
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
                if (this.bool_0)
                {
                    Trade trade = this.framework.DataManager.GetTrade(int_);
                    if (trade != null && this.method_14(order, trade))
                    {
                        this.method_11();
                        return;
                    }
                }
                if (this.bool_1)
                {
                    Bar bar = this.framework.DataManager.GetBar(int_);
                    if (this.barFilter_0.Count != 0 && !this.barFilter_0.Contains(bar.Type, bar.Size))
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

        private void method_6(Order order_0, string string_0)
        {
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
                ExecType = ExecType.ExecRejected,
                OrdStatus = OrderStatus.Rejected,
                CurrencyId = order_0.Instrument.CurrencyId,
                OrdType = order_0.Type,
                Side = order_0.Side,
                Text = string_0
            }, this.bool_11);
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
            base.EmitExecutionReport(executionReport, this.bool_11);
        }

        private void method_8(Order order_0, string string_0)
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
                ExecType = ExecType.ExecCancelReject,
                OrdStatus = @class.OrdStatus,
                CurrencyId = order_0.Instrument.CurrencyId,
                OrdType = order_0.Type,
                Side = order_0.Side,
                CumQty = @class.CumQty,
                LeavesQty = @class.LeavesQty,
                AvgPx = @class.AvgPx,
                Text = string_0
            }, this.bool_11);
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
            this.idArray_1[order_.Id] = new Class43(executionReport);
            base.EmitExecutionReport(executionReport, this.bool_11);
        }

        public void OnAsk(Ask ask)
        {
            if (this.idArray_0[ask.InstrumentId] == null)
            {
                return;
            }
            if (this.UcPswEhbh7)
            {
                for (int i = 0; i < this.idArray_0[ask.InstrumentId].Count; i++)
                {
                    Order order_ = this.idArray_0[ask.InstrumentId][i];
                    this.FillWithAsk(order_, ask);
                }
                this.method_11();
            }
        }

        public void OnBar(Bar bar)
        {
            if (this.idArray_0[bar.InstrumentId] == null)
            {
                return;
            }
            if (this.bool_1)
            {
                if (this.barFilter_0.Count != 0 && !this.barFilter_0.Contains(bar.Type, bar.Size))
                {
                    return;
                }
                for (int i = 0; i < this.idArray_0[bar.InstrumentId].Count; i++)
                {
                    Order order_ = this.idArray_0[bar.InstrumentId][i];
                    this.method_15(order_, bar);
                }
                this.method_11();
            }
        }

        public void OnBarOpen(Bar bar)
        {
            if (this.idArray_0[bar.InstrumentId] == null)
            {
                return;
            }
            if (this.bool_2)
            {
                if (this.barFilter_0.Count != 0 && !this.barFilter_0.Contains(bar.Type, bar.Size))
                {
                    return;
                }
                int i = 0;
                while (i < this.idArray_0[bar.InstrumentId].Count)
                {
                    Order order = this.idArray_0[bar.InstrumentId][i];
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
                                            if (!this.bool_9)
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
                                            if (!this.bool_9)
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
                            if (this.bool_8)
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
                            if (this.bool_8)
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

        public void OnBid(Bid bid)
        {
            if (this.idArray_0[bid.InstrumentId] == null)
            {
                return;
            }
            if (this.UcPswEhbh7)
            {
                for (int i = 0; i < this.idArray_0[bid.InstrumentId].Count; i++)
                {
                    Order order_ = this.idArray_0[bid.InstrumentId][i];
                    this.method_12(order_, bid);
                }
                this.method_11();
            }
        }

        public void OnLevel2(Level2Snapshot snapshot)
        {
        }

        public void OnLevel2(Level2Update update)
        {
        }

        public void OnTrade(Trade trade)
        {
            if (this.idArray_0[trade.InstrumentId] == null)
            {
                return;
            }
            if (this.bool_0)
            {
                for (int i = 0; i < this.idArray_0[trade.InstrumentId].Count; i++)
                {
                    Order order_ = this.idArray_0[trade.InstrumentId][i];
                    this.method_14(order_, trade);
                }
                this.method_11();
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
                    this.method_7(command.Order);
                    return;
                case ExecutionCommandType.Replace:
                    this.method_9(command);
                    return;
                default:
                    return;
            }
        }

        public TimeSpan Auction1
        {
            get
            {
                return this.timeSpan_0;
            }
            set
            {
                this.timeSpan_0 = value;
            }
        }

        public TimeSpan Auction2
        {
            get
            {
                return this.timeSpan_1;
            }
            set
            {
                this.timeSpan_1 = value;
            }
        }

        public BarFilter BarFilter
        {
            get
            {
                return this.barFilter_0;
            }
        }

        public ICommissionProvider CommissionProvider
        {
            get
            {
                return this.ginterface2_0;
            }
            set
            {
                this.ginterface2_0 = value;
            }
        }

        public bool FillAtLimitPrice
        {
            get
            {
                return this.bool_8;
            }
            set
            {
                this.bool_8 = value;
            }
        }

        public bool FillAtStopPrice
        {
            get
            {
                return this.bool_9;
            }
            set
            {
                this.bool_9 = value;
            }
        }

        public bool FillLimitOnNext
        {
            get
            {
                return this.bool_5;
            }
            set
            {
                this.bool_5 = value;
            }
        }

        public bool FillMarketOnNext
        {
            get
            {
                return this.bool_4;
            }
            set
            {
                this.bool_4 = value;
            }
        }

        public bool FillOnBar
        {
            get
            {
                return this.bool_1;
            }
            set
            {
                this.bool_1 = value;
            }
        }

        public bool FillOnBarOpen
        {
            get
            {
                return this.bool_2;
            }
            set
            {
                this.bool_2 = value;
            }
        }

        public bool FillOnLevel2
        {
            get
            {
                return this.bool_3;
            }
            set
            {
                this.bool_3 = value;
            }
        }

        public bool FillOnQuote
        {
            get
            {
                return this.UcPswEhbh7;
            }
            set
            {
                this.UcPswEhbh7 = value;
            }
        }

        public bool FillOnTrade
        {
            get
            {
                return this.bool_0;
            }
            set
            {
                this.bool_0 = value;
            }
        }

        public bool FillStopLimitOnNext
        {
            get
            {
                return this.bool_7;
            }
            set
            {
                this.bool_7 = value;
            }
        }

        public bool FillStopOnNext
        {
            get
            {
                return this.bool_6;
            }
            set
            {
                this.bool_6 = value;
            }
        }

        public bool PartialFills
        {
            get
            {
                return this.bool_10;
            }
            set
            {
                this.bool_10 = value;
            }
        }

        public bool Queued
        {
            get
            {
                return this.bool_11;
            }
            set
            {
                this.bool_11 = value;
            }
        }

        public ISlippageProvider SlippageProvider
        {
            get
            {
                return this.islippageProvider_0;
            }
            set
            {
                this.islippageProvider_0 = value;
            }
        }

        private BarFilter barFilter_0;

        private bool bool_0;

        private bool bool_1;

        private bool bool_10;

        private bool bool_11;

        private bool bool_2;

        private bool bool_3;

        private bool bool_4;

        private bool bool_5;

        private bool bool_6;

        private bool bool_7;

        private bool bool_8;

        private bool bool_9;

        private ICommissionProvider ginterface2_0;

        private IdArray<List<Order>> idArray_0;

        private IdArray<Class43> idArray_1;

        private ISlippageProvider islippageProvider_0;

        private List<Order> list_0;

        private List<Order> list_1;

        private TimeSpan timeSpan_0;

        private TimeSpan timeSpan_1;

        private bool UcPswEhbh7;


        [NotOriginal]
        private List<Order> GetOrdersBy(int instrumentId, bool create = false)
        {
            if (this.idArray_0[instrumentId] == null && create)
                this.idArray_0[instrumentId] = new List<Order>();
            return this.idArray_0[instrumentId];
        }
    }
        class Class43
        {
            public Class43(ExecutionReport report)
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
}