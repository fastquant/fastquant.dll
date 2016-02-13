using System;
using System.ComponentModel;
using System.Linq;

namespace SmartQuant.Indicators
{
    [Serializable]
    public class D_Slow : Indicator
    {
        protected int length;
        protected int order1;
        protected int order2;

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

        [Category("Parameters"), Description("")]
        public int Order1
        {
            get
            {
                return this.order1;
            }
            set
            {
                this.order1 = value;
                Init();
            }
        }

        [Category("Parameters"), Description("")]
        public int Order2
        {
            get
            {
                return this.order2;
            }
            set
            {
                this.order2 = value;
                Init();
            }
        }

        public D_Slow(ISeries input, int length, int order1, int order2) : base(input)
        {
            this.length = length;
            this.order1 = order1;
            this.order2 = order2;
            Init();
        }

        protected override void Init()
        {
            this.name = $"%D Slow ({this.length}, {this.order1}, {this.order2})";
            this.description = "%D Slow";
            Clear();
            this.calculate = true;
        }

        public override void Calculate(int index)
        {
            var value = Value(this.input, index, this.length, this.order1, this.order2);
            if (!double.IsNaN(value))
                Add(this.input.GetDateTime(index), value);
        }

        public static double Value(ISeries input, int index, int length, int order1, int order2)
        {
            return index < length + order1 + order2 - 1
                ? double.NaN
                : Enumerable.Range(index - order2 + 1, order2).Reverse().Sum(i => K_Slow.Value(input, i, length, order1))/order2;
        }
    }
}