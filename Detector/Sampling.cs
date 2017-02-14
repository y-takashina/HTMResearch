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

        public static double[] KMeansSampling(double[] data, int n)
        {
            var rand = new Random();
            var means = data.OrderBy(v => rand.Next()).Take(n).ToArray();

            while (true)
            {
                var prevMeans = means.Select(v => v).ToArray();
                var assignments = CalcAssignments(data, means);
                means = CalcMeans(data, assignments, n);
                if (!Enumerable.Range(0, n).Any(i => Math.Abs(means[i] - prevMeans[i]) > 1e-6)) break;
            }
            return means;
        }

        private static int[] CalcAssignments(double[] data, double[] means)
        {
            return data.Select(v => means.ToList().IndexOf(means.MinBy(m => Math.Abs(v - m)))).ToArray();
        }

        private static double[] CalcMeans(double[] data, int[] assignments, int n)
        {
            return Enumerable.Range(0, n).Select(i => data.Where((v, j) => i == assignments[j]).DefaultIfEmpty().Average()).ToArray();
        }
    }
}