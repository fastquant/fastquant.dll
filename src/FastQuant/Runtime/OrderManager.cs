using System;
using System.Collections.Generic;
using System.Linq;

namespace FastQuant
{
    public delegate void OrderManagerClearedEventHandler(object sender, OnOrderManagerCleared data);

    public class OrderManager
    {
        private int counter;
        private object obj = new object(); // lock
        private Framework framework;

        public OrderServer Server { get; set; }

        public List<Order> Orders { get; } = new List<Order>();

        internal List<ExecutionMessage> Messages { get; } = new List<ExecutionMessage>();

        private IdArray<Order> ordersById = new IdArray<Order>(102400);
        private Dictionary<string, Order> ordersByClOrderId = new Dictionary<string, Order>();
        private Dictionary<string, Order> ordersByProviderId = new Dictionary<string, Order>();
        private Dictionary<string, List<Order>> ordersByOCAList = new Dictionary<string, List<Order>>();

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

            var command = new ExecutionCommand(ExecutionCommandType.Cancel, order)
            {
                DateTime = this.framework.Clock.DateTime,
                OrderId = order.Id,
                ClOrderId = order.ClOrderId,
                ProviderOrderId = order.ProviderOrderId,
                ProviderId = order.ProviderId,
                RouteId = order.RouteId,
                PortfolioId = order.PortfolioId
            };
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
                order.ClOrderId = $"{this.framework.Clock.DateTime} {order.Id}";
        }

        public void Register(ExecutionCommand command)
        {
            if (command.Id != -1)
            {
                Console.WriteLine($"OrderManager::Register Error Order is already registered : id = {command.Id}");
                return;
            }
            lock (this.obj)
                command.Id = this.counter++;
            if (this.framework.Mode == FrameworkMode.Realtime && string.IsNullOrEmpty(command.ClOrderId))
                command.ClOrderId = this.framework.Clock.DateTime + " " + command.OrderId;
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
            this.ordersById.Clear();
            this.ordersByClOrderId.Clear();
            this.ordersByProviderId.Clear();
            this.ordersByOCAList.Clear();
            this.counter = 0;
            this.framework.EventServer.OnOrderManagerCleared();
        }

        public void Dump()
        {
            foreach (var order in Orders)
                Console.WriteLine(order);
        }

        public void Load(string name = null, int clientId = -1)
        {
            var messages = LoadMessages(name, clientId);
            foreach (var msg in messages)
            {
                msg.IsLoaded = true;
                var typeId = msg.TypeId;
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
                                            this.ordersByClOrderId[order.ClOrderId] = order;
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
                return clientId == -1 ? messages : messages.Where(m => m.ClientId == clientId).ToList();
            }
            else
                return new List<ExecutionMessage>();
        }

        public Order Get(int id) => this.ordersById[id];

        public Order GetByClId(string id)
        {
            Order result;
            this.ordersByClOrderId.TryGetValue(id, out result);
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
            if (!order.IsNotSent)
                throw new ArgumentException($"Can not send order that has been already sent {order}");

            this.framework.EventServer.OnSendOrder(order);
            if (order.Id == -1)
                Register(order);

            if (order.IsOCA)
            {
                List<Order> list;
                this.ordersByOCAList.TryGetValue(order.OCA, out list);
                this.ordersByOCAList[order.OCA] = list = list ?? new List<Order>();
                list.Add(order);
            }

            Orders.Add(order);
            this.ordersById[order.Id] = order;

            if (this.framework.Mode == FrameworkMode.Realtime)
                this.ordersByClOrderId[order.ClOrderId] = order;

            order.DateTime = this.framework.Clock.DateTime;
            order.Status = OrderStatus.PendingNew;
            var command = new ExecutionCommand(ExecutionCommandType.Send, order)
            {
                DateTime = order.DateTime,
                Id = order.Id,
                OrderId = order.Id,
                ClOrderId = order.ClOrderId,
                ProviderOrderId = order.ProviderOrderId,
                ProviderId = order.ProviderId,
                RouteId = order.RouteId,
                PortfolioId = order.PortfolioId,
                TransactTime = order.TransactTime,
                Instrument = order.Instrument,
                InstrumentId = order.InstrumentId,
                Provider = order.Provider,
                Portfolio = order.Portfolio,
                Side = order.Side,
                OrdType = order.Type,
                TimeInForce = order.TimeInForce,
                Price = order.Price,
                StopPx = order.StopPx,
                Qty = order.Qty,
                OCA = order.OCA,
                Text = order.Text,
                Account = order.Account,
                ClientID = order.ClientID,
                ClientId = order.ClientId
            };
            Messages.Add(command);
            order.OnExecutionCommand(command);
            this.framework.EventServer.OnExecutionCommand(command);
            this.framework.EventServer.OnPendingNewOrder(order, true);
            if (IsPersistent)
                Server?.Save(command, -1);
            order.Provider.Send(command);
        }

        public void Reject(Order order)
        {
            order.Status = OrderStatus.Rejected;
        }

        public void Replace(Order order, double price) => Replace(order, price, order.StopPx, order.Qty);

        public void Replace(Order order, double price, double stopPx, double qty)
        {
            if (order.IsNotSent)
                throw new ArgumentException($"Can not replace order that is not sent {order}");

            var command = new ExecutionCommand(ExecutionCommandType.Replace, order)
            {
                DateTime = this.framework.Clock.DateTime,
                Id = order.Id,
                OrderId = order.Id,
                ClOrderId = order.ClOrderId,
                ProviderOrderId = order.ProviderOrderId,
                ProviderId = order.ProviderId,
                RouteId = order.RouteId,
                PortfolioId = order.PortfolioId,
                TransactTime = order.TransactTime,
                Instrument = order.Instrument,
                InstrumentId = order.InstrumentId,
                Provider = order.Provider,
                Portfolio = order.Portfolio,
                Side = order.Side,
                OrdType = order.Type,
                TimeInForce = order.TimeInForce,
                Price = order.Price,
                StopPx = order.StopPx,
                Qty = order.Qty,
                OCA = order.OCA,
                Text = order.Text,
                Account = order.Account,
                ClientID = order.ClientID,
                ClientId = order.ClientId
            };
            Messages.Add(command);
            order.OnExecutionCommand(command);
            this.framework.EventServer.OnExecutionCommand(command);
            if (IsPersistent)
                Server?.Save(command, -1);
            order.Provider.Send(command);
        }

        internal void OnExecutionReport(ExecutionReport report)
        {
            report.Order = report.Order ?? (report.OrderId == -1 ? ordersByClOrderId[report.ClOrderId] : ordersById[report.OrderId]);
            report.ClientId = report.Order.ClientId;
            report.ClOrderId = report.Order.ClOrderId;
            var order = report.Order;
            var orderStatus = order.Status;
            Messages.Add(report);
            order.OnExecutionReport(report);
            if (orderStatus != order.Status)
                this.framework.EventServer.OnOrderStatusChanged(order, true);

            switch (report.ExecType)
            {
                case ExecType.ExecNew:
                    if (report.ProviderOrderId != null)
                        ordersByProviderId[report.ProviderOrderId] = order;
                    this.framework.EventServer.OnNewOrder(order, true);
                    break;
                case ExecType.ExecRejected:
                    this.framework.EventServer.OnOrderRejected(order, true);
                    this.framework.EventServer.OnOrderDone(order, true);
                    CancelOCAOrder(order);
                    break;
                case ExecType.ExecExpired:
                    this.framework.EventServer.OnOrderExpired(order, true);
                    this.framework.EventServer.OnOrderDone(order, true);
                    CancelOCAOrder(order);
                    break;
                case ExecType.ExecTrade:
                    if (order.Status == OrderStatus.PartiallyFilled)
                        this.framework.EventServer.OnOrderPartiallyFilled(order, true);
                    else
                    {
                        this.framework.EventServer.OnOrderFilled(order, true);
                        this.framework.EventServer.OnOrderDone(order, true);
                        CancelOCAOrder(order);
                    }
                    break;
                case ExecType.ExecCancelled:
                    this.framework.EventServer.OnOrderCancelled(order, true);
                    this.framework.EventServer.OnOrderDone(order, true);
                    CancelOCAOrder(order);
                    break;
                case ExecType.ExecCancelReject:
                    this.framework.EventServer.OnOrderCancelRejected(order, true);
                    break;
                case ExecType.ExecReplace:
                    this.framework.EventServer.OnOrderReplaced(order, true);
                    break;
                case ExecType.ExecReplaceReject:
                    this.framework.EventServer.OnOrderReplaceRejected(order, true);
                    break;
            }
            if (IsPersistent)
                Server?.Save(report, -1);
        }

        internal void OnAccountReport(AccountReport report)
        {
            if (IsPersistent && !report.IsLoaded)
                Server?.Save(report, -1);
        }

        internal void OnExecutionReportLoaded(ExecutionReport report)
        {
            report.Order = report.Order ?? (report.OrderId == -1 ? ordersByClOrderId[report.ClOrderId] : ordersById[report.OrderId]);
            report.Instrument = report.Order.Instrument;
            report.ClientId = report.Order.ClientId;
            var order = report.Order;
            var orderStatus = order.Status;
            Messages.Add(report);
            order.OnExecutionReport(report);

            if (orderStatus != order.Status)
                this.framework.EventServer.OnOrderStatusChanged(order, true);

            switch (report.ExecType)
            {
                case ExecType.ExecNew:
                    if (report.ClOrderId != null)
                        ordersByClOrderId[report.ClOrderId] = order;
                    this.framework.EventServer.OnNewOrder(order, true);
                    return;
                case ExecType.ExecStopped:
                case ExecType.ExecPendingCancel:
                case ExecType.ExecPendingReplace:
                    break;
                case ExecType.ExecRejected:
                    this.framework.EventServer.OnOrderRejected(order, true);
                    this.framework.EventServer.OnOrderDone(order, true);
                    CancelOCAOrder(order);
                    return;
                case ExecType.ExecExpired:
                    this.framework.EventServer.OnOrderExpired(order, true);
                    this.framework.EventServer.OnOrderDone(order, true);
                    CancelOCAOrder(order);
                    return;
                case ExecType.ExecTrade:
                    if (order.Status == OrderStatus.PartiallyFilled)
                        this.framework.EventServer.OnOrderPartiallyFilled(order, true);
                    else
                    {
                        this.framework.EventServer.OnOrderFilled(order, true);
                        this.framework.EventServer.OnOrderDone(order, true);
                        CancelOCAOrder(order);
                    }
                    return;                        
                case ExecType.ExecCancelled:
                    this.framework.EventServer.OnOrderCancelled(order, true);
                    this.framework.EventServer.OnOrderDone(order, true);
                    CancelOCAOrder(order);
                    return;
                case ExecType.ExecCancelReject:
                    this.framework.EventServer.OnOrderCancelRejected(order, true);
                    return;
                case ExecType.ExecReplace:
                    this.framework.EventServer.OnOrderReplaced(order, true);
                    return;
                case ExecType.ExecReplaceReject:
                    this.framework.EventServer.OnOrderReplaceRejected(order, true);
                    break;
                default:
                    return;
            }
        }

        private void CancelOCAOrder(Order order)
        {
            if (!order.IsOCA)
                return;

            List<Order> list;
            this.ordersByOCAList.TryGetValue(order.OCA, out list);
            if (list == null)
                return;

            this.ordersByOCAList.Remove(order.OCA);
            foreach (var o in list.Where(o => o != order))
                Cancel(o);
        }
    }
}