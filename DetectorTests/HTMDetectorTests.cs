using Microsoft.VisualStudio.TestTools.UnitTesting;
using Detector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Detector.Tests
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