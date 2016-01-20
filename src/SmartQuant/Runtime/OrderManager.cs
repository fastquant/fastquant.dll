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

        public void Cancel(Order order)
        {
            if (order.IsNotSent)
                throw new ArgumentException($"Can not cancel order that is not sent {order}");

            var command = new ExecutionCommand(ExecutionCommandType.Cancel, order);
            command.DateTime = this.framework.Clock.DateTime;
            command.OrderId = order.Id;
            command.ClOrderId = order.ClOrderId;
            command.ProviderOrderId = order.ProviderOrderId;
            command.ProviderId = order.ProviderId;
            command.RouteId = order.RouteId;
            command.PortfolioId = order.PortfolioId;
            command.DateTime = order.DateTime;
            command.Instrument = order.Instrument;
            command.InstrumentId = order.InstrumentId;
            command.Provider = order.Provider;
            command.Portfolio = order.Portfolio;
            command.Side = order.Side;
            command.OrdType = order.Type;
            command.TimeInForce = order.TimeInForce;
            command.Price = order.Price;
            command.StopPx = order.StopPx;
            command.Qty = order.Qty;
            command.OCA = order.OCA;
            command.Text = order.Text;
            command.Account = order.Account;
            command.ClientID = order.ClientID;
            command.ClientId = order.ClientId;
            Messages.Add(command);
            order.OnExecutionCommand(command);
            this.framework.EventServer.OnExecutionCommand(command);
            if (IsPersistent)
                Server?.Save(command, -1);
            order.Provider.Send(command);
        }

        public void Register(Order order)
        {
            if (order.Id != -1)
            {
                Console.WriteLine($"OrderManager::Register Error Order is already registered : id = {order.Id}");
                return;
            }

            lock (this.obj)
                order.Id = this.counter++;

            if (this.framework.Mode == FrameworkMode.Realtime && string.IsNullOrEmpty(order.ClOrderId))
                order.ClOrderId = $"{this.framework.Clock.DateTime} { order.Id}";
        }

        public void Delete(string name)
        {
            if (Server != null)
                Server.Delete(name);
            else
                Console.WriteLine($"OrderManager::Delete Can not delete order series {name} Server is null.");
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

        public void Send(Order order)
        {
            throw new NotImplementedException();
        }

        public void Reject(Order order)
        {
            order.Status = OrderStatus.Rejected;
        }

        public void Replace(Order order, double price)
        {
            this.Replace(order, price, order.StopPx, order.Qty);
        }

        public void Replace(Order order, double price, double stopPx, double qty)
        {
            throw new NotImplementedException();
        }

        internal void method_0(ExecutionReport report)
        {
            throw new NotImplementedException();
        }

        internal void OnAccountReport(AccountReport report)
        {
            throw new NotImplementedException();
        }

        internal void method_5(ExecutionReport report)
        {
            throw new NotImplementedException();
        }


    }
}