using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Detector
{
    public abstract class Cluster
    {
        public void Print(string indent = "")
        {
            if (GetType() == typeof(Single)) Console.WriteLine(indent + "[ " + ((Single) this).Value + " ]");
            else
            {
                var c = (Couple) this;
                c.Left.Print(indent + "|-");
                c.Right.Print(Regex.Replace(indent, @"\-|\+", " ") + "|-");
            }
        }
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
            if (from.GetType() == typeof(Single)) return DistanceFromSingleToCluster((Single) from, to, metrics);
            var left = DistanceFromClusterToCluster(((Couple) from).Left, to, metrics);
            var right = DistanceFromClusterToCluster(((Couple) from).Right, to, metrics);
            return left < right ? left : right;
        }

        public static Cluster SingleLinkage(int[] data, Func<int, int, double> metrics)
        {
            var clusters = data.Distinct().Select(value => (Cluster) new Single(value)).ToList();
            while (clusters.Count != 1)
            {
                var min = double.MaxValue;
                var c1 = clusters.First();
                var c2 = clusters.Last();
                for (var i = 0; i < clusters.Count; i++)
                {
                    for (var j = i + 1; j < clusters.Count; j++)
                    {
                        var d = DistanceFromClusterToCluster(clusters[i], clusters[j], metrics);
                        if (d < min)
                        {
                            min = d;
                            c1 = clusters[i];
                            c2 = clusters[j];
                        }
                    }
                }
                clusters.Add(new Couple(c1, c2));
                clusters.Remove(c1);
                clusters.Remove(c2);
            }
            return clusters.First();
        }
    }
}