using System;
using System.Diagnostics;

namespace FastQuant.Indicators
{
    [Serializable]
    public class NVI : Indicator
    {
        public NVI(ISeries input) : base(input)
        {
            Init();
        }

        protected override void Init()
        {
            this.name = "NVI";
            this.description = "Negative Volume Index";
            base.Clear();
            this.calculate = true;
        }

        public override void Calculate(int index)
        {
            var value = index == 0 ? this.input[0, BarData.Volume] : ValueWithLastValue(this.input, index, this[index - 1]);
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
            return index == 0 ? input[0, BarData.Volume] : double.NaN;
        }

        private static double ValueWithLastValue(ISeries input, int index, double last)
        {
            Debug.Assert(index >= 1);
            var c = input[index, BarData.Close];
            var lc = input[index - 1, BarData.Close];
            var v = input[index, BarData.Volume];
            var lv = input[index - 1, BarData.Volume];
            return v < lv ? last * (1 + (c - lc) / lc) : last;
        }
    }
}