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
                this.framework.Bus.LogPipe.Add(this.queue);
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

        internal EventBus Bus => bus;

        public string Name { get; internal set; }
        public AccountDataManager AccountDataManager { get; }
        public Clock Clock { get; }
        public Clock ExchangeClock { get; }
        public Configuration Configuration { get; }
        public Controller Controller { get; set; }
        public DataFileManager DataFileManager { get; }
        public DataManager DataManager { get; }
        public EventManager EventManager { get; }
        public GroupManager GroupManager { get; }
        public InstrumentManager InstrumentManager { get; }

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
                    ProviderManager.DisconnectAll();
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
        public ProviderManager ProviderManager { get; }
        public StatisticsManager StatisticsManager { get; }
        public StrategyManager StrategyManager { get; }
        public UserServer UserServer { get; }
        public UserManager UserManager { get; }

        public static Framework Current => instance = instance ?? new Framework("", FrameworkMode.Simulation, true);

        public EventServer EventServer { get; internal set; }

        public PortfolioManager PortfolioManager { get; internal set; }
        public EventBus EventBus { get; internal set; }
        public InstrumentServer InstrumentServer { get; internal set; }

        public Framework(string name = "", FrameworkMode mode = FrameworkMode.Simulation, bool createServers = true)
        {
        }

        public Framework(string name, InstrumentServer instrumentServer, DataServer dataServer)
        {
        }

        public Framework(string name, EventBus externalBus, InstrumentServer instrumentServer, DataServer dataServer = null)
        {
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        internal void Clear()
        {
            throw new NotImplementedException();
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