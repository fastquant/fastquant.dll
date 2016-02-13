using System;

namespace SmartQuant.Indicators
{
    [Serializable]
    public class MDM : Indicator
    {
        public MDM(ISeries input) : base(input)
        {
            Init();
        }

        protected override void Init()
        {
            this.name = nameof(MDM);
            this.description = "Minus Directional Movement";
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
            return ndm > pdm ? ndm : 0.0;
        }
    }
}