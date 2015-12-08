using System;
using System.ComponentModel;

namespace SmartQuant
{
    public class Leg
    {
        private Framework framework;

        private Instrument instrument;

        public double Weight { get; set; }

        internal Leg()
        {
        }

        internal Leg(Framework framework)
        {
            this.framework = framework;
        }

        public Leg(Instrument instrument, double weight = 1.0)
        {
            this.instrument = instrument;
            Weight = weight;
            this.instrumentId = instrument.Id;
            this.framework = instrument.Framework;
        }

        internal void Init(Framework framework)
        {
            this.framework = framework;
            this.instrument = framework.InstrumentManager.GetById(this.instrumentId);
            if (this.instrument == null)
                Console.WriteLine($"{nameof(Leg)}::{nameof(Init)} Can not find leg instrument in the framework instrument manager. Id = {this.instrumentId}");
        }

        [Browsable(false)]
        public Instrument Instrument
        {
            get
            {
                return this.instrument;
            }
            set
            {
                this.instrument = value;
                this.instrumentId = this.instrument.Id;
            }
        }

        public string Symbol
        {
            get
            {
                return this.instrument?.Symbol;
            }
            set
            {
                var instrument = this.framework.InstrumentManager[value];
                if (instrument == null)
                    Console.WriteLine($"Leg::Symbol Can not find instrument with such symbol in the framework instrument manager. Symbol = {this.instrumentId}");

                this.instrument = instrument;
                this.instrumentId = instrument.Id;
            }
        }

        private int instrumentId;
    }

}