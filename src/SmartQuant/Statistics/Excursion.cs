namespace SmartQuant.Statistics
{
    public class MaxAdverseExcursion : PortfolioStatisticsItem
    {
        protected internal override void OnRoundTrip(TradeInfo trade)
        {
            if (trade.IsLong)
            {
                this.longValue = trade.MAE;
                this.longValues.Add(base.Clock.DateTime, this.longValue);
            }
            else
            {
                this.shortValue = trade.MAE;
                this.shortValues.Add(base.Clock.DateTime, this.shortValue);
            }
            this.totalValue = trade.MAE;
            this.totalValues.Add(base.Clock.DateTime, this.totalValue);
            base.Emit();
        }

        public override string Category => "Trades";

        public override string Name => "Maximum Adverse Excursion";

        public override bool Show => false;

        public override int Type => PortfolioStatisticsType.MaxAdverseExcursion;
    }

    public class MaxFavorableExcursion : PortfolioStatisticsItem
    {
        protected internal override void OnRoundTrip(TradeInfo trade)
        {
            if (trade.IsLong)
            {
                this.longValue = trade.MFE;
                this.longValues.Add(base.Clock.DateTime, this.longValue);
            }
            else
            {
                this.shortValue = trade.MFE;
                this.shortValues.Add(base.Clock.DateTime, this.shortValue);
            }
            this.totalValue = trade.MFE;
            this.totalValues.Add(base.Clock.DateTime, this.totalValue);
            base.Emit();
        }

        public override string Category => "Trades";

        public override string Name => "Maximum Favorable Excursion";

        public override bool Show => false;

        public override int Type => PortfolioStatisticsType.MaxFavorableExcursion;
    }
}