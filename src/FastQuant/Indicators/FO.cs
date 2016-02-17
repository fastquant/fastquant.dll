using System;
using System.ComponentModel;

namespace FastQuant.Indicators
{
    [Serializable]
    public class FO : Indicator
    {
        protected int length;
        protected BarData barData;
        protected RegressionDistanceMode distanceMode;

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
        public BarData BarData
        {
            get
            {
                return this.barData;
            }
            set
            {
                this.barData = value;
                Init();
            }
        }

        [Category("Parameters"), Description("")]
        public RegressionDistanceMode DistanceMode
        {
            get
            {
                return this.distanceMode;
            }
            set
            {
                this.distanceMode = value;
                Init();
            }
        }

        public FO(ISeries input, int length, BarData barData = BarData.Close,
            RegressionDistanceMode distanceMode = RegressionDistanceMode.Time) : base(input)
        {
            this.length = length;
            this.barData = barData;
            this.distanceMode = distanceMode;
            Init();
        }

        protected override void Init()
        {
            this.name = this.input is BarSeries ? $"FO ({this.length}, {this.barData})" : $"FO ({this.length})";
            this.description = "Forecast Oscillator";
            Clear();
            this.calculate = true;
        }

        public override void Calculate(int index)
        {
            var value = Value(this.input, index, this.length, this.barData, this.distanceMode);
            if (!double.IsNaN(value))
                Add(this.input.GetDateTime(index), value);
        }

        public static double Value(ISeries input, int index, int length, BarData barData = BarData.Close, RegressionDistanceMode distanceMode = RegressionDistanceMode.Time)
        {
            if (index >= length - 1)
            {
                var lri = LRI.Value(input, index, length, barData, distanceMode);
                return 100.0 * (input[index, barData] - lri) / lri;
            }
            return double.NaN;
        }
    }
}