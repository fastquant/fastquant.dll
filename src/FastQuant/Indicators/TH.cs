using System;

namespace FastQuant.Indicators
{
    [Serializable]
    public class TH : Indicator
    {
        public TH(ISeries input) : base(input)
        {
            Init();
        }

        protected override void Init()
        {
            this.name = "TH";
            this.description = "True High";
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
            return index >= 1 ? Math.Max(input[index, BarData.High], input[index - 1, BarData.Close]) : double.NaN;
        }
    }
}