using System;

namespace SmartQuant
{
    public class PortfolioManager
    {
        private int counter;

        private Framework framework;

        public PortfolioList Portfolios { get; } = new PortfolioList();

        public PortfolioServer Server { get; set; }

        public Portfolio this[string name] => Portfolios[name];

        public Pricer Pricer { get; set; }

        public PortfolioManager(Framework framework, PortfolioServer portfolioServer)
        {
            this.framework = framework;
            Server = portfolioServer;
            Server?.Open();
            Pricer = new Pricer(framework);
        }

        public void Init()
        {
            //foreach (ExecutionMessage current in this.framework.OrderManager.list_0)
            //{
            //    if (current.TypeId == 13)
            //    {
            //        ExecutionReport executionReport = (ExecutionReport)current;
            //        Portfolio byId = this.GetById(executionReport.order_0.int_4);
            //        if (byId != null)
            //        {
            //            Console.WriteLine(string.Concat(new object[]
            //            {
            //        byId.string_0,
            //        " ",
            //        executionReport.order_0.int_4,
            //        " ",
            //        byId.int_0
            //            }));
            //            if (executionReport.order_0.int_4 == byId.int_0)
            //            {
            //                byId.method_1(executionReport, false);
            //            }
            //        }
            //    }
            //}
        }

        public void Clear()
        {
            foreach (var p in Portfolios)
                this.framework.EventServer.OnPortfolioRemoved(p);
            Portfolios.Clear();
            this.counter = 0;
        }

        public void Add(Portfolio portfolio, bool emitEvent = true)
        {
            if (portfolio.Id == -1)
            { 
                portfolio.Id = this.counter++;
            }
            else
            {
                if (Portfolios.Contains(portfolio.Id))
                    Console.WriteLine($"PortfolioManager::Add portfolio {portfolio.Name} error. Portfolio with Id {portfolio.Id} already added.");
                if (portfolio.Id >= this.counter)
                    this.counter = portfolio.Id + 1;
            }

            Portfolios.Add(portfolio);
            if (emitEvent)
                this.framework.EventServer.OnPortfolioAdded(portfolio);
        }

        public void Remove(Portfolio portfolio)
        {
            Portfolios.Remove(portfolio);
            this.framework.EventServer.OnPortfolioRemoved(portfolio);
        }

        public void Remove(string name)
        {
            var portfolio = this[name];
            if (portfolio != null)
                Remove(portfolio);
        }

        public void Remove(int id)
        {
            var portfolio = GetById(id);
            if (portfolio != null)
                Remove(portfolio);
        }

        public Portfolio GetById(int id) => Portfolios.GetById(id);

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
                return Server.Load(name);
            else
            {
                Console.WriteLine($"PortfolioManager::Load Can not load portfolio {name } Server is null.");
                return null;
            }
        }

        internal void OnAccountReport(AccountReport report)
        {
            var portfolio = Portfolios.GetById(report.PortfolioId);
            if (portfolio != null)
                portfolio.Add(report);
            else
                Console.WriteLine($"PortfolioManager::{nameof(OnAccountReport)} Error. Portfolio does not exist. Id = {report.PortfolioId}");
        }

        internal void OnExecutionReport(ExecutionReport report)
        {
            report.Order.Portfolio.method_1(report, true);
        }
    }
}