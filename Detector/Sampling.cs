using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoreLinq;
using PipExtensions;

namespace Detector
{
    public static class Sampling
    {
        public static double[] CalcSamplePoints(IEnumerable<double> data, int n, bool allowNaN = false)
        {
            var array = data.Where(v => !double.IsNaN(v)).ToArray();
            var average = array.Average();
            var stddev = array.StandardDeviation();
            var min = average - 3*stddev;
            var max = average + 3*stddev;
            var unit = (max - min)/n;
            var points = Enumerable.Range(0, n).Select(i => max - i*unit).ToArray();
            points[0] = average + 4*stddev;
            points[n - 1] = average - 4*stddev;
            if (allowNaN) points[1] = double.NaN; // 修正すべき。
            return points;
        }

        public static double[] KMeansSampling(double[] data, int k)
        {
            return Clustering.Clustering.KMeansClustering(data, k).Item1;
        }

        public static double[] KMedoidsSampling(double[] data, int k)
        {
            return Clustering.Clustering.KMedoidsClustering(data, k).Item1;
        }
    }
}