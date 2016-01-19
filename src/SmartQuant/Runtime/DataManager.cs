using System;
using System.Collections.Generic;
using System.Threading;

namespace SmartQuant
{
    public class DataManager
    {
        private class Class21
        {
            public List<HistoricalData> mYrahvpSih = new List<HistoricalData>();
            public ManualResetEvent manualResetEvent_0 = new ManualResetEvent(false);
        }

        private Framework framework;
        private Thread thread;
        private volatile bool exit;
        public DataServer Server { get; set; }

        private IdArray<Bid> idArray_0 = new IdArray<Bid>(10240);
        private IdArray<Ask> idArray_1 = new IdArray<Ask>(10240);
        private IdArray<Trade> idArray_2 = new IdArray<Trade>(10240);
        private IdArray<Bar> idArray_3 = new IdArray<Bar>(10240);
        private IdArray<News> idArray_5 = new IdArray<News>(10240);
        private IdArray<Fundamental> idArray_6 = new IdArray<Fundamental>(10240);

        private IdArray<IdArray<Bid>> idArray_7= new IdArray<IdArray<Bid>>(256);
        private IdArray<IdArray<Ask>> idArray_8 = new IdArray<IdArray<Ask>>(256);
        private IdArray<IdArray<Trade>> idArray_9 = new IdArray<IdArray<Trade>>(256);

        private Dictionary<string, DataManager.Class21> dictionary_0 = new Dictionary<string, Class21>();

        public DataManager(Framework framework, DataServer server)
        {
            this.framework = framework;
            Server = server;
            Server?.Open();
            this.thread = new Thread(ThreadRun) { Name = "Data Manager Thread", IsBackground = true };
            this.thread.Start();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.exit = true;
                this.thread.Join();
            }
        }

        public void Dump()
        {
            Console.WriteLine("Bid");
            for (int i = 0; i < this.idArray_0.Size; i++)
                if (this.idArray_0[i] != null)
                    Console.WriteLine(this.idArray_0[i]);

            Console.WriteLine("Ask");
            for (int i = 0; i < this.idArray_1.Size; i++)
                if (this.idArray_1[i] != null)
                    Console.WriteLine(this.idArray_1[i]);

            Console.WriteLine("Trade");
            for (int i = 0; i < this.idArray_2.Size; i++)
                if (this.idArray_2[i] != null)
                    Console.WriteLine(this.idArray_2[i]);
        }

        public void Clear()
        {
            this.idArray_0.Clear();
            this.idArray_1.Clear();
            this.idArray_2.Clear();
            //       this.djtaJvnAjO.Clear();
            this.idArray_3.Clear();
            //        this.idArray_4.Clear();
            this.idArray_5.Clear();
            this.idArray_6.Clear();
            this.idArray_7.Clear();
            this.idArray_8.Clear();
            this.idArray_9.Clear();
            //         this.idArray_10.Clear();
        }

        private void ThreadRun()
        {
            Console.WriteLine($"{DateTime.Now} Data manager thread started: Framework = {this.framework.Name}  Clock = {this.framework.Clock.GetModeAsString()}");
            var pipe = this.framework.EventBus.HistoricalPipe;
            while (!this.exit)
            {
                if (!pipe.IsEmpty())
                {
                    var e = pipe.Read();
                    byte typeId = e.TypeId;
                    switch (typeId)
                    {
                        case EventType.HistoricalData:
                            this.method_8((HistoricalData)e);
                            break;
                        case EventType.HistoricalDataEnd:
                            this.method_9((HistoricalDataEnd)e);
                            break;
                        case EventType.OnQueueOpened:
                        case EventType.OnQueueClosed:
                            break;
                        default:
                            Console.WriteLine($"DataManager::ThreadRun Error. Unknown event type : {e.TypeId}");
                            break;
                    }
                    this.framework.EventManager.Dispatcher.OnEvent(e);
                }
                else
                {
                    Thread.Sleep(1);
                }
            }
            Console.WriteLine($"{DateTime.Now} Data manager thread stopped: Framework = {this.framework.Name}  Clock = {this.framework.Clock.GetModeAsString()}");
        }

        #region Data Management
        public DataSeries GetDataSeries(Instrument instrument, byte type, BarType barType = BarType.Time, long barSize = 60) => Server.GetDataSeries(instrument, type, barType, barSize);

        public DataSeries GetDataSeries(string symbol, byte type, BarType barType = BarType.Time, long barSize = 60) => Server.GetDataSeries(this.framework.InstrumentManager.Instruments[symbol], type, barType, barSize);

        public DataSeries GetDataSeries(string name) => Server.GetDataSeries(name);

        public DataSeries AddDataSeries(string name) => Server.AddDataSeries(name);

        public DataSeries AddDataSeries(Instrument instrument, byte type) => Server.AddDataSeries(instrument, type, BarType.Time, 60);

        public DataSeries AddDataSeries(Instrument instrument, byte type, BarType barType = BarType.Time, long barSize = 60) => Server.AddDataSeries(instrument, type, barType, barSize);

        public void DeleteDataSeries(string name) => Server.DeleteDataSeries(name);

        public void DeleteDataSeries(Instrument instrument, byte type, BarType barType = BarType.Time, long barSize = 60) => Server.DeleteDataSeries(instrument, type, barType, barSize);

        public void DeleteDataSeries(string symbol, byte type, BarType barType = BarType.Time, long barSize = 60) => Server.DeleteDataSeries(this.framework.InstrumentManager.Instruments[symbol], type, barType, barSize);

        public List<DataSeries> GetDataSeriesList(Instrument instrument = null, string pattern = null) => Server.GetDataSeriesList(instrument, pattern);

        public Bid GetBid(int instrumentId) => this.idArray_0[instrumentId];

        public Ask GetAsk(int instrumentId) => this.idArray_1[instrumentId];

        #endregion

        #region EventHandlers

        internal void OnBid(Bid bid)
        {
            var iId = bid.InstrumentId;
            var pId = bid.ProviderId + 1;
            this.idArray_0[iId] = bid;
            this.idArray_7[pId] = this.idArray_7[pId] ?? new IdArray<Bid>(10240);
            this.idArray_7[pId][iId] = bid;
        }

        internal void OnAsk(Ask ask)
        {
            var iId = ask.InstrumentId;
            var pId = ask.ProviderId + 1;
            this.idArray_1[iId] = ask;
            this.idArray_8[pId] = this.idArray_8[pId] ?? new IdArray<Ask>(10240);
            this.idArray_8[pId][iId] = ask;
        }

        internal void OnTrade(Trade trade)
        {
            var iId = trade.InstrumentId;
            var pId = trade.ProviderId + 1;
            this.idArray_2[iId] = trade;
            this.idArray_9[pId] = this.idArray_9[pId]?? new IdArray<Trade>(10240);
            this.idArray_9[pId][iId] = trade;
        }

        internal void OnBar(Bar bar)
        {
            this.idArray_3[bar.InstrumentId] = bar;
        }

        internal void method_4(Level2Snapshot level2Snapshot_0)
        {
            //if (this.idArray_10[(int)(level2Snapshot_0.byte_0 + 1)] == null)
            //{
            //    this.idArray_10[(int)(level2Snapshot_0.byte_0 + 1)] = new IdArray<OrderBook>(10000);
            //}
            //OrderBook orderBook = this.idArray_10[(int)(level2Snapshot_0.byte_0 + 1)][level2Snapshot_0.int_0];
            //if (orderBook == null)
            //{
            //    orderBook = new OrderBook();
            //    this.idArray_10[(int)(level2Snapshot_0.byte_0 + 1)][level2Snapshot_0.int_0] = orderBook;
            //}
            //orderBook.method_0(level2Snapshot_0);
        }

        internal void method_5(Level2Update level2Update_0)
        {
            //if (this.idArray_10[(int)(level2Update_0.byte_0 + 1)] == null)
            //{
            //    this.idArray_10[(int)(level2Update_0.byte_0 + 1)] = new IdArray<OrderBook>(10000);
            //}
            //OrderBook orderBook = this.idArray_10[(int)(level2Update_0.byte_0 + 1)][level2Update_0.int_0];
            //if (orderBook == null)
            //{
            //    orderBook = new OrderBook();
            //    this.idArray_10[(int)(level2Update_0.byte_0 + 1)][level2Update_0.int_0] = orderBook;
            //}
            //orderBook.method_1(level2Update_0);
        }

        internal void method_8(HistoricalData historicalData_0)
        {
            //DataManager.Class21 @class;
            //bool flag2;
            //lock (this.dictionary_0)
            //{
            //    flag2 = this.dictionary_0.TryGetValue(historicalData_0.RequestId, out @class);
            //}
            //if (flag2)
            //{
            //    @class.zfxaWepjJx().Add(historicalData_0);
            //}
        }

        internal void method_9(HistoricalDataEnd historicalDataEnd_0)
        {
            //DataManager.Class21 @class;
            //bool flag2;
            //lock (this.dictionary_0)
            //{
            //    flag2 = this.dictionary_0.TryGetValue(historicalDataEnd_0.RequestId, out @class);
            //}
            //if (flag2)
            //{
            //    @class.method_1().Set();
            //}
        }

        internal void OnNews(News news)
        {
            this.idArray_5[news.InstrumentId] = news;
        }

        internal void OnFundamental(Fundamental fundamental)
        {
            if (fundamental.InstrumentId != -1)
                this.idArray_6[fundamental.InstrumentId] = fundamental;
        }

        #endregion
    }
}