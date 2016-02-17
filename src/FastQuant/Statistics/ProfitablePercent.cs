namespace FastQuant.Statistics
{
    public class ProfitablePercent : PortfolioStatisticsItem
    {
        protected internal override void OnInit()
        {
            Subscribe(PortfolioStatisticsType.NumOfWinTrades);
            Subscribe(PortfolioStatisticsType.NumOfTrades);
        }

        protected internal override void OnStatistics(PortfolioStatisticsItem statistics)
        {
            bool changed = false;
            if (statistics.Type == PortfolioStatisticsType.NumOfWinTrades)
            {
                this.numOfWinTrades = statistics;
                changed = true;
            }
            if (statistics.Type == PortfolioStatisticsType.NumOfTrades)
            {
                this.numOfTrades = statistics;
                changed = true;
            }
            if (changed && this.numOfWinTrades != null && this.numOfTrades != null)
            {
                bool updated = false;
                if (this.numOfTrades.LongValue != 0)
                {
                    this.longValue = this.numOfWinTrades.LongValue / this.numOfTrades.LongValue;
                    LongValues.Add(Clock.DateTime, this.longValue);
                    updated = true;
                }
                if (this.numOfTrades.ShortValues.Count != 0)
                {
                    this.shortValue = this.numOfWinTrades.ShortValue / this.numOfTrades.ShortValue;
                    ShortValues.Add(Clock.DateTime, this.shortValue);
                    updated = true;
                }
                if (this.numOfTrades.TotalValues.Count != 0)
                {
                    this.totalValue = this.numOfWinTrades.TotalValue / this.numOfTrades.TotalValue;
                    TotalValues.Add(Clock.DateTime, this.totalValue);
                    updated = true;
                }
                if (updated)
                {
                    Emit();
                }
            }
        }

        public override string Category => "Trades";

        public override string Format => "P2";

        public override string Name => "Profitable %";

        public override int Type => PortfolioStatisticsType.ProfitablePercent;

        protected PortfolioStatisticsItem numOfTrades;

        protected PortfolioStatisticsItem numOfWinTrades;
    }
}
