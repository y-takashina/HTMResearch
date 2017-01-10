﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MoreLinq;

namespace Detector
{
    public abstract class Cluster
    {
        public int Size;

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
            Size = 1;
        }

        public override bool Equals(object obj)
        {
            var single = obj as Single;
            return single != null && Value == single.Value ;
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
            Size = left.Size + right.Size;
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
            if (self.Size < n) throw new Exception("Size of cluster must be greater than n.");
            var list = new List<Cluster> {self};
            for (var i = 1; i < n; i++)
            {
                var curr = list.MaxBy(c => c.Size);
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