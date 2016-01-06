namespace SmartQuant.Statistics
{
    public class NetProfit : PortfolioStatisticsItem
    {
        protected PortfolioStatisticsItem grossLoss;
        protected PortfolioStatisticsItem grossProfit;

        public override string Category => "Summary";

        public override string Name => "Net Profit";

        public override int Type => PortfolioStatisticsType.NetProfit;

        protected override void OnInit()
        {
            Subscribe(PortfolioStatisticsType.GrossProfit);
            Subscribe(PortfolioStatisticsType.GrossLoss);
        }

        protected override void OnStatistics(PortfolioStatisticsItem statistics)
        {
            bool changed = false;
            if (statistics.Type == PortfolioStatisticsType.GrossProfit)
            {
                this.grossProfit = statistics;
                changed = true;
            }
            if (statistics.Type == PortfolioStatisticsType.GrossLoss)
            {
                this.grossLoss = statistics;
                changed = true;
            }
            if (changed)
            {
                this.longValue = this.shortValue = this.totalValue = 0;
                if (this.grossProfit != null)
                {
                    this.longValue += this.grossProfit.LongValue;
                    this.shortValue += this.grossProfit.ShortValue;
                    this.totalValue += this.grossProfit.TotalValue;
                }
                if (this.grossLoss != null)
                {
                    this.longValue += this.grossLoss.LongValue;
                    this.shortValue += this.grossLoss.ShortValue;
                    this.totalValue += this.grossLoss.TotalValue;
                }
                LongValues.Add(Clock.DateTime, this.longValue);
                ShortValues.Add(Clock.DateTime, this.shortValue);
                TotalValues.Add(Clock.DateTime, this.totalValue);
                Emit();
            }
        }
    }
}
