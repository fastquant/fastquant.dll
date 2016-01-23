using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartQuant
{
    public static class FrameworkExtensions
    {
        public static Portfolio GetOrCreatePortfolio(this Framework framework, string name, bool emitEvent = true)
        {
            Portfolio portfolio;
            if (framework.PortfolioManager.Portfolios.Contains(name))
                portfolio  = framework.PortfolioManager.Portfolios.GetByName(name);
            else
            {
                portfolio = new Portfolio(framework, name);
                framework.PortfolioManager.Add(portfolio, emitEvent);
            }
            return portfolio;
        }
    }
}
