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

    #endregion

    public class EventDispatcher
    {

        private Framework framework;
        private IdArray<IEventClient> clients = new IdArray<IEventClient>();
        private IdArray<List<IEventClient>> clientsByEventType = new IdArray<List<IEventClient>>();
        private EventQueue commandQueue;

        public PermanentQueue<Event> PermanentQueue { get; } = new PermanentQueue<Event>();

        public EventController Controller { get; set; }

        public event FrameworkEventHandler FrameworkCleared;
        public event GroupEventHandler NewGroup;
        public event GroupEventEventHandler NewGroupEvent;
        public event GroupUpdateEventHandler NewGroupUpdate;

        public event BidEventHandler Bid;
        public event AskEventHandler Ask;
        public event TradeEventHandler Trade;
        public event BarEventHandler BarOpen;
        public event BarEventHandler Bar;


        public event ExecutionCommandEventHandler ExecutionCommand;
        public event ExecutionReportEventHandler ExecutionReport;
        public event OrderManagerClearedEventHandler OrderManagerCleared;
        public event PositionEventHandler PositionOpened;
        public event PositionEventHandler PositionChanged;
        public event PositionEventHandler PositionClosed;
        public event FillEventHandler Fill;
        public event TransactionEventHandler Transaction;
        public event PortfolioEventHandler PortfolioAdded;
        public event PortfolioEventHandler PortfolioRemoved;
        public event PortfolioEventHandler PortfolioParentChanged;

        public event AccountDataEventHandler AccountData;
        public event HistoricalDataEventHandler HistoricalData;
        public event HistoricalDataEndEventHandler HistoricalDataEnd;

        public event InstrumentEventHandler InstrumentAdded;
        public event InstrumentEventHandler InstrumentDeleted;
        public event InstrumentDefinitionEventHandler InstrumentDefinition;
        public event InstrumentDefinitionEndEventHandler InstrumentDefinitionEnd;

        public EventDispatcher()
        {
            throw new NotImplementedException("Don't use this constructor");
        }

        public EventDispatcher(Framework framework)
        {
            this.framework = framework;
        }

        public void Add(IEventClient client)
        {
            this.clients[client.Id] = client;
            foreach (var type in client.EventTypes)
            {
                if (this.clientsByEventType[type] == null)
                    this.clientsByEventType[type] = new List<IEventClient>();
                this.clientsByEventType[type].Add(client);
            }
            PermanentQueue.AddReader(client);
        }

        public void Remove(IEventClient client)
        {
            this.clients[client.Id] = client;
            foreach (var type in client.EventTypes)
                this.clientsByEventType[type].Remove(client);
            PermanentQueue.RemoveReader(client);
        }

        public void Emit(Event e)
        {
            if (this.commandQueue == null)
            {
                this.commandQueue = new EventQueue(EventQueueId.Service, EventQueueType.Master, EventQueuePriority.Normal, 102400, null);
                this.commandQueue.Enqueue(new OnQueueOpened(this.commandQueue));
                this.framework.EventBus.CommandPipe.Add(this.commandQueue);
            }
            this.commandQueue.Enqueue(e);
            if (e.TypeId == EventType.Command)
            {
                var command = (Command)e;
                var response = new Response(command);
                if (Controller != null)
                {
                    Controller.OnCommand(command);
                    return;
                }
                switch (command.Type)
                {
                    case CommandType.GetInformation:
                        if (command[0] != null && command[0] is int && (int)command[0] == 0)
                        {
                            response.Type = ResponseType.Information;
                            response[0] = "Framework";
                            response[1] = this.framework.Name;
                            this.commandQueue.Enqueue(response);
                            return;
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        public void OnEvent(Event e)
        {
            if (e.TypeId == EventType.Response && ((Response)e).ReceiverId != -1)
            {
                this.clients[((Response)e).ReceiverId].OnEvent(e);
            }
            else
            {
                var clients = this.clientsByEventType[e.TypeId];
                if (clients != null)
                {
                    foreach (var c in clients)
                        if (c.IsOnEventEnabled)
                            c.OnEvent(e);
                }
            }

        }
    }
}