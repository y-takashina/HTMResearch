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
        private readonly int _numberTemporalGroup;

        /// <summary>
        /// alias of _numberTemporalGroup
        /// </summary>
        private int M => _numberTemporalGroup;

        private int N => SpatialPooler?.Count ?? 0;

        public IEnumerable<int> Stream { get; set; }
        public List<int> SpatialPooler { get; set; }
        public IEnumerable<Node> ChildNodes { get; set; }
        public int[,] Membership { get; set; }

        public Node(IEnumerable<int> inputStream, int numberTemporalGroup)
        {
            Stream = inputStream;
            _numberTemporalGroup = numberTemporalGroup;
        }

        public Node(IEnumerable<Node> childNodes, int numberTemporalGroup)
        {
            ChildNodes = childNodes;
            _numberTemporalGroup = numberTemporalGroup;
        }

        /// <summary>
        /// 1-of-k な値を入力として受け取ったら、
        /// 1-of-k なクラスタ割り当てを上に出力する。
        /// 主に学習時に使う。
        /// </summary>
        public int[] Forward(int[] input)
        {
            if (input.Length != N) throw new IndexOutOfRangeException("Feedforward input to a node must have the same length as the node's spatial pooler.");
            return Membership.T().Mul(input);
        }

        /// <summary>
        /// soft な値を入力として受け取ったら、
        /// soft なクラスタ割り当てを上に出力する。
        /// 主に推論時に使う。
        /// </summary>
        public double[] Forward(double[] input)
        {
            if (input.Length != N) throw new IndexOutOfRangeException("Feedforward input to a node must have the same length as the node's spatial pooler.");
            var temporalGroup = Enumerable.Range(0, M).Select(i => Enumerable.Range(0, N).Select(j => input[j] * Membership[j, i]).Max());
            return temporalGroup.ToArray();
        }

        public int[] Backward(int[] input)
        {
            if (input.Length != M) throw new IndexOutOfRangeException("Feedback input to a node must have the same length as the node's temporal pooler.");
            throw new NotImplementedException();
        }

        public double[] Backward(double[] input)
        {
            if (input.Length != M) throw new IndexOutOfRangeException("Feedback input to a node must have the same length as the node's temporal pooler.");
            throw new NotImplementedException();
        }

        public void Learn()
        {
            SpatialPooler = Stream.Distinct().ToList();

            var transitions = new double[N, N];
            foreach (var (src, dst) in Stream.Take(Stream.Count() - 1).Zip(Stream.Skip(1), Tuple.Create))
            {
                transitions[SpatialPooler.IndexOf(src), SpatialPooler.IndexOf(dst)]++;
            }
            var probabilities = transitions.NormalizeToRaw();
            var distances = probabilities.Add(probabilities.T());
            var cluster = AggregativeHierarchicalClustering(Enumerable.Range(0, N), (i, j) => distances[i, j], Metrics.GroupAverage);
            var clusterwiseMembers = cluster.Extract(M).Select(c => c.SelectMany()).ToArray();
            cluster.Print();
            Membership = new int[N, M];
            for (var i = 0; i < N; i++)
            {
                for (var j = 0; j < M; j++)
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