using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using MoreLinq;
using Clustering;
using PipExtensions;


namespace Detector
{
    public class Node
    {
        public IEnumerable<int> Stream { get; set; }
        public List<int> SpatialPooler { get; set; }
        public List<int> TemporalPooler { get; set; }
        public IEnumerable<Node> ChildNodes { get; set; }
        public int[,] Membership { get; set; }


        public Node(IEnumerable<int> inputStream, int numberTemporalGroup)
        {
            Stream = inputStream;
            TemporalPooler = new int[numberTemporalGroup].ToList();
        }

        /// <summary>
        /// 1-of-k な値を入力として受け取ったら、
        /// 1-of-k なクラスタ割り当てを上に出力する。
        /// </summary>
        public int[] Forward(int[] input)
        {
            throw new NotImplementedException();
        }

        public double[] Forward(double[] input)
        {
            throw new NotImplementedException();
        }

        public int[] Backward(int[] input)
        {
            throw new NotImplementedException();
        }

        public double[] Backward(double[] input)
        {
            throw new NotImplementedException();
        }

        public void Learn()
        {
            SpatialPooler = Stream.Distinct().ToList();
            var m = TemporalPooler.Count;
            var n = SpatialPooler.Count;

            var transitions = new double[n, n];
            foreach (var (src, dst) in Stream.Take(n - 1).Zip(Stream.Skip(1), Tuple.Create))
            {
                transitions[SpatialPooler.IndexOf(src), SpatialPooler.IndexOf(dst)]++;
            }
            var probabilities = transitions.NormalizeToRaw();
            var distances = probabilities.Add(probabilities.T());
            var cluster = Clustering.Clustering.AggregativeHierarchicalClustering(Enumerable.Range(0, n), (i, j) => distances[i, j], Metrics.GroupAverage);
            var clusterwiseMembers = cluster.Extract(m).Select(c => c.GetMembers().Select(s => s.Value)).ToArray();
            Membership = new int[n, m];
            for (var i = 0; i < n; i++)
            {
                for (var j = 0; j < m; j++)
                {
                    Membership[i, j] = clusterwiseMembers[j].Contains(i) ? 1 : 0;
                }
            }
        }

        public void Predict()
        {
            throw new NotImplementedException();
        }
    }
}