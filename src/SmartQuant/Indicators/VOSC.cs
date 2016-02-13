using System;
using System.ComponentModel;

namespace SmartQuant.Indicators
{
    [Serializable]
    public class VOSC : Indicator
    {
        protected int length1;
        protected int length2;

        [Category("Parameters"), Description("")]
        public int Length1
        {
            get
            {
                return this.length1;
            }
            set
            {
                this.length1 = value;
                Init();
            }
        }

        [Category("Parameters"), Description("")]
        public int Length2
        {
            get
            {
                return this.length2;
            }
            set
            {
                this.length2 = value;
                Init();
            }
        }


        public VOSC(ISeries input, int length1, int length2) : base(input)
        {
            this.length1 = length1;
            this.length2 = length2;
            Init();
        }

        protected override void Init()
        {
            this.name = $"VOSC ({this.length1}, {this.length2})";
            this.description = "Volume Oscillator";
            Clear();
            this.calculate = true;
        }
        public override void Calculate(int index)
        {
            var value = Value(this.input, index, this.length1, this.length2);
            if (!double.IsNaN(value))
                Add(this.input.GetDateTime(index), value);
        }

        public static double Value(ISeries input, int index, int length1, int length2)
        {
            if (index >= length1 - 1 && index >= length2 - 1)
            {
                var ts = new TimeSeries();
                for (var i = index - Math.Max(length1, length2) + 1; i <= index; i++)
                    ts.Add(input.GetDateTime(i), input[i, BarData.Volume]);
                return SMA.Value(ts, length1 - 1, length1) - SMA.Value(ts, length2 - 1, length2);
            }
            return double.NaN;
        }
    }
}