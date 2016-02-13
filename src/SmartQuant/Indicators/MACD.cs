using System;
using System.ComponentModel;

namespace SmartQuant.Indicators
{
    [Serializable]
    public class MACD : Indicator
    {
        protected int length1;
        protected int length2;
        protected BarData barData;
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

        [Category("Parameters"), Description("")]
        public BarData BarData
        {
            get
            {
                return this.barData;
            }
            set
            {
                this.barData = value;
                Init();
            }
        }


        public MACD(ISeries input, int length1, int length2, BarData barData = BarData.Close) : base(input)
        {
            this.length1 = length1;
            this.length2 = length2;
            this.barData = barData;
            Init();
        }

        protected override void Init()
        {
            this.name = this.input is BarSeries ? $"MACD ({this.length1}, {this.length2}, {this.barData}" : $"MACD ({this.length1}, {this.length2})";
            this.description = "Moving Average Convergence Divergence";
            Clear();
            this.calculate = true;
            Detach();
            this.ema1 = new EMA(this.input, this.length1, this.barData);
            this.ema2 = new EMA(this.input, this.length2, this.barData);
            Attach();
        }

        public override void Calculate(int index)
        {
            if (index >= 0)
                Add(this.input.GetDateTime(index), this.ema1[index] - this.ema2[index]);
        }

        public static double Value(ISeries input, int index, int length1, int length2, BarData barData = BarData.Close)
        {
            return index < 0
                ? double.NaN
                : EMA.Value(input, index, length1, barData) - EMA.Value(input, index, length2, barData);
        }
    }
}