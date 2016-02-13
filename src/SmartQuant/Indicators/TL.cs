using System;

namespace SmartQuant.Indicators
{
    [Serializable]
    public class TL : Indicator
    {
        public TL(ISeries input) : base(input)
        {
            Init();
        }

        protected override void Init()
        {
            this.name = "TL";
            this.description = "True Low";
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
            return index >= 1 ? Math.Min(input[index, BarData.Low], input[index - 1, BarData.Close]) : double.NaN;
        }
    }
}