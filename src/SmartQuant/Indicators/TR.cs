using System;

namespace SmartQuant.Indicators
{
    [Serializable]
    public class TR : Indicator
    {
        public TR(ISeries input) : base(input)
        {
            Init();
        }

        protected override void Init()
        {
            this.name = "TR";
            this.description = "True Range";
            Clear();
            this.calculate = true;
        }

        public override void Calculate(int index)
        {
            var value = Value(this.input, index);
            if (!double.IsNaN(value))
                Add(this.input.GetDateTime(index), value);
        }

        public static double Value(ISeries input, int index)
        {
            if (index >= 1)
            {
                var high = input[index, BarData.High];
                var low = input[index, BarData.Low];
                var previous_close = input[index - 1, BarData.Close];
                return Math.Max(Math.Abs(high - low), Math.Max(Math.Abs(high - previous_close), Math.Abs(previous_close - low)));
            }
            return double.NaN;
        }
    }
}