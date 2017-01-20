using System;
using System.IO;
using System.Linq;
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
            //var files = new DirectoryInfo(@"..\data").GetDirectories().SelectMany(dir => dir.GetFiles()).Select(f => f.FullName);
            var detector = new HTMDetector();
            //var path = @"..\data\realAWSCloudwatch\grok_asg_anomaly.csv";
            //var path = @"..\data\artificialWithAnomaly\art_daily_jumpsdown.csv";
            //var path = @"..\data\artificialWithAnomaly\art_load_balancer_spikes.csv";
            var path = @"..\data\realAWSCloudwatch\ec2_cpu_utilization_5f5533.csv";
            using (var dr = new DataReader(path))
            {
                var data = dr.GetLines();
                detector.Initialize(data);
            }
            detector.Learn();
            detector.Predict();
            detector.SaveResultImages(path.Split('\\').Last().Split('.').First());
        }
    }
}