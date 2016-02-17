using System;
using System.ComponentModel;

namespace FastQuant.Indicators
{
    [Serializable]
    public class R : Indicator
    {
        protected int length;

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

        public R(ISeries input, int length) : base(input)
        {
            this.length = length;
            Init();
        }

        protected override void Init()
        {
            this.name = $"%R ({this.length})";
            this.description = "Williams %R";
            Clear();
            this.calculate = true;
        }

        public override void Calculate(int index)
        {
            var value = Value(this.input, index, this.length);
            if (!double.IsNaN(value))
                Add(this.input.GetDateTime(index), value);
        }

        public static double Value(ISeries input, int index, int length)
        {
            if (index >= length - 1)
            {
                var close = input[index, BarData.Close];
                var lowest_low = input.GetMin(index - length + 1, index, BarData.Low);
                var highest_high = input.GetMax(index - length + 1, index, BarData.High);
                return -100.0 * (highest_high - close) / (highest_high - lowest_low);
            }
            return double.NaN;
        }
    }
}