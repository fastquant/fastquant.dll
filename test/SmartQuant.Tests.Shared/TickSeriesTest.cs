using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SmartQuant.Tests
{
    public class TickSeriesTest
    {
        [Fact]
        public void TestGetIndex()
        {
            var ts = new TickSeries("test");
            for (int i = 0; i < 10; ++i)
                ts.Add(new Tick { DateTime = new DateTime(2000, 1, 1, 10, i, 30) });

            var firstDt = new DateTime(2000, 1, 1, 10, 3, 30);
            var firstTick = new Tick { DateTime = firstDt };
            var lastDt = new DateTime(2000, 1, 1, 10, 9, 30);
            var lastTick = new Tick { DateTime = lastDt };
            // DateTime is in the middle;
            Assert.Equal(3, ts.GetIndex(firstDt, IndexOption.Null));
            Assert.Equal(-1, ts.GetIndex(new DateTime(2000, 1, 1, 10, 4, 25), IndexOption.Null));
            Assert.Equal(4, ts.GetIndex(new DateTime(2000, 1, 1, 10, 4, 30), IndexOption.Null));
            Assert.Equal(4, ts.GetIndex(new DateTime(2000, 1, 1, 10, 4, 30), IndexOption.Prev));
            Assert.Equal(4, ts.GetIndex(new DateTime(2000, 1, 1, 10, 4, 30), IndexOption.Next));
            Assert.Equal(-1, ts.GetIndex(new DateTime(2000, 1, 1, 10, 4, 25), IndexOption.Null));
            Assert.Equal(3, ts.GetIndex(new DateTime(2000, 1, 1, 10, 4, 25), IndexOption.Prev));
            Assert.Equal(4, ts.GetIndex(new DateTime(2000, 1, 1, 10, 4, 25), IndexOption.Next));
            Assert.Equal(-1, ts.GetIndex(new DateTime(2000, 1, 1, 10, 4, 40), IndexOption.Null));
            Assert.Equal(4, ts.GetIndex(new DateTime(2000, 1, 1, 10, 4, 40), IndexOption.Prev));
            Assert.Equal(5, ts.GetIndex(new DateTime(2000, 1, 1, 10, 4, 40), IndexOption.Next));
            // DateTime > LastDateTime
            Assert.Equal(5, ts.GetIndex(new DateTime(2000, 1, 1, 10, 4, 40), IndexOption.Next));
            Assert.Equal(-1, ts.GetIndex(new DateTime(2000, 1, 1, 10, 11, 30), IndexOption.Null));
            Assert.Equal(9, ts.GetIndex(new DateTime(2000, 1, 1, 10, 11, 30), IndexOption.Prev));
            Assert.Equal(-1, ts.GetIndex(new DateTime(2000, 1, 1, 10, 11, 30), IndexOption.Next));
            // DateTime < FirstDateTime
            Assert.Equal(-1, ts.GetIndex(new DateTime(2000, 1, 1, 9, 31, 30), IndexOption.Null));
            Assert.Equal(-1, ts.GetIndex(new DateTime(2000, 1, 1, 9, 31, 30), IndexOption.Prev));
            Assert.Equal(0, ts.GetIndex(new DateTime(2000, 1, 1, 9, 31, 30), IndexOption.Next));
        }
    }
}
