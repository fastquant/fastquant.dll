using System;

namespace SmartQuant
{
    public class OrderManager
    {
        private Framework framework;

        public OrderServer Server { get; set; }

        public OrderManager(Framework framework, OrderServer orderServer)
        {
            this.framework = framework;
            Server = orderServer;
        }


        public void Cancel(Order stopLossOrder)
        {
            throw new NotImplementedException();
        }
    }
}