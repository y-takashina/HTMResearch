using Detector;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DetectorTests
{
    [TestClass()]
    public class HTMDetectorTests
    {
        [TestMethod()]
        public void TestTest()
        {
            Assert.AreEqual(new HTMDetector().Test(), 1);
        }
    }
}