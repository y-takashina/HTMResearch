using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Clustering;
using MoreLinq;
using PipExtensions;
using static Clustering.Clustering;
using static MatViz.MatViz;
using static PipExtensions.PipExtensions;

namespace Detector
{
    public class DetectorForWTP
    {
        private List<List<double>> _rawSeries;
        private readonly List<List<int>> _discretizedSeries = new List<List<int>>();
        private double[,] _relationships;

        public void Initialize(List<List<double>> rawSeries)
        {
            _rawSeries = rawSeries.Select(v => v).ToList();
            _discretizeSeries();
            _relationships = MutualInformationMatrix(_discretizedSeries);
        }

        public void ClusterSeries()
        {
            var cluster = AggregativeHierarchicalClustering(Enumerable.Range(0, _relationships.GetLength(0)).ToArray(), (i, j) => _relationships[i, j], Metrics.GroupAverage);
            var cluster1Members = cluster.Extract(8).Select(c => c.GetMembers().Select(s => s.Value)).ToArray();
            var order = cluster1Members.SelectMany(i => i).ToArray();
            var memberships = new double[38, 8];
            for (var k = 0; k < 8; k++)
            {
                for (var j = 0; j < 38; j++)
                {
                    memberships[j, k] = cluster1Members[k].Contains(j) ? 1.0 : 1e-6;
                }
            }
            SaveMatrixImage(memberships, "membership");
            _relationships = _relationships.OrderRaws(order);
            _relationships = _relationships.OrderCols(order);
            SaveMatrixImage(_relationships, "sorted");
            cluster.Print();
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
            SaveMatrixImage(_relationships, path + "mutual_information_matrix");
        }
    }
}