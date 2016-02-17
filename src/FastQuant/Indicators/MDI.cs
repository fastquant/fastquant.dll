using System;
using System.ComponentModel;

namespace FastQuant.Indicators
{
    [Serializable]
    public class MDI : Indicator
    {
        protected int length;
        protected IndicatorStyle style;
        protected TimeSeries mdmTS;
        protected TimeSeries trTS;

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
        public IndicatorStyle Style
        {
            get
            {
                return this.style;
            }
            set
            {
                this.style = value;
                Init();
            }
        }

        public MDI(ISeries input, int length, IndicatorStyle style = IndicatorStyle.QuantStudio) : base(input)
        {
            this.length = length;
            this.style = style;
            Init();
        }

        protected override void Init()
        {
            this.name = $"MDI ({this.length})";
            this.description = "Minus Directional Indicator";
            Clear();
            this.calculate = true;
            this.mdmTS = new TimeSeries();
            this.trTS = new TimeSeries();
        }

        public override void Calculate(int index)
        {
            if (this.style == IndicatorStyle.QuantStudio)
            {
                double mdm = 0.0;
                double tr = 0.0;
                if (index >= this.length)
                {
                    if (index == this.length)
                    {
                        for (int i = index; i >= index - this.length + 1; i--)
                        {
                            tr += TR.Value(this.input, i);
                            mdm += MDM.Value(this.input, i);
                        }
                    }
                    else
                    {
                        mdm = this.mdmTS[index - 1] - MDM.Value(this.input, index - this.length) +
                              MDM.Value(this.input, index);
                        tr = this.trTS[index - 1] - TR.Value(this.input, index - this.length) +
                             TR.Value(this.input, index);
                    }
                    if (tr != 0.0)
                    {
                        double value = mdm/tr*100.0;
                        if (!double.IsNaN(value))
                            Add(this.input.GetDateTime(index), value);
                    }
                }
                this.mdmTS.Add(this.input.GetDateTime(index), mdm);
                this.trTS.Add(this.input.GetDateTime(index), tr);
            }
            else
            {
                double mdm = 0.0;
                double tr = 0.0;
                if (index >= this.length)
                {
                    if (index == this.length)
                    {
                        for (var j = index; j >= index - this.length + 1; j--)
                        {
                            tr += TR.Value(this.input, j);
                            mdm += MDM.Value(this.input, j);
                        }
                    }
                    else
                    {
                        mdm = this.mdmTS[index - 1] - this.mdmTS[index - 1]/this.length + MDM.Value(this.input, index);
                        tr = this.trTS[index - 1] - this.trTS[index - 1]/this.length + TR.Value(this.input, index);
                    }
                    if (tr != 0.0)
                    {
                        double value = mdm/tr*100.0;
                        if (!double.IsNaN(value))
                            Add(this.input.GetDateTime(index), value);
                    }
                }
                this.mdmTS.Add(this.input.GetDateTime(index), mdm);
                this.trTS.Add(this.input.GetDateTime(index), tr);
            }
        }

        public static double Value(ISeries input, int index, int length, IndicatorStyle style = IndicatorStyle.QuantStudio)
        {
            if (style == IndicatorStyle.QuantStudio)
            {
                var mdm = 0.0;
                var tr = 0.0;
                if (index >= length)
                {
                    for (var i = index; i > index - length; i--)
                    {
                        tr += TR.Value(input, i);
                        mdm += MDM.Value(input, i);
                    }
                    return mdm / tr * 100;
                }
                return double.NaN;
            }
            else
            {
                double mdm = 0.0;
                double tr = 0.0;
                if (index >= length)
                {
                    for (var j = length; j >= 1; j--)
                    {
                        tr += TR.Value(input, j);
                        mdm += MDM.Value(input, j);
                    }
                    for (var k = length + 1; k <= index; k++)
                    {
                        mdm = mdm - mdm / length + MDM.Value(input, k);
                        tr = tr - tr / length + TR.Value(input, k);
                    }
                    return mdm / tr * 100;
                }
                return double.NaN;
            }
        }
    }
}