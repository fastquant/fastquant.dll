// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;

namespace FastQuant
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
            //this.tcpClient = new TcpClient(this.host, this.port);
            this.tcpClient = new TcpClient(AddressFamily.InterNetwork);
            this.stream = this.tcpClient.GetStream();
            this.reader = new BinaryReader(this.stream);
            this.writer = new BinaryWriter(this.stream);
            this.thread = new Thread(Run) { IsBackground = true };
            this.thread.Start();
        }

        public void Log(GroupEvent e)
        {
            if (this.queue == null)
            {
                this.queue = new EventQueue(EventQueueId.Service, EventQueueType.Master, EventQueuePriority.Normal, 100000, null);
                this.queue.Enqueue(new OnQueueOpened(this.queue));
                this.framework.EventBus.CommandPipe.Add(this.queue);
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
        private bool disposed;
        private string configFile;

        private DataServer dataServer;
        private OrderServer orderServer;
        private InstrumentServer instrumentServer;
        private PortfolioServer portfolioServer;

        public string Name { get; private set; }

        public bool IsExternalDataQueue { get; set; }

        public bool IsDisposable { get; set; } = true;

        private bool bool_1 { get; set; } = true;

        private bool bool_2 { get; set; } = true;

        private bool bool_3 { get; set; } = true;

        private bool bool_4 { get; set; } = true;

        public Clock Clock { get; }

        public Clock ExchangeClock { get; }

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
                            if (EventBus != null)
                            {
                                EventBus.Mode = EventBusMode.Simulation;
                                return;
                            }
                            break;
                        case FrameworkMode.Realtime:
                            Clock.Mode = ClockMode.Realtime;
                            if (EventBus != null)
                            {
                                EventBus.Mode = EventBusMode.Realtime;
                            }
                            break;
                        default:
                            return;
                    }
                }
            }
        }

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

        public ICurrencyConverter CurrencyConverter { get; set; }

        public IDataProvider DataProvider => ProviderManager.GetDataProvider(Configuration.DefaultDataProvider);

        public IExecutionProvider ExecutionProvider => ProviderManager.GetExecutionProvider(Configuration.DefaultExecutionProvider);

        public InstrumentServer InstrumentServer
        {
            get { return this.instrumentServer; }
            set
            {
                this.instrumentServer = value;
                InstrumentManager.Server = value;
            }
        }

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

        public OrderManager OrderManager { get; }

        public ProviderManager ProviderManager { get; }

        public LicenseManager LicenseManager { get; }

        public StatisticsManager StatisticsManager { get; }

        public StrategyManager StrategyManager { get; }

        public UserServer UserServer { get; }

        public UserManager UserManager { get; }

        public static Framework Current => instance = instance ?? new Framework("", FrameworkMode.Simulation, true);

        public EventServer EventServer { get; internal set; }

        public PortfolioManager PortfolioManager { get; internal set; }

        public OutputManager OutputManager { get; }

        public EventBus EventBus { get; }

        public Framework(string name = "", FrameworkMode mode = FrameworkMode.Simulation, bool createServers = true, string fileServerPath = "FileServer.exe") : this(name, mode, createServers, externalBus: null, instrumentServer: null, dataServer: null, fileServerPath: String.Empty)
        {
        }

        public Framework(string name, InstrumentServer instrumentServer, DataServer dataServer) : this(name, FrameworkMode.Simulation, createServers: false, externalBus: null, instrumentServer: instrumentServer, dataServer: dataServer, fileServerPath: String.Empty)
        {
        }

        public Framework(string name, EventBus externalBus, InstrumentServer instrumentServer, DataServer dataServer = null) : this(name, FrameworkMode.Simulation, createServers: false, externalBus: externalBus, instrumentServer: instrumentServer, dataServer: dataServer, fileServerPath: String.Empty)
        {
        }

        private Framework(string name, FrameworkMode mode, bool createServers, EventBus externalBus, InstrumentServer instrumentServer, DataServer dataServer, string fileServerPath)
        {
            Name = name;
            this.mode = mode;
            LoadConfiguration();

            // Setup events compoents setup
            EventBus = new EventBus(this);
            OutputManager = new OutputManager(this, Configuration.IsOutputLogEnabled ? Configuration.OutputLogFileName : null);
            Clock = new Clock(this, ClockType.Local, false);
            EventBus.LocalClockEventQueue = Clock.ReminderQueue;
            ExchangeClock = new Clock(this, ClockType.Exchange, false);
            EventBus.ExchangeClockEventQueue = ExchangeClock.ReminderQueue;
            externalBus?.Attach(EventBus);
            EventServer = new EventServer(this, EventBus);
            EventManager = new EventManager(this, EventBus);

            // Setup streamers
            StreamerManager = new StreamerManager();
            StreamerManager.AddDefaultStreamers();

            // Create Servers
            var iServer = instrumentServer ?? new FileInstrumentServer(this, Configuration.InstrumentFileName);
            var dServer = dataServer ?? new FileDataServer(this, Configuration.DataFileName);
            var oServer = new FileOrderServer(this, Configuration.OrderFileName);
            var pServer = new FilePortfolioServer(this, Configuration.PortfolioFileName);

            InstrumentManager = new InstrumentManager(this, iServer);
            InstrumentServer = iServer;
            InstrumentManager.Load();
            DataManager = new DataManager(this, dServer);
            DataServer = dServer;
            OrderManager = new OrderManager(this, oServer);
            OrderServer = oServer;
            PortfolioManager = new PortfolioManager(this, pServer);
            PortfolioServer = pServer;
            UserServer = new XmlUserServer(this);
            UserManager = new UserManager(this, UserServer);
            UserManager.Load();

            // Create Providers
            ProviderManager = new ProviderManager(this);

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

            instance = instance ?? this;
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
                Console.WriteLine($"Framework::Dispose Framework is not disposable {Name}");
            }
        }

        private void Dispose(bool dispoing)
        {
            if (!this.disposed)
            {
                if (dispoing)
                {
                    SaveConfiguration();
                    if (this.bool_1) InstrumentServer?.Dispose();
                    if (this.bool_2) DataServer?.Dispose();
                    if (this.bool_3) OrderServer?.Dispose();
                    if (this.bool_4) PortfolioServer?.Dispose();
                    ProviderManager?.Dispose();
                    DataManager?.Dispose();
                    EventManager?.Dispose();
                    OutputManager?.Dispose();
                }
                this.disposed = true;
            }
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
            Name = reader.ReadString();
            DataManager.Load(reader);
            SubscriptionManager.Load(reader);
            PortfolioManager.Load(reader);
        }

        public void Save(BinaryWriter writer)
        {
            writer.Write(Name);
            DataManager.Save(writer);
            SubscriptionManager.Save(writer);
            PortfolioManager.Save(writer);
        }

        private void LoadConfiguration()
        {
            string content = null;
            try
            {
                var entryAssembly = Assembly.GetEntryAssembly();
                var customAttribute = entryAssembly.GetCustomAttribute<AssemblyProductAttribute>();
                var customAttribute2 = entryAssembly.GetCustomAttribute<AssemblyCompanyAttribute>();
                var text = customAttribute == null ? entryAssembly.GetName().Name : customAttribute.Product;
                var text2 = customAttribute2 == null ? entryAssembly.GetName().Name : customAttribute2.Company;
                var path = Path.Combine(InstallationUtils.GetApplicationDataPath(), text2, text, "framework", "configuration.xml");
                this.configFile = File.Exists(path) ? path : Path.Combine(Installation.ConfigDir.FullName, "configuration.xml");
                content = File.ReadAllText(this.configFile);
            }
            catch (Exception)
            {
                // ignored
            }
            if (content != null)
            {
                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(content)))
                {
                    var serializer = new XmlSerializer(typeof(Configuration));
                    Configuration = (Configuration)serializer.Deserialize(stream);
                    if (string.IsNullOrEmpty(Configuration.InstrumentFileName))
                        Configuration.SetDefaultInstrumentConfiguration();
                    if (string.IsNullOrEmpty(Configuration.DataFileName))
                        Configuration.SetDefaultDataConfiguration();
                    if (string.IsNullOrEmpty(Configuration.OrderFileName))
                        Configuration.SetDefaultOrderConfiguration();
                    if (string.IsNullOrEmpty(Configuration.PortfolioFileName))
                        Configuration.SetDefaultPortfolioConfiguration();
                    if (Configuration.Streamers.Count == 0)
                        Configuration.AddDefaultStreamers();
                    if (Configuration.Providers.Count == 0)
                        Configuration.AddDefaultProviders();
                    return;
                }
            }
            Configuration = Configuration.DefaultConfiguaration();
        }

        private void SaveConfiguration()
        {
            try
            {
                var serializer = new XmlSerializer(typeof(Configuration));
                this.configFile = this.configFile ?? Path.Combine(Installation.ConfigDir.FullName, "configuration.xml");
                using (var fs = File.OpenWrite(this.configFile))
                using (var writer = new StreamWriter(fs))
                    serializer.Serialize(writer, Configuration);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error during SaveConfiguration: {e}");
            }
        }
    }

    public class FrameworkServer : IDisposable
    {
        private readonly Dictionary<string, string> _dictionary = new Dictionary<string, string>();

        private string _connectionString;

        public Framework Framework { get; internal set; }

        public string ConnectionString
        {
            get
            {
                return this._connectionString;
            }
            set
            {
                this._connectionString = value;
                Update();
            }
        }

        protected FrameworkServer()
        {
        }

        public virtual void Close()
        {
            // do nothing
        }

        public void Dispose() => Close();


        public virtual void Flush()
        {
            // noop
        }

        protected bool GetBooleanValue(string key, bool defaultValue)
        {
            bool result;
            return bool.TryParse(GetStringValue(key, string.Empty), out result) ? result : defaultValue;
        }

        protected int GetInt32Value(string key, int defaultValue)
        {
            int result;
            return int.TryParse(GetStringValue(key, string.Empty), out result) ? result : defaultValue;
        }

        protected string GetStringValue(string key, string defaultValue)
        {
            string result;
            return this._dictionary.TryGetValue(key.ToUpper(), out result) ? result : defaultValue;
        }

        public virtual void Open()
        {
            // noop
        }

        private void Update()
        {
            this._dictionary.Clear();
            if (this._connectionString != null)
            {
                foreach (var text in this._connectionString.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var comps = text.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                    if (comps.Length == 2)
                        this._dictionary[comps[0].Trim().ToUpper()] = comps[1].Trim();
                }
            }
        }
    }
}