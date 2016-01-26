namespace SmartQuant.Statistics
{
    public class TradesDuration : PortfolioStatisticsItem
    {
        protected internal override void OnRoundTrip(TradeInfo trade)
        {
            double num = (double)(trade.ExitDate.Ticks - trade.EntryDate.Ticks);
            if (trade.IsLong)
            {
                this.longValue = num;
                this.longValues.Add(base.Clock.DateTime, this.longValue);
            }
            else
            {
                this.shortValue = num;
                this.shortValues.Add(base.Clock.DateTime, this.shortValue);
            }
            this.totalValue = num;
            this.totalValues.Add(base.Clock.DateTime, this.totalValue);
            base.Emit();
        }

        public override string Category => "Trades";

        public override string Name => "Trades Duration";

        public override bool Show => false;

        public override int Type => PortfolioStatisticsType.TradesDuration;
    }
}