using System;
using System.Collections.Generic;

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
    #endregion

    public class EventDispatcher
    {
        private Framework framework;

        private IdArray<IEventClient> clients = new IdArray<IEventClient>(1024);

        private IdArray<List<IEventClient>> clientsByEventType = new IdArray<List<IEventClient>>(1024);

        private EventQueue commandQueue;

        public PermanentQueue<Event> PermanentQueue { get; } = new PermanentQueue<Event>();

        public EventController Controller { get; set; }

        public FrameworkEventHandler FrameworkCleared { get; internal set; }

        public GroupEventHandler NewGroup { get; internal set; }

        public GroupEventEventHandler NewGroupEvent { get; internal set; }

        public GroupUpdateEventHandler NewGroupUpdate { get; internal set; }

        public event BidEventHandler Bid;
        public event AskEventHandler Ask;
        public event BarEventHandler Bar;

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