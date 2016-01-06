namespace SmartQuant.Statistics
{
    public class NumOfTrades : PortfolioStatisticsItem
    {
        protected  override void OnRoundTrip(TradeInfo trade)
        {
            if (trade.IsLong)
            {
                LongValues.Add(Clock.DateTime, this.longValue += 1);
            }
            else
            {
                ShortValues.Add(Clock.DateTime, this.shortValue += 1);
            }
            TotalValues.Add(Clock.DateTime, this.totalValue += 1);
            Emit();
        }

        public override string Category => "Trades";

        public override string Format => "F0";

        public override string Name => "Number of Trades";

        public override int Type => PortfolioStatisticsType.NumOfTrades;
    }

    public class NumOfLossTrades : PortfolioStatisticsItem
    {
        protected override void OnRoundTrip(TradeInfo trade)
        {
            if (!trade.IsWinning)
            {
                if (trade.IsLong)
                    LongValues.Add(Clock.DateTime, this.longValue += 1);
                else
                    ShortValues.Add(Clock.DateTime, this.shortValue += 1);
                TotalValues.Add(Clock.DateTime, this.totalValue += 1);
                Emit();
            }
        }

        public override string Category => "Trades";

        public override string Format => "F0";

        public override string Name => "Number of Losing Trades";

        public override int Type => PortfolioStatisticsType.NumOfLossTrades;
    }

    public class NumOfWinTrades : PortfolioStatisticsItem
    {
        protected override void OnRoundTrip(TradeInfo trade)
        {
            if (trade.IsWinning)
            {
                if (trade.IsLong)
                    LongValues.Add(Clock.DateTime, this.longValue += 1.0);
                else
                    ShortValues.Add(Clock.DateTime, this.shortValue += 1.0);
                TotalValues.Add(Clock.DateTime, this.totalValue += 1.0);
                Emit();
            }
        }

        public override string Category => "Trades";

        public override string Format => "F0";

        public override string Name => "Number of Winning Trades";

        public override int Type => PortfolioStatisticsType.NumOfWinTrades;
    }
}
