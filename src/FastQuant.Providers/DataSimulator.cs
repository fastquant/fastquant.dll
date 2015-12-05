using System;
using System.Collections.Generic;
using System.Threading;

namespace SmartQuant
{
    public class DataSimulator : Provider, IDataSimulator
    {
        public BarFilter BarFilter { get; } = new BarFilter();
        public DataProcessor Processor { get; set; } = new DataProcessor();

        public bool RunOnSubscribe { get; set; }
        public DateTime DateTime1 { get; set; }
        public DateTime DateTime2 { get; set; }
        public bool SubscribeBid { get; set; }
        public bool SubscribeAsk { get; set; }
        public bool SubscribeBar { get; set; }
        public bool SubscribeTrade { get; set; }
        public bool SubscribeQuote { get; set; }
        public bool SubscribeLevelII { get; set; }
        public bool SubscribeNews { get; set; }
        public bool SubscribeFundamental { get; set; }

        public bool SubscribeAll { set { SubscribeBid = SubscribeAsk = SubscribeBar = SubscribeTrade = SubscribeQuote = SubscribeLevelII = SubscribeNews = SubscribeFundamental = value; } }

        public List<IDataSeries> Series { get; set; }

        private LinkedList<DataSeriesEmitter> emitters = new LinkedList<DataSeriesEmitter>();

        public DataSimulator(Framework framework) : base(framework)
        {
            this.id = ProviderId.DataSimulator;
            this.name = "DataSimulator";
            this.description = "Default data simulator";
            this.url = "fastquant.org";
            DateTime1 = DateTime.MinValue;
            DateTime2 = DateTime.MaxValue;
        }

        public override void Clear()
        {
            Series.Clear();
            DateTime1 = DateTime.MinValue;
            DateTime2 = DateTime.MaxValue;
        }

        public override void Connect()
        {
        }

        public override void Disconnect()
        {
        }

        public void Run()
        {
            new Thread(() =>
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

                Console.WriteLine($"{DateTime.Now} Data simulator thread stopped");
            })
            {
                Name = "Data Simulator Thread",
                IsBackground = true
            }.Start();
        }

        public override void Subscribe(InstrumentList instrument)
        {
            throw new NotImplementedException();
        }

        public override void Subscribe(Instrument instrument)
        {
            throw new NotImplementedException();
        }

        private void Subscribe(Instrument instrument, DateTime dateTime1, DateTime dateTime2)
        {
            Console.WriteLine($"{DateTime.Now} DataSimulator::Subscribe {instrument.Symbol}");
            if (SubscribeTrade)
            {
                var series = this.framework.DataManager.GetDataSeries(instrument, DataObjectType.Trade, BarType.Time, 60);
                if (series != null)
                    this.emitters.Add(CreateDataSeriesEmitter(series, dateTime1, dateTime2));
            }
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
        internal bool done;
        internal DataObject dataObject_0;
        internal EventQueue eventQueue_0;
        internal EventQueue eventQueue_1;
        private DataProcessor processor;
        private IDataSeries series;

        private int step;
        private int count;
        private int percent;
        private long index1;
        private long index2;
        private long current;

        internal long long_2;

        public DataSeriesEmitter(IDataSeries series, DateTime dateTime1, DateTime dateTime2, EventQueue queue, DataProcessor processor)
        {
            this.eventQueue_1 = new EventQueue(EventQueueId.All, EventQueueType.Master, EventQueuePriority.Normal, 128, null);
            this.series = series;
           // this.queue = queue;
            this.processor = processor ?? new DataProcessor();
            this.index1 = dateTime1 == DateTime.MinValue || dateTime1 < this.series.DateTime1
                ? 0
                : this.series.GetIndex(dateTime1, SearchOption.Next);
            this.index2 = dateTime2 == DateTime.MaxValue || dateTime2 > this.series.DateTime2
                ? this.series.Count - 1
                : this.series.GetIndex(dateTime2, SearchOption.Prev);
            this.current = this.index1;
            this.step = (int)Math.Ceiling(this.index2 - this.index1 + 1 / 100.0);
            this.count = this.step;
            this.percent = 0;
        }

        //internal bool method_1()
        //{
        //    if (!this.queue.IsFull())
        //    {
        //        DataObject obj;
        //        while (this.eventQueue_1.IsEmpty())
        //        {
        //            if (this.dataObject_0 != null)
        //            {
        //                obj = this.dataObject_0;
        //                this.dataObject_0 = null;
        //                IL_B3:
        //                this.eventQueue_0.Write(obj);
        //                if (this.long_3 == this.count)
        //                {
        //                    this.count += this.step;
        //                    this.percent++;
        //                    this.eventQueue_0.Enqueue(new OnSimulatorProgress(this.count, this.percent));
        //                }
        //                return true;
        //            }
        //            if (this.long_2 > this.long_1)
        //            {
        //                this.done = true;
        //                return false;
        //            }
        //            this.dataObject_0 = this.series[this.long_2];
        //            this.dataObject_0 = this.processor.method_0(this);
        //            this.long_2++;
        //            this.long_3++;
        //        }
        //        obj = (DataObject)this.eventQueue_1.Read();
        //        goto IL_B3;
        //    }
        //    return false;
        //}


    }
}