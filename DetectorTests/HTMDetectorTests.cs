using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
            var files = new DirectoryInfo(@"..\data").GetDirectories().SelectMany(dir => dir.GetFiles()).Select(f => f.FullName);
            Func<string, double> func = file =>
            {
                var detector = new HTMDetector();
                using (var dr = new DataReader(file))
                {
                    var data = dr.GetLines();
                    detector.Initialize(data);
                }
                detector.Learn();
                detector.Predict();
                detector.SaveResultImages("results\\" + file.Split('\\').Last().Split('.').First());
                return detector.CompressRatioHTM;
            };
            var query = from file in files.AsParallel() select func(file);
            var x = query.Average();
            Console.WriteLine(x);
//            Parallel.ForEach(files, file =>
//            {
//                var detector = new HTMDetector();
//                using (var dr = new DataReader(file))
//                {
//                    var data = dr.GetLines();
//                    detector.Initialize(data);
//                }
//                detector.Learn();
//                detector.Predict();
//                detector.SaveResultImages("results\\" + file.Split('\\').Last().Split('.').First());
//            });
//            var path = @"..\data\realAWSCloudwatch\grok_asg_anomaly.csv";
//            var path = @"..\data\artificialWithAnomaly\art_daily_jumpsdown.csv";
//            var path = @"..\data\artificialWithAnomaly\art_load_balancer_spikes.csv";
//            var path = @"..\data\realAWSCloudwatch\ec2_cpu_utilization_5f5533.csv";
//            var detector = new HTMDetector();
//            using (var dr = new DataReader(path))
//            {
//                var data = dr.GetLines();
//                detector.Initialize(data);
//            }
//            detector.Learn();
//            detector.Predict();
//            detector.SaveResultImages(path.Split('\\').Last().Split('.').First());
        }
    }
}