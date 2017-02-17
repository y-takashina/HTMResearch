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
        private readonly List<List<int>> _series = new List<List<int>>();
        private double[,] _relationships;
        // 各階層におけるクラスタ数
        private const int N1 = 32;
        private const int N2 = 8;
        private const int N3 = 4;
        // 各階層におけるノード数
        private const int M1 = 38;
        private const int M2 = 6;
        private const int M3 = 1;
        // Level 1
        private readonly double[,,] _transitions1 = new double[M1, N1, N1];
        private double[,,] _probabilities1 = new double[M1, N1, N1];
        private readonly double[,,] _distances1 = new double[M1, N1, N1];
        // Level 2
        private readonly double[,,] _transitions2 = new double[M2, N2, N2];
        private double[,,] _probabilities2 = new double[M2, N2, N2];
        private readonly double[,,] _distances2 = new double[M2, N2, N2];
        // Level 3
        private readonly double[,,] _transitions3 = new double[M3, N3, N3];
        private double[,,] _probabilities3 = new double[M3, N3, N3];
        private readonly double[,,] _distances3 = new double[M3, N3, N3];


        public void Initialize(List<List<double>> rawSeries)
        {
            _discretizeSeries(rawSeries);
            _relationships = MutualInformationMatrix(_series);
        }

        public void Learn()
        {
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

        private void _discretizeSeries(List<List<double>> rawSeries)
        {
            var discretizedValues = rawSeries.Select(series => Sampling.CalcSamplePoints(series, 32, true).ToList()).ToList();
            for (var i = 0; i < rawSeries.Count; i++)
            {
                var discretizedSeries = new List<int>();
                foreach (var value in rawSeries[i])
                {
                    var discretizedValue = double.IsNaN(value) ? value : discretizedValues[i].Where(v => !double.IsNaN(v)).MinBy(v => Math.Abs(v - value));
                    var discretizedValueIndex = discretizedValues[i].IndexOf(discretizedValue);
                    discretizedSeries.Add(discretizedValueIndex);
                }
                _series.Add(discretizedSeries);
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