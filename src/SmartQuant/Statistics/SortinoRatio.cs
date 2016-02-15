namespace SmartQuant.Statistics
{
    public class SortinoRatio : PortfolioStatisticsItem
    {
        protected internal override void OnInit()
        {
            RiskFreeReturn = 0;
            Subscribe(PortfolioStatisticsType.AvgAnnualReturnPercent);
            Subscribe(PortfolioStatisticsType.AnnualDownsideRisk);
        }

        protected internal override void OnStatistics(PortfolioStatisticsItem statistics)
        {
            bool changed = false;
            if (statistics.Type == PortfolioStatisticsType.AvgAnnualReturnPercent)
            {
                this.avgReturnPercent = statistics;
                changed = true;
            }
            if (statistics.Type == PortfolioStatisticsType.AnnualDownsideRisk)
            {
                this.downsideRisk = statistics;
                changed = true;
            }
            if (changed && this.avgReturnPercent != null && this.downsideRisk != null && this.downsideRisk.TotalValue != 0)
            {
                this.totalValue = (this.avgReturnPercent.TotalValue - RiskFreeReturn) / this.downsideRisk.TotalValue;
                TotalValues.Add(Clock.DateTime, this.totalValue);
                Emit();
            }
        }

        public override string Category => "Daily / Annual returns";

        public override string Name => "Sortino Ratio";

        public double RiskFreeReturn { get; set; }

        public override int Type => PortfolioStatisticsType.SortinoRatio;

        protected PortfolioStatisticsItem avgReturnPercent;

        protected PortfolioStatisticsItem downsideRisk;
    }
}
