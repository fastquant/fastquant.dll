using System;

namespace FastQuant.Indicators
{
    [Serializable]
    public class WAD : Indicator
    {
        public WAD(ISeries input) : base(input)
        {
            Init();
        }

        protected override void Init()
        {
            this.name = "WAD";
            this.description = "Williams Accumulation/Distribution";
            Clear();
            this.calculate = true;
        }

        public override void Calculate(int index)
        {
            var value = index == 0 ? 0 : ValueWithLastValue(this.input, index, this[index - 1]);
            Add(this.input.GetDateTime(index), value);
        }

        public static double Value(ISeries input, int index)
        {
            var value = index == 0 ? 0 : ValueWithLastValue(input, index, Value(input, index - 1));
            return value;
        }

        private static double ValueWithLastValue(ISeries input, int index, double last)
        {
            var value = 0d;
            var hight = input[index, BarData.High];
            var low = input[index, BarData.Low];
            var current_close = input[index, BarData.Close];
            var last_close = input[index - 1, BarData.Close];
            if (current_close > last_close)
                value = last + current_close - Math.Min(low, last_close);
            if (current_close < last_close)
                value = last + current_close - Math.Max(hight, last_close);
            if (current_close == last_close)
                value = last;
            return value;
        }
    }
}