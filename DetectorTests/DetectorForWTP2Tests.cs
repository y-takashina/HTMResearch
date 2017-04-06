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
    public class DetectorForWTP2Tests
    {
        [TestMethod()]
        public void DetectorForWTP2Test()
        {
            var rawStreams = new List<List<double>>();
            using (var sr = new StreamReader(@"..\data\water-treatment.csv"))
            {
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine().Split(',').Skip(1).ToArray();
                    if (!rawStreams.Any()) rawStreams.AddRange(line.Select(_ => new List<double>()));
                    for (var i = 0; i < line.Length; i++)
                    {
                        rawStreams[i].Add(double.Parse(line[i]));
                    }
                }
            }
            var detector = new DetectorForWTP2(rawStreams);
        }
    }
}