using System;
using System.ComponentModel;

namespace SmartQuant.Indicators
{
    [Serializable]
    public class K_Fast : Indicator
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

        public K_Fast(ISeries input, int length) : base(input)
        {
            this.length = length;
            Init();
        }

        protected override void Init()
        {
            this.name = $"%K Fast ({this.length})";
            this.description = "%K Fast";
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
                double current_close = input[index, BarData.Close];
                double lowest_low = input.GetMin(index - length + 1, index, BarData.Low);
                double highest_high = input.GetMax(index - length + 1, index, BarData.High);
                return 100.0 * (current_close - lowest_low) / (highest_high - lowest_low);
            }
            return double.NaN;
        }
    }
}