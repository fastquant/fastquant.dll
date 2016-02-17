using System;
using Xunit;
using Xunit.Abstractions;

#if USE_FASTQUANT
using FastQuant.Indicators;
namespace FastQuant.Tests
#else
using SmartQuant.Indicators;
namespace SmartQuant.Tests
#endif
{
    public class IndicatorsTest
    {
        private static int precision = 8;

        public IndicatorsTest(ITestOutputHelper output)
        {
            this.output = output;
        }

        private readonly ITestOutputHelper output;

        [Fact]
        public void TestWMA()
        {
            long ticks = 1000;
            var data = new double[] { 20, 40, 22, 35, 33, 78, 21, 45, 33, 5, 67, 22, 98, 22, 34, 54 };
            var input = new TimeSeries();
            var wma = new WMA(input, 10);
            foreach (var d in data)
                input.Add(new DateTime().AddTicks(ticks++), d);
            Assert.Equal(wma[0], 32.6, precision);
            Assert.Equal(wma[1], 38.7454545454545, precision);
            Assert.Equal(wma[2], 35.8545454545455, precision);
            Assert.Equal(wma[3], 47.1090909090909, precision);
            Assert.Equal(wma[4], 43.1636363636364, precision);
            Assert.Equal(wma[5], 41.6363636363636, precision);
            Assert.Equal(wma[6], 43.7272727272727, precision);
        }


        [Fact]
        public void TestEMA()
        {
            long ticks = 1000;
            var data = new double[] { 20, 40, 22, 35, 33, 78, 21, 45, 33, 5, 67, 22, 98, 22, 34, 54 };
            var input = new TimeSeries();
            var ema = new EMA(input, 10);
            foreach (var d in data)
                input.Add(new DateTime().AddTicks(ticks++), d);
            for (var i = 0; i < ema.Count; i++)
                output.WriteLine(ema[i].ToString());


            Assert.Equal(ema[0], 20, precision);
            Assert.Equal(ema[1], 23.6363636363636, precision);
            Assert.Equal(ema[2], 23.3388429752066, precision);
            Assert.Equal(ema[3], 25.4590533433509, precision);
            Assert.Equal(ema[4], 26.8301345536507, precision);
            Assert.Equal(ema[5], 36.1337464529869, precision);
            Assert.Equal(ema[6], 33.3821561888075, precision);
            Assert.Equal(ema[7], 35.4944914272061, precision);
            Assert.Equal(ema[8], 35.0409475313505, precision);
            Assert.Equal(ema[9], 29.5789570711049, precision);
            Assert.Equal(ema[10], 36.3827830581768, precision);
            Assert.Equal(ema[11], 33.7677315930537, precision);
            Assert.Equal(ema[12], 45.4463258488621, precision);
            Assert.Equal(ema[13], 41.1833575127054, precision);
            Assert.Equal(ema[14], 39.8772925103953, precision);
            Assert.Equal(ema[15], 42.4450575085053, precision);
        }
    }
}