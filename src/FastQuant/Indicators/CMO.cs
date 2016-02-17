using System;
using System.ComponentModel;

namespace FastQuant.Indicators
{
    [Serializable]
    public class CMO : Indicator
    {
        protected int length;
        protected BarData barData;

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
                this.Init();
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
                this.Init();
            }
        }

        public CMO(ISeries input, int length, BarData barData = BarData.Close) : base(input)
        {
            this.length = length;
            this.barData = barData;
            Init();
        }

        protected override void Init()
        {
            this.name = this.input is BarSeries ? $"CMO ({this.length}, {this.barData})" : $"CMO ({this.length})";
            this.description = "Change Momentum Oscillator";
            Clear();
            this.calculate = true;
        }

        public override void Calculate(int index)
        {
            var value = Value(this.input, index, this.length, this.barData);
            if (!double.IsNaN(value))
                Add(this.input.GetDateTime(index), value);
        }

        public static double Value(ISeries input, int index, int length, BarData barData = BarData.Close)
        {
            if (index >= length)
            {
                var positive = 0.0;
                var negative = 0.0;
                for (var i = index; i > index - length; i--)
                {
                    var diff = input[i, barData] - input[i - 1, barData];
                    if (diff > 0.0)
                        positive += diff;
                    else
                        negative -= diff;
                }
                return 100.0 * (positive - negative) / (positive + negative);
            }
            return double.NaN;
        }
    }
}