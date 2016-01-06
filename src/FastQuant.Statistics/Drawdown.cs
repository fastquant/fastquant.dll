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

        protected override void OnEquity(double equity)
        {
            if (this.longAccValue == 0 && this.shortAccValue == 0)
                this.longAccValue = this.shortAccValue = this.portfolio.AccountValue;

            double longValue = this.longAccValue;
            double shortValue = this.shortAccValue;
            foreach (Position position in this.portfolio.Positions)
            {
                double value = position.Value;
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

        protected override void OnPositionChanged(Position position)
        {
            var fill = position.Fills[position.Fills.Count - 1];
            if (position.Side == PositionSide.Long)
                this.longAccValue += fill.CashFlow;
            else if (position.Side == PositionSide.Short)
                this.shortAccValue -= fill.CashFlow;
        }

        protected override void OnPositionClosed(Position position)
        {
            var fill = position.Fills[position.Fills.Count - 1];
            if (fill.Side == OrderSide.Sell)
                this.longAccValue += fill.CashFlow;
            else if (fill.Side == OrderSide.Buy)
                this.shortAccValue -= fill.CashFlow;
        }

        protected override void OnPositionSideChanged(Position position)
        {
            var fill = position.Fills[position.Fills.Count - 1];
            double v = fill.CashFlow * (fill.Qty - position.Qty) / fill.Qty;
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

        protected override void OnRoundTrip(TradeInfo trade)
        {
            if (trade.IsLong)
                this.longAccValue += trade.NetPnL;
            else
                this.shortAccValue += trade.NetPnL;
        }
    }
}
