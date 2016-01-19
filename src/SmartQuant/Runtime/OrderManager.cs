using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartQuant
{
    public class OrderManager
    {
        private int counter;
        private object obj = new object(); // lock
        private Framework framework;

        public OrderServer Server { get; set; }

        public List<Order> Orders { get; } = new List<Order>();

        internal List<ExecutionMessage> Messages { get; } = new List<ExecutionMessage>();

        private IdArray<Order> ordersById = new IdArray<Order>(102400);
        private Dictionary<string, Order> ordersByClId = new Dictionary<string, Order>();
        private Dictionary<string, Order> ordersByProviderId = new Dictionary<string, Order>();
        private Dictionary<string, List<Order>> dictionary_2 = new Dictionary<string, List<Order>>();

        public bool IsPersistent { get; set; }

        public OrderManager(Framework framework, OrderServer orderServer)
        {
            this.framework = framework;
            Server = orderServer;
            Server?.Open();
            this.counter = 0;
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
            Orders.Clear();
            Messages.Clear();
            ordersById.Clear();
            ordersByClId.Clear();
            this.counter = 0;
            this.framework.EventServer.OnOrderManagerCleared();

        }

        public void Dump()
        {
            foreach (Order order in Orders)
                Console.WriteLine(order);
        }


        public void Load(string name = null, int clientId = -1)
        {
            var messages = LoadMessages(name, clientId);
            foreach (var msg in messages)
            {
                msg.IsLoaded = true;
                byte typeId = msg.TypeId;
                switch (typeId)
                {
                    case DataObjectType.ExecutionReport:
                        this.framework.EventServer.OnExecutionReport((ExecutionReport)msg);
                        break;
                    case DataObjectType.ExecutionCommand:
                        {
                            var command = (ExecutionCommand)msg;
                            switch (command.Type)
                            {
                                case ExecutionCommandType.Send:
                                    {
                                        var order = new Order(command);
                                        command.Order = order;
                                        command.Instrument = this.framework.InstrumentManager.GetById(command.InstrumentId);
                                        command.Portfolio = this.framework.PortfolioManager.GetById(command.PortfolioId);
                                        command.Provider = this.framework.ProviderManager.GetExecutionProvider(command.ProviderId);
                                        order.Instrument = command.Instrument;
                                        order.Portfolio = command.Portfolio;
                                        order.Provider = command.Provider;
                                        lock (this.obj)
                                            this.counter = Math.Max(this.counter, order.Id);
                                        this.framework.EventServer.OnSendOrder(order);
                                        Orders.Add(order);
                                        this.ordersById[order.Id] = order;
                                        if (this.framework.Mode == FrameworkMode.Realtime)
                                            this.ordersByClId[order.ClOrderId] = order;
                                        Messages.Add(command);
                                        order.OnExecutionCommand(command);
                                        this.framework.EventServer.OnExecutionCommand(command);
                                        this.framework.EventServer.OnPendingNewOrder(order, true);
                                        break;
                                    }
                                case ExecutionCommandType.Cancel:
                                case ExecutionCommandType.Replace:
                                    this.ordersById[command.OrderId].OnExecutionCommand(command);
                                    break;
                            }
                            break;
                        }
                    default:
                        if (typeId == DataObjectType.AccountReport)
                            this.framework.EventServer.OnAccountReport((AccountReport)msg);
                        break;
                }
            }
        }

        public List<ExecutionMessage> LoadMessages(string name = null, int clientId = -1)
        {
            if (Server != null)
            {
                Server.Open();
                var messages = Server.Load(name);
                var result = clientId == -1 ? messages.Take(messages.Count) : messages.TakeWhile(m => m.ClientId == clientId);
                return result.ToList();
            }
            else
                return new List<ExecutionMessage>();
        }

        public Order Get(int id) => this.ordersById[id];

        public Order GetByClId(string id)
        {
            Order result;
            this.ordersByClId.TryGetValue(id, out result);
            return result;
        }

        public Order GetByProviderId(string id)
        {
            Order result;
            this.ordersByProviderId.TryGetValue(id, out result);
            return result;
        }


        public void Replace(Order order, double price, double stopPx, double qty)
        {
            throw new NotImplementedException();
        }

        internal void method_0(ExecutionReport report)
        {
            throw new NotImplementedException();
        }

        internal void method_1(AccountReport report)
        {
            throw new NotImplementedException();
        }

        internal void method_5(ExecutionReport report)
        {
            throw new NotImplementedException();
        }
    }
}