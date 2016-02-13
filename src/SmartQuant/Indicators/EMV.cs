using System;

namespace SmartQuant.Indicators
{
    [Serializable]
    public class EMV : Indicator
    {
        public EMV(ISeries input) : base(input)
        {
            Init();
        }
        protected override void Init()
        {
            this.name = nameof(EMV);
            this.description = "Ease of Movement";
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
                var h = input[index, BarData.High];
                var lh = input[index - 1, BarData.High];
                var l = input[index, BarData.Low];
                var ll = input[index - 1, BarData.Low];
                var v = input[index, BarData.Volume];
                var distance_moved = (h + l)/2 - (lh + ll)/2;
                var box_ratio = v/1000000.0/(h - l);
                return distance_moved / box_ratio;
            }
            return double.NaN;
        }
    }
}