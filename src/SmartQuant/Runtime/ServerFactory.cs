using System;

namespace SmartQuant
{
    public interface IServerFactory
    {
        InstrumentServer CreateInstrumentServer(params object[] args);
        DataServer CreateDataServer(params object[] args);
        OrderServer CreateOrderServer(params object[] args);
        PortfolioServer CreatePortfolioServer(params object[] args);
        UserServer CreateUserServer(params object[] args);
    }
}