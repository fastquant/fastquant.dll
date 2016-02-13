using System;
using System.ComponentModel;

namespace SmartQuant.Indicators
{
    [Serializable]
    public class CAD : Indicator
    {
        protected int length1;
        protected int length2;
        protected AD ad;
        protected EMA ema1;
        protected EMA ema2;

        [Category("Parameters"), Description("")]
        public int Length1
        {
            get
            {
                return this.length1;
            }
            set
            {
                this.length1 = value;
                Init();
            }
        }

        [Category("Parameters"), Description("")]
        public int Length2
        {
            get
            {
                return this.length2;
            }
            set
            {
                this.length2 = value;
                Init();
            }
        }

        public CAD(ISeries input, int length1, int length2) : base(input)
        {
            this.length1 = length1;
            this.length2 = length2;
            Init();
        }

        protected override void Init()
        {
            this.name = $"CAD ({this.length1}, {this.length2})";
            this.description = "Chaikin A/D Oscillator";
            Clear();
            this.calculate = true;
            Detach();
            this.ad?.Detach();
            this.ema1?.Detach();
            this.ema2?.Detach();
            this.ad = new AD(this.input);
            this.ema1 = new EMA(this.ad, this.length1);
            this.ema2 = new EMA(this.ad, this.length2);
            Attach();
        }

        public override void Calculate(int index)
        {
            if (index >= Math.Max(this.length1, this.length2))
                Add(this.input.GetDateTime(index), this.ema1[index] - this.ema2[index]);
        }

        public static double Value(ISeries input, int index, int length1, int length2)
        {
            if (index >= Math.Max(length1, length2))
            {
                var ad = new AD(input);
                var ema1 = new EMA(ad, length1);
                var ema2 = new EMA(ad, length2);
                return ema1[index] - ema2[index];
            }
            return double.NaN;
        }
    }
}