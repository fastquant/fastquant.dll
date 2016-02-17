using System;
using System.ComponentModel;

namespace FastQuant.Indicators
{
    [Serializable]
    public class AroonL : Indicator
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

        public AroonL(ISeries input, int length) : base(input)
        {
            this.length = length;
            Init();
        }

        protected override void Init()
        {
            this.name = $"AroonL ({this.length})";
            this.description = "Aroon Lower";
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
                var l = input[index, BarData.Low];
                double li = index;
                for (var i = index; i >= index - length + 1; i--)
                {
                    if (input[i, BarData.Low] < l)
                    {
                        li = i;
                        l = input[i, BarData.Low];
                    }
                }
                return 100.0 * (1.0 - (index - li) / (double)length);
            }
            return double.NaN;
        }

    }
}