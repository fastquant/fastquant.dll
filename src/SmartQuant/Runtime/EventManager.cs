using System;
using System.Diagnostics;
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

        private Stopwatch stopwatch = new Stopwatch();
        private delegate void Delegate(Event e);
        private IdArray<Delegate> gates = new IdArray<Delegate>(256);
        public IdArray<bool> Enabled { get; } = new IdArray<bool>(256);

        public EventManagerStatus Status { get; private set; } = EventManagerStatus.Stopped;
        public BarFactory BarFactory { get; set; }
        public BarSliceFactory BarSliceFactory { get; set; }
        public EventDispatcher Dispatcher { get; set; }
        public EventLogger Logger { get; set; }
        public EventFilter Filter { get; set; }

        public long EventCount { get; private set; }
        public long DataEventCount { get; private set; }

        public EventManager(Framework framework, EventBus bus)
        {
            this.framework = framework;
            this.bus = bus;
            BarFactory = new BarFactory(this.framework);
            BarSliceFactory = new BarSliceFactory(this.framework);
            Dispatcher = new EventDispatcher(this.framework);

            // Event Gates
            this.gates[EventType.Bid] = new Delegate(OnBid);
            this.gates[EventType.Ask] = new Delegate(OnAsk);
            this.gates[EventType.Trade] = new Delegate(OnTrade);
            this.gates[EventType.Quote] = new Delegate(OnQuote);
            this.gates[EventType.Bar] = new Delegate(OnBar);
            this.gates[EventType.Level2Snapshot] = new Delegate(OnLevel2Snapshot);
            this.gates[EventType.Level2Update] = new Delegate(OnLevel2Update);            
            this.gates[EventType.ExecutionReport] = new Delegate(OnExecutionReport);
            this.gates[EventType.Reminder] = new Delegate(OnReminder);
            this.gates[EventType.ProviderError] = new Delegate(OnProviderError);
            this.gates[EventType.Fundamental] = new Delegate(OnFundamental);
            this.gates[EventType.News] = new Delegate(OnNews);
            this.gates[EventType.Group] = new Delegate(OnGroup);
            this.gates[EventType.GroupEvent] = new Delegate(OnGroupEvent);
            this.gates[EventType.Command] = new Delegate(OnCommand);
            this.gates[EventType.OnPositionOpened] = new Delegate(OnPositionOpened);
            this.gates[EventType.OnPositionClosed] = new Delegate(OnPositionClosed);
            this.gates[EventType.OnPositionChanged] = new Delegate(OnPositionChanged);
            this.gates[EventType.OnFill] = new Delegate(OnFill);
            this.gates[EventType.OnTransaction] = new Delegate(OnTransaction);
            this.gates[EventType.OnSendOrder] = new Delegate(OnSendOrder);
            this.gates[EventType.OnPendingNewOrder] = new Delegate(OnPendingNewOrder);
            this.gates[EventType.OnNewOrder] = new Delegate(OnNewOrder);
            this.gates[EventType.OnOrderStatusChanged] = new Delegate(OnOrderStatusChanged);
            this.gates[EventType.OnOrderPartiallyFilled] = new Delegate(OnOrderPartiallyFilled);
            this.gates[EventType.OnOrderFilled] = new Delegate(OnOrderFilled);
            this.gates[EventType.OnOrderReplaced] = new Delegate(OnOrderReplaced);
            this.gates[EventType.OnOrderCancelled] = new Delegate(OnOrderCancelled);
            this.gates[EventType.OnOrderRejected] = new Delegate(OnOrderRejected);
            this.gates[EventType.OnOrderExpired] = new Delegate(OnOrderExpired);
            this.gates[EventType.OnOrderCancelRejected] = new Delegate(OnOrderCancelRejected);
            this.gates[EventType.OnOrderReplaceRejected] = new Delegate(OnOrderReplaceRejected);
            this.gates[EventType.OnOrderDone] = new Delegate(OnOrderDone);
            this.gates[EventType.OnPortfolioAdded] = new Delegate(OnPortfolioAdded);
            this.gates[EventType.OnPortfolioRemoved] = new Delegate(OnPortfolioRemoved);
            this.gates[EventType.OnPortfolioParentChanged] = new Delegate(OnPortfolioParentChanged);
            this.gates[EventType.HistoricalData] = new Delegate(OnHistoricalData);
            this.gates[EventType.HistoricalDataEnd] = new Delegate(OnHistoricalDataEnd);
            this.gates[EventType.BarSlice] = new Delegate(OnBarSlice);
            this.gates[EventType.OnStrategyEvent] = new Delegate(OnStrategyEvent);
            this.gates[EventType.AccountData] = new Delegate(OnAccountData);
            this.gates[EventType.OnPropertyChanged] = new Delegate(OnPropertyChanged);
            this.gates[EventType.OnException] = new Delegate(OnException);
            this.gates[EventType.AccountReport] = new Delegate(OnAccountReport);
            this.gates[EventType.OnProviderConnected] = new Delegate(OnProviderConnected);
            this.gates[EventType.OnProviderDisconnected] = new Delegate(OnProviderDisconnected);
            this.gates[EventType.OnSimulatorStart] = new Delegate(OnSimulatorStart);
            this.gates[EventType.OnSimulatorStop] = new Delegate(OnSimulatorStop);

            // Enable all events by defaults
            for (int i = 0; i < byte.MaxValue; i++)
                Enabled[i] = true;

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
                this.thread.Join();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Clear()
        {
            EventCount = DataEventCount = 0;
            BarFactory.Clear();
            BarSliceFactory.Clear();
        }

        // Thread Worker
        private void Run()
        {
            Console.WriteLine($"{DateTime.Now} Event manager thread started: Framework = {this.framework.Name} Clock = {this.framework.Clock.GetModeAsString()}");
            Status = EventManagerStatus.Running;
            while (!this.exiting)
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
            this.framework.Clock.AddReminder(new ReminderCallback(((time, data) => Pause())), dateTime, null);
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

        #region EventHandlers

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
                OnEvent(new OnException("EventLogger", e, ex));
            }
        }

        private void OnBid(Event e)
        {
            DataEventCount++;
            var bid = (Bid)e;
            SyncLocalClockWithDataObject(bid);
            //if (this.framework.Clock.Mode == ClockMode.Simulation)
            //{
            //    this.framework.Clock.DateTime = bid.DateTime;
            //}
            //else
            //{
            //    bid.dateTime = this.framework.Clock.DateTime;
            //}
            SyncExchangeClockWithTick(bid, nameof(OnBid));
            //if (bid.ExchangeDateTime > this.framework.ExchangeClock.DateTime)
            //{
            //    this.framework.ExchangeClock.DateTime = bid.ExchangeDateTime;
            //}
            //else if (bid.ExchangeDateTime > this.framework.ExchangeClock.DateTime)
            //{
            //    Console.WriteLine($"EventManager::OnBid Exchange datetime is out of synch : bid datetime = {bid.ExchangeDateTime} clock datetime = {this.framework.ExchangeClock.DateTime}");
            //}
            BarFactory.OnData(bid);
            this.framework.DataManager.OnBid(bid);
            this.framework.InstrumentManager.GetById(bid.InstrumentId).Bid = bid;
            this.framework.ProviderManager.ExecutionSimulator.OnBid(bid);
            this.framework.StrategyManager.OnBid(bid);
        }

        private void OnAsk(Event e)
        {
            DataEventCount++;
            var ask = (Ask)e;
            SyncLocalClockWithDataObject(ask);

            //if (this.framework.Clock.Mode == ClockMode.Simulation)
            //{
            //    this.framework.Clock.DateTime = ask.DateTime;
            //}
            //else
            //{
            //    ask.dateTime = this.framework.Clock.DateTime;
            //}
            SyncExchangeClockWithTick(ask, nameof(OnAsk));

            //if (ask.ExchangeDateTime > this.framework.ExchangeClock.DateTime)
            //{
            //    this.framework.ExchangeClock.DateTime = ask.ExchangeDateTime;
            //}
            //else if (ask.ExchangeDateTime > this.framework.ExchangeClock.DateTime)
            //{
            //    Console.WriteLine($"EventManager::OnAsk Exchange datetime is out of synch : ask datetime = {ask.ExchangeDateTime} clock datetime = {this.framework.ExchangeClock.DateTime}");
            //}

            BarFactory.OnData(ask);
            this.framework.DataManager.OnAsk(ask);
            this.framework.InstrumentManager.GetById(ask.InstrumentId).Ask = ask;
            this.framework.ProviderManager.ExecutionSimulator.OnAsk(ask);
            this.framework.StrategyManager.OnAsk(ask);
        }

        private void OnTrade(Event e)
        {
            DataEventCount++;
            var trade = (Trade)e;
            SyncLocalClockWithDataObject(trade);

            //if (this.framework.Clock.Mode == ClockMode.Simulation)
            //    this.framework.Clock.DateTime = trade.DateTime;
            //else
            //    trade.DateTime = this.framework.Clock.DateTime;
            SyncExchangeClockWithTick(trade, nameof(OnTrade));

            //if (trade.ExchangeDateTime > this.framework.ExchangeClock.DateTime)
            //{
            //    this.framework.ExchangeClock.DateTime = trade.ExchangeDateTime;
            //}
            //else if (trade.ExchangeDateTime > this.framework.ExchangeClock.DateTime)
            //{
            //    Console.WriteLine($"EventManager::OnTrade Exchange datetime is out of synch : trade datetime = {trade.ExchangeDateTime} clock datetime = {this.framework.ExchangeClock.DateTime}");
            //}
            BarFactory.OnData(trade);
            this.framework.DataManager.OnTrade(trade);
            this.framework.InstrumentManager.GetById(trade.InstrumentId).Trade = trade;
            this.framework.ProviderManager.ExecutionSimulator.OnTrade(trade);
            this.framework.StrategyManager.OnTrade(trade);
        }

        private void OnQuote(Event e)
        {
            var quote = (Quote)e;
            if (quote.Bid != null && quote.Bid.Price != 0)
            {
                var bid = this.framework.DataManager.GetBid(quote.Bid.InstrumentId);
                if (bid == null || quote.Bid.Price != bid.Price || quote.Bid.Size != bid.Size)
                    OnBid(quote.Bid);
            }

            if (quote.Ask != null && quote.Ask.Price != 0)
            {
                var ask = this.framework.DataManager.GetAsk(quote.Ask.InstrumentId);
                if (ask == null || quote.Ask.Price != ask.Price || quote.Ask.Size != ask.Size)
                    OnAsk(quote.Ask);
            }
        }

        private void OnBar(Event e)
        {
            DataEventCount++;
            var bar = (Bar)e;
            if (this.framework.Clock.Mode == ClockMode.Simulation)
                this.framework.Clock.DateTime = bar.DateTime;

            if (bar.Status != BarStatus.Open)
            {
                this.framework.DataManager.OnBar(bar);
                this.framework.InstrumentManager.GetById(bar.InstrumentId).Bar = bar;
                this.framework.ProviderManager.ExecutionSimulator.OnBar(bar);
                this.framework.StrategyManager.OnBar(bar);
                if (bar.Type == BarType.Time || bar.Type == BarType.Session)
                {
                    BarSliceFactory.method_1(bar);
                }
                return;
            }
            if ((bar.Type == BarType.Time || bar.Type == BarType.Session) && !BarSliceFactory.method_0(bar))
            {
                return;
            }
            this.framework.ProviderManager.ExecutionSimulator.OnBarOpen(bar);
            this.framework.StrategyManager.OnBar(bar);
        }

        private void OnLevel2Snapshot(Event e)
        {
            DataEventCount++;
            var l2s = (Level2Snapshot)e;
            SyncLocalClockWithDataObject(l2s);
            //if (this.framework.Clock.Mode == ClockMode.Simulation)
            //{
            //    this.framework.Clock.DateTime = l2s.dateTime;
            //}
            //else
            //{
            //    l2s.dateTime = this.framework.Clock.DateTime;
            //}
            this.framework.DataManager.method_4(l2s);
            this.framework.ProviderManager.ExecutionSimulator.OnLevel2(l2s);
            this.framework.StrategyManager.method_10(l2s);
        }

        private void OnLevel2Update(Event e)
        {
            DataEventCount++;
            var l2u = (Level2Update)e;
            SyncLocalClockWithDataObject(l2u);

            //if (this.framework.Clock.Mode == ClockMode.Simulation)
            //{
            //    this.framework.Clock.DateTime = l2u.dateTime;
            //}
            //else
            //{
            //    l2u.dateTime = this.framework.Clock.DateTime;
            //}
            this.framework.DataManager.method_5(l2u);
            this.framework.ProviderManager.ExecutionSimulator.OnLevel2(l2u);
            this.framework.StrategyManager.method_11(l2u);
        }

        private void OnNews(Event e)
        {
            DataEventCount++;
            var news = (News)e;
            SyncLocalClockWithDataObject(news);

            //if (this.framework.Clock.Mode == ClockMode.Simulation)
            //{
            //    this.framework.Clock.DateTime = news.dateTime;
            //}
            //else
            //{
            //    news.dateTime = this.framework.Clock.DateTime;
            //}
            this.framework.DataManager.method_6(news);
            this.framework.StrategyManager.method_15(news);
        }

        private void OnFundamental(Event e)
        {
            DataEventCount++;
            var fundamental = (Fundamental)e;
            SyncLocalClockWithDataObject(fundamental);
            //if (this.framework.Clock.Mode == ClockMode.Simulation)
            //{
            //    this.framework.Clock.DateTime = fundamental.dateTime;
            //}
            //else
            //{
            //    fundamental.dateTime = this.framework.Clock.DateTime;
            //}
            this.framework.DataManager.nBvFytknIm(fundamental);
            this.framework.StrategyManager.gudLdqclqe(fundamental);
        }

        private void OnFill(Event e)
        {
            this.framework.StrategyManager.method_30((OnFill)e);
        }

        private void OnTransaction(Event e)
        {
            this.framework.StrategyManager.method_31((OnTransaction)e);
        }

        private void OnAccountReport(Event e)
        {
            var report = (AccountReport)e;
            this.framework.OrderManager.method_1(report);
            this.framework.PortfolioManager.method_1(report);
            this.framework.StrategyManager.method_29(report);
        }

        private void OnAccountData(Event e)
        {
            var data = (AccountData)e;
            this.framework.AccountDataManager.method_1(data);
            this.framework.StrategyManager.method_37(data);
        }

        private void OnHistoricalDataEnd(Event e)
        {
            this.framework.DataManager.method_8((HistoricalDataEnd)e);
        }

        private void OnHistoricalData(Event e)
        {
            this.framework.DataManager.method_7((HistoricalData)e);
        }

        private void OnOrderDone(Event e)
        {
            this.framework.StrategyManager.method_28(((OnOrderDone)e).Order);
        }

        private void OnOrderReplaceRejected(Event e)
        {
            this.framework.StrategyManager.method_27(((OnOrderReplaceRejected)e).Order);
        }

        private void OnOrderCancelRejected(Event e)
        {
            this.framework.StrategyManager.method_26(((OnOrderCancelRejected)e).Order);
        }

        private void OnOrderExpired(Event e)
        {
            this.framework.StrategyManager.method_25(((OnOrderExpired)e).Order);
        }

        private void OnOrderRejected(Event e)
        {
            this.framework.StrategyManager.method_24(((OnOrderRejected)e).Order);
        }

        private void OnOrderCancelled(Event e)
        {
            this.framework.StrategyManager.method_22(((OnOrderCancelled)e).Order);
        }

        private void OnOrderReplaced(Event e)
        {
            this.framework.StrategyManager.method_23(((OnOrderReplaced)e).Order);
        }

        private void OnOrderFilled(Event e)
        {
            this.framework.StrategyManager.method_20(((OnOrderFilled)e).Order);
        }

        private void OnOrderPartiallyFilled(Event e)
        {
            this.framework.StrategyManager.method_21(((OnOrderPartiallyFilled)e).Order);
        }

        private void OnOrderStatusChanged(Event e)
        {
            this.framework.StrategyManager.method_19(((OnOrderStatusChanged)e).Order);
        }

        private void OnNewOrder(Event e)
        {
            this.framework.StrategyManager.method_18(((OnNewOrder)e).Order);
        }

        private void OnSendOrder(Event e)
        {
            this.framework.StrategyManager.method_16(((OnSendOrder)e).Order);
        }

        private void OnCommand(Event e)
        {
            this.framework.StrategyManager.method_38((Command)e);
        }

        private void OnGroupEvent(Event e)
        {
            this.framework.GroupManager.OnGroupEvent((GroupEvent)e);
        }

        private void OnGroup(Event e)
        {
            this.framework.GroupManager.OnGroup((Group)e);
        }


        private void OnExecutionReport(Event e)
        {
            var report = (ExecutionReport)e;
            if (!report.IsLoaded)
            {
                if (this.framework.Clock.Mode == ClockMode.Realtime)
                {
                    report.dateTime = this.framework.Clock.DateTime;
                }
                this.framework.OrderManager.method_0(report);
                this.framework.PortfolioManager.method_0(report);
                this.framework.StrategyManager.IbsLpdRkc3(report);
                this.framework.EventServer.EmitQueued();
                return;
            }
            this.framework.OrderManager.method_5(report);
            if (report.Order != null && report.Order.Portfolio != null)
            {
                this.framework.PortfolioManager.method_0(report);
            }
            this.framework.StrategyManager.IbsLpdRkc3(report);
            this.framework.EventServer.EmitQueued();
        }

        private void OnReminder(Event e)
        {
            var reminder = (Reminder)e;
            if ((reminder.Clock.Type == ClockType.Local && reminder.Clock.Mode == ClockMode.Simulation) || reminder.Clock.Type == ClockType.Exchange)
                reminder.Clock.DateTime = e.dateTime;
            ((Reminder)e).Execute();
        }


        private void OnProviderError(Event e)
        {
            this.framework.StrategyManager.method_6((ProviderError)e);
        }

        public void OnPortfolioAdded(Event e)
        {
            this.framework.StrategyManager.method_34(((OnPortfolioAdded)e).Portfolio);
        }

        public void OnPortfolioParentChanged(Event e)
        {
            this.framework.StrategyManager.method_36(((OnPortfolioParentChanged)e).Portfolio);
        }

        public void OnPortfolioRemoved(Event e)
        {
            this.framework.StrategyManager.method_35(((OnPortfolioRemoved)e).Portfolio);
        }

        private void OnPropertyChanged(Event e)
        {
            this.framework.StrategyManager.method_40((OnPropertyChanged)e);
        }

        private void OnBarSlice(Event e)
        {
            BarSlice barSlice = (BarSlice)e;
            barSlice.DateTime = this.framework.Clock.DateTime;
            this.framework.StrategyManager.method_14(barSlice);
        }

        private void OnPositionOpened(Event e)
        {
            var opened = (OnPositionOpened)e;
            this.framework.StrategyManager.vMaLxjraoe(opened.Portfolio, opened.Position);
        }

        private void OnPositionClosed(Event e)
        {
            var closed = (OnPositionClosed)e;
            this.framework.StrategyManager.method_32(closed.Portfolio, closed.Position);
        }

        private void OnPositionChanged(Event e)
        {
            var changed = (OnPositionChanged)e;
            this.framework.StrategyManager.method_33(changed.Portfolio, changed.Position);
        }

        public void OnException(Event e)
        {
            var ex = (OnException)e;
            Console.WriteLine($"EventManager::OnException Exception occured in {ex.Source} - {ex.Event} - {ex.Exception}");
            this.framework.StrategyManager.method_46(ex.Source, ex.Event, ex.Exception);
        }

        private void OnPendingNewOrder(Event e)
        {
            this.framework.StrategyManager.method_17(((OnPendingNewOrder)e).Order);
        }

        private void OnStrategyEvent(Event e)
        {
            this.framework.StrategyManager.method_45(((OnStrategyEvent)e).Data);
        }

        private void OnProviderConnected(Event e)
        {
            var p = ((OnProviderConnected)e).Provider;
            if (p is IDataProvider)
                this.framework.SubscriptionManager.OnProviderConnected((IDataProvider)p);
            this.framework.StrategyManager.method_4(p);
        }

        private void OnProviderDisconnected(Event e)
        {
            var p = ((OnProviderDisconnected)e).Provider;
            if (p is IDataProvider)
                this.framework.SubscriptionManager.OnProviderDisconnected((IDataProvider)p);
            this.framework.StrategyManager.method_5(p);
        }

        private void OnSimulatorStart(Event e)
        {
            var start = (OnSimulatorStart)e;
            if (this.framework.Clock.Mode == ClockMode.Simulation)
                this.framework.Clock.DateTime = start.DateTime1;

            this.bus?.ResetCounts();
            EventCount = 0;
            DataEventCount = 0;
            this.stopwatch.Reset();
            this.stopwatch.Start();
        }

        private void OnSimulatorStop(Event e)
        {
            this.framework.StrategyManager.Stop();
            this.stopwatch.Stop();
            long ms = this.stopwatch.ElapsedMilliseconds;
            if (ms != 0)
                Console.WriteLine($"{DateTime.Now} Data run done, count = {DataEventCount} ms = {this.stopwatch.ElapsedMilliseconds}  event/sec = {DataEventCount / ms * 1000}");
            else
                Console.WriteLine($"{DateTime.Now} Data run done, count = {DataEventCount} ms = 0");
        }

        #endregion

        #region Helper Functions
        private void SyncLocalClockWithDataObject(DataObject obj)
        {
            if (this.framework.Clock.Mode == ClockMode.Simulation)
                this.framework.Clock.DateTime = obj.DateTime;
            else
                obj.DateTime = this.framework.Clock.DateTime;
        }

        private void SyncExchangeClockWithTick(Tick tick, string funcName)
        {
            if (tick.ExchangeDateTime > this.framework.ExchangeClock.DateTime)
            {
                this.framework.ExchangeClock.DateTime = tick.ExchangeDateTime;
            }
            else if (tick.ExchangeDateTime > this.framework.ExchangeClock.DateTime)
            {
                Console.WriteLine($"{nameof(EventManager)}::{funcName} Exchange datetime is out of synch : {tick.GetType().Name.ToLower()} datetime = {tick.ExchangeDateTime} clock datetime = {this.framework.ExchangeClock.DateTime}");
            }
        }
        #endregion
    }
}