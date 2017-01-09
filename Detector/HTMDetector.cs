using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Detector
{
    public class HTMDetector
    {
        private double[] _samplePoints;
        private int[] _series;
        private int[,] _transitions1;
        private int[,] _transitions2;
        private int[,] _transitions3;
        private int[,] _membership1;
        private int[,] _membership2;
        private int[,] _membership3;
        private const int N1 = 32;
        private const int N2 = 8;
        private const int N3 = 2;

        private void Initialize(double[] rawData)
        {
            _samplePoints = Sampling.CalcSamplePoints(rawData, N1);
            _series = new int[rawData.Length];
            for (var i = 0; i < rawData.Length; i++)
            {
                var mini = 0;
                var min = double.MaxValue;
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

   }
}