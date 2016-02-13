using System;
using System.ComponentModel;

namespace SmartQuant.Indicators
{
    [Serializable]
    public class AroonU : Indicator
    {
        protected int length;

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
                this.Init();
            }
        }

        public AroonU(ISeries input, int length) : base(input)
        {
            this.length = length;
            Init();
        }

        protected override void Init()
        {
            this.name = $"AroonU ({this.length})";
            this.description = "Aroon Upper";
            Clear();
            this.calculate = true;
        }

        public override void Calculate(int index)
        {
            var value = Value(this.input, index, this.length);
            if (!double.IsNaN(value))
                Add(this.input.GetDateTime(index), value);
        }

        public static double Value(ISeries input, int index, int length)
        {
            if (index >= length - 1)
            {
                var h = input[index, BarData.High];
                double hi = index;
                for (var i = index; i >= index - length + 1; i--)
                {
                    if (input[i, BarData.High] > h)
                    {
                        hi = i;
                        h = input[i, BarData.High];
                    }
                }
                return 100.0 * (1.0 - (index - hi) / length);
            }
            return double.NaN;
        }
    }
}