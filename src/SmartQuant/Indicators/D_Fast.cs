using System;
using System.ComponentModel;
using System.Linq;

namespace SmartQuant.Indicators
{
    [Serializable]
    public class D_Fast : Indicator
    {
        protected int length;
        protected int order;

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
        public int Order
        {
            get
            {
                return this.order;
            }
            set
            {
                this.order = value;
                Init();
            }
        }

        public D_Fast(ISeries input, int length, int order) : base(input)
        {
            this.length = length;
            this.order = order;
            Init();
        }

        protected override void Init()
        {
            this.name = $"%D Fast ({this.length}, {this.order})";
            this.description = "%D Fast";
            Clear();
            this.calculate = true;
        }

        public override void Calculate(int index)
        {
            var value = Value(this.input, index, this.length, this.order);
            if (!double.IsNaN(value))
                Add(this.input.GetDateTime(index), value);
        }

        public static double Value(ISeries input, int index, int length, int order)
        {
            return index < length + order - 1
                ? double.NaN
                : Enumerable.Range(index - order + 1, order).Reverse().Sum(i => K_Fast.Value(input, i, length))/order;
        }
    }
}