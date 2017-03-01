using Microsoft.VisualStudio.TestTools.UnitTesting;
using Detector;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Detector.Tests
{
    [TestClass()]
    public class GaussianDetectorTests
    {
        [TestMethod()]
        public void DetectTest()
        {
            var rawSeries = new List<List<double>>();
            using (var sr = new StreamReader(@"..\data\water-treatment.csv"))
            {
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine().Split(',').Skip(1).ToArray();
                    if (!rawSeries.Any()) foreach (var _ in line) rawSeries.Add(new List<double>());
                    for (var i = 0; i < line.Length; i++)
                    {
                        double value;
                        rawSeries[i].Add(double.TryParse(line[i], out value) ? value : double.NaN);
                    }
                }
            }

            var anomalySeries = new List<bool>();
            var anomaly = rawSeries.Select(GaussianDetector.AnomalySeries).ToArray();
            for (var i = 0; i < rawSeries.First().Count; i++)
            {
                var isAnomaly = anomaly.Aggregate(false, (current, series) => current || series[i]);
                anomalySeries.Add(isAnomaly);
            }
            anomalySeries.ForEach(Console.WriteLine);
        }
    }
}