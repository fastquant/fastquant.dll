using System;

namespace SmartQuant.Indicators
{
    [Serializable]
    public class PDM : Indicator
    {
        public PDM(ISeries input) : base(input)
        {
            Init();
        }

        protected override void Init()
        {
            this.name = nameof(PDM);
            this.description = "Plus Directional Movement";
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
            if (index < 1)
                return double.NaN;
            var h = input[index, BarData.High];
            var l = input[index, BarData.Low];
            var lh = input[index - 1, BarData.High];
            var ll = input[index - 1, BarData.Low];
            var pdm = Math.Max(0, h - lh);
            var ndm = Math.Max(0, ll - l);
            return pdm > ndm ? pdm : 0.0;
        }
    }
}