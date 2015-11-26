using System;
using System.Collections.Generic;
using SmartQuant;

namespace FastQuant.Core
{
    public class DataProcessor
    {
        public DataProcessor()
        {
        }

        protected void Emit(DataObject obj)
        {
            throw new NotImplementedException();
        }

        protected virtual DataObject OnData(DataObject obj)
        {
            throw new NotImplementedException();
        }

        public bool EmitBar { get; set; } 

        public bool EmitBarCloseTrade { get; set; }

        public bool EmitBarHighTrade { get; set; }

        public bool EmitBarLowTrade { get; set; }

        public bool EmitBarOpen { get; set; }

        public bool EmitBarOpenTrade { get; set; }

    }

    public class DataSimulator : Provider, IDataSimulator
    {
        public bool RunOnSubscribe { get; set; }
        public DateTime DateTime1 { get; set; }
        public DateTime DateTime2 { get; set; }
        public BarFilter BarFilter { get; }
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

        public DataProcessor Processor { get; set; }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public void Subscribe(InstrumentList instrument)
        {
            throw new NotImplementedException();
        }

        public void Subscribe(Instrument instrument)
        {
            throw new NotImplementedException();
        }

        public void Unsubscribe(InstrumentList instrument)
        {
            throw new NotImplementedException();
        }

        public void Unsubscribe(Instrument instrument)
        {
            throw new NotImplementedException();
        }
    }
}