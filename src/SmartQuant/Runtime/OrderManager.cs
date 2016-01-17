using System;

namespace SmartQuant
{
    public class OrderManager
    {
        private Framework framework;

        public OrderServer Server { get; set; }

        public bool IsPersistent { get; set; }

        public OrderManager(Framework framework, OrderServer orderServer)
        {
            this.framework = framework;
            Server = orderServer;
        }


        public void Cancel(Order stopLossOrder)
        {
            throw new NotImplementedException();
        }

        public void Register(Order order)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public void Load(string name = null, int clientId = -1)
        {
            throw new NotImplementedException();
        }

        public void Replace(Order order, double price, double stopPx, double qty)
        {
                throw new NotImplementedException();
        }
    }
}