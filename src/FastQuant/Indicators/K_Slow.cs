using System;
using System.ComponentModel;

namespace FastQuant.Indicators
{
    [Serializable]
    public class K_Slow : Indicator
    {
        protected int length;
        protected int order;

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

        public K_Slow(ISeries input, int length, int order) : base(input)
        {
            this.length = length;
            this.order = order;
            Init();
        }

        protected override void Init()
        {
            this.name =  $"%K Slow ({this.length}, {this.order})";
            this.description = "%K SLow";
            Clear();
            this.calculate = true;
        }

        public override void Calculate(int index)
        {
            var value = Value(this.input, index, this.length, this.order);
            if (!double.IsNaN(value))
                Add(this.input.GetDateTime(index), value);
        }

        public static double Value(ISeries input, int index, int length, int order)
        {
            if (index >= length + order - 1)
            {
                var result = 0.0;
                for (var i = index; i > index - order; i--)
                {
                    var lowest_low = input.GetMin(i - length + 1, i, BarData.Low);
                    var highest_high = input.GetMax(i - length + 1, i, BarData.High);
                    var close = input[i, BarData.Close];
                    result += 100.0 * (close - lowest_low) / (highest_high - lowest_low);
                }
                return result / order;
            }
            return double.NaN;
        }
    }
}