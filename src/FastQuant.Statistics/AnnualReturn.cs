using System;

namespace SmartQuant.Statistics
{
    public class AnnualReturn : PortfolioStatisticsItem
    {
        protected DateTime dateTime;
        protected double initial;

        public override int Type => PortfolioStatisticsType.AnnualReturn;

        public override bool Show => false;
        public override string Name => "Annual Return";
        public override string Category => "Daily / Annual returns";

        protected override void OnEquity(double equity)
        {
            throw new NotImplementedException();
        }
    }

    public class AnnualReturnPercent : PortfolioStatisticsItem
    {
        protected DateTime dateTime;

        protected double initial;

        protected override void OnEquity(double equity)
        {
            if (this.dateTime == DateTime.MinValue)
            {
                this.dateTime = Clock.DateTime;
                this.initial = equity;
            }
            if (Clock.DateTime.Year > this.dateTime.Year)
            {
                if (this.initial != 0)
                {
                    this.totalValue = (equity - this.initial) / this.initial;
                    TotalValues.Add(Clock.DateTime, this.totalValue);
                    Emit();
                }
                this.dateTime = Clock.DateTime;
                this.initial = equity;
            }
        }

        public override string Category => "Daily / Annual returns";

        public override string Format => "P2";

        public override string Name => "Annual Return %";

        public override bool Show => false;

        public override int Type => PortfolioStatisticsType.AnnualReturnPercent;
    }

    public class AnnualReturnPercentDownsideRisk : PortfolioStatisticsItem
    {
        protected override void OnInit()
        {
            AnnualizedFactor = 252;
            Subscribe(PortfolioStatisticsType.DailyDownsideRisk);
        }

        protected  override void OnStatistics(PortfolioStatisticsItem statistics)
        {
            if (statistics.Type == PortfolioStatisticsType.DailyDownsideRisk)
            {
                this.totalValue = AnnualizedFactor * statistics.TotalValue;
                TotalValues.Add(Clock.DateTime, this.totalValue);
                Emit();
            }
        }

        public double AnnualizedFactor { get; set; }

        public override string Category => "Daily / Annual returns";

        public override string Name => "Annual Return % Downside Risk";

        public override int Type => PortfolioStatisticsType.AnnualDownsideRisk;
    }

    public class AnnualReturnPercentStdDev : PortfolioStatisticsItem
    {
        protected override void OnInit()
        {
            AnnualizedFactor = 252;
            Subscribe(PortfolioStatisticsType.DailyReturnPercentStdDev);
        }

        protected override void OnStatistics(PortfolioStatisticsItem statistics)
        {
            if (statistics.Type == PortfolioStatisticsType.DailyReturnPercentStdDev)
            {
                this.totalValue = this.AnnualizedFactor * statistics.TotalValue;
                TotalValues.Add(Clock.DateTime, this.totalValue);
                Emit();
            }
        }

        public double AnnualizedFactor { get; set; }

        public override string Category => "Daily / Annual returns";

        public override string Format => "P2";

        public override string Name => "Annual Return % Standard Deviation";

        public override int Type => PortfolioStatisticsType.AnnualReturnPercentStdDev;
    }

    public class AvgAnnualReturnPercent : PortfolioStatisticsItem
    {
        protected override void OnInit()
        {
            AnnualizedFactor = 252;
            Subscribe(PortfolioStatisticsType.AvgDailyReturnPercent);
        }

        // Token: 0x0600136B RID: 4971 RVA: 0x000486C8 File Offset: 0x000468C8
        protected  override void OnStatistics(PortfolioStatisticsItem statistics)
        {
            if (statistics.Type == 66)
            {
                this.totalValue = statistics.TotalValue * AnnualizedFactor;
                TotalValues.Add(Clock.DateTime, this.totalValue);
                Emit();
            }
        }

        // Token: 0x1700045A RID: 1114
        public double AnnualizedFactor { get; set; }

        public override string Category => "Daily / Annual returns";

        public override string Format => "P2";

        public override string Name => "Average Annual Return %";

        public override int Type => PortfolioStatisticsType.AvgAnnualReturnPercent;
    }
}