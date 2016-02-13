using System;
using System.ComponentModel;

namespace SmartQuant.Indicators
{
    [Serializable]
    public class ADXR : Indicator
    {
        protected int length;
        protected IndicatorStyle style;
        protected ADX adx;

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

        [Category("Parameters"), Description("")]
        public IndicatorStyle Style
        {
            get
            {
                return this.style;
            }
            set
            {
                this.style = value;
                this.Init();
            }
        }

        public ADXR(ISeries input, int length, IndicatorStyle style = IndicatorStyle.QuantStudio) : base(input)
        {
            this.length = length;
            this.style = style;
            Init();
        }

        protected override void Init()
        {
            this.name = $"ADXR ({this.length})";
            this.description = "Average Directional Index Rating";
            Clear();
            this.calculate = true;
            Detach();
            this.adx = new ADX(this.input, this.length, this.style);
            Attach();
        }

        public override void Calculate(int index)
        {
            if (index >= 3 * this.length - 1)
                Add(this.input.GetDateTime(index), (this.adx[index + -2*this.length] + this.adx[index - this.length + 1 + -2*this.length])/2.0);
        }
    }
}