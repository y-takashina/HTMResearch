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
using static Clustering.Clustering;

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

        public Node(IEnumerable<Node> childNodes, int numberTemporalGroup)
        {
            ChildNodes = childNodes;
            TemporalPooler = new int[numberTemporalGroup].ToList();
        }

        /// <summary>
        /// 1-of-k な値を入力として受け取ったら、
        /// 1-of-k なクラスタ割り当てを上に出力する。
        /// 主に学習時に使う。
        /// </summary>
        public int[] Forward(int[] input)
        {
            if (input.Length != SpatialPooler.Count) throw new IndexOutOfRangeException("Feedforward input to a node must have the same length as its spatial pooler.");
            return Membership.T().Mul(input);
        }

        /// <summary>
        /// soft な値を入力として受け取ったら、
        /// soft なクラスタ割り当てを上に出力する。
        /// 主に推論時に使う。
        /// </summary>
        public List<double> Forward(List<double> input)
        {
            if (input.Count != SpatialPooler.Count) throw new IndexOutOfRangeException("Feedforward input to a node must have the same length as its spatial pooler.");
            throw new NotImplementedException();
        }

        public List<int> Backward(List<int> input)
        {
            if (input.Count != TemporalPooler.Count) throw new IndexOutOfRangeException("Feedback input to a node must have the same length as its spatial pooler.");
            throw new NotImplementedException();
        }

        public List<double> Backward(List<double> input)
        {
            if (input.Count != TemporalPooler.Count) throw new IndexOutOfRangeException("Feedback input to a node must have the same length as its spatial pooler.");
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
            var cluster = AggregativeHierarchicalClustering(Enumerable.Range(0, n), (i, j) => distances[i, j], Metrics.GroupAverage);
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