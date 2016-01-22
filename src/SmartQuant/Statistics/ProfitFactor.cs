namespace SmartQuant.Statistics
{
    public class ProfitFactor : PortfolioStatisticsItem
    {
        protected internal override void OnInit()
        {
            this.shortValue = 1;
            this.longValue = 1;
            this.totalValue = 1;
            LongValues.Add(Clock.DateTime, this.longValue);
            ShortValues.Add(Clock.DateTime, this.shortValue);
            TotalValues.Add(Clock.DateTime, this.totalValue);
            base.Subscribe(PortfolioStatisticsType.GrossProfit);
            base.Subscribe(PortfolioStatisticsType.GrossLoss);
        }

        protected internal override void OnStatistics(PortfolioStatisticsItem statistics)
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
            if (changed && this.grossProfit != null && this.grossLoss != null)
            {
                bool updated = false;
                if (this.grossLoss.LongValue != 0)
                {
                    this.longValue = this.grossProfit.LongValue / -this.grossLoss.LongValue;
                    LongValues.Add(base.Clock.DateTime, this.longValue);
                    updated = true;
                }
                if (this.grossLoss.ShortValue != 0)
                {
                    this.shortValue = this.grossProfit.ShortValue / -this.grossLoss.ShortValue;
                    ShortValues.Add(Clock.DateTime, this.shortValue);
                    updated = true;
                }
                if (this.grossLoss.TotalValue != 0.0)
                {
                    this.totalValue = this.grossProfit.TotalValue / -this.grossLoss.TotalValue;
                    TotalValues.Add(Clock.DateTime, this.totalValue);
                    updated = true;
                }
                if (updated)
                {
                    Emit();
                }
            }
        }

        public override string Category => "Summary";

        public override string Name => "Profit Factor";

        public override int Type => PortfolioStatisticsType.ProfitFactor;

        protected PortfolioStatisticsItem grossLoss;

        protected PortfolioStatisticsItem grossProfit;
    }
}
