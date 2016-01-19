using System;
using System.IO;

namespace SmartQuant
{
    public class FilePortfolioServer : PortfolioServer
    {
        private bool opened;
        private DataFile dataFile;

        private string host;
        private int port;

        public FilePortfolioServer(Framework framework, string fileName, string host = null, int port = -1) : base(framework)
        {
            this.host = host;
            this.port = port;
            this.dataFile= host == null? new DataFile(fileName, framework.StreamerManager): new NetDataFile(fileName, host, port, framework.StreamerManager);
        }

        public override void Open()
        {
            if (!this.opened)
            {
                this.dataFile.Open(FileMode.OpenOrCreate);
                this.opened = true;
            }
        }

        public override void Close()
        {
            if (this.opened)
            {
                this.dataFile.Close();
                this.opened = false;
            }
        }

        public override void Delete(string name) => this.dataFile.Delete(name);

        public override void Flush()
        {
            if (this.opened)
            {
                this.dataFile.Flush();
            }
        }

        public override Portfolio Load(string name)
        {
            var portfolio = (Portfolio)this.dataFile.Get(name);
            if (portfolio != null)
                this.Init(portfolio);
            return portfolio;
        }

        public override void Save(Portfolio portfolio)
        {
            foreach (var p in portfolio.Children)
                Console.WriteLine(p.Name);
            this.dataFile.Write(portfolio.Name, portfolio);
        }

        private void Init(Portfolio portfolio)
        {
            portfolio.IsLoaded = true;
            portfolio.Init(this.framework);
            this.framework.PortfolioManager.Add(portfolio, true);
            foreach (Portfolio p in portfolio.Children)
            {
                Init(p);
                p.Parent = portfolio;
            }
        }
    }
}