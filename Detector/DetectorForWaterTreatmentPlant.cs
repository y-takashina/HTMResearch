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
        private readonly List<List<int>> _discretizedSeries = new List<List<int>>();

        public DetectorForWaterTreatmentPlant()
        {
            // データ読み込み
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
            // 離散化
            var discretizedValues = _rawSeries.Select(series => Sampling.CalcSamplePoints(series, 32, true).ToList()).ToList();
            for (var i = 0; i < _rawSeries.Count; i++)
            {
                var discretizedSeries = new List<int>();
                foreach (var value in _rawSeries[i])
                {
                    var discretizedValue = double.IsNaN(value) ? value : discretizedValues[i].Where(v => !double.IsNaN(v)).MinBy(v => Math.Abs(v - value));
                    var discretizedValueIndex = discretizedValues[i].IndexOf(discretizedValue);
                    discretizedSeries.Add(discretizedValueIndex);
                }
                _discretizedSeries.Add(discretizedSeries);
            }

            var analizer = new RelationAnalyzer();
            foreach (var series in _discretizedSeries)
            {
                analizer.AddSeries(series.ToArray());
            }
            analizer.CalcMutualInformation();
            analizer.SaveRelationImage();
        }
    }
}