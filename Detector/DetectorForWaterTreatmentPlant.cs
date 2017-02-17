using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoreLinq;

namespace Detector
{
    public class DetectorForWaterTreatmentPlant
    {
        private readonly List<List<double>> _rawSeries = new List<List<double>>();
        private readonly List<List<int>> _sampledSeriesSet = new List<List<int>>();

        public DetectorForWaterTreatmentPlant()
        {
            using (var sr = new StreamReader(@"..\data\water-treatment.csv"))
            {
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine().Split(',').Skip(1).ToArray();
                    if (!_rawSeries.Any()) foreach (var _ in line) _rawSeries.Add(new List<double>());
                    for (var i = 0; i < line.Length; i++)
                    {
                        double value;
                        _rawSeries[i].Add(double.TryParse(line[i], out value) ? value : double.NaN);
                    }
                }
            }
            var samplePointsSet = _rawSeries.Select(series => Sampling.CalcSamplePoints(series, 32, true).ToList()).ToList();
            foreach (var samplePoints in samplePointsSet)
            {
                foreach (var point in samplePoints)
                {
                    Console.Write($@"{point}, ");
                }
                Console.WriteLine();
            }

            for (var i = 0; i < _sampledSeriesSet.Count; i++)
            {
                for (var j = 0; j < _rawSeries[i].Count; j++)
                {
                    var point = _rawSeries[i][j];
                    if (double.IsNaN(point)) _sampledSeriesSet[i].Add(samplePointsSet[i].IndexOf(double.NaN));
                    else _sampledSeriesSet[i].Add(samplePointsSet[i].IndexOf(samplePointsSet[i].MinBy(m => double.IsNaN(m) ? 1 : Math.Abs(m - _rawSeries[i][j]))));
                    Console.Write($@"{_sampledSeriesSet[i][j]}, ");
                }
                Console.WriteLine();
            }
            var analizer = new RelationAnalyzer();
            foreach (var series in _sampledSeriesSet)
            {
                analizer.AddSeries(series.ToArray());
            }
            analizer.CalcMutualInformation();
            analizer.SaveRelationImage();
        }
    }
}