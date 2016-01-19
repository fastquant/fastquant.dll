using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartQuant
{
    public class StatisticsManager
    {
        private Framework framework;

        public PortfolioStatisticsItemList Statistics { get; } = new PortfolioStatisticsItemList();

        public StatisticsManager(Framework framework)
        {
            this.framework = framework;
        }

        public void Add(PortfolioStatisticsItem item) => Statistics.Add(item);

        public bool Contains(int type) => Statistics.Contains(type);

        public PortfolioStatisticsItem Get(int type) => Statistics.GetByType(type);

        public void Remove(int type) => Statistics.Remove(type);

        public PortfolioStatisticsItem Clone(int type) => (PortfolioStatisticsItem)Activator.CreateInstance(Get(type).GetType());

        public List<PortfolioStatisticsItem> CloneAll() => Statistics.Select(item => (PortfolioStatisticsItem)Activator.CreateInstance(item.GetType())).ToList();
    }
}