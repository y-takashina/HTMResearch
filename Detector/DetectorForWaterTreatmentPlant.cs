using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MoreLinq;
using static MatViz.MatViz;
using static PipExtensions.PipExtensions;

namespace Detector
{
    public class DetectorForWaterTreatmentPlant
    {
        private List<List<double>> _rawSeries;
        private readonly List<List<int>> _discretizedSeries = new List<List<int>>();

        public void Initialize(List<List<double>> rawSeries)
        {
            _rawSeries = rawSeries.Select(v => v).ToList();
            _discretizeSeries();
        }

        private void _discretizeSeries()
        {
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
        }

        public void SaveResultImages(string path = ".")
        {
            Directory.CreateDirectory(path);
            path += '\\';
            SaveMatrixImage(MutualInformationMatrix(_discretizedSeries), path + "mutual_information_matrix");
        }
    }
}