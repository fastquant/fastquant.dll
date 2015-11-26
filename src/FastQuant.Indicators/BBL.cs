namespace SmartQuant.Indicators
{
    public class BBL : Indicator
    {
        private double k;
        private int length;

        public BBL(ISeries input, int length, double k, BarData barData = BarData.Close)
        {
            this.length = length;
            this.k = k;
        }
    }
}