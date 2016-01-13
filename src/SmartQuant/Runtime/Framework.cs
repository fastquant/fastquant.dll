// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace SmartQuant
{
    public class FrameworkEventArgs : EventArgs
    {
        public Framework Framework { get; }

        public FrameworkEventArgs(Framework framework)
        {
            Framework = framework;
        }
    }

    public delegate void FrameworkEventHandler(object sender, FrameworkEventArgs args);

    public enum FrameworkMode
    {
        Simulation,
        Realtime
    }

    public class Controller
    {
        private string host = "localhost";
        private int port = 1002;
        private Thread thread;
        private TcpClient tcpClient;
        private Stream stream;
        private BinaryReader reader;
        private BinaryWriter writer;

        protected internal string name;
        protected internal EventQueue queue;
        protected internal Framework framework;

        public string Name => this.name;

        public Controller()
        {
            throw new NotSupportedException("Don't use this constructor.");
        }

        public Controller(Framework framework, string name)
        {
            this.framework = framework;
            this.name = name;
            this.tcpClient = new TcpClient(this.host, this.port);
            this.stream = this.tcpClient.GetStream();
            this.reader = new BinaryReader(this.stream);
            this.writer = new BinaryWriter(this.stream);
            this.thread = new Thread(new ThreadStart(Run));
            this.thread.IsBackground = true;
            this.thread.Start();
        }

        public void Log(GroupEvent e)
        {
            if (this.queue == null)
            {
                this.queue = new EventQueue(EventQueueId.Service, EventQueueType.Master, EventQueuePriority.Normal, 100000, null);
                this.queue.Enqueue(new OnQueueOpened(this.queue));
                this.framework.Bus.CommandPipe.Add(this.queue);
            }
            this.queue.Enqueue(e);
        }

        private void Run()
        {
            throw new NotImplementedException();
        }
    }

    public class Framework : IDisposable
    {
        private static Framework instance;
        private FrameworkMode mode;
        private EventBus bus;
        private bool disposed;
        private bool isDisposable;

        private DataServer dataServer;
        private OrderServer orderServer;
        private InstrumentServer instrumentServer;
        private PortfolioServer portfolioServer;

        internal EventBus Bus => bus;

        public string Name { get; }

        public bool IsExternalDataQueue { get; set; }

        public bool IsDisposable { get; set; }

        public Clock Clock { get; }

        public Clock ExchangeClock { get; }

        public Configuration Configuration { get; private set; }

        public AccountDataManager AccountDataManager { get; }

        public StreamerManager StreamerManager { get; }

        public Controller Controller { get; set; }

        public DataFileManager DataFileManager { get; }

        public DataManager DataManager { get; }

        public EventManager EventManager { get; }

        public EventLoggerManager EventLoggerManager { get; }

        public SubscriptionManager SubscriptionManager { get; }

        public ScenarioManager ScenarioManager { get; }

        public GroupManager GroupManager { get; }

        public GroupDispatcher GroupDispatcher { get; set; }

        public InstrumentManager InstrumentManager { get; set; }

        public InstrumentServer InstrumentServer
        {
            get { return this.instrumentServer; }
            set
            {
                this.instrumentServer = value;
                InstrumentManager.Server = value;
            }
        }

        public ICurrencyConverter CurrencyConverter { get; set; }

        public IDataProvider DataProvider => ProviderManager.GetDataProvider(Configuration.DefaultDataProvider);

        public IExecutionProvider ExecutionProvider => ProviderManager.GetExecutionProvider(Configuration.DefaultExecutionProvider);

        public DataServer DataServer
        {
            get
            {
                return this.dataServer;
            }
            set
            {
                this.dataServer = value;
                DataManager.Server = value;
            }
        }

        public FrameworkMode Mode
        {
            get
            {
                return this.mode;
            }
            set
            {
                if (this.mode != value)
                {
                    ProviderManager?.DisconnectAll();
                    this.mode = value;
                    switch (this.mode)
                    {
                        case FrameworkMode.Simulation:
                            Clock.Mode = ClockMode.Simulation;
                            if (this.bus != null)
                            {
                                this.bus.Mode = EventBusMode.Simulation;
                                return;
                            }
                            break;
                        case FrameworkMode.Realtime:
                            Clock.Mode = ClockMode.Realtime;
                            if (this.bus != null)
                            {
                                this.bus.Mode = EventBusMode.Realtime;
                            }
                            break;
                        default:
                            return;
                    }
                }
            }
        }

        public OrderManager OrderManager { get; }

        public OrderServer OrderServer
        {
            get
            {
                return this.orderServer;
            }
            set
            {
                this.orderServer = value;
                OrderManager.Server = value;
            }
        }

        public ProviderManager ProviderManager { get; }

        public LicenseManager LicenseManager { get; }
        
        public StatisticsManager StatisticsManager { get; }

        public StrategyManager StrategyManager { get; }

        public UserServer UserServer { get; }

        public UserManager UserManager { get; }

        public static Framework Current => instance = instance ?? new Framework("", FrameworkMode.Simulation, true);

        public EventServer EventServer { get; internal set; }

        public PortfolioManager PortfolioManager { get; internal set; }

        public OutputManager OutputManager { get; private set; }

        public PortfolioServer PortfolioServer
        {
            get
            {
                return this.portfolioServer;
            }
            set
            {
                this.portfolioServer = value;
                PortfolioManager.Server = value;
            }
        }

        public EventBus EventBus { get; }

        internal IServerFactory ServerFactory { get; }

        public Framework(string name = "", FrameworkMode mode = FrameworkMode.Simulation, bool createServers = true) : this(name, mode, createServers, externalBus: null, instrumentServer: null, dataServer: null)
        {
        }

        public Framework(string name, InstrumentServer instrumentServer, DataServer dataServer) : this(name, FrameworkMode.Simulation, createServers: false, externalBus: null, instrumentServer: instrumentServer, dataServer: dataServer)
        {
        }

        public Framework(string name, EventBus externalBus, InstrumentServer instrumentServer, DataServer dataServer = null) : this(name, FrameworkMode.Simulation, createServers: false, externalBus: externalBus, instrumentServer: instrumentServer, dataServer: dataServer)
        {
        }

        private Framework(string name, FrameworkMode mode, bool createServers, EventBus externalBus, InstrumentServer instrumentServer, DataServer dataServer)
        {
            Name = name;
            LoadConfiguration();

            // Setup events compoents setup
            EventBus = new EventBus(this);
            Clock = new Clock(this, ClockType.Local, false);
            EventBus.LocalClockEventQueue = Clock.ReminderEventQueue;
            ExchangeClock = new Clock(this, ClockType.Exchange, false);
            EventBus.ExchangeClockEventQueue = ExchangeClock.ReminderEventQueue;
            if (externalBus != null)
                externalBus.Attach(EventBus);
            EventServer = new EventServer(this, EventBus);
            EventManager = new EventManager(this, EventBus);

            // Now we can setup Mode since the Clock is all set
            Mode = mode;

            // Setup streamers
            StreamerManager = new StreamerManager();
            StreamerManager.AddDefaultStreamers();
            //foreach (var streamer in Configuration.Streamers)
            //{
            //    var type = Type.GetType(streamer.TypeName);
            //    var s = (ObjectStreamer)Activator.CreateInstance(type);
            //    StreamerManager.Add(s);
            //}

            // Create Servers
            //ServerFactory = (IServerFactory)Activator.CreateInstance(Type.GetType("SmartQuant.DefaultServerFactory,FastQuant.Servers"));
            //ServerFactory = new DefaultServerFactory();

            //InstrumentServer = instrumentServer ?? ServerFactory.CreateInstrumentServer();
            var iServer = instrumentServer ?? new FileInstrumentServer(this, "d:\\instruments.quant");
            InstrumentManager = new InstrumentManager(this, iServer);
            InstrumentServer = iServer;
            InstrumentManager.Load();
            //, "instruments.quant", "", this.configuration_0.InstrumentFilePort);
            //DataServer = dataServer ?? ServerFactory.CreateDataServer();
            //OrderServer = ServerFactory.CreateOrderServer();
            //PortfolioServer = ServerFactory.CreatePortfolioServer();
            //UserServer = ServerFactory.CreateUserServer();
            //InstrumentManager.Load();
            DataManager = new DataManager(this, DataServer);
            //UserManager = new UserManager(this, UserServer);
            //UserManager.Load();
            OrderManager = new OrderManager(this, OrderServer);
            PortfolioManager = new PortfolioManager(this, PortfolioServer);

            // Create Providers
            ProviderManager = new ProviderManager(this);
            //ProviderManager.DataSimulator = (IDataSimulator)Activator.CreateInstance(Type.GetType(Configuration.DefaultDataSimulator));
            //ProviderManager.ExecutionSimulator = (IExecutionSimulator)Activator.CreateInstance(Type.GetType(Configuration.DefaultExecutionSimulator));

            // Other stuff
            EventLoggerManager = new EventLoggerManager();
            SubscriptionManager = new SubscriptionManager(this);
            ScenarioManager = new ScenarioManager(this);
            StrategyManager = new StrategyManager(this);
            StatisticsManager = new StatisticsManager(this);
            GroupManager = new GroupManager(this);
            AccountDataManager = new AccountDataManager(this);
            LicenseManager = new LicenseManager();
            CurrencyConverter = new CurrencyConverter(this);
            DataFileManager = new DataFileManager(Installation.DataDir.FullName);

            Framework.instance = Framework.instance ?? this;
        }

        ~Framework()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            if (IsDisposable)
            {
                Console.WriteLine($"Framework::Dispose {Name}");
                Dispose(true);
                GC.SuppressFinalize(this);
            }
            else
            {
                Console.WriteLine($"Framework::Dispose Framework is not disposable{Name}");
            }
        }

        private void Dispose(bool dispoing)
        {

        }

        public void Clear()
        {
            Console.WriteLine($"{DateTime.Now} Framework::Clear");
            Clock?.Clear();
            ExchangeClock?.Clear();
            EventBus?.Clear();
            EventServer?.Clear();
            EventManager?.Clear();
            ProviderManager?.DisconnectAll();
            ProviderManager?.Clear();
            InstrumentManager?.Clear();
            DataManager?.Clear();
            SubscriptionManager?.Clear();
            OrderManager?.Clear();
            PortfolioManager?.Clear();
            StrategyManager?.Clear();
            AccountDataManager?.Clear();
            GroupManager?.Clear();
            OutputManager?.Clear();
            GC.Collect();
            EventServer.OnFrameworkCleared(this);
        }

        public void Dump()
        {
            Console.WriteLine($"Framework {Name}");
            DataManager.Dump();
        }

        public void Load(BinaryReader reader)
        {
            throw new NotImplementedException();
        }

        public void Save(BinaryWriter writer)
        {
            throw new NotImplementedException();
        }

        private void LoadConfiguration()
        {
            //var text = File.ReadAllText(Path.Combine(Installation.ConfigDir.FullName, "configuration.xml"));
            Configuration = Configuration.DefaultConfiguaration();
        }
    }

    public class FrameworkServer : IDisposable
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}