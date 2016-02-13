using System;
using System.ComponentModel;
using System.Linq;

namespace SmartQuant.Indicators
{
    [Serializable]
    public class KCU : Indicator
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

        public KCU(ISeries input, int length) : base(input)
        {
            this.length = length;
            Init();
        }

        protected override void Init()
        {
            this.name = $"KCU ({this.length})";
            this.description = "Keltner Channel Upper";
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
                : SMA.Value(input, index, length, BarData.Typical) + Enumerable.Range(index - length + 1, length).Reverse().Sum(i => TR.Value(input, i)) / length;
        }
    }
}