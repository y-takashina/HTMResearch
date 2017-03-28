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
//            var relationships = MutualInformationMatrix(series);
//            MatViz.MatViz.SaveMatrixImage(relationships, "relationships");
//            var cor = Clustering.Clustering.AggregativeHierarchicalClustering(Enumerable.Range(0, series.Length), (i, j) => relationships[i, j], Metrics.GroupAverage);
//            cor.Print();

//            /*
            var level1Nodes = streams.Select(s => new LeafNode(s, 8)).ToArray();
            var level2Nodes2 = Enumerable.Range(0, 19).Select(i =>
                new InternalNode(level1Nodes.Where((v, j) => j % 19 == i).ToArray(), 8, Metrics.GroupAverage)
            );
            var level3Nodes2 = Enumerable.Range(0, 10).Select(i =>
                new InternalNode(level2Nodes2.Where((v, j) => j % 10 == i).ToArray(), 8, Metrics.GroupAverage)
            );
            var level4Nodes2 = Enumerable.Range(0, 5).Select(i =>
                new InternalNode(level3Nodes2.Where((v, j) => j % 5 == i).ToArray(), 8, Metrics.Shortest)
            );
            var level5Nodes2 = Enumerable.Range(0, 2).Select(i =>
                new InternalNode(level4Nodes2.Where((v, j) => j % 2 == i).ToArray(), 8, Metrics.Shortest)
            );
            var level6Node2 = new InternalNode(level5Nodes2.ToArray(), 4, Metrics.Shortest);
            var level7Node2 = new InternalNode(new[] {level6Node2}, 4);
            level7Node2.Learn();
            foreach (var value in level7Node2.Stream) Console.WriteLine(value);
//            */
            /*
            var level1Nodes = streams.Select(stream => new LeafNode(stream, 4, Metrics.GroupAverage));
            var level2Nodes = Enumerable.Range(0, 6).Select(i =>
                new InternalNode(level1Nodes.Where((v, j) => j % 6 == i), 4, Metrics.GroupAverage)
            );
            var level3Node = new InternalNode(level2Nodes.ToArray(), 4, Metrics.Shortest);
            var level4Node = new InternalNode(new[] {level3Node}, 4, Metrics.Shortest);
            level4Node.Learn();
            foreach (var value in level4Node.Stream) Console.WriteLine(value);
            */
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