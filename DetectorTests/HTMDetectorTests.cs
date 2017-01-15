using System;
using Detector;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Detector.Tests
{
    [TestClass()]
    public class HTMDetectorTests
    {
        [TestMethod()]
        public void DetectTest()
        {
            var detector = new HTMDetector();
            using (var dr = new DataReader(@"..\data\realAWSCloudwatch\ec2_cpu_utilization_5f5533.csv"))
            {
                var data = dr.GetLines();
                detector.Initialize(data);
            }
            detector.Detect();
        }
    }
}