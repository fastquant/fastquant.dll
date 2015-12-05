using System;

namespace SmartQuant
{
    internal interface IServerFactory
    {
        InstrumentServer CreateInstrumentServer(params object[] args);
        DataServer CreateDataServer(params object[] args);
        OrderServer CreateOrderServer(params object[] args);
        PortfolioServer CreatePortfolioServer(params object[] args);
        UserServer CreateUserServer(params object[] args);
    }

    class DefaultServerFactory : IServerFactory
    {
        public DataServer CreateDataServer(params object[] args)
        {
            throw new NotImplementedException();
        }

        public InstrumentServer CreateInstrumentServer(params object[] args)
        {
            throw new NotImplementedException();
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