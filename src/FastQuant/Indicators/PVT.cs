using System;

namespace FastQuant.Indicators
{
    [Serializable]
    public class PVT : Indicator
    {
        public PVT(ISeries input) : base(input)
        {
            Init();
        }

        protected override void Init()
        {
            this.name = nameof(PVT);
            this.description = "Price and Volume Trend";
            Clear();
            this.calculate = true;
        }

        public override void Calculate(int index)
        {
            if (index >= 1)
            {
                var last = index >= 2 ? this[index - 1 + -1] : 0;
                var value = ValueWithLastValue(this.input, index, last);
                if (!double.IsNaN(value))
                    Add(this.input.GetDateTime(index), value);
            }
        }

        public static double Value(ISeries input, int index)
        {
            if (index >= 1)
            {
                var last = index >= 2 ? Value(input, index - 1) : 0;
                return ValueWithLastValue(input, index, last);
            }
            return double.NaN;
        }

        public static double ValueWithLastValue(ISeries input, int index, double last)
        {
            var c = input[index, BarData.Close];
            var lc = input[index - 1, BarData.Close];
            var v = input[index, BarData.Volume];
            return (c - lc)/lc*v + last;
        }
    }
}