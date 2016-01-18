using System;
using System.Collections.Generic;
using System.Threading;

namespace SmartQuant
{
    public class DataSimulator : Provider, IDataSimulator
    {
        private volatile bool exit;
        private volatile bool running;
        private long objCount;
        private Thread thread;

        public BarFilter BarFilter { get; } = new BarFilter();

        public DataProcessor Processor { get; set; } = new DataProcessor();

        public bool RunOnSubscribe { get; set; } = true;

        public DateTime DateTime1 { get; set; } = DateTime.MinValue;

        public DateTime DateTime2 { get; set; } = DateTime.MaxValue;

        public bool SubscribeBid { get; set; } = true;

        public bool SubscribeAsk { get; set; } = true;

        public bool SubscribeQuote { get; set; } = true;

        public bool SubscribeTrade { get; set; } = true;

        public bool SubscribeLevelII { get; set; } = true;

        public bool SubscribeBar { get; set; } = true;

        public bool SubscribeNews { get; set; } = true;

        public bool SubscribeFundamental { get; set; } = true;

        public bool SubscribeAll { set { SubscribeBid = SubscribeAsk = SubscribeBar = SubscribeTrade = SubscribeQuote = SubscribeLevelII = SubscribeNews = SubscribeFundamental = value; } }

        public List<IDataSeries> Series { get; set; } = new List<IDataSeries>();

        private LinkedList<DataSeriesEmitter> emitters = new LinkedList<DataSeriesEmitter>();

        public DataSimulator(Framework framework) : base(framework)
        {
            this.id = ProviderId.DataSimulator;
            this.name = "DataSimulator";
            this.description = "Default data simulator";
            this.url = "fastquant.org";
        }

        public override void Clear()
        {
            Series.Clear();
            this.emitters.Clear();
            DateTime1 = DateTime.MinValue;
            DateTime2 = DateTime.MaxValue;
        }

        public override void Connect()
        {
            if (!IsConnected)
                Status = ProviderStatus.Connected;
        }

        protected override void OnConnected()
        {
            foreach (var s in Series)
            {
                var q = new EventQueue(EventQueueId.Data, EventQueueType.Master, EventQueuePriority.Normal, 25600, null)
                {
                    IsSynched = true,
                    Name = s.Name
                };
                q.Enqueue(new OnQueueOpened(q));
                this.framework.EventBus.DataPipe.Add(q);
                this.emitters.Add(new DataSeriesEmitter(s, DateTime1, DateTime2, q, Processor));
            }
        }


        public override void Disconnect()
        {
            if (!IsDisconnected)
            {
                this.exit = true;
                while (this.running)
                    Thread.Sleep(1);
                Clear();
                Status = ProviderStatus.Disconnected;
            }
        }

        protected override void OnDisconnected()
        {
            // do nothing
        }

        public void Run()
        {
            this.thread = new Thread(() =>
            {
                Console.WriteLine($"{DateTime.Now} Data simulator thread started");
                if (!IsConnected)
                    Connect();

                var q = new EventQueue(EventQueueId.Data, EventQueueType.Master, EventQueuePriority.Normal, 16, null)
                {
                    Name = "Data Simulator Start Queue",
                    IsSynched = true
                };
                q.Enqueue(new OnQueueOpened(q));
                q.Enqueue(new OnSimulatorStart(DateTime1, DateTime2, 0));
                q.Enqueue(new OnQueueClosed(q));
                this.framework.EventBus.DataPipe.Add(q);

                this.running = true;
                this.exit = false;
                while (!this.exit)
                {
                    LinkedListNode<DataSeriesEmitter> lastNode = null;
                    var node = this.emitters.First;
                    while (node != null)
                    {
                        var emitter = node.Data;
                        if (!emitter.Done)
                        {
                            if (emitter.Emit())
                                this.objCount++;
                            lastNode = node;
                        }
                        else
                        {
                            if (lastNode == null)
                                this.emitters.First = node.Next;
                            else
                                lastNode.Next = node.Next;
                            this.emitters.Count--;
                            emitter.dataQueue.Enqueue(new OnQueueClosed(emitter.dataQueue));
                        }
                        node = node.Next;
                    }
                }
                this.exit = false;
                this.running = false;

                Console.WriteLine($"{DateTime.Now} Data simulator thread stopped");
            })
            {
                Name = "Data Simulator Thread",
                IsBackground = true
            };
            this.thread.Start();
        }

        public override void Subscribe(InstrumentList instruments)
        {
            foreach (var instrument in instruments)
                Subscribe(instrument, this.running ? this.framework.Clock.DateTime : DateTime1, DateTime2);

            if (!this.running && RunOnSubscribe)
                Run();
        }

        public override void Subscribe(Instrument instrument)
        {
            Subscribe(instrument, this.running ? this.framework.Clock.DateTime : DateTime1, DateTime2);
            if (!this.running && RunOnSubscribe)
                Run();
        }

        private void Subscribe(Instrument instrument, DateTime dateTime1, DateTime dateTime2)
        {
            Console.WriteLine($"{DateTime.Now} DataSimulator::Subscribe {instrument.Symbol}");
            var list = new List<DataSeries>();
            if (SubscribeTrade)
            {
                var series = this.framework.DataManager.GetDataSeries(instrument, DataObjectType.Trade, BarType.Time, 60);
                if (series != null)
                    list.Add(series);
            }
            if (SubscribeBid)
            {
                var series = this.framework.DataManager.GetDataSeries(instrument, DataObjectType.Bid, BarType.Time, 60);
                if (series != null)
                    list.Add(series);
            }
            if (SubscribeAsk)
            {
                var series = this.framework.DataManager.GetDataSeries(instrument, DataObjectType.Ask, BarType.Time, 60);
                if (series != null)
                    list.Add(series);
            }
            if (SubscribeQuote)
            {
                var series = this.framework.DataManager.GetDataSeries(instrument, DataObjectType.Quote, BarType.Time, 60);
                if (series != null)
                    list.Add(series);
            }
            if (SubscribeBar)
            {
                var dataSeriesList = this.framework.DataManager.GetDataSeriesList(instrument, "Bar");
                foreach (DataSeries series in dataSeriesList)
                {
                    if (BarFilter.Count != 0)
                    {
                        BarType barType;
                        long barSize;
                        DataSeriesNameHelper.TryGetBarTypeSize(series, out barType, out barSize);
                        if (!BarFilter.Contains(barType, barSize))
                            continue;
                    }
                    list.Add(series);
                }
            }
            if (SubscribeLevelII)
            {
                var series = this.framework.DataManager.GetDataSeries(instrument, DataObjectType.Level2, BarType.Time, 60);
                if (series != null)
                    list.Add(series);
            }
            if (SubscribeFundamental)
            {
                var series = this.framework.DataManager.GetDataSeries(instrument, DataObjectType.Fundamental, BarType.Time, 60);
                if (series != null)
                    list.Add(series);
            }
            if (SubscribeNews)
            {
                var series = this.framework.DataManager.GetDataSeries(instrument, DataObjectType.News, BarType.Time, 60);
                if (series != null)
                    list.Add(series);
            }

            foreach (var s in list)
                this.emitters.Add(CreateDataSeriesEmitter(s, dateTime1, dateTime2));
        }

        private DataSeriesEmitter CreateDataSeriesEmitter(DataSeries series, DateTime dateTime1, DateTime dateTime2)
        {
            var q = new EventQueue(EventQueueId.Data, EventQueueType.Master, EventQueuePriority.Normal, 25600, null)
            {
                IsSynched = true,
                Name = series.Name
            };
            q.Enqueue(new OnQueueOpened(q));
            this.framework.EventBus.DataPipe.Add(q);
            return new DataSeriesEmitter(series, dateTime1, dateTime2, q, Processor);
        }
    }

    class DataSeriesEmitter
    {
        public DataSeriesEmitter(IDataSeries series, DateTime dateTime1, DateTime dateTime2, EventQueue dataQueue, DataProcessor processor)
        {
            this.queue = new EventQueue(EventQueueId.All, EventQueueType.Master, EventQueuePriority.Normal, 128, null);
            this.series = series;
            this.dataQueue = dataQueue;
            this.processor = processor ?? new DataProcessor();
            this.index1 = dateTime1 == DateTime.MinValue || dateTime1 < series.DateTime1 ? 0 : series.GetIndex(dateTime1, SearchOption.Next);
            this.index2 = dateTime2 == DateTime.MaxValue || dateTime2 > series.DateTime2 ? series.Count - 1 : series.GetIndex(dateTime2, SearchOption.Prev);
            this.current = this.index1;
            this.step = (int)Math.Ceiling((this.index2 - this.index1 + 1) / 100.0);
            this.count = this.step;
            this.percent = 0;
        }

        //internal bool Emit()
        //{
        //    if (this.dataQueue.IsFull())
        //        return false;
        //    DataObject obj;
        //    while (this.queue.IsEmpty())
        //    {
        //        if (this.obj != null)
        //        {
        //            obj = this.obj;
        //            this.obj = null;
        //            IL_B3:
        //            this.dataQueue.Write(obj);
        //            if (this.long_3 == this.count)
        //            {
        //                this.count += this.step;
        //                this.percent++;
        //                this.dataQueue.Enqueue(new OnSimulatorProgress(this.count, this.percent));
        //            }
        //            return true;
        //        }
        //        if (this.current > this.index2)
        //        {
        //            Done = true;
        //            return false;
        //        }
        //        this.obj = this.series[this.current];
        //        this.obj = this.processor.Process(this);
        //        this.current += 1;
        //        this.long_3 += 1;
        //    }
        //    obj = (DataObject)this.queue.Read();
        //    goto IL_B3;
        //}


        internal bool Emit()
        {
            if (this.dataQueue.IsFull())
                return false;

            DataObject obj;
            while (this.queue.IsEmpty())
            {
                if (this.obj != null)
                {
                    obj = this.obj;
                    this.obj = null;

                    this.dataQueue.Write(obj);
                    if (this.long_3 == this.count)
                    {
                        this.count += this.step;
                        this.percent++;
                        this.dataQueue.Enqueue(new OnSimulatorProgress(this.count, this.percent));
                    }
                    return true;
                }
                if (this.current > this.index2)
                {
                    Done = true;
                    return false;
                }
                this.obj = this.series[this.current];
                this.obj = this.processor.Process(this);
                this.current++;
                this.long_3++;
            }

            obj = (DataObject)this.queue.Read();

            this.dataQueue.Write(obj);
            if (this.long_3 == this.count)
            {
                this.count += this.step;
                this.percent++;
                this.dataQueue.Enqueue(new OnSimulatorProgress(this.count, this.percent));
            }
            return true;
        }

        internal bool Done { get; set; }

        internal DataObject obj;

        private DataProcessor processor;

        internal EventQueue dataQueue;

        internal EventQueue queue;

        private IDataSeries series;

        private int step;

        private long count;

        private int percent;

        private long index1;

        private long index2;

        private long current;

        private long long_3;
    }
}