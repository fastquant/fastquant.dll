using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SmartQuant.Statistics;

namespace SmartQuant
{
    public class StatisticsManager
    {
        private Framework framework;

        public PortfolioStatisticsItemList Statistics { get; } = new PortfolioStatisticsItemList();

        public StatisticsManager(Framework framework)
        {
            this.framework = framework;
            var types = typeof(PortfolioStatisticsType).GetFields(BindingFlags.Public | BindingFlags.Static)
                .Select(f => f.Name)
                .Except(new[] { "DailyDownsideRisk", "AnnualDownsideRisk" })
                .Concat(new[] { "DailyReturnPercentDownsideRisk", "AnnualReturnPercentDownsideRisk" });
            foreach (var t in types)
            {
                var type = Type.GetType($"{nameof(SmartQuant)}.Statistics.{t}");
                var item = (PortfolioStatisticsItem)Activator.CreateInstance(type);
                Add(item);
            }
        }

        public void Add(PortfolioStatisticsItem item) => Statistics.Add(item);

        public bool Contains(int type) => Statistics.Contains(type);

        public PortfolioStatisticsItem Get(int type) => Statistics.GetByType(type);

        public void Remove(int type) => Statistics.Remove(type);

        public PortfolioStatisticsItem Clone(int type) => (PortfolioStatisticsItem)Activator.CreateInstance(Get(type).GetType());

        public List<PortfolioStatisticsItem> CloneAll() => Statistics.Select(item => (PortfolioStatisticsItem)Activator.CreateInstance(item.GetType())).ToList();
    }
}