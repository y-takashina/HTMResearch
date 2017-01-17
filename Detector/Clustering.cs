using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MoreLinq;

namespace Detector
{
    public abstract class Cluster
    {
        public int Count;

        public void Print(string indent = "")
        {
            if (GetType() == typeof(Single)) Console.WriteLine(indent + "[ " + ((Single) this).Value + " ]");
            else
            {
                var c = (Couple) this;
                c.Left.Print(indent + "|-");
                c.Right.Print(Regex.Replace(indent, @"\-|\+", " ") + "+-");
            }
        }
    }

    public class Single : Cluster
    {
        public readonly int Value;

        public Single(int value)
        {
            Value = value;
            Count = 1;
        }

        public override bool Equals(object obj)
        {
            var single = obj as Single;
            return single != null && Value == single.Value;
        }

        protected bool Equals(Single other)
        {
            return Value == other.Value;
        }

        public override int GetHashCode()
        {
            return Value;
        }

        public override string ToString()
        {
            return "Single(" + Value + ")";
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
            Count = left.Count + right.Count;
        }

        public override string ToString()
        {
            return "Couple(" + Left + ", " + Right + ")";
        }
    }

    public static class Clustering
    {
        public static Single Cluster(int value)
        {
            return new Single(value);
        }

        public static Couple Cluster(Cluster left, Cluster right)
        {
            return new Couple(left, right);
        }

        public static Cluster[] Extract(this Cluster self, int n)
        {
            if (self.Count < n) throw new Exception("Count of cluster must be greater than n.");
            var list = new List<Cluster> {self};
            for (var i = 1; i < n; i++)
            {
                var curr = list.MaxBy(c => c.Count);
                var couple = (Couple) curr;
                list.Add(couple.Left);
                list.Add(couple.Right);
                list.Remove(curr);
            }
            return list.ToArray();
        }

        public static List<Single> GetMembers(this Cluster self, List<Single> acc = null)
        {
            if (acc == null) acc = new List<Single>();
            if (self.GetType() == typeof(Single)) acc.Add((Single) self);
            else
            {
                var couple = (Couple) self;
                acc.AddRange(couple.Left.GetMembers());
                acc.AddRange(couple.Right.GetMembers());
            }
            return acc.Distinct().ToList();
        }

        public static double DistanceFromSingleToCluster(Single from, Cluster to, Func<int, int, double> metrics, Func<Tuple<double, int>, Tuple<double, int>, double> metrics2)
        {
            if (to.GetType() == typeof(Single)) return metrics(from.Value, ((Single) to).Value);
            var left = DistanceFromSingleToCluster(from, ((Couple) to).Left, metrics, metrics2);
            var right = DistanceFromSingleToCluster(from, ((Couple) to).Right, metrics, metrics2);
            return metrics2(Tuple.Create(left, ((Couple) to).Left.Count), Tuple.Create(right, ((Couple) to).Right.Count));
        }

        public static double DistanceFromClusterToCluster(Cluster from, Cluster to, Func<int, int, double> metrics, Func<Tuple<double, int>, Tuple<double, int>, double> metrics2)
        {
            if (from.GetType() == typeof(Single)) return DistanceFromSingleToCluster((Single) from, to, metrics, metrics2);
            var left = DistanceFromClusterToCluster(((Couple) from).Left, to, metrics, metrics2);
            var right = DistanceFromClusterToCluster(((Couple) from).Right, to, metrics, metrics2);
            return metrics2(Tuple.Create(left, ((Couple) from).Left.Count), Tuple.Create(right, ((Couple) from).Right.Count));
        }

        public static double ShortestDistanceFromSingleToCluster(Single from, Cluster to, Func<int, int, double> metrics)
        {
            return DistanceFromSingleToCluster(from, to, metrics, (t1, t2) => t1.Item1 < t2.Item1 ? t1.Item1 : t2.Item1);
        }

        public static double ShortestDistanceFromClusterToCluster(Cluster from, Cluster to, Func<int, int, double> metrics)
        {
            return DistanceFromClusterToCluster(from, to, metrics, (t1, t2) => t1.Item1 < t2.Item1 ? t1.Item1 : t2.Item1);
        }

        public static double LongestDistanceFromSingleToCluster(Single from, Cluster to, Func<int, int, double> metrics)
        {
            return DistanceFromSingleToCluster(from, to, metrics, (t1, t2) => t1.Item1 > t2.Item1 ? t1.Item1 : t2.Item1);
        }

        public static double LongestDistanceFromClusterToCluster(Cluster from, Cluster to, Func<int, int, double> metrics)
        {
            return DistanceFromClusterToCluster(from, to, metrics, (t1, t2) => t1.Item1 > t2.Item1 ? t1.Item1 : t2.Item1);
        }

        public static double GroupAverageDistanceFromSingleToCluster(Single from, Cluster to, Func<int, int, double> metrics)
        {
            return DistanceFromSingleToCluster(from, to, metrics, (t1, t2) => (double) t1.Item2/(t1.Item2 + t2.Item2)*t1.Item1 + (double) t2.Item2/(t1.Item2 + t2.Item2)*t2.Item1);
        }

        public static double GroupAverageDistanceFromClusterToCluster(Cluster from, Cluster to, Func<int, int, double> metrics)
        {
            return DistanceFromClusterToCluster(from, to, metrics, (t1, t2) => (double) t1.Item2/(t1.Item2 + t2.Item2)*t1.Item1 + (double) t2.Item2/(t1.Item2 + t2.Item2)*t2.Item1);
        }

        public static Cluster AggregativeHierarchicalClustering(int[] data, Func<int, int, double> metrics, Func<Tuple<double, int>, Tuple<double, int>, double> metrics2)
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
                        var d = DistanceFromClusterToCluster(clusters[i], clusters[j], metrics, metrics2);
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

        public enum AHCType
        {
            Shortest,
            Longest,
            GroupAverage
        }

        public static Cluster AggregativeHierarchicalClusteringByName(int[] data, Func<int, int, double> metrics, AHCType type)
        {
            Func<Tuple<double, int>, Tuple<double, int>, double> metrics2;
            switch (type)
            {
                case AHCType.Shortest:
                    metrics2 = (t1, t2) => t1.Item1 < t2.Item1 ? t1.Item1 : t2.Item1;
                    break;
                case AHCType.Longest:
                    metrics2 = (t1, t2) => t1.Item1 > t2.Item1 ? t1.Item1 : t2.Item1;
                    break;
                case AHCType.GroupAverage:
                    metrics2 = (t1, t2) => (double) t1.Item2/(t1.Item2 + t2.Item2)*t1.Item1 + (double) t2.Item2/(t1.Item2 + t2.Item2)*t2.Item1;
                    break;
                default:
                    metrics2 = (t1, t2) => (double) t1.Item2/(t1.Item2 + t2.Item2)*t1.Item1 + (double) t2.Item2/(t1.Item2 + t2.Item2)*t2.Item1;
                    break;
            }
            return AggregativeHierarchicalClustering(data, metrics, metrics2);
        }
    }
}