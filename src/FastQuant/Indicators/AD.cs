using System;

namespace FastQuant.Indicators
{
    [Serializable]
    public class AD : Indicator
    {
        public AD(ISeries input) : base(input)
        {
            Init();
        }

        protected override void Init()
        {
            this.name = nameof(AD);
            this.description = "Accumulation/Distribution";
            Clear();
            this.calculate = true;
        }

        public override void Calculate(int index)
        {
            var h = this.input[index, BarData.High];
            var l = this.input[index, BarData.Low];
            var c = this.input[index, BarData.Close];
            var v = this.input[index, BarData.Volume];
            var value = double.NaN;
            var last = index > 0 ? this[index - 1] : 0.0;
            value = h != l ? v * (c - l - (h - c)) / (h - l) + last : last;
            if (!double.IsNaN(value))
                Add(this.input.GetDateTime(index), value);
        }

        public static double Value(ISeries input, int index)
        {
            if (index >= 0)
            {
                var h = input[index, BarData.High];
                var l = input[index, BarData.Low];
                var c = input[index, BarData.Close];
                var v = input[index, BarData.Volume];
                double result;
                var last = index > 0 ? Value(input, index - 1) : 0.0;
                result = h != l ? v * (c - l - (h - c)) / (h - l) + last : last;
                return result;
            }
            return double.NaN;
        }
    }
}