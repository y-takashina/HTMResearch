using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Detector.Tests
{
    [TestClass()]
    public class DetectorForWaterTreatmentPlantTests
    {
        [TestMethod()]
        public void DetectorForWaterTreatmentPlantTest()
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

            var detector = new DetectorForWaterTreatmentPlant();
            detector.Initialize(rawSeries);
        }
    }
}