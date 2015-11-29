using System;
using System.Collections.Generic;
using SmartQuant;
using System.Threading;

namespace SmartQuant
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
        private Thread thread;

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

        public DataSimulator(Framework framework) : base(framework)
        {
            this.id = ProviderId.DataSimulator;
            this.name = "DataSimulator";
            this.description = "Default data simulator";
            this.url = "fastquant.org";
            DateTime1 = DateTime.MinValue;
            DateTime2 = DateTime.MaxValue;
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public void Run()
        {
            this.thread = new Thread(() =>
            {
                Console.WriteLine($"{DateTime.Now} Data simulator thread started");
                if (!IsConnected)
                    Connect();

                Console.WriteLine($"{DateTime.Now} Data simulator thread stopped");
            });
            this.thread.Name = "Data Simulator Thread";
            this.thread.IsBackground = true;
            this.thread.Start();
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

        private void Subscribe(Instrument instrument, DateTime dateTime1, DateTime dateTime2)
        {
        }
    }
}