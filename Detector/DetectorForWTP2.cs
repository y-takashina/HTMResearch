﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clustering;
using MoreLinq;
using PipExtensions;
using static PipExtensions.PipExtensions;
using static Clustering.Clustering;

namespace Detector
{
    public class DetectorForWTP2
    {
        public DetectorForWTP2(List<List<double>> rawStreams)
        {
            //*
            var n = 4;
            var discretizedStreams = _discretizeSeries(rawStreams).ToArray();
            var relationships = MutualInformationMatrix(discretizedStreams);
            var rootCluster = AggregativeHierarchicalClustering(Enumerable.Range(0, discretizedStreams.Length), (i, j) => relationships[i, j], Metrics.GroupAverage);
            var leafNodes = rawStreams.Select(stream => new LeafNode(stream, stream, 4)).ToArray();
            var root = AggregateClusters((rootCluster, null)).node;
            root.Learn();
            var streamsByCluster = Enumerable.Range(0, n)
                .Select(k => root.ClusterStream
                    .Select((c, i) => (c, i))
                    .Where(t => t.Item1 == k)
                    .Select(t => t.Item2))
                .OrderByDescending(s => s.Count());
            for (var i = 0; i < n + 1; i++)
            {
                var pr = CalcPr(streamsByCluster.Skip(i));
                var f = 2 * pr.Item1 * pr.Item2 / (pr.Item1 + pr.Item2);
                Console.WriteLine($"Precision: {pr.Item1,-6:f4}, Recall: {pr.Item2,-6:f4}, FMeasure: {f}");
            }
            while (root.CanPredict)
            {
                root.Predict().Print();
            }

            (Cluster<int> cluster, Node node) AggregateClusters((Cluster<int> cluster, Node node) acc)
            {
                if (acc.cluster is Single<int> single) return (null, leafNodes[single.Value]);
                var couple = (Couple<int>) acc.cluster;
                var left = AggregateClusters((couple.Left, null)).node;
                var right = AggregateClusters((couple.Right, null)).node;
                return (null, new InternalNode(new[] {left, right}, n, Metrics.GroupAverage));
            }
            //*/
            /*
            var n = 8;
            var level1Nodes = rawStreams.Select(stream => new LeafNode(stream, stream, 8, Metrics.GroupAverage));
            var level2Nodes = Enumerable.Range(0, 6)
                .Select(i => new InternalNode(level1Nodes.Where((v, j) => j % 6 == i).ToArray(), 8, Metrics.GroupAverage));
            var level3Node = new InternalNode(level2Nodes.ToArray(), n, Metrics.Shortest);
            level3Node.Learn();
//            foreach (var value in level3Node.ClusterStream) Console.WriteLine(value);
            var streamsByCluster = Enumerable.Range(0, n)
                .Select(k => level3Node.ClusterStream
                    .Select((c, i) => (c, i))
                    .Where(t => t.Item1 == k)
                    .Select(t => t.Item2))
                .OrderByDescending(stream => stream.Count());
            for (var i = 0; i < n + 1; i++)
            {
                var pr = CalcPr(streamsByCluster.Skip(i));
                var f = 2 * pr.Item1 * pr.Item2 / (pr.Item1 + pr.Item2);
                Console.WriteLine($"Precision: {pr.Item1,-6:f4}, Recall: {pr.Item2,-6:f4}, FMeasure: {f}");
            }
            while (level3Node.CanPredict)
            {
                var distribution = level3Node.Predict();
            }
            //*/

            (double, double) CalcPr(IEnumerable<IEnumerable<int>> streams)
            {
                var stream = streams.SelectMany(v => v);
                var anomalies = new[] {10, 11, 12, 78, 148, 186, 209, 292, 395, 398, 401, 441, 442, 443};
                var nTp = stream.Intersect(anomalies).Count();
                return ((double) nTp / stream.Count(), (double) nTp / anomalies.Length);
            }
        }

        private static List<List<int>> _discretizeSeries(List<List<double>> rawSeries)
        {
            var list = new List<List<int>>();
            var discretizedValues = rawSeries.Select(series => Sampling.CalcSamplePoints(series, 16, true).ToList()).ToList();
            for (var i = 0; i < rawSeries.Count; i++)
            {
                var discretizedSeries = new List<int>();
                foreach (var value in rawSeries[i])
                {
                    var discretizedValue = double.IsNaN(value) ? value : discretizedValues[i].Where(v => !double.IsNaN(v)).MinBy(v => Math.Abs(v - value));
                    var discretizedValueIndex = discretizedValues[i].IndexOf(discretizedValue);
                    discretizedSeries.Add(discretizedValueIndex);
                }
                list.Add(discretizedSeries);
            }
            return list;
        }
    }
}