using System;
using System.ComponentModel;
using System.Linq;

namespace FastQuant.Indicators
{
    [Serializable]
    public class MASS : Indicator
    {
        protected int length;
        protected int order;
        protected TimeSeries hlTS;
        protected EMA ema;
        protected EMA ema_ema;

        [Category("Parameters"), Description("")]
        public int Length
        {
            get
            {
                return this.length;
            }
            set
            {
                this.length = value;
                Init();
            }
        }

        [Category("Parameters"), Description("")]
        public int Order
        {
            get
            {
                return this.order;
            }
            set
            {
                this.order = value;
                Init();
            }
        }

        public MASS(ISeries input, int length, int order) : base(input)
        {
            this.length = length;
            this.order = order;
            Init();
        }

        protected override void Init()
        {
            this.name = $"MASS ({this.length}, {this.order})";
            this.description = "Mass Index";
            Clear();
            this.calculate = true;
            this.hlTS = new TimeSeries();
            for (var i = 0; i < this.input.Count; i++)
                this.hlTS.Add(this.input.GetDateTime(i), this.input[i, BarData.High] - this.input[i, BarData.Low]);
            Detach();
            this.ema = new EMA(this.hlTS, this.order);
            this.ema_ema = new EMA(this.ema, this.order);
            Attach();
        }

        public override void Calculate(int index)
        {
            this.hlTS.Add(this.input.GetDateTime(index), this.input[index, BarData.High] - this.input[index, BarData.Low]);
            if (index >= this.length - 1)
            {
                var value = Enumerable.Range(index - this.length + 1, this.length).Reverse().Sum(i => this.ema[i] / this.ema_ema[i]);
                Add(this.input.GetDateTime(index), value);
            }
        }

        public static double Value(ISeries input, int index, int length, int order)
        {
            if (index >= length - 1)
            {
                var ts = new TimeSeries();
                for (int i = 0; i <= index; i++)
                    ts.Add(input.GetDateTime(i), input[i, BarData.High] - input[i, BarData.Low]);
                var ema = new EMA(ts, order);
                var ema_ema = new EMA(ema, order);
                return Enumerable.Range(index - length + 1, length).Reverse().Sum(i => ema[i]/ema_ema[i]);
            }
            return double.NaN;
        }

    }
}