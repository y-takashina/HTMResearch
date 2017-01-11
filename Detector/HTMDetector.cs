using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MatrixVisualizer;
using MoreLinq;
using PipExtensions;

namespace Detector
{
    public class HTMDetector
    {
        private double[] _samplePoints;
        private int[] _series;
        private const int N1 = 32;
        private const int N2 = 8;
        private const int N3 = 2;

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

        public void Detect()
        {
            var transitions1 = new int[N1, N1];
            var probabilities1 = new double[N1, N1];
            var distances1Mean = new double[N1, N1];
            var distances1Min = new double[N1, N1];
            var transitions2 = new int[N2, N2];
            var probabilities2 = new double[N2, N2];
            var distances2Mean = new double[N2, N2];
            var distances2Min = new double[N2, N2];
            var transitions3 = new int[N3, N3];
            var probabilities3 = new double[N3, N3];
            var distances3Mean = new double[N3, N3];
            var distances3Min = new double[N3, N3];
            var membership12 = new double[N1, N2];
            var membership23 = new double[N2, N3];

            for (var i = 0; i < _series.Length - 1; i++)
            {
                // Level 1 の遷移
                transitions1[_series[i], _series[i + 1]] += 1;
                for (var j = 0; j < N1; j++)
                {
                    var sum = 0;
                    for (var k = 0; k < N1; k++)
                    {
                        sum += transitions1[k, j];
                    }
                    for (var k = 0; k < N1; k++)
                    {
                        probabilities1[k, j] = sum == 0 ? 1.0/N1 : (double) transitions1[k, j]/sum;
                    }
                }
                for (var j = 0; j < N1; j++)
                {
                    for (var k = 0; k < N1; k++)
                    {
                        distances1Mean[j, k] = 1 - (probabilities1[j, k] + probabilities1[k, j])/2;
                        distances1Min[j, k] = 1 - Math.Max(probabilities1[j, k], probabilities1[k, j]);
                    }
                }
                var cluster1 = Clustering.SingleLinkage(Enumerable.Range(0, N1).ToArray(), (j, k) => distances1Mean[j, k]);
                var cluster1Members = cluster1.Extract(N2).Select(c => c.GetMembers().Select(s => s.Value)).ToArray();
                for (var j = 0; j < N2; j++)
                {
                    for (var k = 0; k < N1; k++)
                    {
                        membership12[k, j] = cluster1Members[j].Contains(k) ? 1 : 0;
                    }
                }
            }
            //var cluster1 = Clustering.SingleLinkage(Enumerable.Range(0, N1).ToArray(), (j, k) => distances1Mean[j, k]);
            //var cluster1Members = cluster1.Extract(N2).Select(c => c.GetMembers().Select(s => s.Value).ToArray()).ToArray();
            //var cluster1Order = cluster1Members.SelectMany(j => j).ToArray();
            //distances1Mean = distances1Mean.OrderRaws(cluster1Order);
            //distances1Mean = distances1Mean.OrderCols(cluster1Order);
            //distances1Min = distances1Min.OrderRaws(cluster1Order);
            //distances1Min = distances1Min.OrderCols(cluster1Order);
            //cluster1.Extract(N2).Select(c => c.GetMembers()).ForEach((singles, idx) => Console.WriteLine(idx + ": " + singles.Select(s => s.Value).Concatenate()));
            //cluster1Order.ForEach(x => Console.Write(x + ", "));

            MatrixVisualizer.MatrixVisualizer.SaveMatrixImage(probabilities1, "layer1_probabilities", threshold: 1);
            MatrixVisualizer.MatrixVisualizer.SaveMatrixImage(distances1Mean, "layer1_distances_mean", threshold: 1, bgWhite: false);
            MatrixVisualizer.MatrixVisualizer.SaveMatrixImage(distances1Min, "layer1_distances_min", threshold: 1, bgWhite: false);
            MatrixVisualizer.MatrixVisualizer.SaveMatrixImage(membership12, "layer12_membership", threshold: 1, bgWhite: false);
        }
    }
}