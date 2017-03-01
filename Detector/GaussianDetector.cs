using System;
using System.Collections.Generic;
using System.Linq;
using PipExtensions;
using static PipExtensions.PipExtensions;

namespace Detector
{
    public static class GaussianDetector
    {
        public static bool[] AnomalySeries(IEnumerable<double> rawData)
        {
            var array = rawData.ToArray();
            var dataNotNaN = array.Where(v => !double.IsNaN(v)).ToArray();
            var mean = dataNotNaN.Average();
            var stddev = dataNotNaN.StandardDeviation();
            return array.Select(v => !double.IsNaN(v) && QFunction(v, mean, stddev) < 1e-10).ToArray();
        }
    }
}