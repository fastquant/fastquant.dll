// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;

namespace FastQuant
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
            foreach (var msg in this.framework.OrderManager.Messages)
            {
                if (msg.TypeId == DataObjectType.ExecutionReport)
                {
                    var report = (ExecutionReport)msg;
                    var portfolio = GetById(report.Order.PortfolioId);
                    if (portfolio != null)
                    {
                        Console.WriteLine($"{portfolio.Name} {report.Order.PortfolioId} {portfolio.Id}");
                        if (report.Order.PortfolioId == portfolio.Id)
                            portfolio.OnExecutionReport(report, false);
                    }
                }
            }
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
                portfolio.Id = this.counter++;
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

        public void Delete(string name)
        {
            if (Server != null)
                Server.Delete(name);
            else
                Console.WriteLine($"PortfolioManager::Delete Can not delete portfolio {name} Server is null.");
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
            var portfolio = GetById(report.PortfolioId);
            if (portfolio != null)
                portfolio.Add(report);
            else
                Console.WriteLine($"PortfolioManager::{nameof(OnAccountReport)} Error. Portfolio does not exist. Id = {report.PortfolioId}");
        }

        internal void OnExecutionReport(ExecutionReport report)
        {
            report.Order.Portfolio.OnExecutionReport(report, true);
        }

        [InferredNaming]
        internal void Load(BinaryReader reader)
        {
            var count = reader.ReadInt32();
            for (var i = 0; i < count; i++)
            {
                var portfolio = (Portfolio)this.framework.StreamerManager.Deserialize(reader);
                // TODO: more strict visibility
                portfolio.framework = this.framework;
                Add(portfolio, true);
            }
        }

        [InferredNaming]
        internal void Save(BinaryWriter writer)
        {
            writer.Write(Portfolios.Count);
            foreach (var p in Portfolios)
                this.framework.StreamerManager.Serialize(writer, p);
        }
    }
}