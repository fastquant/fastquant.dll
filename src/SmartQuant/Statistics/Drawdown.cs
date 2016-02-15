using System;
using static System.Math;

namespace SmartQuant.Statistics
{
    public class Drawdown : PortfolioStatisticsItem
    {
        protected double longAccValue;

        protected double longEquityMax;

        protected double shortAccValue;

        protected double shortEquityMax;

        protected double totalEquityMax;

        public override string Category => "Summary";

        public override string Name => "Drawdown";

        public override bool Show => false;

        public override int Type => PortfolioStatisticsType.Drawdown;

        protected internal override void OnEquity(double equity)
        {
            if (this.longAccValue == 0 && this.shortAccValue == 0)
                this.longAccValue = this.shortAccValue = this.portfolio.AccountValue;

            var longValue = this.longAccValue;
            var shortValue = this.shortAccValue;
            foreach (var position in this.portfolio.Positions)
            {
                var value = position.Value;
                if (position.Side == PositionSide.Long)
                    longValue += value;
                else
                    shortValue -= value;
            }
            this.longEquityMax = Max(longValue, this.longEquityMax);
            this.shortEquityMax = Max(shortValue, this.shortEquityMax);
            this.totalEquityMax = Max(equity, this.totalEquityMax);
            this.longValue = longValue - this.longEquityMax;
            this.shortValue = shortValue - this.shortEquityMax;
            this.totalValue = equity - this.totalEquityMax;
            LongValues.Add(Clock.DateTime, this.longValue);
            ShortValues.Add(Clock.DateTime, this.shortValue);
            TotalValues.Add(Clock.DateTime, this.totalValue);
            Emit();
        }

        protected internal override void OnPositionChanged(Position position)
        {
            var fill = position.Fills[position.Fills.Count - 1];
            if (position.Side == PositionSide.Long)
                this.longAccValue += fill.CashFlow;
            else if (position.Side == PositionSide.Short)
                this.shortAccValue -= fill.CashFlow;
        }

        protected internal override void OnPositionClosed(Position position)
        {
            var fill = position.Fills[position.Fills.Count - 1];
            if (fill.Side == OrderSide.Sell)
                this.longAccValue += fill.CashFlow;
            else if (fill.Side == OrderSide.Buy)
                this.shortAccValue -= fill.CashFlow;
        }

        protected internal override void OnPositionSideChanged(Position position)
        {
            var fill = position.Fills[position.Fills.Count - 1];
            var v = fill.CashFlow * (fill.Qty - position.Qty) / fill.Qty;
            if (fill.Side == OrderSide.Sell)
            {
                this.longAccValue += v;
                this.shortAccValue += v;
            }
            else if (fill.Side == OrderSide.Buy)
            {
                this.shortAccValue -= v;
                this.longAccValue -= v;
            }
        }

        protected internal override void OnRoundTrip(TradeInfo trade)
        {
            if (trade.IsLong)
                this.longAccValue += trade.NetPnL;
            else
                this.shortAccValue += trade.NetPnL;
        }
    }

    public class DrawdownPercent : PortfolioStatisticsItem
    {
        protected double longAccValue;
        protected double longEquityMax;
        protected double shortAccValue;
        protected double shortEquityMax;
        protected double totalEquityMax;

        protected internal override void OnEquity(double equity)
        {
            if (this.longAccValue == 0.0 && this.shortAccValue == 0.0)
            {
                this.longAccValue = this.portfolio.AccountValue;
                this.shortAccValue = this.portfolio.AccountValue;
            }
            double num = this.longAccValue;
            double num2 = this.shortAccValue;
            foreach (Position current in this.portfolio.Positions)
            {
                double value = current.Value;
                if (current.Side == PositionSide.Long)
                {
                    num += value;
                }
                else
                {
                    num2 -= value;
                }
            }
            this.longEquityMax = Math.Max(num, this.longEquityMax);
            this.shortEquityMax = Math.Max(num2, this.shortEquityMax);
            this.totalEquityMax = Math.Max(equity, this.totalEquityMax);
            bool flag = false;
            if (this.longEquityMax != 0.0)
            {
                this.longValue = num / this.longEquityMax - 1.0;
                this.longValues.Add(base.Clock.DateTime, this.longValue);
                flag = true;
            }
            if (this.shortEquityMax != 0.0)
            {
                this.shortValue = num2 / this.shortEquityMax - 1.0;
                this.shortValues.Add(base.Clock.DateTime, this.shortValue);
                flag = true;
            }
            if (this.totalEquityMax != 0.0)
            {
                this.totalValue = equity / this.totalEquityMax - 1.0;
                this.totalValues.Add(base.Clock.DateTime, this.totalValue);
                flag = true;
            }
            if (flag)
            {
                base.Emit();
            }
        }

        protected internal override void OnPositionChanged(Position position)
        {
            Fill fill = position.Fills[position.Fills.Count - 1];
            if (position.Side == PositionSide.Long)
            {
                this.longAccValue += fill.CashFlow;
                return;
            }
            if (position.Side == PositionSide.Short)
            {
                this.shortAccValue -= fill.CashFlow;
            }
        }

        protected internal override void OnPositionClosed(Position position)
        {
            Fill fill = position.Fills[position.Fills.Count - 1];
            if (fill.Side == OrderSide.Sell)
            {
                this.longAccValue += fill.CashFlow;
                return;
            }
            if (fill.Side == OrderSide.Buy)
            {
                this.shortAccValue -= fill.CashFlow;
            }
        }

        protected internal override void OnPositionSideChanged(Position position)
        {
            Fill fill = position.Fills[position.Fills.Count - 1];
            double num = fill.CashFlow * (fill.Qty - position.Qty) / fill.Qty;
            if (fill.Side == OrderSide.Sell)
            {
                this.longAccValue += num;
                this.shortAccValue += num;
                return;
            }
            if (fill.Side == OrderSide.Buy)
            {
                this.shortAccValue -= num;
                this.longAccValue -= num;
            }
        }

        protected internal override void OnRoundTrip(TradeInfo trade)
        {
            if (trade.IsLong)
            {
                this.longAccValue += trade.NetPnL;
                return;
            }
            this.shortAccValue += trade.NetPnL;
        }

        public override string Category => "Summary";

        public override string Format => "P2";

        public override string Name => "Drawdown %";

        public override bool Show => true;

        public override int Type => PortfolioStatisticsType.DrawdownPercent;
    }

    public class EndOfTradeDrawdown : PortfolioStatisticsItem
    {
        protected internal override void OnRoundTrip(TradeInfo trade)
        {
            if (trade.IsLong)
            {
                this.longValue = trade.ETD;
                this.longValues.Add(base.Clock.DateTime, this.longValue);
            }
            else
            {
                this.shortValue = trade.ETD;
                this.shortValues.Add(base.Clock.DateTime, this.shortValue);
            }
            this.totalValue = trade.ETD;
            this.totalValues.Add(base.Clock.DateTime, this.totalValue);
            base.Emit();
        }

        public override string Category => "Trades";

        public override string Name => "End of Trade Drawdown";

        public override bool Show => false;

        public override int Type => PortfolioStatisticsType.EndOfTradeDrawdown;
    }
}
