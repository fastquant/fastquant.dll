using System;

namespace SmartQuant.Indicators
{
    [Serializable]
    public class MarketFI : Indicator
    {
        public MarketFI(ISeries input) : base(input)
        {
            Init();
        }

        protected override void Init()
        {
            this.name = nameof(MarketFI);
            this.description = "Market Force Index";
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
            if (index >= 0)
            {
                var h = input[index, BarData.High];
                var l = input[index, BarData.Low];
                var v = input[index, BarData.Volume];
                return v != 0 ? (h - l) / v * 1000.0 : 0.0;
            }
            return double.NaN;
        }
    }
}