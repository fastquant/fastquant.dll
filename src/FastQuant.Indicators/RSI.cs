namespace SmartQuant.Indicators
{
    public class RSI : Indicator
    {
        protected IndicatorStyle style;
        protected int length;
        protected BarData barData;
        protected TimeSeries upTS;
        protected TimeSeries downTS;

        public RSI(ISeries input, int length, BarData barData = BarData.Close, IndicatorStyle style = IndicatorStyle.QuantStudio):base(input)
        {
            this.length = length;
            this.barData = barData;
            this.style = style;
            this.Init();
        }
    }
}