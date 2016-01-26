using System;
using static System.Math;

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

        protected internal override void OnEquity(double equity)
        {
            if (this.dateTime == DateTime.MinValue)
            {
                this.dateTime = base.Clock.DateTime;
                this.initial = equity;
            }
            if (base.Clock.DateTime.Year > this.dateTime.Year)
            {
                if (this.initial != 0.0)
                {
                    this.totalValue = equity - this.initial;
                    this.totalValues.Add(base.Clock.DateTime, this.totalValue);
                    base.Emit();
                }
                this.dateTime = base.Clock.DateTime;
                this.initial = equity;
            }
        }
    }

    public class AnnualReturnPercent : PortfolioStatisticsItem
    {
        protected DateTime dateTime;

        protected double initial;

        protected internal override void OnEquity(double equity)
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
        protected internal override void OnInit()
        {
            AnnualizedFactor = 252;
            Subscribe(PortfolioStatisticsType.DailyDownsideRisk);
        }

        protected internal override void OnStatistics(PortfolioStatisticsItem statistics)
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
        protected internal override void OnInit()
        {
            AnnualizedFactor = 252;
            Subscribe(PortfolioStatisticsType.DailyReturnPercentStdDev);
        }

        protected internal override void OnStatistics(PortfolioStatisticsItem statistics)
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
        protected internal override void OnInit()
        {
            AnnualizedFactor = 252;
            Subscribe(PortfolioStatisticsType.AvgDailyReturnPercent);
        }

        protected internal override void OnStatistics(PortfolioStatisticsItem statistics)
        {
            if (statistics.Type == 66)
            {
                this.totalValue = statistics.TotalValue * AnnualizedFactor;
                TotalValues.Add(Clock.DateTime, this.totalValue);
                Emit();
            }
        }

        public double AnnualizedFactor { get; set; }

        public override string Category => "Daily / Annual returns";

        public override string Format => "P2";

        public override string Name => "Average Annual Return %";

        public override int Type => PortfolioStatisticsType.AvgAnnualReturnPercent;
    }

    public class DailyReturnPercent : PortfolioStatisticsItem
    {
        protected internal override void OnEquity(double equity)
        {
            if (this.dateTime == DateTime.MinValue)
            {
                this.dateTime = Clock.DateTime;
                this.initial = equity;
            }
            if (Clock.DateTime.Date > this.dateTime.Date)
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

        public override string Name => "Daily Return %";

        public override bool Show => true;

        public override int Type => PortfolioStatisticsType.DailyReturnPercent;

        protected DateTime dateTime;

        protected double initial;
    }

    public class DailyReturnPercentDownsideRisk : PortfolioStatisticsItem
    {
        protected internal override void OnInit()
        {
            Threshold = 0;
            Subscribe(PortfolioStatisticsType.DailyReturnPercent);
        }

        protected internal override void OnStatistics(PortfolioStatisticsItem statistics)
        {
            if (statistics.Type == PortfolioStatisticsType.DailyReturnPercent)
            {
                this.sumsq += Pow(Max(0, statistics.TotalValue - Threshold), 2);
                if (statistics.TotalValue < 0)
                {
                    this.count++;
                }
                if (this.count > 0)
                {
                    this.totalValue = Sqrt(this.sumsq / this.count);
                    TotalValues.Add(Clock.DateTime, this.totalValue);
                    Emit();
                }
            }
        }

        public override string Category => "Daily / Annual returns";

        public override string Name => "Daily Return % Downside Risk";

        public double Threshold { get; set; }

        public override int Type => PortfolioStatisticsType.DailyDownsideRisk;

        protected int count;

        protected double sumsq;
    }

}