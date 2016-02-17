namespace FastQuant.Statistics
{
    public class SharpeRatio : PortfolioStatisticsItem
    {
        protected PortfolioStatisticsItem avgReturnPercent;
        protected PortfolioStatisticsItem stdDev;

        public override string Category => "Daily / Annual returns";

        public override string Name => "Sharpe Ratio";

        public double RiskFreeReturn { get; set; }

        public override int Type => PortfolioStatisticsType.SharpeRatio;

        protected internal override void OnInit()
        {
            RiskFreeReturn = 0;
            Subscribe(PortfolioStatisticsType.AvgAnnualReturnPercent);
            Subscribe(PortfolioStatisticsType.AnnualReturnPercentStdDev);
        }

        protected internal override void OnStatistics(PortfolioStatisticsItem statistics)
        {
            bool changed = false;
            if (statistics.Type == PortfolioStatisticsType.AvgAnnualReturnPercent)
            {
                this.avgReturnPercent = statistics;
                changed = true;
            }
            if (statistics.Type == PortfolioStatisticsType.AnnualReturnPercentStdDev)
            {
                this.stdDev = statistics;
                changed = true;
            }
            if (changed && this.avgReturnPercent != null && this.stdDev != null && this.stdDev.TotalValue != 0)
            {
                this.totalValue = (this.avgReturnPercent.TotalValue - RiskFreeReturn) / this.stdDev.TotalValue;
                this.totalValues.Add(Clock.DateTime, this.totalValue);
                Emit();
            }
        }
    }
}
