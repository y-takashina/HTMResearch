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
using static PipExtensions.MatrixExtensions;

namespace Detector
{
    public class LeafNode : Node
    {
        public LeafNode(IEnumerable<int> inputStream, int numberTemporalGroup, Func<(double, int), (double, int), double> metrics = null) : base(numberTemporalGroup, metrics)
        {
            Memoize(inputStream.Select(v => new[] {v}));
        }

        public override double[] Predict(int[] input) => Forward(Quantize(input).Cast<double>().ToArray());
    }

    public class InternalNode : Node
    {
        private readonly IEnumerable<Node> _childNodes;

        public InternalNode(IEnumerable<Node> childNodes, int numberTemporalGroup, Func<(double, int), (double, int), double> metrics = null) : base(numberTemporalGroup, metrics)
        {
            _childNodes = childNodes;
        }

        private IEnumerable<int[]> _aggregateChildStreams()
        {
            var childStreams = _childNodes.Select(node => node.Stream.Select(i => node.Forward(OneHot(node.N, i))).ToArray()).ToArray();
            var rawStream = childStreams.First().Select(_ => new List<int>()).ToList();
            foreach (var childStream in childStreams)
            {
                for (var j = 0; j < childStream.Length; j++)
                {
                    rawStream[j].AddRange(childStream[j]);
                }
            }
            return rawStream.Select(value => value.ToArray());
        }

        public override void Learn()
        {
            foreach (var childNode in _childNodes) childNode.Learn();
            Memoize(_aggregateChildStreams());
            base.Learn();
        }

        public override double[] Predict(int[] input)
        {
            throw new NotImplementedException();
        }
    }

    public abstract class Node
    {
        protected int NumberTemporalGroup;

        /// <summary>
        /// alias of NumberTemporalGroup
        /// </summary>
        public int M => NumberTemporalGroup;

        public int N => SpatialPooler?.Count ?? 0;
        public IEnumerable<int> Stream { get; set; }
        public List<int[]> SpatialPooler { get; set; }
        public int[,] Membership { get; set; }
        private readonly Func<(double, int), (double, int), double> _metrics;

        protected Node(int numberTemporalGroup, Func<(double, int), (double, int), double> metrics)
        {
            _metrics = metrics ?? Metrics.GroupAverage;
            NumberTemporalGroup = numberTemporalGroup;
        }

        public int[] Quantize(int[] rawInput)
        {
            if (!SpatialPooler.Any(v => v.SequenceEqual(rawInput))) throw new ArgumentOutOfRangeException("input must have been memoized in SpatialPooer");
            return OneHot(N, SpatialPooler.IndexOf<int[]>(rawInput));
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
            var temporalGroup = Enumerable.Range(0, M).Select(i => input.Select((v, j) => v * Membership[j, i]).Max()).Normalize();
            return temporalGroup;
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

        public void Memoize(IEnumerable<int[]> rawStream)
        {
            SpatialPooler = new List<int[]>();
            foreach (var value in rawStream)
            {
                var memoized = SpatialPooler.Any(memoizedValue => memoizedValue.SequenceEqual(value));
                if (!memoized) SpatialPooler.Add(value);
            }
            Stream = rawStream.Select(value => SpatialPooler.IndexOf<int[]>(value));
        }

        public virtual void Learn()
        {
            var transitions = new double[N, N];
            var stream = Stream.ToArray();
            for (var i = 0; i < stream.Length - 1; i++)
            {
                //if (double.IsNaN(stream[i]) || double.IsNaN(stream[i + 1])) continue;
                transitions[stream[i], stream[i + 1]]++;
            }
            var probabilities = transitions.NormalizeToRaw();
            var distances = probabilities.Add(probabilities.T()).Mul(-1);
            var cluster = AggregativeHierarchicalClustering(Enumerable.Range(0, N), (i, j) => distances[i, j], _metrics);
            var clusterwiseMembers = cluster.Extract(M).Select(c => c.SelectMany()).ToArray();
            Membership = new int[N, M];
            for (var i = 0; i < N; i++)
            {
                for (var j = 0; j < M; j++)
                {
                    Membership[i, j] = clusterwiseMembers[j].Contains(i) ? 1 : 0;
                }
            }
        }

        public abstract double[] Predict(int[] input);
    }
}