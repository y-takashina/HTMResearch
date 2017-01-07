using System;

namespace Detector
{
    public abstract class Cluster
    {
    }

    public class Single : Cluster
    {
        public readonly int Value;

        public Single(int value)
        {
            Value = value;
        }
    }

    public class Couple : Cluster
    {
        public readonly Cluster Left;
        public readonly Cluster Right;

        public Couple(Cluster left, Cluster right)
        {
            Left = left;
            Right = right;
        }
    }

    public static class Clustering
    {
        public static double DistanceFromSingleToCluster(Single from, Cluster to, Func<int, int, double> metrics)
        {
            if (to.GetType() == typeof(Single)) return metrics(from.Value, ((Single) to).Value);
            var left = DistanceFromSingleToCluster(from, ((Couple) to).Left, metrics);
            var right = DistanceFromSingleToCluster(from, ((Couple) to).Right, metrics);
            return left < right ? left : right;
        }

        public static double DistanceFromClusterToCluster(Cluster from, Cluster to, Func<int, int, double> metrics)
        {
            if (from.GetType() == typeof(Single)) DistanceFromSingleToCluster((Single) from, to, metrics);
            var left = DistanceFromClusterToCluster(((Couple) from).Left, to, metrics);
            var right = DistanceFromClusterToCluster(((Couple) from).Right, to, metrics);
            return left < right ? left : right;
        }
    }
}