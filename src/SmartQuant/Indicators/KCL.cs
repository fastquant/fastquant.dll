using System;
using System.ComponentModel;
using System.Linq;

namespace SmartQuant.Indicators
{
    [Serializable]
    public class KCL : Indicator
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
                Init();
            }
        }

        public KCL(ISeries input, int length) : base(input)
        {
            this.length = length;
            Init();
        }

        protected override void Init()
        {
            this.name = "KCL ({this.length})";
            this.description = "Keltner Channel Lower";
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
            return index < length
                ? double.NaN
                : SMA.Value(input, index, length, BarData.Typical) - Enumerable.Range(index - length + 1, length).Reverse().Sum(i => TR.Value(input, i))/length;
        }
    }
}