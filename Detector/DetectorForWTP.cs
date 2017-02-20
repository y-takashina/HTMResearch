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
        // Spatial Pooler
        private readonly List<List<int>>[] _spatialPoolerList1 = new List<List<int>>[M1];
        private readonly List<List<int>>[] _spatialPoolerList2 = new List<List<int>>[M2];
        private readonly List<List<int>>[] _spatialPoolerList3 = new List<List<int>>[M3];
        // Level 1
        private readonly double[][,] _transitions1 = Enumerable.Repeat(new double[N1, N1], M1).ToArray();
        private readonly double[][,] _probabilities1 = Enumerable.Repeat(new double[N1, N1], M1).ToArray();
        private readonly double[][,] _distances1 = Enumerable.Repeat(new double[N1, N1], M1).ToArray();
        // Level 2
        private readonly double[][,] _transitions2 = Enumerable.Repeat(new double[N2, N2], M2).ToArray();
        private readonly double[][,] _probabilities2 = Enumerable.Repeat(new double[N2, N2], M2).ToArray();
        private readonly double[][,] _distances2 = Enumerable.Repeat(new double[N2, N2], M2).ToArray();
        // Level 3
        private readonly double[][,] _transitions3 = Enumerable.Repeat(new double[N3, N3], M3).ToArray();
        private readonly double[][,] _probabilities3 = Enumerable.Repeat(new double[N3, N3], M3).ToArray();
        private readonly double[][,] _distances3 = Enumerable.Repeat(new double[N3, N3], M3).ToArray();
        // 帰属度行列1
        private readonly int[][,] _membership1TP = Enumerable.Repeat(new int[N1, N2], M1).ToArray();
        private readonly int[][,] _membership2TP = Enumerable.Repeat(new int[N2, N3], M2).ToArray();
        // 帰属度行列2
        private readonly int[][,] _membership12SP = Enumerable.Repeat(new int[M1, M2], N2).ToArray();
        private readonly int[][,] _membership23SP = Enumerable.Repeat(new int[M2, M3], N3).ToArray();

        public void Initialize(List<List<double>> rawSeries)
        {
            _discretizeSeries(rawSeries);
            _relationships = MutualInformationMatrix(_series);
            for (var m1 = 0; m1 < M1; m1++) _spatialPoolerList1[m1] = new List<List<int>>();
            for (var m2 = 0; m2 < M2; m2++) _spatialPoolerList2[m2] = new List<List<int>>();
            for (var m3 = 0; m3 < M3; m3++) _spatialPoolerList2[m3] = new List<List<int>>();
        }

        public void Learn()
        {
            for (var i = 0; i < _series.Count; i++)
            {
                // 遷移のカウント
                for (var j = 0; j < _series[i].Count - 1; j++)
                {
                    _transitions1[i][_series[i][j], _series[i][j + 1]] += 1;
                }
                // Level 1
                _probabilities1[i] = _transitions1[i].NormalizeToRaw();
                for (var j = 0; j < N1; j++)
                {
                    for (var k = 0; k < N1; k++)
                    {
                        _distances1[i][j, k] = 1 - (_probabilities1[i][j, k] + _probabilities1[i][k, j])/2;
                    }
                }
                // Level 1 の Temporal Pooling
                var cluster1 = AggregativeHierarchicalClustering(Enumerable.Range(0, N1).ToArray(), (j, k) => _distances1[i][j, k], Metrics.GroupAverage);
                var cluster1Members = cluster1.Extract(N2).Select(c => c.GetMembers().Select(s => s.Value)).ToArray();
                for (var k = 0; k < N2; k++)
                {
                    for (var j = 0; j < N1; j++)
                    {
                        _membership1TP[i][j, k] = cluster1Members[k].Contains(j) ? 1 : 0;
                    }
                }
            }
            // Level 1-2 間の Spatial Pooling
            for (var i = 0; i < _series.First().Count; i++)
            {
                for (var m2 = 0; m2 < M2; m2++)
                {
                    var list = new List<int>();
                    for (var m1 = m2; m1 < M1; m1 += M2)
                    {
                        var pattern = Enumerable.Range(0, N1).Select(j => j == _series[m1][i] ? 1 : 0).ToArray();
                        var group = _membership1TP[m1].T().Mul(pattern);
                        list.AddRange(group);
                    }
                    if (_spatialPoolerList2[m2].All(pattern => !pattern.SequenceEqual(list))) _spatialPoolerList2[m2].Add(list);
                }
            }
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