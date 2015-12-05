using System;
using System.Linq;

namespace SmartQuant
{
    public class InstrumentManager
    {
        private Framework framework;

        public InstrumentServer Server { get; set; }
        public InstrumentList Instruments { get; } = new InstrumentList();

        public InstrumentManager(Framework framework, InstrumentServer server)
        {
            this.framework = framework;
            Server = server;
        }

        public void Add(Instrument instrument, bool save = true)
        {
            throw new NotImplementedException();
        }

        public void Delete(Instrument instrument)
        {
        }

        public void Delete(string symbol)
        {
            throw new NotImplementedException();
        }

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public void Dump()
        {
            throw new NotImplementedException();
        }

        public Instrument GetById(int id)
        {
            throw new NotImplementedException();
        }

        public void Load()
        {
            throw new NotImplementedException();
        }

        public void Save(Instrument instrument)
        {
            throw new NotImplementedException();
        }

        public Instrument Get(string symbol) => Instruments.Get(symbol);

        public bool Contains(string symbol) => Instruments.Contains(symbol);

        public Instrument this[string symbol] => Instruments.Get(symbol);

    }
}