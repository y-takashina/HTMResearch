using System;
using System.IO;
using System.Linq;
using PipExtensions;
using RakuChart;
using Clustering;
using static MatViz.MatViz;

namespace Detector
{
    public class HTMDetector
    {
        private double[] _samplePoints;
        private int[] _series;
        private double[] _predictionEntropySeriesHTM;
        private double[] _predictionEntropySeriesFreq;
        // 遷移確率行列、距離行列
        // Level 1
        private readonly double[,] _transitions1 = new double[N1, N1];
        private double[,] _probabilities1 = new double[N1, N1];
        private readonly double[,] _distances1 = new double[N1, N1];
        // Level 2
        private readonly double[,] _transitions2 = new double[N2, N2];
        private double[,] _probabilities2 = new double[N2, N2];
        private readonly double[,] _distances2 = new double[N2, N2];
        // Level 3
        private double[,] _probabilities3 = new double[N3, N3];
        private readonly double[,] _distances3 = new double[N3, N3];
        // 帰属度行列
        private readonly double[,] _membership12 = new double[N1, N2];
        private readonly double[,] _membership23 = new double[N2, N3];
        // 各階層におけるクラスタ数
        private const int N1 = 32;
        private const int N2 = 8;
        private const int N3 = 4;

        public void Initialize(double[] rawData)
        {
            _samplePoints = Sampling.CalcSamplePoints(rawData, N1);
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

        public void Learn()
        {
            // 遷移のカウント
            for (var i = 0; i < _series.Length / 2; i++)
            {
                _transitions1[_series[i], _series[i + 1]] += 1;
            }
            // Level 1
            _probabilities1 = _transitions1.NormalizeToRaw();
            for (var j = 0; j < N1; j++)
            {
                for (var k = 0; k < N1; k++)
                {
                    _distances1[j, k] = 1 - (_probabilities1[j, k] + _probabilities1[k, j]) / 2;
                }
            }
            // Level 1 の Level 2 に対する帰属度
            var cluster1 = Clustering.Clustering.AggregativeHierarchicalClustering(Enumerable.Range(0, N1).ToArray(), (j, k) => _distances1[j, k], Metrics.GroupAverage);
            var cluster1Members = cluster1.Extract(N2).Select(c => c.SelectMany()).ToArray();
            for (var k = 0; k < N2; k++)
            {
                var sum = cluster1Members[k].Count();
                for (var j = 0; j < N1; j++)
                {
                    _membership12[j, k] = cluster1Members[k].Contains(j) ? 1.0 : 1e-6;
                }
            }
            // Level 2
            for (var i = 0; i < _series.Length - 2; i++)
            {
                // cluster1membersを使う
                int from2 = -1, to2 = -1;
                for (var j = 0; j < N2; j++)
                {
                    if (1 - _membership12[_series[i], j] < 1e-6) from2 = j;
                    if (1 - _membership12[_series[i + 1], j] < 1e-6) to2 = j;
                }
                _transitions2[from2, to2] += 1;
            }
            _probabilities2 = _transitions2.NormalizeToRaw();
//            _probabilities2 = _membership12.PseudoInverse().Mul(_probabilities1).Mul(_membership12);
            for (var j = 0; j < N2; j++)
            {
                for (var k = 0; k < N2; k++)
                {
                    _distances2[j, k] = 1 - (_probabilities2[j, k] + _probabilities2[k, j]) / 2;
                }
            }
            // Level 2 の Level 3 に対する帰属度
            var cluster2 = Clustering.Clustering.AggregativeHierarchicalClustering(Enumerable.Range(0, N2).ToArray(), (j, k) => _distances2[j, k], Metrics.GroupAverage);
            var cluster2Members = cluster2.Extract(N3).Select(c => c.SelectMany()).ToArray();
            for (var k = 0; k < N3; k++)
            {
                var sum = cluster2Members[k].Count();
                for (var j = 0; j < N2; j++)
                {
                    _membership23[j, k] = cluster2Members[k].Contains(j) ? 1.0 : 1e-6;
                }
            }
            // Level 3
            _probabilities3 = _membership23.PseudoInverse().Mul(_probabilities2).Mul(_membership23).NormalizeToRaw();
            for (var j = 0; j < N3; j++)
            {
                for (var k = 0; k < N3; k++)
                {
                    _distances3[j, k] = 1 - (_probabilities3[j, k] + _probabilities3[k, j]) / 2;
                }
            }
        }

        public void Predict()
        {
            _predictionEntropySeriesHTM = new double[_series.Length];
            _predictionEntropySeriesFreq = new double[_series.Length];
            // 事前分布
            var prior1 = Enumerable.Range(0, N1).Select(i => (double) _series.Take(_series.Length / 2).Count(v => v == i) / _series.Length * 2).ToArray();
            for (var i = 0; i < prior1.Length; i++) if (prior1[i] < 1e-6) prior1[i] = 1e-6;
            var prior2 = _membership12.T().Mul(prior1);
            var state2 = prior2.Select(x => x).ToArray();
            // 予測
            for (var i = _series.Length / 2; i < _series.Length - 1; i++)
            {
                // 状態
                var state1 = Enumerable.Range(0, N1).Select(j => j == _series[i] ? 1.0 : 1e-6).ToArray();
                var message1 = _membership12.T().Mul(state1);
                var message2 = _probabilities2.Mul(state2);
                for (var j = 0; j < N2; j++) state2[j] = message1[j] * message2[j];
                var sum = state2.Sum();
                for (var j = 0; j < N2; j++) state2[j] = state2[j] / sum;
                var prediction = _membership12.Mul(_probabilities2.Mul(state2));
                _predictionEntropySeriesHTM[i + 1] = -Math.Log(prediction[_series[i + 1]], 2);
                _predictionEntropySeriesFreq[i + 1] = -Math.Log(prior1[_series[i + 1]], 2);
            }
            // 並べ替えて表示云々
            //var c1 = Clustering.AggregativeHierarchicalClusteringByName(Enumerable.Range(0, N1).ToArray(), (j, k) => distances1Mean[j, k], Clustering.AHCType.GroupAverage);
            //var cluster1Order = c1.Extract(N2).Select(c => c.SelectMany().Select(s => s.Value)).SelectMany(i => i).ToArray();
            //distances1Mean = distances1Mean.OrderRaws(cluster1Order);
            //distances1Mean = distances1Mean.OrderCols(cluster1Order);
            //distances1Min = distances1Min.OrderRaws(cluster1Order);
            //distances1Min = distances1Min.OrderCols(cluster1Order);
            //_membership12 = _membership12.OrderRaws(cluster1Order);
            //c1.Extract(N2).Select(c => c.SelectMany()).ForEach((singles, idx) => Console.WriteLine(idx + ": " + singles.Select(s => s.Value).Concatenate()));
            //cluster1Order.ForEach(x => Console.Write(x + ", "));
        }

        public void SaveResultImages(string path = ".")
        {
            Directory.CreateDirectory(path);
            path += '\\';
            SaveMatrixImage(_transitions1, path + "layer1_transitions");
            SaveMatrixImage(_probabilities1, path + "layer1_probabilities");
            SaveMatrixImage(_distances1, path + "layer1_distances_mean", threshold: double.MaxValue, bgWhite: false);
            SaveMatrixImage(_membership12, path + "layer12_membership");
            SaveMatrixImage(_probabilities2, path + "layer2_probabilities");
            SaveMatrixImage(_distances2, path + "layer2_distances_mean", threshold: double.MaxValue, bgWhite: false);
            SaveMatrixImage(_membership23, path + "layer23_membership");
            SaveMatrixImage(_probabilities3, path + "layer3_probabilities");
            SaveMatrixImage(_distances3, path + "layer3_distances_mean", threshold: double.MaxValue, bgWhite: false);
            ChartExtensions.CreateChart(_series.Select(i => _samplePoints[i]).ToArray()).SaveImage(path + "original");
            ChartExtensions.CreateChart(_predictionEntropySeriesHTM).SaveImage(path + "prediction_entropy_htm");
            ChartExtensions.CreateChart(_predictionEntropySeriesFreq).SaveImage(path + "prediction_entropy_freq");
        }
    }
}