using System;

namespace SmartQuant
{
    public class PortfolioManager
    {
        private Framework framework;

        public PortfolioServer Server { get; set; }

        public Portfolio this[string name] => Portfolios[name];

        public PortfolioManager(Framework framework, PortfolioServer portfolioServer)
        {
            this.framework = framework;
            Server = portfolioServer;
        }

        public Pricer Pricer { get; internal set; }

        public PortfolioList Portfolios { get; }



        public void Clear()
        {
            throw new NotImplementedException();
        }

        public void Save(Portfolio portfolio)
        {
            if (Server != null)
                Server.Save(portfolio);
            else
                Console.WriteLine($"PortfolioManager::Save Can not save portfolio {portfolio.Name} Server is null.");
        }

        public Portfolio Load(string name)
        {
            if (this.Server != null)
            {
                return Server.Load(name);
            }
            else
            {
                Console.WriteLine($"PortfolioManager::Load Can not load portfolio {name } Server is null.");
                return null;
            }
        }

        public void Add(Portfolio portfolio, bool emitEvent = true)
        {
            throw new NotImplementedException();
        }

        public void Remove(string name)
        {
            throw new NotImplementedException();
        }
    }
}