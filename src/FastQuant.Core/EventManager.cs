using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SmartQuant
{
    public class EventFilter
    {
        private Framework framework;

        public EventFilter(Framework framework)
        {
            this.framework = framework;
        }

        public virtual Event Filter(Event e)
        {
            return e;
        }
    }

    public enum EventManagerStatus
    {
        Running,
        Paused,
        Stopping,
        Stopped
    }

    public class EventManager
    {
        private Framework framework;
        private EventBus bus;
        private bool stepping;
        private byte stepEvent = EventType.Bar;
        private volatile bool exiting;
        private Thread thread;
        private long eventCount;
        private delegate void Delegate(Event e);
        private IdArray<Delegate> gates = new IdArray<Delegate>(256);

        public EventManagerStatus Status { get; private set; }

        public BarFactory BarFactory { get; set; }
        public BarSliceFactory BarSliceFactory { get; set; }
        public EventDispatcher Dispatcher { get; set; }
        public EventLogger Logger { get; set; }
        public EventFilter Filter { get; set; }

        public long EventCount { get; private set; }
        public long DataEventCount { get; private set; }

        public IdArray<bool> Enabled { get; } = new IdArray<bool>(256);
        public EventManager(Framework framework, EventBus bus)
        {
            this.framework = framework;
            this.bus = bus;
            BarFactory = new BarFactory(this.framework);
            BarSliceFactory = new BarSliceFactory(this.framework);
            Dispatcher = new EventDispatcher(this.framework);

            this.gates[EventType.Bid] = new Delegate(OnBid);

            // Enable all events by defaults
            Parallel.ForEach(Enumerable.Range(0, 255), i => Enabled[i] = true);

            if (bus != null)
            {
                this.thread = new Thread(Run)
                {
                    Name = "Event Manager Thread",
                    IsBackground = true
                };
                this.thread.Start();
            }
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.exiting = true;
                //this.thread.Abort();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Thread Worker
        private void Run()
        {
            Console.WriteLine($"{DateTime.Now} Event manager thread started: Framework = {this.framework.Name} Clock = {this.framework.Clock.GetModeAsString()}");
            Status = EventManagerStatus.Running;
            while (exiting)
            {
                if (Status == EventManagerStatus.Running || (Status == EventManagerStatus.Paused && this.stepping))
                    OnEvent(this.bus.Dequeue());
                else
                    Thread.Sleep(1);
            }
            Console.WriteLine($"{DateTime.Now} Event manager thread stopped: Framework = {this.framework.Name} Clock = {this.framework.Clock.GetModeAsString()}");
        }

        public void Start()
        {
            if (Status != EventManagerStatus.Running)
            {
                Console.WriteLine($"{DateTime.Now} Event manager started at { this.framework.Clock.DateTime}");
                Status = EventManagerStatus.Running;
                OnEvent(new OnEventManagerStarted());
            }
        }

        public void Stop()
        {
            if (Status != EventManagerStatus.Stopped)
            {
                Console.WriteLine($"{DateTime.Now} Event manager stopping at {this.framework.Clock.DateTime}");
                Status = EventManagerStatus.Stopping;
                if (this.framework.Mode == FrameworkMode.Simulation)
                    OnEvent(new OnSimulatorStop());
                
                Status = EventManagerStatus.Stopped;
                this.framework.EventBus.Clear();
                OnEvent(new OnEventManagerStopped());
                Console.WriteLine($"{DateTime.Now} Event manager stopped at {this.framework.Clock.DateTime}");
            }
        }

        public void Pause(DateTime dateTime)
        {
            this.framework.Clock.AddReminder(new ReminderCallback(((time, data) => { Pause(); })), dateTime, null);
        }

        public void Pause()
        {
            if (Status != EventManagerStatus.Paused)
            {
                Console.WriteLine($"{DateTime.Now} Event manager paused at {this.framework.Clock.DateTime}");
                Status = EventManagerStatus.Paused;
                OnEvent(new OnEventManagerPaused());
            }
        }

        public void Resume()
        {
            if (Status != EventManagerStatus.Running)
            {
                Console.WriteLine($"{DateTime.Now} Event manager resumed at {this.framework.Clock.DateTime}");
                Status = EventManagerStatus.Running;
                OnEvent(new OnEventManagerResumed());
            }
        }

        public void Step(byte typeId = EventType.Event)
        {
            this.stepping = true;
            this.stepEvent = typeId;
            OnEvent(new OnEventManagerStep());
        }

        public void Clear()
        {
            EventCount = DataEventCount = 0;
            BarFactory.Clear();
            BarSliceFactory.Clear();
        }

        // The entry point of comsumer-side of events, may be recursively called.
        public void OnEvent(Event e)
        {
            if (Status == EventManagerStatus.Paused && this.stepping && (this.stepEvent == EventType.Event || this.stepEvent == e.TypeId))
            {
                Console.WriteLine($"{DateTime.Now} Event : {e}");
                this.stepping = false;
            }
            EventCount++;

            // Skip when this event is disabled
            if (!Enabled[e.TypeId])
                return;

            try
            {
                if (Filter != null && Filter.Filter(e) == null)
                    return;
            }
            catch (Exception ex)
            {
                OnEvent(new OnException("EventFilter", e, ex));
            }

            try
            {
                this.gates[e.TypeId]?.Invoke(e);
            }
            catch (Exception ex)
            {
                OnEvent(new OnException("EventHandler", e, ex));
            }

            try
            {
                Dispatcher?.OnEvent(e);
            }
            catch (Exception ex)
            {
                OnEvent(new OnException("EventDispatcher", e, ex));
            }

            try
            {
                Logger?.OnEvent(e);
            }
            catch (Exception ex)
            {
                this.OnEvent(new OnException("EventLogger", e, ex));
            }
        }

        #region eventhandlers

        private void OnBid(Event e)
        {
            DataEventCount++;
            var bid = (Bid)e;

            // Adjust clock
            if (this.framework.Clock.Mode == ClockMode.Simulation)
                this.framework.Clock.DateTime = bid.dateTime;
            else
                bid.dateTime = this.framework.Clock.DateTime;
            //TODO:WTF???
            //if (bid.ExchangeDateTime > this.framework.ExchangeClock.DateTime)
            //    this.framework.ExchangeClock.DateTime = bid.ExchangeDateTime;
            //else 
            //    Console.WriteLine($"EventManager::OnBid Exchange datetime is out of synch : bid datetime = { bid.ExchangeDateTime} clock datetime = { this.framework.ExchangeClock.DateTime}");

            BarFactory.OnData(bid);
            this.framework.DataManager.OnBid(bid);
            this.framework.InstrumentManager.GetById(bid.InstrumentId).Bid = bid;
            this.framework.ProviderManager.ExecutionSimulator.OnBid(bid);
            this.framework.StrategyManager.OnBid(bid);
        }
        
        #endregion

        private void AdjustClock(Tick tick)
        {
            if (this.framework.Clock.Mode == ClockMode.Simulation)
                this.framework.Clock.DateTime = tick.DateTime;
            else
                tick.DateTime = this.framework.Clock.DateTime;
        }
    }
}