using System;

namespace Detector
{
    public static class Metrics
    {
        public static Func<Tuple<double, int>, Tuple<double, int>, double> Shortest = (t1, t2) => t1.Item1 < t2.Item1 ? t1.Item1 : t2.Item1;
        public static Func<Tuple<double, int>, Tuple<double, int>, double> Longest = (t1, t2) => t1.Item1 > t2.Item1 ? t1.Item1 : t2.Item1;
        public static Func<Tuple<double, int>, Tuple<double, int>, double> GroupAverage = (t1, t2) => (double) t1.Item2/(t1.Item2 + t2.Item2)*t1.Item1 + (double) t2.Item2/(t1.Item2 + t2.Item2)*t2.Item1;
    }
}