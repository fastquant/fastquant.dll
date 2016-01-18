using System;
using System.Collections.Generic;

namespace SmartQuant
{
    public class DataManager
    {
        private Framework framework;
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

        public DataManager(Framework framework, DataServer server)
        {
            this.framework = framework;
            Server = server;
            Server?.Open();
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
            }
        }

        public DataSeries GetDataSeries(Instrument instrument, byte type, BarType barType = BarType.Time, long barSize = 60) => Server.GetDataSeries(instrument, type, barType, barSize);

        public DataSeries GetDataSeries(string symbol, byte type, BarType barType = BarType.Time, long barSize = 60) => Server.GetDataSeries(this.framework.InstrumentManager.Instruments[symbol], type, barType, barSize);

        public DataSeries GetDataSeries(string name) => Server.GetDataSeries(name);

        public DataSeries AddDataSeries(string name) => Server.AddDataSeries(name);

        public DataSeries AddDataSeries(Instrument instrument, byte type) => Server.AddDataSeries(instrument, type, BarType.Time, 60);

        public DataSeries AddDataSeries(Instrument instrument, byte type, BarType barType = BarType.Time, long barSize = 60) => Server.AddDataSeries(instrument, type, barType, barSize);

        public void DeleteDataSeries(string name) => Server.DeleteDataSeries(name);

        public void DeleteDataSeries(Instrument instrument, byte type, BarType barType = BarType.Time, long barSize = 60) => Server.DeleteDataSeries(instrument, type, barType, barSize);

        public void DeleteDataSeries(string symbol, byte type, BarType barType = BarType.Time, long barSize = 60) => Server.DeleteDataSeries(this.framework.InstrumentManager.Instruments[symbol], type, barType, barSize);

        public void Dump()
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public List<DataSeries> GetDataSeriesList(Instrument instrument = null, string pattern = null) => Server.GetDataSeriesList(instrument, pattern);

        public Bid GetBid(int instrumentId) => this.idArray_0[instrumentId];

        public Ask GetAsk(int instrumentId) => this.idArray_1[instrumentId];

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
            throw new NotImplementedException();
        }

        internal void OnTrade(Trade trade)
        {
            throw new NotImplementedException();
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

        internal void method_8(HistoricalDataEnd e)
        {
            throw new NotImplementedException();
        }

        internal void method_7(HistoricalData e)
        {
            throw new NotImplementedException();
        }

        internal void method_6(News news)
        {
            throw new NotImplementedException();
        }

        internal void nBvFytknIm(Fundamental fundamental)
        {
            throw new NotImplementedException();
        }
    }
}