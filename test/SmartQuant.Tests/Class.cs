using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SmartQuant.Test
{
    public class TestClass1
    {
        [Fact]
        public void testMax()
        {
            Assert.Equal(3, Math.Max(3, 2));
        }
        [Fact]
        public void testFail()
        {
            Assert.Equal(2, Math.Max(3, 2));
        }

        [Fact]
        public void DateTimeComparisonReturnCorrectInteger()
        {
            var d1 = DateTime.Parse("2016-01-01 12:00:01");
            var d2 = DateTime.Parse("2016-01-01 12:00:11");
            var d3 = DateTime.Parse("2016-02-01 12:00:11");
            var d4 = DateTime.Parse("2016/02/01 12:00:11");
            Assert.Equal(-1, d1.CompareTo(d2));
            Assert.Equal(1, d3.CompareTo(d1));
            Assert.Equal(0, d3.CompareTo(d4));
        }

        [Fact]
        public void EventSortedSet_Add()
        {
            var t1 = DateTime.Parse("2000/01/01 00:00:01");
            var t2 = DateTime.Parse("2000/01/01 00:00:05");
            var t3 = DateTime.Parse("2000/01/01 00:01:01");
            var t4 = DateTime.Parse("2000/01/01 00:01:01");
            var e1 = new Event() { DateTime = t1 };
            var e2 = new Event() { DateTime = t2 };
            var e3 = new Event() { DateTime = t3 };
            var e4 = new Event() { DateTime = t4 };
            var e5 = new Event() { DateTime = t3 };
            var e6 = new Event() { DateTime = t2 };
            var sset = new EventSortedSet();
            sset.Add(e1);
            sset.Add(e2);
            sset.Add(e3);
            sset.Add(e4);
            sset.Add(e5);
            Assert.Equal(5, sset.Count);
            Assert.Same(e1, sset[0]);
            Assert.Same(e2, sset[1]);
            Assert.Same(e3, sset[2]);
            Assert.Same(e4, sset[3]);
            Assert.Same(e5, sset[4]);
            sset.Add(e6);
            Assert.Equal(6, sset.Count);
            Assert.Same(e1, sset[0]);
            Assert.Same(e2, sset[1]);
            Assert.Same(e6, sset[2]);
            Assert.Same(e3, sset[3]);
            Assert.Same(e4, sset[4]);
            Assert.Same(e5, sset[5]);
        }
    }
}
