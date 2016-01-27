#pragma warning disable CS0067

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;


namespace SmartQuant
{
    public class EventController
    {
        private Framework framework;

        public EventController(Framework framework)
        {
            this.framework = framework;
        }

        public void Emit(Event e)
        {
            this.framework.EventManager.Dispatcher.Emit(e);
        }

        public virtual void OnCommand(Command command)
        {
            // noop
        }
    }

    #region many eventHandler defenitions
    public delegate void BidEventHandler(object sender, Bid bid);
    public delegate void AskEventHandler(object sender, Ask ask);
    public delegate void BarEventHandler(object sender, Bar bar);
    public delegate void FillEventHandler(object sender, OnFill fill);
    public delegate void AccountReportEventHandler(object sender, AccountReport report);
    public delegate void SimulatorProgressEventHandler(object sender, SimulatorProgressEventArgs args);
    public delegate void StrategyEventHandler(object sender, StrategyEventArgs args);
    public delegate void ResponseEventHandler(object sender, Response response);
    public delegate void OutputEventHandler(object sender, Output output);

    public class SimulatorProgressEventArgs : EventArgs
    {
        public long Count { get; }
        public int Percent { get; }
        public SimulatorProgressEventArgs(long count, int percent)
        {
            Count = count;
            Percent = percent;
        }
    }

    #endregion

    public class EventDispatcherServerClient
    {
        public EventDispatcherServerClient()
        {
            this.IsStopped = true;
            this.streamerManager_0 = new StreamerManager();
            this.streamerManager_0.AddDefaultStreamers();
        }

        public void Emit(Event e)
        {
            this.streamerManager_0.Serialize(this.binaryWriter_0, e);
        }

        private void method_0()
        {
            Console.WriteLine(DateTime.Now + " Event Dispatcher Server Client thread started");
            try
            {
                while (true)
                {
                    Event e = (Event)this.streamerManager_0.Deserialize(this.binaryReader_0);
                    this.OnEvent(e);
                }
            }
            catch (Exception value)
            {
                Console.WriteLine(DateTime.Now + " Event Dispatcher Server Client exception ");
                Console.WriteLine(value);
            }
            finally
            {
                Console.WriteLine(DateTime.Now + " Event Dispatcher Server Client closing connection... ");
#if DNXCORE50
                this.tcpClient_0.Dispose();
#else
                this.tcpClient_0.Close();
#endif
            }
        }

        public virtual void OnEvent(Event e)
        {
            if (this.eventDispatcher_0 != null)
            {
                this.eventDispatcher_0.OnEvent(e);
            }
        }

        public void Start()
        {
            this.stream_0 = this.tcpClient_0.GetStream();
            this.stream_1 = this.tcpClient_1.GetStream();
            this.binaryReader_0 = new BinaryReader(this.stream_0);
            this.binaryWriter_0 = new BinaryWriter(this.stream_1);
            this.thread_0 = new Thread(this.method_0);
            this.thread_0.IsBackground = true;
            this.thread_0.Start();
            this.IsStopped = false;
        }

        public void Stop()
        {
        }

        public EventDispatcher Dispatcher
        {
            get
            {
                return this.eventDispatcher_0;
            }
            set
            {
                this.eventDispatcher_0 = value;
                this.eventDispatcher_0.eventDispatcherServerClient_0 = this;
            }
        }

        internal BinaryReader binaryReader_0;

        internal BinaryWriter binaryWriter_0;

        internal EventDispatcher eventDispatcher_0;

        public bool IsStopped;

        private StreamerManager streamerManager_0;

        internal Stream stream_0;

        internal Stream stream_1;

        internal TcpClient tcpClient_0;

        internal TcpClient tcpClient_1;

        private Thread thread_0;
    }

    //public class EventDispatcher
    //{

    //    private Framework framework;
    //    private IdArray<IEventClient> clientsById = new IdArray<IEventClient>();
    //    private IdArray<List<IEventClient>> clientsByEventType = new IdArray<List<IEventClient>>();
    //    private EventQueue commandQueue;

    //    public PermanentQueue<Event> PermanentQueue { get; } = new PermanentQueue<Event>();

    //    public EventController Controller { get; set; }

    //    public event FrameworkEventHandler FrameworkCleared;
    //    public event GroupEventHandler NewGroup;
    //    public event GroupEventEventHandler NewGroupEvent;
    //    public event GroupUpdateEventHandler NewGroupUpdate;

    //    public event BidEventHandler Bid;
    //    public event AskEventHandler Ask;
    //    public event TradeEventHandler Trade;
    //    public event BarEventHandler BarOpen;
    //    public event BarEventHandler Bar;

    //    public event ExecutionCommandEventHandler ExecutionCommand;
    //    public event ExecutionReportEventHandler ExecutionReport;
    //    public event OrderManagerClearedEventHandler OrderManagerCleared;
    //    public event PositionEventHandler PositionOpened;
    //    public event PositionEventHandler PositionClosed;
    //    public event PositionEventHandler PositionChanged;
    //    public event FillEventHandler Fill;
    //    public event TransactionEventHandler Transaction;

    //    public event PortfolioEventHandler PortfolioAdded;
    //    public event PortfolioEventHandler PortfolioRemoved;
    //    public event PortfolioEventHandler PortfolioParentChanged;

    //    public event AccountDataEventHandler AccountData;
    //    public event AccountReportEventHandler AccountReport;
    //    public event HistoricalDataEventHandler HistoricalData;
    //    public event HistoricalDataEndEventHandler HistoricalDataEnd;

    //    public event InstrumentEventHandler InstrumentAdded;
    //    public event InstrumentEventHandler InstrumentDeleted;
    //    public event InstrumentDefinitionEventHandler InstrumentDefinition;
    //    public event InstrumentDefinitionEndEventHandler InstrumentDefinitionEnd;

    //    public event EventHandler EventManagerPaused;
    //    public event EventHandler EventManagerResumed;
    //    public event EventHandler EventManagerStep;
    //    public event EventHandler EventManagerStarted;
    //    public event EventHandler EventManagerStopped;

    //    public event SimulatorProgressEventHandler SimulatorProgress;
    //    public event EventHandler SimulatorStop;

    //    public event StrategyEventHandler StrategyAdded;

    //    public event ProviderEventHandler ProviderAdded;
    //    public event ProviderEventHandler ProviderRemoved;
    //    public event ProviderErrorEventHandler ProviderError;
    //    public event ProviderEventHandler ProviderConnected;
    //    public event ProviderEventHandler ProviderDisconnected;
    //    public event ProviderEventHandler ProviderStatusChanged;

    //    public EventDispatcher()
    //    {
    //        throw new NotImplementedException("Don't use this constructor");
    //    }

    //    public EventDispatcher(Framework framework)
    //    {
    //        this.framework = framework;
    //    }

    //    public void Add(IEventClient client)
    //    {
    //        this.clientsById[client.Id] = client;
    //        foreach (var type in client.EventTypes)
    //        {
    //            if (this.clientsByEventType[type] == null)
    //                this.clientsByEventType[type] = new List<IEventClient>();
    //            this.clientsByEventType[type].Add(client);
    //        }
    //        PermanentQueue.AddReader(client);
    //    }

    //    public void Remove(IEventClient client)
    //    {
    //        this.clientsById[client.Id] = client;
    //        foreach (var type in client.EventTypes)
    //            this.clientsByEventType[type].Remove(client);
    //        PermanentQueue.RemoveReader(client);
    //    }

    //    public void Emit(Event e)
    //    {
    //        if (this.commandQueue == null)
    //        {
    //            this.commandQueue = new EventQueue(EventQueueId.Service, EventQueueType.Master, EventQueuePriority.Normal, 102400, null);
    //            this.commandQueue.Enqueue(new OnQueueOpened(this.commandQueue));
    //            this.framework.EventBus.CommandPipe.Add(this.commandQueue);
    //        }
    //        this.commandQueue.Enqueue(e);
    //        if (e.TypeId == EventType.Command)
    //        {
    //            var command = (Command)e;
    //            var response = new Response(command);
    //            if (Controller != null)
    //            {
    //                Controller.OnCommand(command);
    //                return;
    //            }
    //            switch (command.Type)
    //            {
    //                case CommandType.GetInformation:
    //                    if (command[0] != null && command[0] is int && (int)command[0] == 0)
    //                    {
    //                        response.Type = ResponseType.Information;
    //                        response[0] = "Framework";
    //                        response[1] = this.framework.Name;
    //                        this.commandQueue.Enqueue(response);
    //                        return;
    //                    }
    //                    break;
    //                default:
    //                    break;
    //            }
    //        }
    //    }

    //    public void OnEvent(Event e)
    //    {
    //        if (e.TypeId == EventType.Response && ((Response)e).ReceiverId != -1)
    //        {
    //            this.clientsById[((Response)e).ReceiverId].OnEvent(e);
    //        }
    //        else
    //        {
    //            var clients = this.clientsByEventType[e.TypeId];
    //            if (clients != null)
    //            {
    //                foreach (var c in clients)
    //                    if (c.IsOnEventEnabled)
    //                        c.OnEvent(e);
    //            }
    //        }

    //    }
    //}


    public class EventDispatcher
    {
        public EventDispatcher()
        {
            this.idArray_0 = new IdArray<IEventClient>(1000);
            this.idArray_1 = new IdArray<List<IEventClient>>(1000);
            this.permanentQueue_0 = new PermanentQueue<Event>();
        }

        public EventDispatcher(Framework framework)
        {

            this.idArray_0 = new IdArray<IEventClient>(1000);
            this.idArray_1 = new IdArray<List<IEventClient>>(1000);
            this.permanentQueue_0 = new PermanentQueue<Event>();
            this.framework = framework;
            if (this.eventQueue_1 != null)
            {
                this.thread_0 = new Thread(method_0);
                this.thread_0.IsBackground = true;
                this.thread_0.Name = "Event Dispatcher Thread";
                this.thread_0.Start();
            }
        }

        public void Add(IEventClient client)
        {
            this.idArray_0[client.Id] = client;
            byte[] eventTypes = client.EventTypes;
            for (int i = 0; i < eventTypes.Length; i++)
            {
                byte id = eventTypes[i];
                if (this.idArray_1[(int)id] == null)
                {
                    this.idArray_1[(int)id] = new List<IEventClient>();
                }
                this.idArray_1[(int)id].Add(client);
            }
            this.permanentQueue_0.AddReader(client);
        }
        public void Emit(Event e) { }
        //public void Emit(Event e)
        //{
        //    if (this.eventDispatcherServerClient_0 != null)
        //    {
        //        this.eventDispatcherServerClient_0.Emit(e);
        //        return;
        //    }
        //    if (this.eventQueue_0 == null)
        //    {
        //        this.eventQueue_0 = new EventQueue(4, 0, 2, 100000, null);
        //        this.eventQueue_0.Enqueue(new OnQueueOpened(this.eventQueue_0));
        //        this.framework.EventBus.CommandPipe.Add(this.eventQueue_0);
        //    }
        //    this.eventQueue_0.Enqueue(e);
        //    if (e.TypeId == 54)
        //    {
        //        Command command_ = (Command)e;
        //        Response response_ = new Response(command_);
        //        if (this.eventController_0 != null)
        //        {
        //            this.eventController_0.OnCommand(command_);
        //            return;
        //        }
        //        switch (command_.Type)
        //        {
        //            case 0:
        //                if (command_[0] != null && command_[0] is int && (int)command_[0] == 0)
        //                {
        //                    response_.Type = 2;
        //                    response_[0] = "Framework";
        //                    response_[1] = this.framework.Name;
        //                    this.eventQueue_0.Enqueue(response_);
        //                    return;
        //                }
        //                break;
        //            case 1:
        //                {
        //                    response_.Type = 3;
        //                    InstrumentList instruments = this.framework.InstrumentManager.Instruments;
        //                    response_[0] = instruments.Count;
        //                    for (int i = 0; i < instruments.Count; i++)
        //                    {
        //                        response_[i + 1] = instruments.GetByIndex(i);
        //                    }
        //                    this.eventQueue_0.Enqueue(response_);
        //                    return;
        //                }
        //            case 2:
        //                this.framework.InstrumentManager.Add((Instrument)command_[0], true);
        //                return;
        //            case 3:
        //                this.framework.InstrumentManager.Delete((int)command_[0]);
        //                return;
        //            case 4:
        //                {
        //                    response_.Type = 4;
        //                    ProviderList providers = this.framework.ProviderManager.Providers;
        //                    response_[0] = providers.Count;
        //                    for (int j = 0; j < providers.Count; j++)
        //                    {
        //                        response_[j + 1] = new ProviderInfo(providers.GetByIndex(j) as Provider);
        //                    }
        //                    this.eventQueue_0.Enqueue(response_);
        //                    return;
        //                }
        //            case 5:
        //                {
        //                    byte id = (byte)command_[0];
        //                    IProvider provider = this.framework.ProviderManager.GetProvider((int)id);
        //                    provider.Connect();
        //                    return;
        //                }
        //            case 6:
        //                {
        //                    byte id2 = (byte)command_[0];
        //                    IProvider provider2 = this.framework.ProviderManager.GetProvider((int)id2);
        //                    provider2.Disconnect();
        //                    return;
        //                }
        //            case 7:
        //            case 8:
        //            case 20:
        //            case 21:
        //                break;
        //            case 9:
        //                this.framework.ScenarioManager.Start();
        //              //  this.framework.scenarioManager__0.Start();
        //                return;
        //            case 10:
        //                this.framework.ScenarioManager.Stop();
        //            //    this.framework.scenarioManager__0.Stop();
        //                return;
        //            case 11:
        //                {
        //                    string path = (string)command_[0];
        //                    string value = null;
        //                    try
        //                    {
        //                        value = File.ReadAllText(path);
        //                    }
        //                    catch (Exception arg)
        //                    {
        //                        Console.WriteLine("Can not read configuation file : " + arg);
        //                    }
        //                    response_.Type = 5;
        //                    response_[0] = value;
        //                    this.eventQueue_0.Enqueue(response_);
        //                    return;
        //                }
        //            case 12:
        //                {
        //                    string path2 = (string)command_[0];
        //                    string value2 = (string)command_[1];
        //                    StreamWriter streamWriter = new StreamWriter(path2);
        //                    streamWriter.Write(value2);
        //                    streamWriter.Close();
        //                    return;
        //                }
        //            case 13:
        //                {
        //                    int id3 = (int)command_[0];
        //                    if (this.framework.StrategyManager.Strategy != null)
        //                    {
        //                        this.framework.StrategyManager.Strategy.AddInstrument(id3);
        //                    }
        //                    //if (this.framework.strategyManager__0.strategy__0 != null)
        //                    //{
        //                    //    this.framework.strategyManager__0.strategy__0.Add(id3);
        //                    //    return;
        //                    //}
        //                    break;
        //                }
        //            case 14:
        //                {
        //                    int id4 = (int)command_[0];
        //                    if (this.framework.StrategyManager.Strategy != null)
        //                    {
        //                        this.framework.StrategyManager.Strategy.RemoveInstrument(id4);
        //                    }
        //                    //if (this.framework.strategyManager__0.strategy__0 != null)
        //                    //{
        //                    //    this.framework.strategyManager__0.strategy__0.Remove(id4);
        //                    //    return;
        //                    //}
        //                    break;
        //                }
        //            case 15:
        //                {
        //                    string symbol = (string)command_[0];
        //                    if (this.framework.StrategyManager.Strategy != null)
        //                    {
        //                        this.framework.StrategyManager.Strategy.AddInstrument(symbol);
        //                    }
        //                    //if (this.framework.strategyManager__0.strategy__0 != null)
        //                    //{
        //                    //    this.framework.strategyManager__0.strategy__0.Add(symbol);
        //                    //    return;
        //                    //}
        //                    break;
        //                }
        //            case 16:
        //                {
        //                    string symbol2 = (string)command_[0];
        //                    if (this.framework.StrategyManager.Strategy != null)
        //                    {
        //                        this.framework.StrategyManager.Strategy.RemoveInstrument(symbol2);
        //                    }
        //                    //if (this.framework.strategyManager__0.strategy__0 != null)
        //                    //{
        //                    //    this.framework.strategyManager__0.strategy__0.Remove(symbol2);
        //                    //    return;
        //                    //}
        //                    break;
        //                }
        //            case 17:
        //                response_.Type = 6;
        //                response_[0] = this.framework.StrategyManager.Status;
        //                this.eventQueue_0.Enqueue(response_);
        //                return;
        //            case 18:
        //                response_.Type = 7;
        //                response_[0] = this.framework.StrategyManager.Status;
        //                this.eventQueue_0.Enqueue(response_);
        //                return;
        //            case 19:
        //                {
        //                    object obj = command_.Fields[0];
        //                    if (obj is Instrument)
        //                    {
        //                        this.framework.InstrumentManager.Save((Instrument)obj);
        //                        this.framework.InstrumentManager.Server.Flush();
        //                        return;
        //                    }
        //                    if (obj is ProviderInfo)
        //                    {
        //                        ProviderInfo providerInfo = (ProviderInfo)obj;
        //                        IProvider provider3 = this.framework.ProviderManager.GetProvider((int)providerInfo.byte_0);
        //                        ParameterHelper parameterHelper = new ParameterHelper();
        //                        parameterHelper.SetParameters(providerInfo.parameterList_0, provider3);
        //                        this.framework.ProviderManager.SaveSettings(provider3);
        //                        return;
        //                    }
        //                    break;
        //                }
        //            case 22:
        //                {
        //                    string name = (string)command_[0];
        //                    response_.Type = 9;
        //                    response_[0] = this.framework.PortfolioManager[name];
        //                    this.eventQueue_0.Enqueue(response_);
        //                    return;
        //                }
        //            case 23:
        //                {
        //                    response_.Type = 10;
        //                    PortfolioList portfolios = this.framework.PortfolioManager.Portfolios;
        //                    response_[0] = portfolios.Count;
        //                    for (int k = 0; k < portfolios.Count; k++)
        //                    {
        //                        response_[k + 1] = portfolios.GetByIndex(k);
        //                    }
        //                    this.eventQueue_0.Enqueue(response_);
        //                    return;
        //                }
        //            case 24:
        //                {
        //                    int num = (int)command_[0];
        //                    Order order = this.framework.OrderManager.idArray_0[num];
        //                    if (order != null)
        //                    {
        //                        this.framework.OrderManager.Cancel(order);
        //                        return;
        //                    }
        //                    Console.WriteLine("CancelOrder failed, Order with id: " + num + " not found");
        //                    return;
        //                }
        //            case 25:
        //                {
        //                    int num2 = (int)command_[0];
        //                    Order order2 = this.framework.OrderManager.idArray_0[num2];
        //                    if (order2 != null)
        //                    {
        //                        double price = (double)command_[1];
        //                        double stopPx = (double)command_[2];
        //                        int num3 = (int)command_[3];
        //                        this.framework.OrderManager.Replace(order2, price, stopPx, (double)num3);
        //                        return;
        //                    }
        //                    Console.WriteLine("ReplaceOrder failed, Order with id: " + num2 + " not found");
        //                    return;
        //                }
        //            case 26:
        //                {
        //                    string text = (string)command_[0];
        //                    Portfolio portfolio = this.framework.PortfolioManager[text];
        //                    if (portfolio != null)
        //                    {
        //                        byte byte_ = (byte)command_[1];
        //                        double double_ = (double)command_[2];
        //                        string string_ = (string)command_[3];
        //                        this.Emit(new AccountReport
        //                        {
        //                            dateTime = this.framework.Clock.DateTime,
        //                            byte_1 = byte_,
        //                            double_0 = double_,
        //                            string_2 = string_,
        //                            PortfolioId = portfolio.Id
        //                        });
        //                        return;
        //                    }
        //                    Console.WriteLine("AdjustAccountMoney failed, Portfolio with name: " + text + " not found");
        //                    return;
        //                }
        //            case 27:
        //                this.framework.StrategyManager.method_1();
        //                return;
        //            case 28:
        //                {
        //                    string command = (string)command_[0];
        //                    this.framework.EventServer.OnEvent(new OnUserCommand(command));
        //                    return;
        //                }
        //            case 29:
        //                {
        //                    UserList users = this.framework.UserManager.Users;
        //                    response_.Type = 11;
        //                    response_[0] = users.Count;
        //                    for (int l = 0; l < users.Count; l++)
        //                    {
        //                        response_[l + 1] = users.users[l];
        //                    }
        //                    this.eventQueue_0.Enqueue(response_);
        //                    return;
        //                }
        //            case 30:
        //                {
        //                    int clientId = (int)command_[0];
        //                    response_.Type = 12;
        //                    response_[0] = this.framework.UserManager.GetById(clientId);
        //                    this.eventQueue_0.Enqueue(response_);
        //                    return;
        //                }
        //            case 31:
        //                {
        //                    User user = (User)command_[0];
        //                    this.framework.UserManager.Add(user);
        //                    this.framework.UserManager.Save();
        //                    response_.Type = 0;
        //                    response_[0] = 31;
        //                    response_[1] = user;
        //                    this.eventQueue_0.Enqueue(response_);
        //                    return;
        //                }
        //            case 32:
        //                {
        //                    User user2 = (User)command_[0];
        //                    if (this.framework.UserManager.Delete(user2))
        //                    {
        //                        this.framework.UserManager.Save();
        //                        response_.Type = 0;
        //                        response_[0] = 32;
        //                        response_[1] = user2;
        //                        this.eventQueue_0.Enqueue(response_);
        //                        return;
        //                    }
        //                    break;
        //                }
        //            case 33:
        //                {
        //                    User arg_A31_0 = (User)command_[0];
        //                    this.framework.UserManager.Save();
        //                    return;
        //                }
        //            case 34:
        //                this.framework.SubscriptionManager.Subscribe((int)((byte)command_[0]), (int)command_[1]);
        //                return;
        //            case 35:
        //                this.framework.SubscriptionManager.Unsubscribe((int)((byte)command_[0]), (int)command_[1]);
        //                return;
        //            case 36:
        //                {
        //                    object obj2 = command_.Fields[0];
        //                    if (obj2 is User)
        //                    {
        //                        User user3 = (User)obj2;
        //                        this.framework.EventServer.OnEvent(new OnPropertyChanged(146, user3.Id, (string)command_.Fields[1]));
        //                        return;
        //                    }
        //                    break;
        //                }
        //            case 37:
        //                {
        //                    Order order3 = (Order)command_[0];
        //                    this.framework.OrderManager.Send(order3);
        //                    return;
        //                }
        //            case 38:
        //                {
        //                    int num4 = (int)command_[0];
        //                    int num5 = (int)command_[1];
        //                    Portfolio byId = this.framework.PortfolioManager.GetById(num4);
        //                    if (byId == null)
        //                    {
        //                        Console.WriteLine("ClosePosition::Provider not found, id: " + num4);
        //                        return;
        //                    }
        //                    Position position = byId.idArray_1[num5];
        //                    if (position == null)
        //                    {
        //                        Console.WriteLine("ClosePosition::Position not found, instrumentId: " + num5);
        //                        return;
        //                    }
        //                    if (position.double_0 == 0.0)
        //                    {
        //                        Console.WriteLine("ClosePosition::Position already closed");
        //                        return;
        //                    }
        //                    if (this.framework.ExecutionProvider == null)
        //                    {
        //                        Console.WriteLine("ClosePosition::Provider not found");
        //                        return;
        //                    }
        //                    OrderSide side = (position.double_0 > 0.0) ? OrderSide.Sell : OrderSide.Buy;
        //                    Order order4 = new Order(this.framework.ExecutionProvider, position.instrument_0, OrderType.Market, side, position.Qty, 0.0, 0.0, TimeInForce.Day, "ClosePosition");
        //                    order4.portfolio_0 = byId;
        //                    order4.int_5 = num4;
        //                    this.framework.orderManager_0.Send(order4);
        //                    break;
        //                }
        //            default:
        //                return;
        //        }
        //    }
        //}

        private void method_0()
        {
            Console.WriteLine("Event dispatcher thread started: Framework = " + this.framework.Name + " Clock = " + this.framework.Clock.GetModeAsString());
            while (true)
            {
                if (this.framework.Mode != FrameworkMode.Simulation && !this.eventQueue_1.IsEmpty())
                {
                    this.method_1(this.eventQueue_1.Read());
                }
                else
                {
                    Thread.Sleep(1);
                }
            }
        }

        private void method_1(Event event_0)
        {
            if (event_0.TypeId == 55 && ((Response)event_0).ReceiverId != -1)
            {
                this.idArray_0[((Response)event_0).ReceiverId].OnEvent(event_0);
            }
            else
            {
                List<IEventClient> list = this.idArray_1[(int)event_0.TypeId];
                if (list != null)
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        if (list[i].IsOnEventEnabled)
                        {
                            list[i].OnEvent(event_0);
                        }
                    }
                }
            }
            byte typeId = event_0.TypeId;
            if (typeId <= 55)
            {
                if (typeId <= 14)
                {
                    switch (typeId)
                    {
                        case 2:
                            if (this.bidEventHandler_0 != null)
                            {
                                this.bidEventHandler_0(this, (Bid)event_0);
                                return;
                            }
                            break;
                        case 3:
                            if (this.askEventHandler_0 != null)
                            {
                                this.askEventHandler_0(this, (Ask)event_0);
                                return;
                            }
                            break;
                        case 4:
                            if (this.tradeEventHandler_0 != null)
                            {
                                this.tradeEventHandler_0(this, (Trade)event_0);
                                return;
                            }
                            break;
                        case 5:
                            break;
                        case 6:
                            {
                                Bar bar = (Bar)event_0;
                                BarStatus barStatus_ = bar.Status;
                                if (barStatus_ != BarStatus.Open)
                                {
                                    if (barStatus_ != BarStatus.Close)
                                    {
                                        return;
                                    }
                                    if (this.barEventHandler_0 != null)
                                    {
                                        this.barEventHandler_0(this, bar);
                                        return;
                                    }
                                }
                                else if (this.barEventHandler_1 != null)
                                {
                                    this.barEventHandler_1(this, bar);
                                    return;
                                }
                                break;
                            }
                        default:
                            switch (typeId)
                            {
                                case 13:
                                    this.permanentQueue_0.Enqueue(event_0);
                                    if (this.executionReportEventHandler_0 != null)
                                    {
                                        this.executionReportEventHandler_0(this, (ExecutionReport)event_0);
                                        return;
                                    }
                                    break;
                                case 14:
                                    this.permanentQueue_0.Enqueue(event_0);
                                    if (this.executionCommandEventHandler_0 != null)
                                    {
                                        this.executionCommandEventHandler_0(this, (ExecutionCommand)event_0);
                                        return;
                                    }
                                    break;
                                default:
                                    return;
                            }
                            break;
                    }
                }
                else if (typeId != 21)
                {
                    if (typeId != 27)
                    {
                        switch (typeId)
                        {
                            case EventType.Group:
                                this.permanentQueue_0.Enqueue(event_0);
                                if (this.groupEventHandler_0 != null)
                                {
                                    this.groupEventHandler_0(this, new GroupEventAgrs((Group)event_0));
                                    return;
                                }
                                break;
                            case EventType.GroupUpdate:
                                this.permanentQueue_0.Enqueue(event_0);
                                if (this.groupUpdateEventHandler_0 != null)
                                {
                                    this.groupUpdateEventHandler_0(this, new GroupUpdateEventAgrs((GroupUpdate)event_0));
                                    return;
                                }
                                break;
                            case EventType.GroupEvent:
                                this.permanentQueue_0.Enqueue(event_0);
                                if (this.groupEventEventHandler_0 != null)
                                {
                                    this.groupEventEventHandler_0(this, new GroupEventEventAgrs((GroupEvent)event_0));
                                    return;
                                }
                                break;
                            case EventType.Message:
                                break;
                            case EventType.Command:
                                if (this.commandEventHandler_0 != null)
                                {
                                    this.commandEventHandler_0(this, (Command)event_0);
                                    return;
                                }
                                break;
                            case EventType.Response:
                                if (this.responseEventHandler_0 != null)
                                {
                                    this.responseEventHandler_0(this, (Response)event_0);
                                    return;
                                }
                                break;
                            default:
                                return;
                        }
                    }
                    else
                    {
                        this.permanentQueue_0.Enqueue(event_0);
                        if (this.outputEventHandler_0 != null)
                        {
                            this.outputEventHandler_0(this, (Output)event_0);
                            return;
                        }
                    }
                }
                else
                {
                    this.permanentQueue_0.Enqueue(event_0);
                    if (this.providerErrorEventHandler_0 != null)
                    {
                        this.providerErrorEventHandler_0(this, new ProviderErrorEventArgs((ProviderError)event_0));
                        return;
                    }
                }
            }
            else if (typeId <= 114)
            {
                if (typeId != 99)
                {
                    switch (typeId)
                    {
                        case 110:
                            if (this.ovQrTixtSx != null)
                            {
                                this.ovQrTixtSx(this, new PositionEventArgs(((OnPositionOpened)event_0).Portfolio, ((OnPositionOpened)event_0).Position));
                                return;
                            }
                            break;
                        case 111:
                            if (this.positionEventHandler_1 != null)
                            {
                                this.positionEventHandler_1(this, new PositionEventArgs(((OnPositionClosed)event_0).Portfolio, ((OnPositionClosed)event_0).Position));
                                return;
                            }
                            break;
                        case 112:
                            if (this.positionEventHandler_0 != null)
                            {
                                this.positionEventHandler_0(this, new PositionEventArgs(((OnPositionChanged)event_0).Portfolio, ((OnPositionChanged)event_0).Position));
                                return;
                            }
                            break;
                        case 113:
                            this.permanentQueue_0.Enqueue(event_0);
                            if (this.fillEventHandler_0 != null)
                            {
                                this.fillEventHandler_0(this, (OnFill)event_0);
                                return;
                            }
                            break;
                        case 114:
                            this.permanentQueue_0.Enqueue(event_0);
                            if (this.transactionEventHandler_0 != null)
                            {
                                this.transactionEventHandler_0(this, (OnTransaction)event_0);
                                return;
                            }
                            break;
                        default:
                            return;
                    }
                }
                else
                {
                    this.permanentQueue_0.Clear();
                   // if (FrameworkCleared != null)
                  //  {
                        FrameworkCleared?.Invoke(this, new FrameworkEventArgs(((OnFrameworkCleared) event_0).Framework));
                 //       return;
                //    }
                }
            }
            else
            {
                switch (typeId)
                {
                    case 130:
                        this.permanentQueue_0.Enqueue(event_0);
                        if (this.orderManagerClearedEventHandler_0 != null)
                        {
                            this.orderManagerClearedEventHandler_0(this, (OnOrderManagerCleared)event_0);
                            return;
                        }
                        break;
                    case 131:
                        if (this.instrumentDefinitionEventHandler_0 != null)
                        {
                            this.instrumentDefinitionEventHandler_0(this, new InstrumentDefinitionEventArgs(((OnInstrumentDefinition)event_0).InstrumentDefinition));
                            return;
                        }
                        break;
                    case 132:
                        if (this.instrumentDefinitionEndEventHandler_0 != null)
                        {
                            this.instrumentDefinitionEndEventHandler_0(this, new InstrumentDefinitionEndEventArgs(((OnInstrumentDefinitionEnd)event_0).InstrumentDefinitionEnd));
                            return;
                        }
                        break;
                    case 133:
                    case 134:
                    case 140:
                    case 141:
                    case 143:
                        break;
                    case 135:
                        if (this.portfolioEventHandler_0 != null)
                        {
                            this.portfolioEventHandler_0(this, new PortfolioEventArgs(((OnPortfolioAdded)event_0).Portfolio));
                            return;
                        }
                        break;
                    case 136:
                        if (this.WixrwvmkIM != null)
                        {
                            this.WixrwvmkIM(this, new PortfolioEventArgs(((OnPortfolioRemoved)event_0).Portfolio));
                            return;
                        }
                        break;
                    case 137:
                        if (this.portfolioEventHandler_1 != null)
                        {
                            this.portfolioEventHandler_1(this, new PortfolioEventArgs(((OnPortfolioParentChanged)event_0).Portfolio));
                            return;
                        }
                        break;
                    case 138:
                        if (this.historicalDataEventHandler_0 != null)
                        {
                            this.historicalDataEventHandler_0(this, new HistoricalDataEventArgs((HistoricalData)event_0));
                            return;
                        }
                        break;
                    case 139:
                        if (this.historicalDataEndEventHandler_0 != null)
                        {
                            this.historicalDataEndEventHandler_0(this, new HistoricalDataEndEventArgs((HistoricalDataEnd)event_0));
                            return;
                        }
                        break;
                    case 142:
                        if (this.accountDataEventHandler_0 != null)
                        {
                            this.accountDataEventHandler_0(this, new AccountDataEventArgs((AccountData)event_0));
                            return;
                        }
                        break;
                    case 144:
                        if (this.strategyEventHandler_0 != null)
                        {
                            this.strategyEventHandler_0(this, new StrategyEventArgs(((OnStrategyAdded)event_0).Strategy));
                            return;
                        }
                        break;
                    default:
                        if (typeId != 162)
                        {
                            switch (typeId)
                            {
                                case 207:
                                    if (this.eventHandler_1 != null)
                                    {
                                        this.eventHandler_1(this, EventArgs.Empty);
                                        return;
                                    }
                                    break;
                                case 208:
                                    if (this.eventHandler_2 != null)
                                    {
                                        this.eventHandler_2(this, EventArgs.Empty);
                                        return;
                                    }
                                    break;
                                case 209:
                                    if (this.eventHandler_3 != null)
                                    {
                                        this.eventHandler_3(this, EventArgs.Empty);
                                        return;
                                    }
                                    break;
                                case 210:
                                    if (this.eventHandler_4 != null)
                                    {
                                        this.eventHandler_4(this, EventArgs.Empty);
                                        return;
                                    }
                                    break;
                                case 211:
                                    if (this.eventHandler_5 != null)
                                    {
                                        this.eventHandler_5(this, EventArgs.Empty);
                                    }
                                    break;
                                case 212:
                                case 213:
                                case 214:
                                case 215:
                                case 216:
                                case 217:
                                case 218:
                                case 219:
                                case 220:
                                case 228:
                                    break;
                                case 221:
                                    if (this.instrumentEventHandler_0 != null)
                                    {
                                        this.instrumentEventHandler_0(this, new InstrumentEventArgs(((OnInstrumentAdded)event_0).Instrument));
                                        return;
                                    }
                                    break;
                                case 222:
                                    if (this.instrumentEventHandler_1 != null)
                                    {
                                        this.instrumentEventHandler_1(this, new InstrumentEventArgs(((OnInstrumentDeleted)event_0).Instrument));
                                        return;
                                    }
                                    break;
                                case 223:
                                    if (this.providerEventHandler_0 != null)
                                    {
                                        this.providerEventHandler_0(this, new ProviderEventArgs(((OnProviderAdded)event_0).Provider));
                                        return;
                                    }
                                    break;
                                case 224:
                                    if (this.TqQiEpesSY != null)
                                    {
                                        this.TqQiEpesSY(this, new ProviderEventArgs(((OnProviderRemoved)event_0).Provider));
                                        return;
                                    }
                                    break;
                                case 225:
                                    if (this.jMmiArAlGk != null)
                                    {
                                        this.jMmiArAlGk(this, new ProviderEventArgs(((OnProviderConnected)event_0).Provider));
                                        return;
                                    }
                                    break;
                                case 226:
                                    if (this.providerEventHandler_2 != null)
                                    {
                                        this.providerEventHandler_2(this, new ProviderEventArgs(((OnProviderDisconnected)event_0).Provider));
                                        return;
                                    }
                                    break;
                                case 227:
                                    if (this.providerEventHandler_1 != null)
                                    {
                                        this.providerEventHandler_1(this, new ProviderEventArgs(((OnProviderStatusChanged)event_0).Provider));
                                        return;
                                    }
                                    break;
                                case 229:
                                    if (this.eventHandler_0 != null)
                                    {
                                        this.eventHandler_0(this, EventArgs.Empty);
                                        return;
                                    }
                                    break;
                                case 230:
                                    if (this.simulatorProgressEventHandler_0 != null)
                                    {
                                        this.simulatorProgressEventHandler_0(this, new SimulatorProgressEventArgs(((OnSimulatorProgress)event_0).Count, ((OnSimulatorProgress)event_0).Percent));
                                        return;
                                    }
                                    break;
                                default:
                                    return;
                            }
                        }
                        else
                        {
                            this.permanentQueue_0.Enqueue(event_0);
                            if (this.accountReportEventHandler_0 != null)
                            {
                                this.accountReportEventHandler_0(this, (AccountReport)event_0);
                                return;
                            }
                        }
                        break;
                }
            }
        }

        public virtual void OnEvent(Event e)
        {
            if (this.framework.Mode != FrameworkMode.Simulation)
                this.eventQueue_1?.Enqueue(e);
            else
                this.method_1(e);
        }

        public void Remove(IEventClient client)
        {
            this.idArray_0[client.Id] = client;
            byte[] eventTypes = client.EventTypes;
            for (int i = 0; i < eventTypes.Length; i++)
            {
                byte id = eventTypes[i];
                this.idArray_1[(int)id].Remove(client);
            }
            this.permanentQueue_0.RemoveReader(client);
        }

        public EventController Controller
        {
            get
            {
                return this.eventController_0;
            }
            set
            {
                this.eventController_0 = value;
            }
        }

        public PermanentQueue<Event> PermanentQueue
        {
            get
            {
                return this.permanentQueue_0;
            }
        }

        public event AccountDataEventHandler AccountData
        {
            add
            {
                AccountDataEventHandler accountDataEventHandler = this.accountDataEventHandler_0;
                AccountDataEventHandler accountDataEventHandler2;
                do
                {
                    accountDataEventHandler2 = accountDataEventHandler;
                    AccountDataEventHandler value2 = (AccountDataEventHandler)Delegate.Combine(accountDataEventHandler2, value);
                    accountDataEventHandler = Interlocked.CompareExchange<AccountDataEventHandler>(ref this.accountDataEventHandler_0, value2, accountDataEventHandler2);
                }
                while (accountDataEventHandler != accountDataEventHandler2);
            }
            remove
            {
                AccountDataEventHandler accountDataEventHandler = this.accountDataEventHandler_0;
                AccountDataEventHandler accountDataEventHandler2;
                do
                {
                    accountDataEventHandler2 = accountDataEventHandler;
                    AccountDataEventHandler value2 = (AccountDataEventHandler)Delegate.Remove(accountDataEventHandler2, value);
                    accountDataEventHandler = Interlocked.CompareExchange<AccountDataEventHandler>(ref this.accountDataEventHandler_0, value2, accountDataEventHandler2);
                }
                while (accountDataEventHandler != accountDataEventHandler2);
            }
        }

        public event AccountReportEventHandler AccountReport
        {
            add
            {
                AccountReportEventHandler accountReportEventHandler = this.accountReportEventHandler_0;
                AccountReportEventHandler accountReportEventHandler2;
                do
                {
                    accountReportEventHandler2 = accountReportEventHandler;
                    AccountReportEventHandler value2 = (AccountReportEventHandler)Delegate.Combine(accountReportEventHandler2, value);
                    accountReportEventHandler = Interlocked.CompareExchange<AccountReportEventHandler>(ref this.accountReportEventHandler_0, value2, accountReportEventHandler2);
                }
                while (accountReportEventHandler != accountReportEventHandler2);
            }
            remove
            {
                AccountReportEventHandler accountReportEventHandler = this.accountReportEventHandler_0;
                AccountReportEventHandler accountReportEventHandler2;
                do
                {
                    accountReportEventHandler2 = accountReportEventHandler;
                    AccountReportEventHandler value2 = (AccountReportEventHandler)Delegate.Remove(accountReportEventHandler2, value);
                    accountReportEventHandler = Interlocked.CompareExchange<AccountReportEventHandler>(ref this.accountReportEventHandler_0, value2, accountReportEventHandler2);
                }
                while (accountReportEventHandler != accountReportEventHandler2);
            }
        }

        public event AskEventHandler Ask
        {
            add
            {
                AskEventHandler askEventHandler = this.askEventHandler_0;
                AskEventHandler askEventHandler2;
                do
                {
                    askEventHandler2 = askEventHandler;
                    AskEventHandler value2 = (AskEventHandler)Delegate.Combine(askEventHandler2, value);
                    askEventHandler = Interlocked.CompareExchange<AskEventHandler>(ref this.askEventHandler_0, value2, askEventHandler2);
                }
                while (askEventHandler != askEventHandler2);
            }
            remove
            {
                AskEventHandler askEventHandler = this.askEventHandler_0;
                AskEventHandler askEventHandler2;
                do
                {
                    askEventHandler2 = askEventHandler;
                    AskEventHandler value2 = (AskEventHandler)Delegate.Remove(askEventHandler2, value);
                    askEventHandler = Interlocked.CompareExchange<AskEventHandler>(ref this.askEventHandler_0, value2, askEventHandler2);
                }
                while (askEventHandler != askEventHandler2);
            }
        }

        public event BarEventHandler Bar
        {
            add
            {
                BarEventHandler barEventHandler = this.barEventHandler_0;
                BarEventHandler barEventHandler2;
                do
                {
                    barEventHandler2 = barEventHandler;
                    BarEventHandler value2 = (BarEventHandler)Delegate.Combine(barEventHandler2, value);
                    barEventHandler = Interlocked.CompareExchange<BarEventHandler>(ref this.barEventHandler_0, value2, barEventHandler2);
                }
                while (barEventHandler != barEventHandler2);
            }
            remove
            {
                BarEventHandler barEventHandler = this.barEventHandler_0;
                BarEventHandler barEventHandler2;
                do
                {
                    barEventHandler2 = barEventHandler;
                    BarEventHandler value2 = (BarEventHandler)Delegate.Remove(barEventHandler2, value);
                    barEventHandler = Interlocked.CompareExchange<BarEventHandler>(ref this.barEventHandler_0, value2, barEventHandler2);
                }
                while (barEventHandler != barEventHandler2);
            }
        }

        public event BarEventHandler BarOpen
        {
            add
            {
                BarEventHandler barEventHandler = this.barEventHandler_1;
                BarEventHandler barEventHandler2;
                do
                {
                    barEventHandler2 = barEventHandler;
                    BarEventHandler value2 = (BarEventHandler)Delegate.Combine(barEventHandler2, value);
                    barEventHandler = Interlocked.CompareExchange<BarEventHandler>(ref this.barEventHandler_1, value2, barEventHandler2);
                }
                while (barEventHandler != barEventHandler2);
            }
            remove
            {
                BarEventHandler barEventHandler = this.barEventHandler_1;
                BarEventHandler barEventHandler2;
                do
                {
                    barEventHandler2 = barEventHandler;
                    BarEventHandler value2 = (BarEventHandler)Delegate.Remove(barEventHandler2, value);
                    barEventHandler = Interlocked.CompareExchange<BarEventHandler>(ref this.barEventHandler_1, value2, barEventHandler2);
                }
                while (barEventHandler != barEventHandler2);
            }
        }

        public event BidEventHandler Bid
        {
            add
            {
                BidEventHandler bidEventHandler = this.bidEventHandler_0;
                BidEventHandler bidEventHandler2;
                do
                {
                    bidEventHandler2 = bidEventHandler;
                    BidEventHandler value2 = (BidEventHandler)Delegate.Combine(bidEventHandler2, value);
                    bidEventHandler = Interlocked.CompareExchange<BidEventHandler>(ref this.bidEventHandler_0, value2, bidEventHandler2);
                }
                while (bidEventHandler != bidEventHandler2);
            }
            remove
            {
                BidEventHandler bidEventHandler = this.bidEventHandler_0;
                BidEventHandler bidEventHandler2;
                do
                {
                    bidEventHandler2 = bidEventHandler;
                    BidEventHandler value2 = (BidEventHandler)Delegate.Remove(bidEventHandler2, value);
                    bidEventHandler = Interlocked.CompareExchange<BidEventHandler>(ref this.bidEventHandler_0, value2, bidEventHandler2);
                }
                while (bidEventHandler != bidEventHandler2);
            }
        }

        public event CommandEventHandler Command
        {
            add
            {
                CommandEventHandler commandEventHandler = this.commandEventHandler_0;
                CommandEventHandler commandEventHandler2;
                do
                {
                    commandEventHandler2 = commandEventHandler;
                    CommandEventHandler value2 = (CommandEventHandler)Delegate.Combine(commandEventHandler2, value);
                    commandEventHandler = Interlocked.CompareExchange<CommandEventHandler>(ref this.commandEventHandler_0, value2, commandEventHandler2);
                }
                while (commandEventHandler != commandEventHandler2);
            }
            remove
            {
                CommandEventHandler commandEventHandler = this.commandEventHandler_0;
                CommandEventHandler commandEventHandler2;
                do
                {
                    commandEventHandler2 = commandEventHandler;
                    CommandEventHandler value2 = (CommandEventHandler)Delegate.Remove(commandEventHandler2, value);
                    commandEventHandler = Interlocked.CompareExchange<CommandEventHandler>(ref this.commandEventHandler_0, value2, commandEventHandler2);
                }
                while (commandEventHandler != commandEventHandler2);
            }
        }

        public event EventHandler EventManagerPaused
        {
            add
            {
                EventHandler eventHandler = this.eventHandler_3;
                EventHandler eventHandler2;
                do
                {
                    eventHandler2 = eventHandler;
                    EventHandler value2 = (EventHandler)Delegate.Combine(eventHandler2, value);
                    eventHandler = Interlocked.CompareExchange<EventHandler>(ref this.eventHandler_3, value2, eventHandler2);
                }
                while (eventHandler != eventHandler2);
            }
            remove
            {
                EventHandler eventHandler = this.eventHandler_3;
                EventHandler eventHandler2;
                do
                {
                    eventHandler2 = eventHandler;
                    EventHandler value2 = (EventHandler)Delegate.Remove(eventHandler2, value);
                    eventHandler = Interlocked.CompareExchange<EventHandler>(ref this.eventHandler_3, value2, eventHandler2);
                }
                while (eventHandler != eventHandler2);
            }
        }

        public event EventHandler EventManagerResumed
        {
            add
            {
                EventHandler eventHandler = this.eventHandler_4;
                EventHandler eventHandler2;
                do
                {
                    eventHandler2 = eventHandler;
                    EventHandler value2 = (EventHandler)Delegate.Combine(eventHandler2, value);
                    eventHandler = Interlocked.CompareExchange<EventHandler>(ref this.eventHandler_4, value2, eventHandler2);
                }
                while (eventHandler != eventHandler2);
            }
            remove
            {
                EventHandler eventHandler = this.eventHandler_4;
                EventHandler eventHandler2;
                do
                {
                    eventHandler2 = eventHandler;
                    EventHandler value2 = (EventHandler)Delegate.Remove(eventHandler2, value);
                    eventHandler = Interlocked.CompareExchange<EventHandler>(ref this.eventHandler_4, value2, eventHandler2);
                }
                while (eventHandler != eventHandler2);
            }
        }

        public event EventHandler EventManagerStarted
        {
            add
            {
                EventHandler eventHandler = this.eventHandler_1;
                EventHandler eventHandler2;
                do
                {
                    eventHandler2 = eventHandler;
                    EventHandler value2 = (EventHandler)Delegate.Combine(eventHandler2, value);
                    eventHandler = Interlocked.CompareExchange<EventHandler>(ref this.eventHandler_1, value2, eventHandler2);
                }
                while (eventHandler != eventHandler2);
            }
            remove
            {
                EventHandler eventHandler = this.eventHandler_1;
                EventHandler eventHandler2;
                do
                {
                    eventHandler2 = eventHandler;
                    EventHandler value2 = (EventHandler)Delegate.Remove(eventHandler2, value);
                    eventHandler = Interlocked.CompareExchange<EventHandler>(ref this.eventHandler_1, value2, eventHandler2);
                }
                while (eventHandler != eventHandler2);
            }
        }

        public event EventHandler EventManagerStep
        {
            add
            {
                EventHandler eventHandler = this.eventHandler_5;
                EventHandler eventHandler2;
                do
                {
                    eventHandler2 = eventHandler;
                    EventHandler value2 = (EventHandler)Delegate.Combine(eventHandler2, value);
                    eventHandler = Interlocked.CompareExchange<EventHandler>(ref this.eventHandler_5, value2, eventHandler2);
                }
                while (eventHandler != eventHandler2);
            }
            remove
            {
                EventHandler eventHandler = this.eventHandler_5;
                EventHandler eventHandler2;
                do
                {
                    eventHandler2 = eventHandler;
                    EventHandler value2 = (EventHandler)Delegate.Remove(eventHandler2, value);
                    eventHandler = Interlocked.CompareExchange<EventHandler>(ref this.eventHandler_5, value2, eventHandler2);
                }
                while (eventHandler != eventHandler2);
            }
        }

        public event EventHandler EventManagerStopped
        {
            add
            {
                EventHandler eventHandler = this.eventHandler_2;
                EventHandler eventHandler2;
                do
                {
                    eventHandler2 = eventHandler;
                    EventHandler value2 = (EventHandler)Delegate.Combine(eventHandler2, value);
                    eventHandler = Interlocked.CompareExchange<EventHandler>(ref this.eventHandler_2, value2, eventHandler2);
                }
                while (eventHandler != eventHandler2);
            }
            remove
            {
                EventHandler eventHandler = this.eventHandler_2;
                EventHandler eventHandler2;
                do
                {
                    eventHandler2 = eventHandler;
                    EventHandler value2 = (EventHandler)Delegate.Remove(eventHandler2, value);
                    eventHandler = Interlocked.CompareExchange<EventHandler>(ref this.eventHandler_2, value2, eventHandler2);
                }
                while (eventHandler != eventHandler2);
            }
        }

        public event ExecutionCommandEventHandler ExecutionCommand
        {
            add
            {
                ExecutionCommandEventHandler executionCommandEventHandler = this.executionCommandEventHandler_0;
                ExecutionCommandEventHandler executionCommandEventHandler2;
                do
                {
                    executionCommandEventHandler2 = executionCommandEventHandler;
                    ExecutionCommandEventHandler value2 = (ExecutionCommandEventHandler)Delegate.Combine(executionCommandEventHandler2, value);
                    executionCommandEventHandler = Interlocked.CompareExchange<ExecutionCommandEventHandler>(ref this.executionCommandEventHandler_0, value2, executionCommandEventHandler2);
                }
                while (executionCommandEventHandler != executionCommandEventHandler2);
            }
            remove
            {
                ExecutionCommandEventHandler executionCommandEventHandler = this.executionCommandEventHandler_0;
                ExecutionCommandEventHandler executionCommandEventHandler2;
                do
                {
                    executionCommandEventHandler2 = executionCommandEventHandler;
                    ExecutionCommandEventHandler value2 = (ExecutionCommandEventHandler)Delegate.Remove(executionCommandEventHandler2, value);
                    executionCommandEventHandler = Interlocked.CompareExchange<ExecutionCommandEventHandler>(ref this.executionCommandEventHandler_0, value2, executionCommandEventHandler2);
                }
                while (executionCommandEventHandler != executionCommandEventHandler2);
            }
        }

        public event ExecutionReportEventHandler ExecutionReport
        {
            add
            {
                ExecutionReportEventHandler executionReportEventHandler = this.executionReportEventHandler_0;
                ExecutionReportEventHandler executionReportEventHandler2;
                do
                {
                    executionReportEventHandler2 = executionReportEventHandler;
                    ExecutionReportEventHandler value2 = (ExecutionReportEventHandler)Delegate.Combine(executionReportEventHandler2, value);
                    executionReportEventHandler = Interlocked.CompareExchange<ExecutionReportEventHandler>(ref this.executionReportEventHandler_0, value2, executionReportEventHandler2);
                }
                while (executionReportEventHandler != executionReportEventHandler2);
            }
            remove
            {
                ExecutionReportEventHandler executionReportEventHandler = this.executionReportEventHandler_0;
                ExecutionReportEventHandler executionReportEventHandler2;
                do
                {
                    executionReportEventHandler2 = executionReportEventHandler;
                    ExecutionReportEventHandler value2 = (ExecutionReportEventHandler)Delegate.Remove(executionReportEventHandler2, value);
                    executionReportEventHandler = Interlocked.CompareExchange<ExecutionReportEventHandler>(ref this.executionReportEventHandler_0, value2, executionReportEventHandler2);
                }
                while (executionReportEventHandler != executionReportEventHandler2);
            }
        }

        public event FillEventHandler Fill
        {
            add
            {
                FillEventHandler fillEventHandler = this.fillEventHandler_0;
                FillEventHandler fillEventHandler2;
                do
                {
                    fillEventHandler2 = fillEventHandler;
                    FillEventHandler value2 = (FillEventHandler)Delegate.Combine(fillEventHandler2, value);
                    fillEventHandler = Interlocked.CompareExchange<FillEventHandler>(ref this.fillEventHandler_0, value2, fillEventHandler2);
                }
                while (fillEventHandler != fillEventHandler2);
            }
            remove
            {
                FillEventHandler fillEventHandler = this.fillEventHandler_0;
                FillEventHandler fillEventHandler2;
                do
                {
                    fillEventHandler2 = fillEventHandler;
                    FillEventHandler value2 = (FillEventHandler)Delegate.Remove(fillEventHandler2, value);
                    fillEventHandler = Interlocked.CompareExchange<FillEventHandler>(ref this.fillEventHandler_0, value2, fillEventHandler2);
                }
                while (fillEventHandler != fillEventHandler2);
            }
        }

        public event FrameworkEventHandler FrameworkCleared;

        public event HistoricalDataEventHandler HistoricalData
        {
            add
            {
                HistoricalDataEventHandler historicalDataEventHandler = this.historicalDataEventHandler_0;
                HistoricalDataEventHandler historicalDataEventHandler2;
                do
                {
                    historicalDataEventHandler2 = historicalDataEventHandler;
                    HistoricalDataEventHandler value2 = (HistoricalDataEventHandler)Delegate.Combine(historicalDataEventHandler2, value);
                    historicalDataEventHandler = Interlocked.CompareExchange<HistoricalDataEventHandler>(ref this.historicalDataEventHandler_0, value2, historicalDataEventHandler2);
                }
                while (historicalDataEventHandler != historicalDataEventHandler2);
            }
            remove
            {
                HistoricalDataEventHandler historicalDataEventHandler = this.historicalDataEventHandler_0;
                HistoricalDataEventHandler historicalDataEventHandler2;
                do
                {
                    historicalDataEventHandler2 = historicalDataEventHandler;
                    HistoricalDataEventHandler value2 = (HistoricalDataEventHandler)Delegate.Remove(historicalDataEventHandler2, value);
                    historicalDataEventHandler = Interlocked.CompareExchange<HistoricalDataEventHandler>(ref this.historicalDataEventHandler_0, value2, historicalDataEventHandler2);
                }
                while (historicalDataEventHandler != historicalDataEventHandler2);
            }
        }

        public event HistoricalDataEndEventHandler HistoricalDataEnd
        {
            add
            {
                HistoricalDataEndEventHandler historicalDataEndEventHandler = this.historicalDataEndEventHandler_0;
                HistoricalDataEndEventHandler historicalDataEndEventHandler2;
                do
                {
                    historicalDataEndEventHandler2 = historicalDataEndEventHandler;
                    HistoricalDataEndEventHandler value2 = (HistoricalDataEndEventHandler)Delegate.Combine(historicalDataEndEventHandler2, value);
                    historicalDataEndEventHandler = Interlocked.CompareExchange<HistoricalDataEndEventHandler>(ref this.historicalDataEndEventHandler_0, value2, historicalDataEndEventHandler2);
                }
                while (historicalDataEndEventHandler != historicalDataEndEventHandler2);
            }
            remove
            {
                HistoricalDataEndEventHandler historicalDataEndEventHandler = this.historicalDataEndEventHandler_0;
                HistoricalDataEndEventHandler historicalDataEndEventHandler2;
                do
                {
                    historicalDataEndEventHandler2 = historicalDataEndEventHandler;
                    HistoricalDataEndEventHandler value2 = (HistoricalDataEndEventHandler)Delegate.Remove(historicalDataEndEventHandler2, value);
                    historicalDataEndEventHandler = Interlocked.CompareExchange<HistoricalDataEndEventHandler>(ref this.historicalDataEndEventHandler_0, value2, historicalDataEndEventHandler2);
                }
                while (historicalDataEndEventHandler != historicalDataEndEventHandler2);
            }
        }

        public event InstrumentEventHandler InstrumentAdded
        {
            add
            {
                InstrumentEventHandler instrumentEventHandler = this.instrumentEventHandler_0;
                InstrumentEventHandler instrumentEventHandler2;
                do
                {
                    instrumentEventHandler2 = instrumentEventHandler;
                    InstrumentEventHandler value2 = (InstrumentEventHandler)Delegate.Combine(instrumentEventHandler2, value);
                    instrumentEventHandler = Interlocked.CompareExchange<InstrumentEventHandler>(ref this.instrumentEventHandler_0, value2, instrumentEventHandler2);
                }
                while (instrumentEventHandler != instrumentEventHandler2);
            }
            remove
            {
                InstrumentEventHandler instrumentEventHandler = this.instrumentEventHandler_0;
                InstrumentEventHandler instrumentEventHandler2;
                do
                {
                    instrumentEventHandler2 = instrumentEventHandler;
                    InstrumentEventHandler value2 = (InstrumentEventHandler)Delegate.Remove(instrumentEventHandler2, value);
                    instrumentEventHandler = Interlocked.CompareExchange<InstrumentEventHandler>(ref this.instrumentEventHandler_0, value2, instrumentEventHandler2);
                }
                while (instrumentEventHandler != instrumentEventHandler2);
            }
        }

        public event InstrumentDefinitionEventHandler InstrumentDefinition
        {
            add
            {
                InstrumentDefinitionEventHandler instrumentDefinitionEventHandler = this.instrumentDefinitionEventHandler_0;
                InstrumentDefinitionEventHandler instrumentDefinitionEventHandler2;
                do
                {
                    instrumentDefinitionEventHandler2 = instrumentDefinitionEventHandler;
                    InstrumentDefinitionEventHandler value2 = (InstrumentDefinitionEventHandler)Delegate.Combine(instrumentDefinitionEventHandler2, value);
                    instrumentDefinitionEventHandler = Interlocked.CompareExchange<InstrumentDefinitionEventHandler>(ref this.instrumentDefinitionEventHandler_0, value2, instrumentDefinitionEventHandler2);
                }
                while (instrumentDefinitionEventHandler != instrumentDefinitionEventHandler2);
            }
            remove
            {
                InstrumentDefinitionEventHandler instrumentDefinitionEventHandler = this.instrumentDefinitionEventHandler_0;
                InstrumentDefinitionEventHandler instrumentDefinitionEventHandler2;
                do
                {
                    instrumentDefinitionEventHandler2 = instrumentDefinitionEventHandler;
                    InstrumentDefinitionEventHandler value2 = (InstrumentDefinitionEventHandler)Delegate.Remove(instrumentDefinitionEventHandler2, value);
                    instrumentDefinitionEventHandler = Interlocked.CompareExchange<InstrumentDefinitionEventHandler>(ref this.instrumentDefinitionEventHandler_0, value2, instrumentDefinitionEventHandler2);
                }
                while (instrumentDefinitionEventHandler != instrumentDefinitionEventHandler2);
            }
        }

        public event InstrumentDefinitionEndEventHandler InstrumentDefinitionEnd
        {
            add
            {
                InstrumentDefinitionEndEventHandler instrumentDefinitionEndEventHandler = this.instrumentDefinitionEndEventHandler_0;
                InstrumentDefinitionEndEventHandler instrumentDefinitionEndEventHandler2;
                do
                {
                    instrumentDefinitionEndEventHandler2 = instrumentDefinitionEndEventHandler;
                    InstrumentDefinitionEndEventHandler value2 = (InstrumentDefinitionEndEventHandler)Delegate.Combine(instrumentDefinitionEndEventHandler2, value);
                    instrumentDefinitionEndEventHandler = Interlocked.CompareExchange<InstrumentDefinitionEndEventHandler>(ref this.instrumentDefinitionEndEventHandler_0, value2, instrumentDefinitionEndEventHandler2);
                }
                while (instrumentDefinitionEndEventHandler != instrumentDefinitionEndEventHandler2);
            }
            remove
            {
                InstrumentDefinitionEndEventHandler instrumentDefinitionEndEventHandler = this.instrumentDefinitionEndEventHandler_0;
                InstrumentDefinitionEndEventHandler instrumentDefinitionEndEventHandler2;
                do
                {
                    instrumentDefinitionEndEventHandler2 = instrumentDefinitionEndEventHandler;
                    InstrumentDefinitionEndEventHandler value2 = (InstrumentDefinitionEndEventHandler)Delegate.Remove(instrumentDefinitionEndEventHandler2, value);
                    instrumentDefinitionEndEventHandler = Interlocked.CompareExchange<InstrumentDefinitionEndEventHandler>(ref this.instrumentDefinitionEndEventHandler_0, value2, instrumentDefinitionEndEventHandler2);
                }
                while (instrumentDefinitionEndEventHandler != instrumentDefinitionEndEventHandler2);
            }
        }

        public event InstrumentEventHandler InstrumentDeleted
        {
            add
            {
                InstrumentEventHandler instrumentEventHandler = this.instrumentEventHandler_1;
                InstrumentEventHandler instrumentEventHandler2;
                do
                {
                    instrumentEventHandler2 = instrumentEventHandler;
                    InstrumentEventHandler value2 = (InstrumentEventHandler)Delegate.Combine(instrumentEventHandler2, value);
                    instrumentEventHandler = Interlocked.CompareExchange<InstrumentEventHandler>(ref this.instrumentEventHandler_1, value2, instrumentEventHandler2);
                }
                while (instrumentEventHandler != instrumentEventHandler2);
            }
            remove
            {
                InstrumentEventHandler instrumentEventHandler = this.instrumentEventHandler_1;
                InstrumentEventHandler instrumentEventHandler2;
                do
                {
                    instrumentEventHandler2 = instrumentEventHandler;
                    InstrumentEventHandler value2 = (InstrumentEventHandler)Delegate.Remove(instrumentEventHandler2, value);
                    instrumentEventHandler = Interlocked.CompareExchange<InstrumentEventHandler>(ref this.instrumentEventHandler_1, value2, instrumentEventHandler2);
                }
                while (instrumentEventHandler != instrumentEventHandler2);
            }
        }

        public event GroupEventHandler NewGroup
        {
            add
            {
                GroupEventHandler groupEventHandler = this.groupEventHandler_0;
                GroupEventHandler groupEventHandler2;
                do
                {
                    groupEventHandler2 = groupEventHandler;
                    GroupEventHandler value2 = (GroupEventHandler)Delegate.Combine(groupEventHandler2, value);
                    groupEventHandler = Interlocked.CompareExchange<GroupEventHandler>(ref this.groupEventHandler_0, value2, groupEventHandler2);
                }
                while (groupEventHandler != groupEventHandler2);
            }
            remove
            {
                GroupEventHandler groupEventHandler = this.groupEventHandler_0;
                GroupEventHandler groupEventHandler2;
                do
                {
                    groupEventHandler2 = groupEventHandler;
                    GroupEventHandler value2 = (GroupEventHandler)Delegate.Remove(groupEventHandler2, value);
                    groupEventHandler = Interlocked.CompareExchange<GroupEventHandler>(ref this.groupEventHandler_0, value2, groupEventHandler2);
                }
                while (groupEventHandler != groupEventHandler2);
            }
        }

        public event GroupEventEventHandler NewGroupEvent
        {
            add
            {
                GroupEventEventHandler groupEventEventHandler = this.groupEventEventHandler_0;
                GroupEventEventHandler groupEventEventHandler2;
                do
                {
                    groupEventEventHandler2 = groupEventEventHandler;
                    GroupEventEventHandler value2 = (GroupEventEventHandler)Delegate.Combine(groupEventEventHandler2, value);
                    groupEventEventHandler = Interlocked.CompareExchange<GroupEventEventHandler>(ref this.groupEventEventHandler_0, value2, groupEventEventHandler2);
                }
                while (groupEventEventHandler != groupEventEventHandler2);
            }
            remove
            {
                GroupEventEventHandler groupEventEventHandler = this.groupEventEventHandler_0;
                GroupEventEventHandler groupEventEventHandler2;
                do
                {
                    groupEventEventHandler2 = groupEventEventHandler;
                    GroupEventEventHandler value2 = (GroupEventEventHandler)Delegate.Remove(groupEventEventHandler2, value);
                    groupEventEventHandler = Interlocked.CompareExchange<GroupEventEventHandler>(ref this.groupEventEventHandler_0, value2, groupEventEventHandler2);
                }
                while (groupEventEventHandler != groupEventEventHandler2);
            }
        }

        public event GroupUpdateEventHandler NewGroupUpdate
        {
            add
            {
                GroupUpdateEventHandler groupUpdateEventHandler = this.groupUpdateEventHandler_0;
                GroupUpdateEventHandler groupUpdateEventHandler2;
                do
                {
                    groupUpdateEventHandler2 = groupUpdateEventHandler;
                    GroupUpdateEventHandler value2 = (GroupUpdateEventHandler)Delegate.Combine(groupUpdateEventHandler2, value);
                    groupUpdateEventHandler = Interlocked.CompareExchange<GroupUpdateEventHandler>(ref this.groupUpdateEventHandler_0, value2, groupUpdateEventHandler2);
                }
                while (groupUpdateEventHandler != groupUpdateEventHandler2);
            }
            remove
            {
                GroupUpdateEventHandler groupUpdateEventHandler = this.groupUpdateEventHandler_0;
                GroupUpdateEventHandler groupUpdateEventHandler2;
                do
                {
                    groupUpdateEventHandler2 = groupUpdateEventHandler;
                    GroupUpdateEventHandler value2 = (GroupUpdateEventHandler)Delegate.Remove(groupUpdateEventHandler2, value);
                    groupUpdateEventHandler = Interlocked.CompareExchange<GroupUpdateEventHandler>(ref this.groupUpdateEventHandler_0, value2, groupUpdateEventHandler2);
                }
                while (groupUpdateEventHandler != groupUpdateEventHandler2);
            }
        }

        public event OrderManagerClearedEventHandler OrderManagerCleared
        {
            add
            {
                OrderManagerClearedEventHandler orderManagerClearedEventHandler = this.orderManagerClearedEventHandler_0;
                OrderManagerClearedEventHandler orderManagerClearedEventHandler2;
                do
                {
                    orderManagerClearedEventHandler2 = orderManagerClearedEventHandler;
                    OrderManagerClearedEventHandler value2 = (OrderManagerClearedEventHandler)Delegate.Combine(orderManagerClearedEventHandler2, value);
                    orderManagerClearedEventHandler = Interlocked.CompareExchange<OrderManagerClearedEventHandler>(ref this.orderManagerClearedEventHandler_0, value2, orderManagerClearedEventHandler2);
                }
                while (orderManagerClearedEventHandler != orderManagerClearedEventHandler2);
            }
            remove
            {
                OrderManagerClearedEventHandler orderManagerClearedEventHandler = this.orderManagerClearedEventHandler_0;
                OrderManagerClearedEventHandler orderManagerClearedEventHandler2;
                do
                {
                    orderManagerClearedEventHandler2 = orderManagerClearedEventHandler;
                    OrderManagerClearedEventHandler value2 = (OrderManagerClearedEventHandler)Delegate.Remove(orderManagerClearedEventHandler2, value);
                    orderManagerClearedEventHandler = Interlocked.CompareExchange<OrderManagerClearedEventHandler>(ref this.orderManagerClearedEventHandler_0, value2, orderManagerClearedEventHandler2);
                }
                while (orderManagerClearedEventHandler != orderManagerClearedEventHandler2);
            }
        }

        public event OutputEventHandler Output
        {
            add
            {
                OutputEventHandler outputEventHandler = this.outputEventHandler_0;
                OutputEventHandler outputEventHandler2;
                do
                {
                    outputEventHandler2 = outputEventHandler;
                    OutputEventHandler value2 = (OutputEventHandler)Delegate.Combine(outputEventHandler2, value);
                    outputEventHandler = Interlocked.CompareExchange<OutputEventHandler>(ref this.outputEventHandler_0, value2, outputEventHandler2);
                }
                while (outputEventHandler != outputEventHandler2);
            }
            remove
            {
                OutputEventHandler outputEventHandler = this.outputEventHandler_0;
                OutputEventHandler outputEventHandler2;
                do
                {
                    outputEventHandler2 = outputEventHandler;
                    OutputEventHandler value2 = (OutputEventHandler)Delegate.Remove(outputEventHandler2, value);
                    outputEventHandler = Interlocked.CompareExchange<OutputEventHandler>(ref this.outputEventHandler_0, value2, outputEventHandler2);
                }
                while (outputEventHandler != outputEventHandler2);
            }
        }

        public event PortfolioEventHandler PortfolioAdded
        {
            add
            {
                PortfolioEventHandler portfolioEventHandler = this.portfolioEventHandler_0;
                PortfolioEventHandler portfolioEventHandler2;
                do
                {
                    portfolioEventHandler2 = portfolioEventHandler;
                    PortfolioEventHandler value2 = (PortfolioEventHandler)Delegate.Combine(portfolioEventHandler2, value);
                    portfolioEventHandler = Interlocked.CompareExchange<PortfolioEventHandler>(ref this.portfolioEventHandler_0, value2, portfolioEventHandler2);
                }
                while (portfolioEventHandler != portfolioEventHandler2);
            }
            remove
            {
                PortfolioEventHandler portfolioEventHandler = this.portfolioEventHandler_0;
                PortfolioEventHandler portfolioEventHandler2;
                do
                {
                    portfolioEventHandler2 = portfolioEventHandler;
                    PortfolioEventHandler value2 = (PortfolioEventHandler)Delegate.Remove(portfolioEventHandler2, value);
                    portfolioEventHandler = Interlocked.CompareExchange<PortfolioEventHandler>(ref this.portfolioEventHandler_0, value2, portfolioEventHandler2);
                }
                while (portfolioEventHandler != portfolioEventHandler2);
            }
        }

        public event PortfolioEventHandler PortfolioParentChanged
        {
            add
            {
                PortfolioEventHandler portfolioEventHandler = this.portfolioEventHandler_1;
                PortfolioEventHandler portfolioEventHandler2;
                do
                {
                    portfolioEventHandler2 = portfolioEventHandler;
                    PortfolioEventHandler value2 = (PortfolioEventHandler)Delegate.Combine(portfolioEventHandler2, value);
                    portfolioEventHandler = Interlocked.CompareExchange<PortfolioEventHandler>(ref this.portfolioEventHandler_1, value2, portfolioEventHandler2);
                }
                while (portfolioEventHandler != portfolioEventHandler2);
            }
            remove
            {
                PortfolioEventHandler portfolioEventHandler = this.portfolioEventHandler_1;
                PortfolioEventHandler portfolioEventHandler2;
                do
                {
                    portfolioEventHandler2 = portfolioEventHandler;
                    PortfolioEventHandler value2 = (PortfolioEventHandler)Delegate.Remove(portfolioEventHandler2, value);
                    portfolioEventHandler = Interlocked.CompareExchange<PortfolioEventHandler>(ref this.portfolioEventHandler_1, value2, portfolioEventHandler2);
                }
                while (portfolioEventHandler != portfolioEventHandler2);
            }
        }

        public event PortfolioEventHandler PortfolioRemoved
        {
            add
            {
                PortfolioEventHandler portfolioEventHandler = this.WixrwvmkIM;
                PortfolioEventHandler portfolioEventHandler2;
                do
                {
                    portfolioEventHandler2 = portfolioEventHandler;
                    PortfolioEventHandler value2 = (PortfolioEventHandler)Delegate.Combine(portfolioEventHandler2, value);
                    portfolioEventHandler = Interlocked.CompareExchange<PortfolioEventHandler>(ref this.WixrwvmkIM, value2, portfolioEventHandler2);
                }
                while (portfolioEventHandler != portfolioEventHandler2);
            }
            remove
            {
                PortfolioEventHandler portfolioEventHandler = this.WixrwvmkIM;
                PortfolioEventHandler portfolioEventHandler2;
                do
                {
                    portfolioEventHandler2 = portfolioEventHandler;
                    PortfolioEventHandler value2 = (PortfolioEventHandler)Delegate.Remove(portfolioEventHandler2, value);
                    portfolioEventHandler = Interlocked.CompareExchange<PortfolioEventHandler>(ref this.WixrwvmkIM, value2, portfolioEventHandler2);
                }
                while (portfolioEventHandler != portfolioEventHandler2);
            }
        }

        public event PositionEventHandler PositionChanged
        {
            add
            {
                PositionEventHandler positionEventHandler = this.positionEventHandler_0;
                PositionEventHandler positionEventHandler2;
                do
                {
                    positionEventHandler2 = positionEventHandler;
                    PositionEventHandler value2 = (PositionEventHandler)Delegate.Combine(positionEventHandler2, value);
                    positionEventHandler = Interlocked.CompareExchange<PositionEventHandler>(ref this.positionEventHandler_0, value2, positionEventHandler2);
                }
                while (positionEventHandler != positionEventHandler2);
            }
            remove
            {
                PositionEventHandler positionEventHandler = this.positionEventHandler_0;
                PositionEventHandler positionEventHandler2;
                do
                {
                    positionEventHandler2 = positionEventHandler;
                    PositionEventHandler value2 = (PositionEventHandler)Delegate.Remove(positionEventHandler2, value);
                    positionEventHandler = Interlocked.CompareExchange<PositionEventHandler>(ref this.positionEventHandler_0, value2, positionEventHandler2);
                }
                while (positionEventHandler != positionEventHandler2);
            }
        }

        public event PositionEventHandler PositionClosed
        {
            add
            {
                PositionEventHandler positionEventHandler = this.positionEventHandler_1;
                PositionEventHandler positionEventHandler2;
                do
                {
                    positionEventHandler2 = positionEventHandler;
                    PositionEventHandler value2 = (PositionEventHandler)Delegate.Combine(positionEventHandler2, value);
                    positionEventHandler = Interlocked.CompareExchange<PositionEventHandler>(ref this.positionEventHandler_1, value2, positionEventHandler2);
                }
                while (positionEventHandler != positionEventHandler2);
            }
            remove
            {
                PositionEventHandler positionEventHandler = this.positionEventHandler_1;
                PositionEventHandler positionEventHandler2;
                do
                {
                    positionEventHandler2 = positionEventHandler;
                    PositionEventHandler value2 = (PositionEventHandler)Delegate.Remove(positionEventHandler2, value);
                    positionEventHandler = Interlocked.CompareExchange<PositionEventHandler>(ref this.positionEventHandler_1, value2, positionEventHandler2);
                }
                while (positionEventHandler != positionEventHandler2);
            }
        }

        public event PositionEventHandler PositionOpened
        {
            add
            {
                PositionEventHandler positionEventHandler = this.ovQrTixtSx;
                PositionEventHandler positionEventHandler2;
                do
                {
                    positionEventHandler2 = positionEventHandler;
                    PositionEventHandler value2 = (PositionEventHandler)Delegate.Combine(positionEventHandler2, value);
                    positionEventHandler = Interlocked.CompareExchange<PositionEventHandler>(ref this.ovQrTixtSx, value2, positionEventHandler2);
                }
                while (positionEventHandler != positionEventHandler2);
            }
            remove
            {
                PositionEventHandler positionEventHandler = this.ovQrTixtSx;
                PositionEventHandler positionEventHandler2;
                do
                {
                    positionEventHandler2 = positionEventHandler;
                    PositionEventHandler value2 = (PositionEventHandler)Delegate.Remove(positionEventHandler2, value);
                    positionEventHandler = Interlocked.CompareExchange<PositionEventHandler>(ref this.ovQrTixtSx, value2, positionEventHandler2);
                }
                while (positionEventHandler != positionEventHandler2);
            }
        }

        public event ProviderEventHandler ProviderAdded
        {
            add
            {
                ProviderEventHandler providerEventHandler = this.providerEventHandler_0;
                ProviderEventHandler providerEventHandler2;
                do
                {
                    providerEventHandler2 = providerEventHandler;
                    ProviderEventHandler value2 = (ProviderEventHandler)Delegate.Combine(providerEventHandler2, value);
                    providerEventHandler = Interlocked.CompareExchange<ProviderEventHandler>(ref this.providerEventHandler_0, value2, providerEventHandler2);
                }
                while (providerEventHandler != providerEventHandler2);
            }
            remove
            {
                ProviderEventHandler providerEventHandler = this.providerEventHandler_0;
                ProviderEventHandler providerEventHandler2;
                do
                {
                    providerEventHandler2 = providerEventHandler;
                    ProviderEventHandler value2 = (ProviderEventHandler)Delegate.Remove(providerEventHandler2, value);
                    providerEventHandler = Interlocked.CompareExchange<ProviderEventHandler>(ref this.providerEventHandler_0, value2, providerEventHandler2);
                }
                while (providerEventHandler != providerEventHandler2);
            }
        }

        public event ProviderEventHandler ProviderConnected
        {
            add
            {
                ProviderEventHandler providerEventHandler = this.jMmiArAlGk;
                ProviderEventHandler providerEventHandler2;
                do
                {
                    providerEventHandler2 = providerEventHandler;
                    ProviderEventHandler value2 = (ProviderEventHandler)Delegate.Combine(providerEventHandler2, value);
                    providerEventHandler = Interlocked.CompareExchange<ProviderEventHandler>(ref this.jMmiArAlGk, value2, providerEventHandler2);
                }
                while (providerEventHandler != providerEventHandler2);
            }
            remove
            {
                ProviderEventHandler providerEventHandler = this.jMmiArAlGk;
                ProviderEventHandler providerEventHandler2;
                do
                {
                    providerEventHandler2 = providerEventHandler;
                    ProviderEventHandler value2 = (ProviderEventHandler)Delegate.Remove(providerEventHandler2, value);
                    providerEventHandler = Interlocked.CompareExchange<ProviderEventHandler>(ref this.jMmiArAlGk, value2, providerEventHandler2);
                }
                while (providerEventHandler != providerEventHandler2);
            }
        }

        public event ProviderEventHandler ProviderDisconnected
        {
            add
            {
                ProviderEventHandler providerEventHandler = this.providerEventHandler_2;
                ProviderEventHandler providerEventHandler2;
                do
                {
                    providerEventHandler2 = providerEventHandler;
                    ProviderEventHandler value2 = (ProviderEventHandler)Delegate.Combine(providerEventHandler2, value);
                    providerEventHandler = Interlocked.CompareExchange<ProviderEventHandler>(ref this.providerEventHandler_2, value2, providerEventHandler2);
                }
                while (providerEventHandler != providerEventHandler2);
            }
            remove
            {
                ProviderEventHandler providerEventHandler = this.providerEventHandler_2;
                ProviderEventHandler providerEventHandler2;
                do
                {
                    providerEventHandler2 = providerEventHandler;
                    ProviderEventHandler value2 = (ProviderEventHandler)Delegate.Remove(providerEventHandler2, value);
                    providerEventHandler = Interlocked.CompareExchange<ProviderEventHandler>(ref this.providerEventHandler_2, value2, providerEventHandler2);
                }
                while (providerEventHandler != providerEventHandler2);
            }
        }

        public event ProviderErrorEventHandler ProviderError
        {
            add
            {
                ProviderErrorEventHandler providerErrorEventHandler = this.providerErrorEventHandler_0;
                ProviderErrorEventHandler providerErrorEventHandler2;
                do
                {
                    providerErrorEventHandler2 = providerErrorEventHandler;
                    ProviderErrorEventHandler value2 = (ProviderErrorEventHandler)Delegate.Combine(providerErrorEventHandler2, value);
                    providerErrorEventHandler = Interlocked.CompareExchange<ProviderErrorEventHandler>(ref this.providerErrorEventHandler_0, value2, providerErrorEventHandler2);
                }
                while (providerErrorEventHandler != providerErrorEventHandler2);
            }
            remove
            {
                ProviderErrorEventHandler providerErrorEventHandler = this.providerErrorEventHandler_0;
                ProviderErrorEventHandler providerErrorEventHandler2;
                do
                {
                    providerErrorEventHandler2 = providerErrorEventHandler;
                    ProviderErrorEventHandler value2 = (ProviderErrorEventHandler)Delegate.Remove(providerErrorEventHandler2, value);
                    providerErrorEventHandler = Interlocked.CompareExchange<ProviderErrorEventHandler>(ref this.providerErrorEventHandler_0, value2, providerErrorEventHandler2);
                }
                while (providerErrorEventHandler != providerErrorEventHandler2);
            }
        }

        public event ProviderEventHandler ProviderRemoved
        {
            add
            {
                ProviderEventHandler providerEventHandler = this.TqQiEpesSY;
                ProviderEventHandler providerEventHandler2;
                do
                {
                    providerEventHandler2 = providerEventHandler;
                    ProviderEventHandler value2 = (ProviderEventHandler)Delegate.Combine(providerEventHandler2, value);
                    providerEventHandler = Interlocked.CompareExchange<ProviderEventHandler>(ref this.TqQiEpesSY, value2, providerEventHandler2);
                }
                while (providerEventHandler != providerEventHandler2);
            }
            remove
            {
                ProviderEventHandler providerEventHandler = this.TqQiEpesSY;
                ProviderEventHandler providerEventHandler2;
                do
                {
                    providerEventHandler2 = providerEventHandler;
                    ProviderEventHandler value2 = (ProviderEventHandler)Delegate.Remove(providerEventHandler2, value);
                    providerEventHandler = Interlocked.CompareExchange<ProviderEventHandler>(ref this.TqQiEpesSY, value2, providerEventHandler2);
                }
                while (providerEventHandler != providerEventHandler2);
            }
        }

        public event ProviderEventHandler ProviderStatusChanged
        {
            add
            {
                ProviderEventHandler providerEventHandler = this.providerEventHandler_1;
                ProviderEventHandler providerEventHandler2;
                do
                {
                    providerEventHandler2 = providerEventHandler;
                    ProviderEventHandler value2 = (ProviderEventHandler)Delegate.Combine(providerEventHandler2, value);
                    providerEventHandler = Interlocked.CompareExchange<ProviderEventHandler>(ref this.providerEventHandler_1, value2, providerEventHandler2);
                }
                while (providerEventHandler != providerEventHandler2);
            }
            remove
            {
                ProviderEventHandler providerEventHandler = this.providerEventHandler_1;
                ProviderEventHandler providerEventHandler2;
                do
                {
                    providerEventHandler2 = providerEventHandler;
                    ProviderEventHandler value2 = (ProviderEventHandler)Delegate.Remove(providerEventHandler2, value);
                    providerEventHandler = Interlocked.CompareExchange<ProviderEventHandler>(ref this.providerEventHandler_1, value2, providerEventHandler2);
                }
                while (providerEventHandler != providerEventHandler2);
            }
        }

        public event ResponseEventHandler Response
        {
            add
            {
                ResponseEventHandler responseEventHandler = this.responseEventHandler_0;
                ResponseEventHandler responseEventHandler2;
                do
                {
                    responseEventHandler2 = responseEventHandler;
                    ResponseEventHandler value2 = (ResponseEventHandler)Delegate.Combine(responseEventHandler2, value);
                    responseEventHandler = Interlocked.CompareExchange<ResponseEventHandler>(ref this.responseEventHandler_0, value2, responseEventHandler2);
                }
                while (responseEventHandler != responseEventHandler2);
            }
            remove
            {
                ResponseEventHandler responseEventHandler = this.responseEventHandler_0;
                ResponseEventHandler responseEventHandler2;
                do
                {
                    responseEventHandler2 = responseEventHandler;
                    ResponseEventHandler value2 = (ResponseEventHandler)Delegate.Remove(responseEventHandler2, value);
                    responseEventHandler = Interlocked.CompareExchange<ResponseEventHandler>(ref this.responseEventHandler_0, value2, responseEventHandler2);
                }
                while (responseEventHandler != responseEventHandler2);
            }
        }

        public event SimulatorProgressEventHandler SimulatorProgress
        {
            add
            {
                SimulatorProgressEventHandler simulatorProgressEventHandler = this.simulatorProgressEventHandler_0;
                SimulatorProgressEventHandler simulatorProgressEventHandler2;
                do
                {
                    simulatorProgressEventHandler2 = simulatorProgressEventHandler;
                    SimulatorProgressEventHandler value2 = (SimulatorProgressEventHandler)Delegate.Combine(simulatorProgressEventHandler2, value);
                    simulatorProgressEventHandler = Interlocked.CompareExchange<SimulatorProgressEventHandler>(ref this.simulatorProgressEventHandler_0, value2, simulatorProgressEventHandler2);
                }
                while (simulatorProgressEventHandler != simulatorProgressEventHandler2);
            }
            remove
            {
                SimulatorProgressEventHandler simulatorProgressEventHandler = this.simulatorProgressEventHandler_0;
                SimulatorProgressEventHandler simulatorProgressEventHandler2;
                do
                {
                    simulatorProgressEventHandler2 = simulatorProgressEventHandler;
                    SimulatorProgressEventHandler value2 = (SimulatorProgressEventHandler)Delegate.Remove(simulatorProgressEventHandler2, value);
                    simulatorProgressEventHandler = Interlocked.CompareExchange<SimulatorProgressEventHandler>(ref this.simulatorProgressEventHandler_0, value2, simulatorProgressEventHandler2);
                }
                while (simulatorProgressEventHandler != simulatorProgressEventHandler2);
            }
        }

        public event EventHandler SimulatorStop
        {
            add
            {
                EventHandler eventHandler = this.eventHandler_0;
                EventHandler eventHandler2;
                do
                {
                    eventHandler2 = eventHandler;
                    EventHandler value2 = (EventHandler)Delegate.Combine(eventHandler2, value);
                    eventHandler = Interlocked.CompareExchange<EventHandler>(ref this.eventHandler_0, value2, eventHandler2);
                }
                while (eventHandler != eventHandler2);
            }
            remove
            {
                EventHandler eventHandler = this.eventHandler_0;
                EventHandler eventHandler2;
                do
                {
                    eventHandler2 = eventHandler;
                    EventHandler value2 = (EventHandler)Delegate.Remove(eventHandler2, value);
                    eventHandler = Interlocked.CompareExchange<EventHandler>(ref this.eventHandler_0, value2, eventHandler2);
                }
                while (eventHandler != eventHandler2);
            }
        }

        public event StrategyEventHandler StrategyAdded
        {
            add
            {
                StrategyEventHandler strategyEventHandler = this.strategyEventHandler_0;
                StrategyEventHandler strategyEventHandler2;
                do
                {
                    strategyEventHandler2 = strategyEventHandler;
                    StrategyEventHandler value2 = (StrategyEventHandler)Delegate.Combine(strategyEventHandler2, value);
                    strategyEventHandler = Interlocked.CompareExchange<StrategyEventHandler>(ref this.strategyEventHandler_0, value2, strategyEventHandler2);
                }
                while (strategyEventHandler != strategyEventHandler2);
            }
            remove
            {
                StrategyEventHandler strategyEventHandler = this.strategyEventHandler_0;
                StrategyEventHandler strategyEventHandler2;
                do
                {
                    strategyEventHandler2 = strategyEventHandler;
                    StrategyEventHandler value2 = (StrategyEventHandler)Delegate.Remove(strategyEventHandler2, value);
                    strategyEventHandler = Interlocked.CompareExchange<StrategyEventHandler>(ref this.strategyEventHandler_0, value2, strategyEventHandler2);
                }
                while (strategyEventHandler != strategyEventHandler2);
            }
        }

        public event TradeEventHandler Trade
        {
            add
            {
                TradeEventHandler tradeEventHandler = this.tradeEventHandler_0;
                TradeEventHandler tradeEventHandler2;
                do
                {
                    tradeEventHandler2 = tradeEventHandler;
                    TradeEventHandler value2 = (TradeEventHandler)Delegate.Combine(tradeEventHandler2, value);
                    tradeEventHandler = Interlocked.CompareExchange<TradeEventHandler>(ref this.tradeEventHandler_0, value2, tradeEventHandler2);
                }
                while (tradeEventHandler != tradeEventHandler2);
            }
            remove
            {
                TradeEventHandler tradeEventHandler = this.tradeEventHandler_0;
                TradeEventHandler tradeEventHandler2;
                do
                {
                    tradeEventHandler2 = tradeEventHandler;
                    TradeEventHandler value2 = (TradeEventHandler)Delegate.Remove(tradeEventHandler2, value);
                    tradeEventHandler = Interlocked.CompareExchange<TradeEventHandler>(ref this.tradeEventHandler_0, value2, tradeEventHandler2);
                }
                while (tradeEventHandler != tradeEventHandler2);
            }
        }

        public event TransactionEventHandler Transaction
        {
            add
            {
                TransactionEventHandler transactionEventHandler = this.transactionEventHandler_0;
                TransactionEventHandler transactionEventHandler2;
                do
                {
                    transactionEventHandler2 = transactionEventHandler;
                    TransactionEventHandler value2 = (TransactionEventHandler)Delegate.Combine(transactionEventHandler2, value);
                    transactionEventHandler = Interlocked.CompareExchange<TransactionEventHandler>(ref this.transactionEventHandler_0, value2, transactionEventHandler2);
                }
                while (transactionEventHandler != transactionEventHandler2);
            }
            remove
            {
                TransactionEventHandler transactionEventHandler = this.transactionEventHandler_0;
                TransactionEventHandler transactionEventHandler2;
                do
                {
                    transactionEventHandler2 = transactionEventHandler;
                    TransactionEventHandler value2 = (TransactionEventHandler)Delegate.Remove(transactionEventHandler2, value);
                    transactionEventHandler = Interlocked.CompareExchange<TransactionEventHandler>(ref this.transactionEventHandler_0, value2, transactionEventHandler2);
                }
                while (transactionEventHandler != transactionEventHandler2);
            }
        }

        private AccountDataEventHandler accountDataEventHandler_0;

        private AccountReportEventHandler accountReportEventHandler_0;

        private AskEventHandler askEventHandler_0;

        private BarEventHandler barEventHandler_0;

        private BarEventHandler barEventHandler_1;

        private BidEventHandler bidEventHandler_0;

        private CommandEventHandler commandEventHandler_0;

        internal EventController eventController_0;

        internal EventDispatcherServerClient eventDispatcherServerClient_0;

        private EventHandler eventHandler_0;

        private EventHandler eventHandler_1;

        private EventHandler eventHandler_2;

        private EventHandler eventHandler_3;

        private EventHandler eventHandler_4;

        private EventHandler eventHandler_5;

        private EventQueue eventQueue_0;

        private EventQueue eventQueue_1;

        private ExecutionCommandEventHandler executionCommandEventHandler_0;

        private ExecutionReportEventHandler executionReportEventHandler_0;

        private FillEventHandler fillEventHandler_0;

        protected internal Framework framework;

     //   private FrameworkEventHandler frameworkEventHandler_0;

        private GroupEventEventHandler groupEventEventHandler_0;

        private GroupEventHandler groupEventHandler_0;

        private GroupUpdateEventHandler groupUpdateEventHandler_0;

        private HistoricalDataEndEventHandler historicalDataEndEventHandler_0;

        private HistoricalDataEventHandler historicalDataEventHandler_0;

        private IdArray<IEventClient> idArray_0;

        private IdArray<List<IEventClient>> idArray_1;

        private InstrumentDefinitionEndEventHandler instrumentDefinitionEndEventHandler_0;

        private InstrumentDefinitionEventHandler instrumentDefinitionEventHandler_0;

        private InstrumentEventHandler instrumentEventHandler_0;

        private InstrumentEventHandler instrumentEventHandler_1;

        private ProviderEventHandler jMmiArAlGk;

        private OrderManagerClearedEventHandler orderManagerClearedEventHandler_0;

        private OutputEventHandler outputEventHandler_0;

        private PositionEventHandler ovQrTixtSx;

        private PermanentQueue<Event> permanentQueue_0;

        private PortfolioEventHandler portfolioEventHandler_0;

        private PortfolioEventHandler portfolioEventHandler_1;

        private PositionEventHandler positionEventHandler_0;

        private PositionEventHandler positionEventHandler_1;

        private ProviderErrorEventHandler providerErrorEventHandler_0;

        private ProviderEventHandler providerEventHandler_0;

        private ProviderEventHandler providerEventHandler_1;

        private ProviderEventHandler providerEventHandler_2;

        private ResponseEventHandler responseEventHandler_0;

        private SimulatorProgressEventHandler simulatorProgressEventHandler_0;

        private StrategyEventHandler strategyEventHandler_0;

        private Thread thread_0;

        private ProviderEventHandler TqQiEpesSY;

        private TradeEventHandler tradeEventHandler_0;

        private TransactionEventHandler transactionEventHandler_0;

        private PortfolioEventHandler WixrwvmkIM;
    }

}