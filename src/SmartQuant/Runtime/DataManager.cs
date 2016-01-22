using System;
using System.Collections.Generic;
using System.Threading;

namespace SmartQuant
{
    public class DataManager
    {
        private class DataNotifier
        {
            public List<HistoricalData> Data { get; } = new List<HistoricalData>();
            public ManualResetEvent ReadyEvent { get; } = new ManualResetEvent(false);
        }

        private Framework framework;
        private Thread thread;
        private volatile bool exit;
        public DataServer Server { get; set; }

        // lastest dataobject by instrument id
        private IdArray<Bid> latestBid = new IdArray<Bid>(10240);
        private IdArray<Ask> latestAsk = new IdArray<Ask>(10240);
        private IdArray<Trade> latestTrade = new IdArray<Trade>(10240);
        private IdArray<Bar> latestBar = new IdArray<Bar>(10240);
        private IdArray<News> latestNews = new IdArray<News>(10240);
        private IdArray<Fundamental> latestFundamental = new IdArray<Fundamental>(10240);

        // lastest dataobject by instrument id and provider id
        private IdArray<IdArray<Bid>> bidByIIdAndPId = new IdArray<IdArray<Bid>>(256);
        private IdArray<IdArray<Ask>> askByIIdAndPId = new IdArray<IdArray<Ask>>(256);
        private IdArray<IdArray<Trade>> tradeByIIdAndPId = new IdArray<IdArray<Trade>>(256);

        private IdArray<OrderBook> idArray_3 = new IdArray<OrderBook>(10240);
        private IdArray<OrderBookAggr> idArray_4 = new IdArray<OrderBookAggr>(10240);
        private IdArray<IdArray<OrderBook>> idArray_10 = new IdArray<IdArray<OrderBook>>(256);

        private Dictionary<string, DataNotifier> dictionary_0 = new Dictionary<string, DataNotifier>();

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
            for (int i = 0; i < this.latestBid.Size; i++)
                if (this.latestBid[i] != null)
                    Console.WriteLine(this.latestBid[i]);

            Console.WriteLine("Ask");
            for (int i = 0; i < this.latestAsk.Size; i++)
                if (this.latestAsk[i] != null)
                    Console.WriteLine(this.latestAsk[i]);

            Console.WriteLine("Trade");
            for (int i = 0; i < this.latestTrade.Size; i++)
                if (this.latestTrade[i] != null)
                    Console.WriteLine(this.latestTrade[i]);
        }

        public void Clear()
        {
            this.latestBid.Clear();
            this.latestAsk.Clear();
            this.latestTrade.Clear();
            this.latestBar.Clear();
            this.latestNews.Clear();
            this.latestFundamental.Clear();
            this.idArray_3.Clear();
            this.idArray_4.Clear();
            this.bidByIIdAndPId.Clear();
            this.askByIIdAndPId.Clear();
            this.tradeByIIdAndPId.Clear();
            this.idArray_10.Clear();
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
                            this.OnHistoricalData((HistoricalData)e);
                            break;
                        case EventType.HistoricalDataEnd:
                            this.OnHistoricalDataEnd((HistoricalDataEnd)e);
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

        public Bid GetBid(int instrumentId) => this.latestBid[instrumentId];

        public Bid GetBid(Instrument instrument) => this.latestBid[instrument.Id];

        public Bid GetBid(Instrument instrument, byte providerId) => this.bidByIIdAndPId[providerId + 1]?[instrument.Id];

        public Bid GetBid(int instrumentId, byte providerId) => this.bidByIIdAndPId[providerId + 1]?[instrumentId];

        public Ask GetAsk(int instrumentId) => this.latestAsk[instrumentId];

        public Ask GetAsk(Instrument instrument) => this.latestAsk[instrument.Id];

        public Ask GetAsk(Instrument instrument, byte providerId) => this.askByIIdAndPId[providerId + 1]?[instrument.Id];

        public Ask GetAsk(int instrumentId, byte providerId) => this.askByIIdAndPId[providerId + 1]?[instrumentId];

        public Trade GetTrade(Instrument instrument) => this.latestTrade[instrument.Id];

        public Trade GetTrade(int instrumentId) => this.latestTrade[instrumentId];

        public Trade GetTrade(Instrument instrument, byte providerId) => this.tradeByIIdAndPId[providerId + 1]?[instrument.Id];

        public Trade GetTrade(int instrumentId, byte providerId) => this.tradeByIIdAndPId[providerId + 1]?[instrumentId];

        public Bar GetBar(int instrumentId) => this.latestBar[instrumentId];

        public Bar GetBar(Instrument instrument) => this.latestBar[instrument.Id];

        public TimeSeries AddTimeSeries(string name) => new TimeSeries(Server.AddDataSeries(name));

        public TimeSeries GetTimeSeries(string name) => new TimeSeries(Server.GetDataSeries(name));

        public OrderBook GetOrderBook(Instrument instrument) => this.idArray_3[instrument.Id];

        public OrderBook GetOrderBook(int instrumentId) => this.idArray_3[instrumentId];

        public OrderBook GetOrderBook(Instrument instrument, byte providerId) => this.idArray_10[providerId + 1]?[instrument.Id];

        public OrderBook GetOrderBook(int instrumentId, byte providerId) => this.idArray_10[providerId + 1]?[instrumentId];

        public OrderBookAggr GetOrderBookAggr(Instrument instrument) => GetOrderBookAggr(instrument.Id);

        public OrderBookAggr GetOrderBookAggr(int instrumentId) => this.idArray_4[instrumentId];

        public TickSeries GetHistoricalTicks(TickType type, string symbol) => GetHistoricalTicks(type, symbol, DateTime.MinValue, DateTime.MaxValue);

        public TickSeries GetHistoricalTicks(TickType type, Instrument instrument) => GetHistoricalTicks(type, instrument, DateTime.MinValue, DateTime.MaxValue);

        public TickSeries GetHistoricalTicks(TickType type, string symbol, DateTime dateTime1, DateTime dateTime2) => GetHistoricalTicks(type, this.framework.InstrumentManager.Instruments[symbol], dateTime1, dateTime2);

        public TickSeries GetHistoricalTicks(TickType type, Instrument instrument, DateTime dateTime1, DateTime dateTime2)
        {
            DataSeries ds = null;
            switch (type)
            {
                case TickType.Bid:
                    ds = GetDataSeries(instrument, DataObjectType.Bid, BarType.Time, 60);
                    break;
                case TickType.Ask:
                    ds = GetDataSeries(instrument, DataObjectType.Ask, BarType.Time, 60);
                    break;
                case TickType.Trade:
                    ds = GetDataSeries(instrument, DataObjectType.Trade, BarType.Time, 60);
                    break;
            }

            var ts = new TickSeries();
            if (ds != null && ds.Count != 0)
            {
                var index1 = ds.GetIndex(dateTime1, SearchOption.Next);
                var index2 = ds.GetIndex(dateTime2, SearchOption.Prev);
                for (long i = index1; i <= index2; i++)
                {
                    var obj = ds[i];
                    switch (type)
                    {
                        case TickType.Bid:
                            if (obj.TypeId == DataObjectType.Bid)
                                ts.Add((Bid)obj);
                            else
                                Console.WriteLine($"DataManager::GetHistoricalTicks Error, object type is not Bid {obj}");
                            break;
                        case TickType.Ask:
                            if (obj.TypeId == DataObjectType.Ask)
                                ts.Add((Ask)obj);
                            else
                                Console.WriteLine($"DataManager::GetHistoricalTicks Error, object type is not Ask {obj}");
                            break;
                        case TickType.Trade:
                            if (obj.TypeId == DataObjectType.Trade)
                                ts.Add((Trade)obj);
                            else
                                Console.WriteLine($"DataManager::GetHistoricalTicks Error, object type is not Trade {obj}");
                            break;
                    }
                }
            }
            return ts;
        }

        public TickSeries GetHistoricalTicks(IHistoricalDataProvider provider, TickType type, Instrument instrument, DateTime dateTime1, DateTime dateTime2)
        {
            if (provider.IsDisconnected)
                provider.Connect();

            DataNotifier @class = new DataNotifier();
            string text = Guid.NewGuid().ToString();
            lock (this.dictionary_0)
            {
                this.dictionary_0.Add(text, @class);
            }
            HistoricalDataRequest request = null;
            switch (type)
            {
                case TickType.Bid:
                    request = new HistoricalDataRequest(instrument, dateTime1, dateTime2, DataObjectType.Bid);
                    break;
                case TickType.Ask:
                    request = new HistoricalDataRequest(instrument, dateTime1, dateTime2, DataObjectType.Ask);
                    break;
                case TickType.Trade:
                    request = new HistoricalDataRequest(instrument, dateTime1, dateTime2, DataObjectType.Trade);
                    break;
            }
            request.RequestId = text;
            provider.Send(request);
            @class.ReadyEvent.WaitOne();
            lock (this.dictionary_0)
            {
                this.dictionary_0.Remove(text);
            }
            var ts = new TickSeries("", "");
            foreach (var data in @class.Data)
            {
                var objs = data.Objects;
                for (int i = 0; i < objs.Length; i++)
                    ts.Add((Tick)objs[i]);
            }
            return ts;
        }


        public TickSeries GetHistoricalBids(string symbol) => GetHistoricalBids(symbol, DateTime.MinValue, DateTime.MaxValue);

        public TickSeries GetHistoricalBids(Instrument instrument) => GetHistoricalTicks(TickType.Bid, instrument, DateTime.MinValue, DateTime.MaxValue);

        public TickSeries GetHistoricalBids(string symbol, DateTime dateTime1, DateTime dateTime2) => GetHistoricalTicks(TickType.Bid, symbol, dateTime1, dateTime2);

        public TickSeries GetHistoricalBids(Instrument instrument, DateTime dateTime1, DateTime dateTime2) => GetHistoricalTicks(TickType.Bid, instrument, dateTime1, dateTime2);

        public TickSeries GetHistoricalBids(string provider, string symbol, DateTime dateTime1, DateTime dateTime2)
        {
            var p = this.framework.ProviderManager.GetHistoricalDataProvider(provider);
            if (p == null)
            {
                Console.WriteLine($"DataManager::GetHistoricalBids Error. Provider does not exist : {provider}");
                return null;
            }
            var i = this.framework.InstrumentManager[symbol];
            if (i == null)
            {
                Console.WriteLine($"DataManager::GetHistoricalBids Error. Instrument with such symbol does not exist : {symbol}");
                return null;
            }
            return GetHistoricalBids(p, i, dateTime1, dateTime2);
        }

        public TickSeries GetHistoricalBids(IHistoricalDataProvider provider, Instrument instrument, DateTime dateTime1, DateTime dateTime2) => GetHistoricalTicks(provider, TickType.Bid, instrument, dateTime1, dateTime2);

        public TickSeries GetHistoricalAsks(string symbol) => GetHistoricalAsks(symbol, DateTime.MinValue, DateTime.MaxValue);

        public TickSeries GetHistoricalAsks(Instrument instrument) => GetHistoricalTicks(TickType.Ask, instrument, DateTime.MinValue, DateTime.MaxValue);

        public TickSeries GetHistoricalAsks(string symbol, DateTime dateTime1, DateTime dateTime2) => GetHistoricalTicks(TickType.Ask, symbol, dateTime1, dateTime2);

        public TickSeries GetHistoricalAsks(Instrument instrument, DateTime dateTime1, DateTime dateTime2) => GetHistoricalTicks(TickType.Ask, instrument, dateTime1, dateTime2);

        public TickSeries GetHistoricalAsks(string provider, string symbol, DateTime dateTime1, DateTime dateTime2)
        {
            var p = this.framework.ProviderManager.GetHistoricalDataProvider(provider);
            if (p == null)
            {
                Console.WriteLine($"DataManager::GetHistoricalAsks Error. Provider does not exist : {provider}");
                return null;
            }
            var i = this.framework.InstrumentManager[symbol];
            if (i == null)
            {
                Console.WriteLine($"DataManager::GetHistoricalAsks Error. Instrument with such symbol does not exist : {symbol}");
                return null;
            }
            return GetHistoricalAsks(p, i, dateTime1, dateTime2);
        }

        public TickSeries GetHistoricalAsks(IHistoricalDataProvider provider, Instrument instrument, DateTime dateTime1, DateTime dateTime2) => GetHistoricalTicks(provider, TickType.Ask, instrument, dateTime1, dateTime2);

        public TickSeries GetHistoricalTrades(string symbol) => GetHistoricalTrades(symbol, DateTime.MinValue, DateTime.MaxValue);

        public TickSeries GetHistoricalTrades(Instrument instrument) => GetHistoricalTicks(TickType.Trade, instrument, DateTime.MinValue, DateTime.MaxValue);

        public TickSeries GetHistoricalTrades(string symbol, DateTime dateTime1, DateTime dateTime2) => GetHistoricalTicks(TickType.Trade, symbol, dateTime1, dateTime2);

        public TickSeries GetHistoricalTrades(Instrument instrument, DateTime dateTime1, DateTime dateTime2) => GetHistoricalTicks(TickType.Trade, instrument, dateTime1, dateTime2);

        public TickSeries GetHistoricalTrades(string provider, Instrument instrument, DateTime dateTime1, DateTime dateTime2)
        {
            var p = this.framework.ProviderManager.GetHistoricalDataProvider(provider);
            if (p == null)
            {
                Console.WriteLine($"DataManager::GetHistoricalTrades Error. Provider does not exist : {provider}");
                return null;
            }
            return GetHistoricalTrades(p, instrument, dateTime1, dateTime2);
        }
        public TickSeries GetHistoricalTrades(string provider, string symbol, DateTime dateTime1, DateTime dateTime2)
        {
            var p = this.framework.ProviderManager.GetHistoricalDataProvider(provider);
            if (p == null)
            {
                Console.WriteLine($"DataManager::GetHistoricalTrades Error. Provider does not exist : {provider}");
                return null;
            }
            var i = this.framework.InstrumentManager[symbol];
            if (i == null)
            {
                Console.WriteLine($"DataManager::GetHistoricalTrades Error. Instrument with such symbol does not exist : {symbol}");
                return null;
            }
            return GetHistoricalTrades(p, i, dateTime1, dateTime2);
        }

        public TickSeries GetHistoricalTrades(IHistoricalDataProvider provider, Instrument instrument, DateTime dateTime1, DateTime dateTime2) => GetHistoricalTicks(provider, TickType.Trade, instrument, dateTime1, dateTime2);

        public BarSeries GetHistoricalBars(string symbol, BarType barType, long barSize)
        {
            return this.GetHistoricalBars(symbol, DateTime.MinValue, DateTime.MaxValue, barType, barSize);
        }
        public BarSeries GetHistoricalBars(Instrument instrument, BarType barType, long barSize)
        {
            return this.GetHistoricalBars(instrument, DateTime.MinValue, DateTime.MaxValue, barType, barSize);
        }
        public BarSeries GetHistoricalBars(string symbol, DateTime dateTime1, DateTime dateTime2, BarType barType, long barSize)
        {
            var i = this.framework.InstrumentManager[symbol];
            if (i == null)
            {
                Console.WriteLine($"DataManager::GetHistoricalBars Error. Instrument with such symbol does not exist : {symbol}");
                return null;
            }
            return this.GetHistoricalBars(i, dateTime1, dateTime2, barType, barSize);
        }

        public BarSeries GetHistoricalBars(Instrument instrument, DateTime dateTime1, DateTime dateTime2, BarType barType, long barSize)
        {
            var ds = GetDataSeries(instrument, DataObjectType.Bar, barType, barSize);
            var bs = new BarSeries();
            if (ds != null && ds.Count != 0)
            {
                long index1 = ds.GetIndex(dateTime1, SearchOption.Next);
                long index2 = ds.GetIndex(dateTime2, SearchOption.Prev);
                for (long i = index1; i <= index2; i++)
                    bs.Add((Bar)ds[i]);
            }
            return bs;
        }

        public BarSeries GetHistoricalBars(string provider, Instrument instrument, DateTime dateTime1, DateTime dateTime2, BarType barType, long barSize)
        {
            var p = this.framework.ProviderManager.GetHistoricalDataProvider(provider);
            if (p == null)
            {
                Console.WriteLine($"DataManager::GetHistoricalBars Error. Provider does not exist : {provider}");
                return null;
            }
            return this.GetHistoricalBars(p, instrument, dateTime1, dateTime2, barType, barSize);
        }

        public BarSeries GetHistoricalBars(string provider, string symbol, DateTime dateTime1, DateTime dateTime2, BarType barType, long barSize)
        {
            var p = this.framework.ProviderManager.GetHistoricalDataProvider(provider);
            if (p == null)
            {
                Console.WriteLine($"DataManager::GetHistoricalBars Error. Provider does not exist : {provider}");
                return null;
            }
            var i = this.framework.InstrumentManager[symbol];
            if (i == null)
            {
                Console.WriteLine($"DataManager::GetHistoricalBars Error. Instrument with such symbol does not exist : {symbol}");
                return null;
            }
            return this.GetHistoricalBars(p, i, dateTime1, dateTime2, barType, barSize);
        }

        public BarSeries GetHistoricalBars(IHistoricalDataProvider provider, Instrument instrument, DateTime dateTime1, DateTime dateTime2, BarType barType, long barSize)
        {
            if (provider.IsDisconnected)
                provider.Connect();

            DataManager.DataNotifier @class = new DataManager.DataNotifier();
            string text = Guid.NewGuid().ToString();
            lock (this.dictionary_0)
            {
                this.dictionary_0.Add(text, @class);
            }
            provider.Send(new HistoricalDataRequest(instrument, dateTime1, dateTime2, 6)
            {
                RequestId = text,
                BarType = new BarType?(barType),
                BarSize = new long?(barSize)
            });
            @class.ReadyEvent.WaitOne();
            lock (this.dictionary_0)
            {
                this.dictionary_0.Remove(text);
            }
            var bs = new BarSeries("", "", -1, -1);
            foreach (var data in @class.Data)
            {
                var objs = data.Objects;
                for (int i = 0; i < objs.Length; i++)
                    bs.Add((Bar)objs[i]);
            }
            return bs;
        }

        public QuoteSeries GetHistoricalQuotes(IHistoricalDataProvider provider, Instrument instrument) => GetHistoricalQuotes(provider, instrument, DateTime.MinValue, DateTime.MaxValue);

        public QuoteSeries GetHistoricalQuotes(IHistoricalDataProvider provider, Instrument instrument, DateTime datetime1, DateTime datetime2)
        {
            if (provider.IsDisconnected)
                provider.Connect();
            DataNotifier @class = new DataNotifier();
            string text = Guid.NewGuid().ToString();
            lock (this.dictionary_0)
                this.dictionary_0.Add(text, @class);

            provider.Send(new HistoricalDataRequest(instrument, datetime1, datetime2, DataObjectType.Quote) { RequestId = text });
            @class.ReadyEvent.WaitOne();
            lock (this.dictionary_0)
                this.dictionary_0.Remove(text);

            var qs = new QuoteSeries("");
            foreach (var data in @class.Data)
            {
                var objs = data.Objects;
                for (int i = 0; i < objs.Length; i++)
                    qs.Add((Quote)objs[i]);
            }
            return qs;
        }

        public void Save(Tick tick, SaveMode option = SaveMode.Add) => Save(tick.InstrumentId, tick, option);

        public void Save(Bar bar, SaveMode option = SaveMode.Add) => Save(bar.InstrumentId, bar, option);

        public void Save(Level2 level2, SaveMode option = SaveMode.Add) => Save(level2.InstrumentId, level2, option);

        public void Save(Level2Snapshot level2, SaveMode option = SaveMode.Add) => Save(level2.InstrumentId, level2, option);

        public void Save(Level2Update level2, SaveMode option = SaveMode.Add) => Save(level2.InstrumentId, level2, option);

        public void Save(News news, SaveMode option = SaveMode.Add) => Save(news.InstrumentId, news, option);

        public void Save(Fundamental fundamental, SaveMode option = SaveMode.Add) => Save(fundamental.InstrumentId, fundamental, option);

        public void Save(Instrument instrument, DataObject obj, SaveMode option = SaveMode.Add) => Server.Save(instrument, obj, option);

        public void Save(int instrumentId, DataObject obj, SaveMode option = SaveMode.Add)
        {
            var i = this.framework.InstrumentManager.GetById(instrumentId);
            if (i == null)
            {
                Console.WriteLine($"DataManager::Save Instrument with id does not exist in the framework id = {instrumentId}");
                return;
            }
            Server.Save(i, obj, option);
        }

        public void Save(string symbol, DataObject obj, SaveMode option = SaveMode.Add)
        {
            var i = this.framework.InstrumentManager[symbol];
            if (i == null)
            {
                Console.WriteLine($"DataManager::Save Instrument with symbol does not exist in the framework {symbol}");
                return;
            }
            Server.Save(i, obj, option);
        }

        public void Save(Instrument instrument, IDataSeries series, SaveMode option = SaveMode.Add)
        {
            for (long i = 0; i < series.Count; i++)
                Server.Save(instrument, series[i], option);
        }

        public void Save(TickSeries series, SaveMode option = SaveMode.Add)
        {
            for (int i = 0; i < series.Count; i++)
                Save(series[i], option);
        }

        public void Save(BarSeries series, SaveMode option = SaveMode.Add)
        {
            for (int i = 0; i < series.Count; i++)
                Save(series[i], option);
        }
        #endregion

        #region EventHandlers

        internal void OnBid(Bid bid)
        {
            var iId = bid.InstrumentId;
            var pId = bid.ProviderId + 1;
            this.latestBid[iId] = bid;
            this.bidByIIdAndPId[pId] = this.bidByIIdAndPId[pId] ?? new IdArray<Bid>(10240);
            this.bidByIIdAndPId[pId][iId] = bid;
        }

        internal void OnAsk(Ask ask)
        {
            var iId = ask.InstrumentId;
            var pId = ask.ProviderId + 1;
            this.latestAsk[iId] = ask;
            this.askByIIdAndPId[pId] = this.askByIIdAndPId[pId] ?? new IdArray<Ask>(10240);
            this.askByIIdAndPId[pId][iId] = ask;
        }

        internal void OnTrade(Trade trade)
        {
            var iId = trade.InstrumentId;
            var pId = trade.ProviderId + 1;
            this.latestTrade[iId] = trade;
            this.tradeByIIdAndPId[pId] = this.tradeByIIdAndPId[pId] ?? new IdArray<Trade>(10240);
            this.tradeByIIdAndPId[pId][iId] = trade;
        }

        internal void OnBar(Bar bar)
        {
            this.latestBar[bar.InstrumentId] = bar;
        }

        internal void OnLevel2(Level2Snapshot l2s)
        {
            var pId = l2s.ProviderId + 1;
            var iId = l2s.InstrumentId;
            this.idArray_10[pId] = this.idArray_10[pId] ?? new IdArray<OrderBook>(10240);
            var orderBook = this.idArray_10[pId][iId] = this.idArray_10[pId][iId] ?? new OrderBook();
            orderBook.OnLevel2(l2s);
            var orderBookAggr = this.idArray_4[iId] = this.idArray_4[iId] ?? new OrderBookAggr(iId);
            orderBookAggr.OnLevel2(l2s);
        }

        internal void OnLevel2(Level2Update l2u)
        {
            var pId = l2u.ProviderId + 1;
            var iId = l2u.InstrumentId;
            this.idArray_10[pId] = this.idArray_10[pId] ?? new IdArray<OrderBook>(10240);
            var orderBook = this.idArray_10[pId][iId] = this.idArray_10[pId][iId] ?? new OrderBook();
            orderBook.OnLevel2(l2u);
            var orderBookAggr = this.idArray_4[iId] = this.idArray_4[iId] ?? new OrderBookAggr(iId);
            orderBookAggr.OnLevel2(l2u);
        }

        internal void OnHistoricalData(HistoricalData historicalData_0)
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

        internal void OnHistoricalDataEnd(HistoricalDataEnd historicalDataEnd_0)
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
            this.latestNews[news.InstrumentId] = news;
        }

        internal void OnFundamental(Fundamental fundamental)
        {
            if (fundamental.InstrumentId != -1)
                this.latestFundamental[fundamental.InstrumentId] = fundamental;
        }

        #endregion
    }
}