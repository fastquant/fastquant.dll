using System;
using System.ComponentModel;

namespace FastQuant.Indicators
{
    [Serializable]
    public class VCH : Indicator
    {
        protected int length1;
        protected int length2;
        protected EMA ema;
        protected TimeSeries hlTS;

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

        public VCH(ISeries input, int length1, int length2) : base(input)
        {
            this.length1 = length1;
            this.length2 = length2;
            Init();
        }

        protected override void Init()
        {
            this.name = $"VCH ({this.length1}, {this.length2})";
            this.description = "Chaikin Volatility";
            Clear();
            this.calculate = true;

            this.hlTS = new TimeSeries();
            for (var i = 0; i < this.input.Count; i++)
                this.hlTS.Add(this.input.GetDateTime(i), this.input[i, BarData.High] - this.input[i, BarData.Low]);
            this.ema = new EMA(this.hlTS, this.length1);
        }

        public override void Calculate(int index)
        {
            this.hlTS.Add(this.input.GetDateTime(index), this.input[index, BarData.High] - this.input[index, BarData.Low]);
            if (index >= this.length2 - 1)
            {
                var i = this.ema.GetIndex(this.input.GetDateTime(index));
                var value = (this.ema[i] - this.ema[i - this.length2 + 1]) / this.ema[i - this.length2 + 1] * 100;
                if (!double.IsNaN(value))
                    Add(this.input.GetDateTime(index), value);
            }
        }

        public static double Value(ISeries input, int index, int length1, int length2)
        {
            if (index >= length2 - 1)
            {
                var ts = new TimeSeries();
                for (var i = 0; i <= index; i++)
                    ts.Add(input.GetDateTime(i), input[i, BarData.High] - input[i, BarData.Low]);
                var ema = new EMA(ts, length1);
                return (ema[index] - ema[index - length2 + 1])/ema[index - length2 + 1]*100;
            }
            return double.NaN;
        }
    }
}