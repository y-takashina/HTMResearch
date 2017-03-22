using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clustering;
using MoreLinq;
using PipExtensions;

namespace Detector
{
    public class DetectorForWTP2
    {
        public DetectorForWTP2(List<List<double>> rawSeries)
        {
            var series = _discretizeSeries(rawSeries).ToArray();
            var level1Nodes = series.Select(s => new LeafNode(s, 8));
            var level2Node = new InternalNode(level1Nodes.ToArray(), 8, Metrics.Shortest);
            var level3Node = new InternalNode(new[] {level2Node}, 8);
            level3Node.Learn();
            foreach (var value in level3Node.Stream)
            {
                Console.WriteLine(value);
            }
        }

        private static List<List<int>> _discretizeSeries(List<List<double>> rawSeries)
        {
            var list = new List<List<int>>();
            var discretizedValues = rawSeries.Select(series => Sampling.CalcSamplePoints(series, 32, true).ToList()).ToList();
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