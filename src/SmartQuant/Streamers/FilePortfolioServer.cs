using System;

namespace SmartQuant
{
    public class FilePortfolioServer : PortfolioServer
    {
        public FilePortfolioServer(Framework framework, string fileName, string host = null, int port = -1) : base(framework)
        {
        }

        public override void Close()
        {
            throw new NotImplementedException();
        }

        public override void Delete(string name)
        {
            throw new NotImplementedException();
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override Portfolio Load(string name)
        {
            throw new NotImplementedException();
        }

        public override void Open()
        {
            throw new NotImplementedException();
        }

        public override void Save(Portfolio portfolio)
        {
            throw new NotImplementedException();
        }
    }
}