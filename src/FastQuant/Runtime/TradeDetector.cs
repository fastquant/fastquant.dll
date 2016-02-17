using System;
using System.Collections.Generic;
using System.Threading;

namespace FastQuant
{
    public enum TradeDetectionType
    {
        FIFO,
        LIFO
    }

    class TradeInfoEventArgs : EventArgs
    {
        public TradeInfo TradeInfo { get; }

        public TradeInfoEventArgs(TradeInfo tradeInfo)
        {
            TradeInfo = tradeInfo;
        }
    }

    internal delegate void TradeInfoEventHandler(object sender, TradeInfoEventArgs args);

    internal interface IFillSet
    {
        void Push(Fill fill);

        Fill Pop();

        Fill Peek();
    }

    class QueueFillSet : IFillSet
    {
        private Queue<Fill> queue = new Queue<Fill>();

        public void Push(Fill fill) => this.queue.Enqueue(fill);

        public Fill Pop() => this.queue.Dequeue();

        public Fill Peek() => this.queue.Count != 0 ? this.queue.Peek() : null;
    }

    class StackFillSet : IFillSet
    {
        private Stack<Fill> stack = new Stack<Fill>();

        public void Push(Fill fill) => this.stack.Push(fill);

        public Fill Pop() => this.stack.Pop();

        public Fill Peek() => this.stack.Count != 0 ? this.stack.Peek() : null;
    }

    public class TradeDetector
    {
        public TradeDetector(TradeDetectionType type, Portfolio portfolio)
        {
            this.portfolio_0 = portfolio;
            if (type == TradeDetectionType.FIFO)
            {
                this.interface0_0 = new QueueFillSet();
            }
            else
            {
                this.interface0_0 = new StackFillSet();
            }
            this.list_0 = new List<TradeInfo>();
            this.timeSeries_0 = new TimeSeries();
        }

        public void Add(Fill fill)
        {
            Fill fill2 = this.interface0_0.Peek();
            if (fill2 == null)
            {
                this.instrument_0 = fill.Instrument;
                this.timeSeries_0.Clear();
                this.timeSeries_0.Add(this.portfolio_0.framework.Clock.DateTime, this.method_4(this.instrument_0));
            }
            if (fill2 != null && (!this.method_3(fill2) || !this.method_3(fill)) && (this.method_3(fill2) || this.method_3(fill)))
            {
                if (this.fill_0 != null)
                {
                    fill = this.method_1(fill);
                }
                double num = fill.Qty;
                while (num > 0.0 && (fill2 = this.interface0_0.Peek()) != null)
                {
                    if (fill2.Qty > num)
                    {
                        this.fill_0 = new Fill(fill);
                        return;
                    }
                    this.BotEqOqmKI(this.method_2(fill2, fill, fill2.Qty));
                    this.interface0_0.Pop();
                    this.double_0 -= Math.Round(fill2.Qty, 5);
                    num -= Math.Round(fill2.Qty, 5);
                    if (this.double_0 > 0.0 && num > 0.0)
                    {
                        fill = this.method_0(fill, num);
                    }
                }
                if (num > 0.0)
                {
                    this.double_0 = num;
                    Fill fill3 = this.method_0(fill, num);
                    this.interface0_0.Push(fill3);
                }
                if (this.fill_0 != null)
                {
                    this.fill_0 = null;
                }
                return;
            }
            this.interface0_0.Push(fill);
            this.double_0 += fill.Qty;
        }

        private void BotEqOqmKI(TradeInfo tradeInfo_0)
        {
            this.list_0.Add(tradeInfo_0);
            TradeDetected?.Invoke(this.portfolio_0, new TradeInfoEventArgs(tradeInfo_0));
            
        }

        private Fill method_0(Fill fill_1, double double_1)
        {
            Fill fill = new Fill(fill_1);
            fill.Commission *= double_1 / fill.Qty;
            fill.Qty = double_1;
            return fill;
        }

        private Fill method_1(Fill fill_1)
        {
            Fill fill = new Fill(fill_1);
            if (this.fill_0 != null)
            {
                if (fill.Instrument.Factor != 0.0)
                {
                    fill.Price = (fill_1.Value + this.fill_0.Value) / (fill_1.Qty + this.fill_0.Qty) / fill.Instrument.Factor;
                }
                else
                {
                    fill.Price = (fill_1.Value + this.fill_0.Value) / (fill_1.Qty + this.fill_0.Qty);
                }
                fill.Qty = fill_1.Qty + this.fill_0.Qty;
                fill.Commission = fill_1.Commission + this.fill_0.Commission;
            }
            this.fill_0 = null;
            return fill;
        }

        private TradeInfo method_2(Fill fill_1, Fill fill_2, double double_1)
        {
            TradeInfo tradeInfo = new TradeInfo();
            tradeInfo.Instrument = fill_1.Instrument;
            tradeInfo.EntryDate = fill_1.DateTime;
            tradeInfo.EntryPrice = fill_1.Price;
            tradeInfo.EntryCost = fill_1.Commission * double_1 / fill_1.Qty;
            tradeInfo.ExitDate = fill_2.DateTime;
            tradeInfo.ExitPrice = fill_2.Price;
            tradeInfo.ExitCost = fill_2.Commission * double_1 / fill_2.Qty;
            tradeInfo.Qty = double_1;
            tradeInfo.IsLong = this.method_3(fill_1);
            tradeInfo.BaseCurrencyId = this.portfolio_0.Account.CurrencyId;
            double num = (tradeInfo.Instrument.Factor == 0.0) ? 1.0 : tradeInfo.Instrument.Factor;
            double max = this.timeSeries_0.GetMax(tradeInfo.EntryDate, tradeInfo.ExitDate);
            double min = this.timeSeries_0.GetMin(tradeInfo.EntryDate, tradeInfo.ExitDate);
            if (tradeInfo.IsLong)
            {
                tradeInfo.MAE = num * tradeInfo.Qty * (min - tradeInfo.EntryPrice) - (tradeInfo.EntryCost + tradeInfo.ExitCost);
                tradeInfo.MFE = num * tradeInfo.Qty * (max - tradeInfo.EntryPrice) - (tradeInfo.EntryCost + tradeInfo.ExitCost);
                tradeInfo.ETD = tradeInfo.MFE - tradeInfo.NetPnL;
            }
            else
            {
                tradeInfo.MAE = num * tradeInfo.Qty * (max - tradeInfo.EntryPrice) * -1.0 - (tradeInfo.EntryCost + tradeInfo.ExitCost);
                tradeInfo.MFE = num * tradeInfo.Qty * (min - tradeInfo.EntryPrice) * -1.0 - (tradeInfo.EntryCost + tradeInfo.ExitCost);
                tradeInfo.ETD = tradeInfo.MFE - tradeInfo.NetPnL;
            }
            return tradeInfo;
        }

        private bool method_3(Fill fill_1)
        {
            return fill_1.Side == OrderSide.Buy;
        }

        private double method_4(Instrument instrument_1)
        {
            Trade trade_ = instrument_1.Trade;
            if (trade_ != null)
            {
                return trade_.Price;
            }
            Bar bar_ = instrument_1.Bar;
            if (bar_ != null)
            {
                return bar_.Close;
            }
            return 0.0;
        }

        public void OnEquity(double equity)
        {
            if (this.HasPosition)
            {
                this.timeSeries_0.Add(this.portfolio_0.framework.Clock.DateTime, this.method_4(this.instrument_0));
            }
        }

        public bool HasPosition
        {
            get
            {
                return this.interface0_0.Peek() != null;
            }
        }

        public DateTime OpenDateTime
        {
            get
            {
                Fill fill = this.interface0_0.Peek();
                if (fill == null)
                {
                    return DateTime.MinValue;
                }
                return fill.DateTime;
            }
        }

        public List<TradeInfo> Trades
        {
            get
            {
                return this.list_0;
            }
        }

        internal event TradeInfoEventHandler TradeDetected;


     //   private Delegate1 delegate1_0;

        private double double_0;

        private Fill fill_0;

        private Instrument instrument_0;

        private IFillSet interface0_0;

        private List<TradeInfo> list_0;

        internal Portfolio portfolio_0;

        private TimeSeries timeSeries_0;
    }

}