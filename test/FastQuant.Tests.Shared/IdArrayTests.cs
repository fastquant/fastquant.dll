using Xunit;
#if USE_FASTQUANT
namespace FastQuant.Tests
#else
namespace SmartQuant.Tests
#endif
{
    public class IdArrayTests
    {
        [Fact]
        public void TestResize()
        {
            var arr = new IdArray<double>(10);
            Assert.Equal(arr.Size, 10);
            //Assert.Throws(() => arr[15] = 20.0);
        }
    }
}