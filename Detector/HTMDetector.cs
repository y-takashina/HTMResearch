using System;
using System.IO;
using System.Linq;
using PipExtensions;
using RakuChart;
using static MatViz.MatViz;

namespace Detector
{
    public class HTMDetector
    {
        private double[] _samplePoints;
        private int[] _series;
        private int[] _predictedSeries;
        private double[] _errorSeries;
        private double[] _errorSeries2;
        // 遷移確率行列
        // Level 1
        private readonly double[,] _transitions1 = new double[N1, N1];
        private double[,] _probabilities1 = new double[N1, N1];
        private readonly double[,] _distances1Mean = new double[N1, N1];
        private readonly double[,] _distances1Min = new double[N1, N1];
        // Level 2
        private double[,] _probabilities2 = new double[N2, N2];
        private readonly double[,] _distances2Mean = new double[N2, N2];
        private readonly double[,] _distances2Min = new double[N2, N2];
        // Level 3
        private double[,] _probabilities3 = new double[N3, N3];
        private readonly double[,] _distances3Mean = new double[N3, N3];
        private readonly double[,] _distances3Min = new double[N3, N3];
        // 帰属度行列
        private readonly double[,] _membership12 = new double[N1, N2];
        private readonly double[,] _membership23 = new double[N2, N3];
        // 各階層におけるクラスタ数
        private const int N1 = 32;
        private const int N2 = 8;
        private const int N3 = 4;

        public void Initialize(double[] rawData)
        {
            _samplePoints = Sampling.KMeansSampling(rawData, N1);
            _series = new int[rawData.Length];
            for (var i = 0; i < rawData.Length; i++)
            {
                var min = double.MaxValue;
                var mini = 0;
                for (var j = 0; j < _samplePoints.Length; j++)
                {
                    var d = Math.Abs(rawData[i] - _samplePoints[j]);
                    if (d < min)
                    {
                        min = d;
                        mini = j;
                    }
                }
                _series[i] = mini;
            }
        }

        /// <summary>
        /// 学習
        /// </summary>
        public void Learn()
        {
            // 遷移のカウント
            for (var i = 0; i < _series.Length/2; i++)
            {
                _transitions1[_series[i], _series[i + 1]] += 1;
            }
            // Level 1
            _probabilities1 = _transitions1.NormalizeToRaw();
            for (var j = 0; j < N1; j++)
            {
                for (var k = 0; k < N1; k++)
                {
                    _distances1Mean[j, k] = 1 - (_probabilities1[j, k] + _probabilities1[k, j])/2;
                    _distances1Min[j, k] = 1 - Math.Max(_probabilities1[j, k], _probabilities1[k, j]);
                }
            }
            // Level 1 の Level 2 に対する帰属度
            var cluster1 = Clustering.AggregativeHierarchicalClustering(Enumerable.Range(0, N1).ToArray(), (j, k) => _distances1Mean[j, k], Metrics.GroupAverage);
            var cluster1Members = cluster1.Extract(N2).Select(c => c.GetMembers().Select(s => s.Value)).ToArray();
            for (var k = 0; k < N2; k++)
            {
                var sum = cluster1Members[k].Count();
                for (var j = 0; j < N1; j++)
                {
                    _membership12[j, k] = cluster1Members[k].Contains(j) ? 1.0/sum : 1e-6;
                }
            }
            // Level 2
            _probabilities2 = _membership12.PseudoInverse().Mul(_probabilities1).Mul(_membership12);
            for (var j = 0; j < N2; j++)
            {
                for (var k = 0; k < N2; k++)
                {
                    _distances2Mean[j, k] = 1 - (_probabilities2[j, k] + _probabilities2[k, j])/2;
                    _distances2Min[j, k] = 1 - Math.Max(_probabilities2[j, k], _probabilities2[k, j]);
                }
            }
            // Level 2 の Level 3 に対する帰属度
            var cluster2 = Clustering.AggregativeHierarchicalClustering(Enumerable.Range(0, N2).ToArray(), (j, k) => _distances2Mean[j, k], Metrics.GroupAverage);
            var cluster2Members = cluster2.Extract(N3).Select(c => c.GetMembers().Select(s => s.Value)).ToArray();
            for (var k = 0; k < N3; k++)
            {
                var sum = cluster2Members[k].Count();
                for (var j = 0; j < N2; j++)
                {
                    _membership23[j, k] = cluster2Members[k].Contains(j) ? 1.0/sum : 1e-6;
                }
            }
            // Level 3
            _probabilities3 = _membership23.PseudoInverse().Mul(_probabilities2).Mul(_membership23);
            for (var j = 0; j < N3; j++)
            {
                for (var k = 0; k < N3; k++)
                {
                    _distances3Mean[j, k] = 1 - (_probabilities3[j, k] + _probabilities3[k, j])/2;
                    _distances3Min[j, k] = 1 - Math.Max(_probabilities3[j, k], _probabilities3[k, j]);
                }
            }
        }

        /// <summary>
        /// 予測
        /// </summary>
        public void Predict()
        {
            _predictedSeries = new int[_series.Length];
            _errorSeries = new double[_series.Length];
            _errorSeries2 = new double[_series.Length];
            // 事前分布
            var prior1 = Enumerable.Range(0, N1).Select(i => 1.0/N1).ToArray();
            var prior2 = Enumerable.Range(0, N2).Select(i => 1.0/N2).ToArray();
            var frequencies = Enumerable.Range(0, N1).Select(i => (double) _series.Take(_series.Length/2).Count(v => v == i)/_series.Length*2).ToArray();
            // 予測
            for (var i = 0; i < _series.Length - 1; i++)
            {
                // 状態
                var state1 = Enumerable.Range(0, N1).Select(j => j == _series[i] ? 1.0 : 0.0).ToArray();
                var state2 = prior2.Select(x => x).ToArray();
                var message1 = _membership12.T().Mul(state1);
                var message2 = _probabilities2.Mul(state2);
                for (var j = 0; j < N2; j++) state2[j] = message1[j]*message2[j];
                var sum = state2.Sum();
                for (var j = 0; j < N2; j++) state2[j] = sum < 1e-12 ? 1.0/N2 : state2[j]/sum;
                var prediction = _membership12.Mul(_probabilities2.Mul(state2));
                var error = -Math.Log(prediction[_series[i + 1]], 2);
                _errorSeries[i + 1] = double.IsPositiveInfinity(error) ? 100 : error;
                var error2 = -Math.Log(frequencies[_series[i + 1]], 2);
                _errorSeries2[i + 1] = double.IsPositiveInfinity(error2) ? 100 : error2;
                _predictedSeries[i + 1] = prediction.ToList().IndexOf(prediction.Max());
            }
            //_mutualInformations.ForEach(v => Console.Write(v.ToString("F4") + "\n"));
            //var g1 = _membership12.Mul(_membership23.Mul(new double[] {1, 0, 0, 0}));
            //var g2 = _membership12.Mul(_membership23.Mul(new double[] {0, 1, 0, 0}));
            //var g3 = _membership12.Mul(_membership23.Mul(new double[] {0, 0, 1, 0}));
            //var g4 = _membership12.Mul(_membership23.Mul(new double[] {0, 0, 0, 1}));
            //g1.ForEach((p, i) => { if (Math.Abs(p - 1) < 1e-6) Console.Write(_samplePoints[i] + ", "); });
            //Console.WriteLine();
            //g2.ForEach((p, i) => { if (Math.Abs(p - 1) < 1e-6) Console.Write(_samplePoints[i] + ", "); });
            //Console.WriteLine();
            //g3.ForEach((p, i) => { if (Math.Abs(p - 1) < 1e-6) Console.Write(_samplePoints[i] + ", "); });
            //Console.WriteLine();
            //g4.ForEach((p, i) => { if (Math.Abs(p - 1) < 1e-6) Console.Write(_samplePoints[i] + ", "); });
            //Console.WriteLine();
            // 並べ替えて表示云々
            //var c1 = Clustering.AggregativeHierarchicalClusteringByName(Enumerable.Range(0, N1).ToArray(), (j, k) => distances1Mean[j, k], Clustering.AHCType.GroupAverage);
            //var cluster1Order = c1.Extract(N2).Select(c => c.GetMembers().Select(s => s.Value)).SelectMany(i => i).ToArray();
            //distances1Mean = distances1Mean.OrderRaws(cluster1Order);
            //distances1Mean = distances1Mean.OrderCols(cluster1Order);
            //distances1Min = distances1Min.OrderRaws(cluster1Order);
            //distances1Min = distances1Min.OrderCols(cluster1Order);
            //_membership12 = _membership12.OrderRaws(cluster1Order);
            //c1.Extract(N2).Select(c => c.GetMembers()).ForEach((singles, idx) => Console.WriteLine(idx + ": " + singles.Select(s => s.Value).Concatenate()));
            //cluster1Order.ForEach(x => Console.Write(x + ", "));
        }

        /// <summary>
        /// 結果の保存
        /// </summary>
        public void SaveResultImages(string path = ".")
        {
            Directory.CreateDirectory(path);
            path += '\\';
            SaveMatrixImage(_transitions1, path + "layer1_transitions");
            SaveMatrixImage(_probabilities1, path + "layer1_probabilities");
            SaveMatrixImage(_distances1Mean, path + "layer1_distances_mean", threshold: double.MaxValue, bgWhite: false);
            SaveMatrixImage(_distances1Min, path + "layer1_distances_min", threshold: double.MaxValue, bgWhite: false);
            SaveMatrixImage(_membership12, path + "layer12_membership");
            SaveMatrixImage(_probabilities2, path + "layer2_probabilities");
            SaveMatrixImage(_distances2Mean, path + "layer2_distances_mean", threshold: double.MaxValue, bgWhite: false);
            SaveMatrixImage(_distances2Min, path + "layer2_distances_min", threshold: double.MaxValue, bgWhite: false);
            SaveMatrixImage(_membership23, path + "layer23_membership");
            SaveMatrixImage(_probabilities3, path + "layer3_probabilities");
            SaveMatrixImage(_distances3Mean, path + "layer3_distances_mean", threshold: double.MaxValue, bgWhite: false);
            SaveMatrixImage(_distances3Min, path + "layer3_distances_min", threshold: double.MaxValue, bgWhite: false);
            ChartExtensions.CreateChart(_series.Select(i => _samplePoints[i]).ToArray()).SaveImage(path + "prediction");
            ChartExtensions.CreateChart(_errorSeries).SaveImage(path + "error");
            ChartExtensions.CreateChart(_errorSeries2).SaveImage(path + "error2");
        }
    }
}