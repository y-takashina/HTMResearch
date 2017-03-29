using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clustering;
using MoreLinq;
using PipExtensions;
using static PipExtensions.PipExtensions;

namespace Detector
{
    public class DetectorForWTP2
    {
        public DetectorForWTP2(List<List<double>> rawSeries)
        {
            var streams = _discretizeSeries(rawSeries).ToArray();

/*
            var relationships = MutualInformationMatrix(streams);
            MatViz.MatViz.SaveMatrixImage(relationships, "relationships");
            var rootCluster = Clustering.Clustering.AggregativeHierarchicalClustering(Enumerable.Range(0, streams.Length), (i, j) => relationships[i, j], Metrics.GroupAverage);
            var leafNodes = streams.Select(s => new LeafNode(s, 8)).ToArray();
            var root = searchCluster((rootCluster, null)).node;
            root.Learn();
            foreach (var value in root.Stream) Console.WriteLine(value);

            (Cluster<int> cluster, Node node) searchCluster((Cluster<int> cluster, Node node) acc)
            {
                if (acc.cluster is Single<int> single) return (null, new InternalNode(new[] {acc.node, leafNodes[single.Value]}, 4));
                if (acc.cluster is Couple<int> couple)
                {
                    var left = searchCluster((couple.Left, null));
                    var right = searchCluster((couple.Right, null));
                    return (null, new InternalNode(new[] {left.node, right.node}, 4));
                }
                return acc;
            }

//*/

/*
            var level1Nodes = streams.Select(s => new LeafNode(s, 4)).ToArray();
            var level2Nodes2 = Enumerable.Range(0, 19).Select(i =>
                new InternalNode(level1Nodes.Where((v, j) => j % 19 == i).ToArray(), 4, Metrics.GroupAverage)
            );
            var level3Nodes2 = Enumerable.Range(0, 9).Select(i =>
                new InternalNode(level2Nodes2.Where((v, j) => j % 10 == i).ToArray(), 4, Metrics.GroupAverage)
            );
            var level4Nodes2 = Enumerable.Range(0, 4).Select(i =>
                new InternalNode(level3Nodes2.Where((v, j) => j % 5 == i).ToArray(), 8, Metrics.GroupAverage)
            );
            var level5Nodes2 = Enumerable.Range(0, 2).Select(i =>
                new InternalNode(level4Nodes2.Where((v, j) => j % 2 == i).ToArray(), 8, Metrics.GroupAverage)
            );
            var level6Node2 = new InternalNode(level5Nodes2.ToArray(), 8, Metrics.Shortest);
            var level7Node2 = new InternalNode(new[] {level6Node2}, 1);
            level7Node2.Learn();
            foreach (var value in level7Node2.Stream) Console.WriteLine(value);
//*/
/*
            for (var a = 4; a <= 32; a *= 2)
            {
                for (var b = 4; b < a * a; b += a)
                {
                    var level1Nodes = streams.Select(stream => new LeafNode(stream, a, Metrics.GroupAverage));
                    var level2Nodes = Enumerable.Range(0, 6).Select(i =>
                        new InternalNode(level1Nodes.Where((v, j) => j % 6 == i).ToArray(), a, Metrics.GroupAverage)
                    );
                    var level3Node = new InternalNode(level2Nodes.ToArray(), b, Metrics.Shortest);
                    var level4Node = new InternalNode(new[] {level3Node}, 1, Metrics.Shortest);
                    try
                    {
                        level4Node.Learn();
                    }
                    catch (Exception e)
                    {
                        continue;
                    }
                    var streamsForEachCluster = Enumerable.Range(0, b).Select(k => level4Node.Stream.Select((cluster, i) => (cluster, i)).Where(t => t.Item1 == k).Select(t => t.Item2));
                    streamsForEachCluster = streamsForEachCluster.OrderByDescending(stream => stream.Count());
                    var fMeasure = Enumerable.Range(0, b).Select(i => _calcPR(streamsForEachCluster.Skip(i))).Max(t => 2 * t.Item1 * t.Item2 / (t.Item1 + t.Item2));
                    Console.WriteLine($"max f: {fMeasure}, params: {a},{b}");
                }
            }
//*/
//*
            var nFinalCluster = 6;
            var level1Nodes = streams.Select(stream => new LeafNode(stream, 8, Metrics.GroupAverage));
            var level2Nodes = Enumerable.Range(0, 6).Select(i =>
                new InternalNode(level1Nodes.Where((v, j) => j % 6 == i).ToArray(), 8, Metrics.GroupAverage)
            );
            var level3Node = new InternalNode(level2Nodes.ToArray(), nFinalCluster, Metrics.Shortest);
            var level4Node = new InternalNode(new[] {level3Node}, 1, Metrics.Shortest);
            level4Node.Learn();
            foreach (var value in level4Node.Stream) Console.WriteLine(value);
            var streamsForEachCluster = Enumerable.Range(0, nFinalCluster).Select(k => level4Node.Stream.Select((cluster, i) => (cluster, i)).Where(t => t.Item1 == k).Select(t => t.Item2));
            streamsForEachCluster = streamsForEachCluster.OrderByDescending(stream => stream.Count());
            for (var i = 0; i < nFinalCluster + 1; i++)
            {
                var pr = _calcPR(streamsForEachCluster.Skip(i));
                Console.WriteLine($"Precision: {pr.Item1,-6:f4}, Recall: {pr.Item2,-6:f4}");
            }
            var fMeasure = Enumerable.Range(0, nFinalCluster).Select(i => _calcPR(streamsForEachCluster.Skip(i))).Max(t => 2 * t.Item1 * t.Item2 / (t.Item1 + t.Item2));
            Console.WriteLine($"max f: {fMeasure}, params: {8},{16}");
//*/
        }

        private static (double, double) _calcPR(IEnumerable<IEnumerable<int>> streams)
        {
            var stream = streams.Aggregate(new List<int>(), (list, enumerable) =>
            {
                list.AddRange(enumerable);
                return list;
            });
            var anomalies = new[] {10, 11, 12, 78, 148, 186, 209, 292, 395, 398, 401, 441, 442, 443};
            var nTp = stream.Intersect(anomalies).Count();
            return ((double) nTp / stream.Count, (double) nTp / anomalies.Length);
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