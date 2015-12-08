using System;

namespace SmartQuant
{

    public class DefaultServerFactory : IServerFactory
    {
        public DataServer CreateDataServer(params object[] args)
        {
            throw new NotImplementedException();
        }

        public InstrumentServer CreateInstrumentServer(params object[] args)
        {
            return new FileInstrumentServer((Framework)args[0], (string)args[1]);
        }

        public OrderServer CreateOrderServer(params object[] args)
        {
            throw new NotImplementedException();
        }

        public PortfolioServer CreatePortfolioServer(params object[] args)
        {
            throw new NotImplementedException();
        }

        public UserServer CreateUserServer(params object[] args)
        {
            throw new NotImplementedException();
        }
    }
}