using System;
using System.ComponentModel;
using System.Linq;

namespace FastQuant.Indicators
{
    [Serializable]
    public class CCI : Indicator
    {
        protected int length = 14;

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

        public CCI(ISeries input, int length) : base(input)
        {
            this.length = length;
            Init();
        }

        protected override void Init()
        {
            this.name = $"CCI ({this.length})";
            this.description = "Commodity Channel Index";
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
                var sum = Enumerable.Range(index - length + 1, length).Reverse().Sum(i => input[i, BarData.Typical])/length;
                var sum2= Enumerable.Range(index - length + 1, length).Reverse().Sum(i => Math.Abs(input[i, BarData.Typical] - sum)) / length;
                return (input[index, BarData.Typical] - sum) / (0.015 * sum2);
            }
            return double.NaN;
        }
    }
}