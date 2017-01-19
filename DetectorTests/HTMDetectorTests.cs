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
            //using (var dr = new DataReader(@"..\data\realAWSCloudwatch\grok_asg_anomaly.csv"))
            using (var dr = new DataReader(@"..\data\artificialWithAnomaly\art_daily_jumpsdown.csv"))
            //using (var dr = new DataReader(@"..\data\artificialWithAnomaly\art_load_balancer_spikes.csv"))
            //using (var dr = new DataReader(@"..\data\realAWSCloudwatch\ec2_cpu_utilization_5f5533.csv"))
            {
                var data = dr.GetLines();
                detector.Initialize(data);
            }
            detector.Learn();
            detector.Predict();
        }
    }
}