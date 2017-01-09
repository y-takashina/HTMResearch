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
            using (var dr = new DataReader(@"..\data\realAWSCloudwatch\ec2_cpu_utilization_53ea38.csv"))
            {
                var data = dr.GetLines();
                Console.WriteLine(data);
                var detector = new HTMDetector();
                detector.Initialize(data);
                detector.Detect();
            }
        }
    }
}