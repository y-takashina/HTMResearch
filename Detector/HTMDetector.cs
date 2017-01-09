using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Detector.Extensions;

namespace Detector
{
    public class HTMDetector
    {
        private double[] _samplePoints;
        private int[] _series;
//        private int[,] _transitions1;
//        private int[,] _transitions2;
//        private int[,] _transitions3;
//        private int[,] _membership1;
//        private int[,] _membership2;
//        private int[,] _membership3;
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
            var transitions2 = new int[N2, N2];
            var probabilities2 = new double[N2, N2];
            var transitions3 = new int[N3, N3];
            var probabilities3 = new double[N3, N3];
            var membership1 = new double[N1, N2];
            var membership2 = new double[N2, N3];

            for (var i = 0; i < _series.Length - 1; i++)
            {
                // Level 0 の遷移
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
            }
        }
    }
}