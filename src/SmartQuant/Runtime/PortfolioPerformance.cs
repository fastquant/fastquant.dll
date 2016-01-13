using System;

namespace SmartQuant
{
    public class PortfolioPerformance
    {
        private Portfolio portfolio;

        public TimeSeries DrawdownSeries { get; }
        public TimeSeries EquitySeries { get; }
        public bool UpdateParent { get; set; }

        public event EventHandler Updated;

        public PortfolioPerformance(Portfolio portfolio)
        {
            this.portfolio = portfolio;
        }

        public void Update()
        {
            throw new NotImplementedException();
        }
    }
}