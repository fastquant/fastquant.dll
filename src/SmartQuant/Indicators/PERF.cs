using System;
using System.ComponentModel;

namespace SmartQuant.Indicators
{
    [Serializable]
    public class PERF : Indicator
    {
        protected double k;
        protected BarData barData;

        [Category("Parameters"), Description("")]
        public double K
        {
            get
            {
                return this.k;
            }
            set
            {
                this.k = value;
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

        public PERF(ISeries input, double k, BarData barData = BarData.Close) : base(input)
        {
            this.k = k;
            this.barData = barData;
            Init();
        }

        protected override void Init()
        {
            this.name = this.input is BarSeries ? $"PERF ({this.k}, {this.barData}" : $"PERF ({this.k})";
            this.description = "Performance Indicator";
            Clear();
            this.calculate = true;
        }

        public override void Calculate(int index)
        {
            var value = Value(this.input, index, this.k, this.barData);
            if (!double.IsNaN(value))
                Add(this.input.GetDateTime(index), value);
        }

        public static double Value(ISeries input, int index, double k, BarData barData = BarData.Close)
        {
            return index < 0 ? double.NaN : 100.0*(input[index, barData] - k)/k;
        }
    }
}