using System;
using System.Diagnostics;

namespace SmartQuant.Indicators
{
    [Serializable]
    public class OBV : Indicator
    {
        public OBV(ISeries input) : base(input)
        {
            Init();
        }

        protected override void Init()
        {
            this.name = nameof(OBV);
            this.description = "On Balance Volume";
            Clear();
            this.calculate = true;
        }

        public override void Calculate(int index)
        {
            var last = index > 1 ? this[index - 1 + -1] : 0;
            var value = ValueWithLastValue(this.input, index, last);
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
            return double.NaN;
        }

        private static double ValueWithLastValue(ISeries input, int index, double last)
        {
            Debug.Assert(index >= 1);
            var c = input[index, BarData.Close];
            var lc = input[index - 1, BarData.Close];
            var v = input[index, BarData.Volume];
            return (index > 1 ? last : 0) + Math.Sign(c - lc) * v;
        }
    }
}