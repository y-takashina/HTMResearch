using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PipExtensions;

namespace Detector
{
    public static class Sampling
    {
        public static double[] CalcSamplePoints(double[] data, int n)
        {
            var average = data.Average();
            var stddev = data.StandardDeviation();
            var min = average - 3*stddev;
            var max = average + 3*stddev;
            var unit = (max - min)/n;
            return Enumerable.Range(0, n).Select(i => max - i*unit).ToArray();
        }
    }
}
