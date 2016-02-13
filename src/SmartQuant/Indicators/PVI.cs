using System;
using System.Diagnostics;

namespace SmartQuant.Indicators
{
    [Serializable]
    public class PVI : Indicator
    {
        public PVI(ISeries input) : base(input)
        {
            Init();
        }

        protected override void Init()
        {
            this.name = "PVI";
            this.description = "Positive Volume Index";
            Clear();
            this.calculate = true;
        }

        public override void Calculate(int index)
        {
            var value = index == 0 ? 10000.0 : ValueWithLastValue(this.input, index, this[index - 1]);
            if (!double.IsNaN(value))
                Add(this.input.GetDateTime(index), value);
        }


        public static double Value(ISeries input, int index)
        {
            if (index >= 1)
            {
                var last = Value(input, index - 1);
                return ValueWithLastValue(input, index, last);
            }
            return index == 0 ? 10000.0 : double.NaN;
        }

        private static double ValueWithLastValue(ISeries input, int index, double last)
        {
            Debug.Assert(index >= 1);
            var c = input[index, BarData.Close];
            var lc = input[index - 1, BarData.Close];
            var v = input[index, BarData.Volume];
            var lv = input[index - 1, BarData.Volume];
            return v > lv ? last * (1 + (c - lc) / lc) : last;
        }
    }
}