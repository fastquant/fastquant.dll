namespace SmartQuant.Statistics
{
    public class GrossProfit : PortfolioStatisticsItem
    {
        public override string Category => "Summary";

        public override string Name => "Gross Profit";

        public override int Type => PortfolioStatisticsType.GrossProfit;

        protected override void OnRoundTrip(TradeInfo trade)
        {
            if (trade.IsWinning)
            {
                if (trade.IsLong)
                {
                    this.longValue += trade.NetPnL;
                    LongValues.Add(Clock.DateTime, this.longValue);
                }
                else
                {
                    this.shortValue += trade.NetPnL;
                    ShortValues.Add(Clock.DateTime, this.shortValue);
                }
                this.totalValue = this.longValue + this.shortValue;
                TotalValues.Add(Clock.DateTime, this.totalValue);
                Emit();
            }
        }
    }
}
