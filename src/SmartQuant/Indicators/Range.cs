using System;
using System.ComponentModel;

namespace SmartQuant.Indicators
{
    [Serializable]
    public class Range : Indicator
    {
        public Range(ISeries input, int length):base(input)
        {
            this.length = length;
            Init();
        }

        public override void Calculate(int index)
        {
            double num = Range.Value(this.input, index, this.length);
            if (!double.IsNaN(num))
            {
                base.Add(this.input.GetDateTime(index), num);
            }
        }

        protected override void Init()
        {
            this.name = $"Range ({this.length})";
            this.description = "Range";
            Clear();
            this.calculate = true;
        }

        public static double Value(ISeries input, int index, int length)
        {
            if (index >= length - 1)
            {
                var min = input.GetMin(index - length + 1, index, BarData.Low);
                var max = input.GetMax(index - length + 1, index, BarData.High);
                return Math.Log(max/min);
            }
            return double.NaN;
        }

        [Category("Parameters"), Description("")]
        public int Length
        {
            get
            {
                return this.length;
            }
            set
            {
                this.length = value;
                Init();
            }
        }

        protected int length;
    }
}
