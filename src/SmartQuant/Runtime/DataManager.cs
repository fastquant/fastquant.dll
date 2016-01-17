using System;
using System.Collections.Generic;

namespace SmartQuant
{
    public class DataManager
    {
        private Framework framework;

        public DataServer Server { get; set; }

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
            }
            throw new NotImplementedException();
        }

        public DataSeries GetDataSeries(Instrument instrument, byte type, BarType barType = BarType.Time, long barSize = 60)
        {
            return Server.GetDataSeries(instrument, type, barType, barSize);
        }

        public DataSeries GetDataSeries(string symbol, byte type, BarType barType = BarType.Time, long barSize = 60)
        {
            return Server.GetDataSeries(this.framework.InstrumentManager.Instruments[symbol], type, barType, barSize);
        }

        public DataSeries GetDataSeries(string name)
        {
            return Server.GetDataSeries(name);
        }

        public DataSeries AddDataSeries(string name)
        {
            return Server.AddDataSeries(name);
        }

        public DataSeries AddDataSeries(Instrument instrument, byte type)
        {
            return Server.AddDataSeries(instrument, type, BarType.Time, 60);
        }

        public DataSeries AddDataSeries(Instrument instrument, byte type, BarType barType = BarType.Time, long barSize = 60)
        {
            return Server.AddDataSeries(instrument, type, barType, barSize);
        }

        public void DeleteDataSeries(string name)
        {
            Server.DeleteDataSeries(name);
        }

        public void DeleteDataSeries(Instrument instrument, byte type, BarType barType = BarType.Time, long barSize = 60)
        {
            Server.DeleteDataSeries(instrument, type, barType, barSize);
        }

        public void DeleteDataSeries(string symbol, byte type, BarType barType = BarType.Time, long barSize = 60)
        {
            Server.DeleteDataSeries(this.framework.InstrumentManager.Instruments[symbol], type, barType, barSize);
        }

        public void Dump()
        {
            throw new NotImplementedException();
        }

        internal void OnBid(Bid bid)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public List<DataSeries> GetDataSeriesList(Instrument instrument = null, string pattern = null) => Server.GetDataSeriesList(instrument, pattern);
    }
}