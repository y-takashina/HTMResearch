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
        public IEnumerable<int> TestStream { get; private set; }

        public LeafNode(IEnumerable<int> trainStream, IEnumerable<int> testStream, int numberTemporalGroup, Func<(double, int), (double, int), double> metrics = null) : base(numberTemporalGroup, metrics)
        {
            Memorize(trainStream.Select(v => new[] {v}));
            TestStream = testStream;
        }

        public override double[] Predict()
        {
            if (!TestStream.Any()) throw new NullReferenceException("Cannot predict anything. TestStream is empty.");
            var input = SpatialPooler.IndexOf<int[]>(new[] {TestStream.First()});
            TestStream = TestStream.Skip(1);
            var coincidence = new double[N];
            coincidence[input] = 1.0;
            return Forward(coincidence);
        }
    }

    public class InternalNode : Node
    {
        private readonly IEnumerable<Node> _childNodes;

        public InternalNode(IEnumerable<Node> childNodes, int numberTemporalGroup, Func<(double, int), (double, int), double> metrics = null) : base(numberTemporalGroup, metrics)
        {
            if (childNodes == null) throw new NullReferenceException("`childNodes` is null.");
            if (childNodes.Contains(null)) throw new NullReferenceException("`childNodes` contains null.");
            _childNodes = childNodes;
        }

        private IEnumerable<int[]> _aggregateChildStreams()
        {
            var childStreams = _childNodes.Select(node => node.Stream.Select(node.Forward).ToArray()).ToArray();
            var rawStream = childStreams.First().Select(_ => new List<int>()).ToList();
            foreach (var childStream in childStreams)
            {
                for (var i = 0; i < childStream.Length; i++)
                {
                    rawStream[i].Add(childStream[i]);
                }
            }
            return rawStream.Select(coincidence => coincidence.ToArray());
        }

        public override void Learn()
        {
            foreach (var childNode in _childNodes) childNode.Learn();
            Memorize(_aggregateChildStreams());
            base.Learn();
        }

        public override double[] Predict()
        {
            var childOutputs = _childNodes.Select(node => node.Predict()).ToArray();
            var coincidence = Enumerable.Repeat(1.0, N).ToArray();
            for (var i = 0; i < N; i++)
            {
                for (var j = 0; j < SpatialPooler[i].Length; j++)
                {
                    coincidence[i] *= childOutputs[j][SpatialPooler[i][j]];
                }
            }
            return Forward(coincidence);
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

        /// <summary>
        /// SpatialPooler における index が格納されている。
        /// </summary>
        public IEnumerable<int> Stream { get; set; }

        public IEnumerable<int> ClusterStream => Stream.Select(Forward);

        public List<int[]> SpatialPooler { get; set; }
        public int[,] Membership { get; set; }
        private readonly Func<(double, int), (double, int), double> _metrics;

        protected Node(int numberTemporalGroup, Func<(double, int), (double, int), double> metrics)
        {
            _metrics = metrics ?? Metrics.GroupAverage;
            NumberTemporalGroup = numberTemporalGroup;
        }

        /// <summary>
        /// 1-of-k な値を入力として受け取ったら、
        /// 1-of-k なクラスタ割り当てを上に出力する。
        /// 主に学習時に使う。
        /// </summary>
        public int Forward(int input)
        {
            for (var i = 0; i < M; i++) if (Membership[input, i] == 1) return i;
            throw new ArgumentOutOfRangeException();
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

        public int[] Backward(int input)
        {
            var coincidence = new int[N];
            for (var i = 0; i < N; i++) coincidence[i] = Membership[i, input];
            return coincidence;
        }

        public double[] Backward(double[] input)
        {
            if (input.Length != M) throw new IndexOutOfRangeException("Feedback input to a node must have the same length as the node's temporal pooler.");
            throw new NotImplementedException();
        }

        public void Memorize(IEnumerable<int[]> rawStream)
        {
            SpatialPooler = new List<int[]>();
            foreach (var value in rawStream)
            {
                var memorized = SpatialPooler.Any(memoizedValue => memoizedValue.SequenceEqual(value));
                if (!memorized) SpatialPooler.Add(value);
            }
            Stream = rawStream.Select(value => SpatialPooler.IndexOf<int[]>(value));
        }

        public virtual void Learn()
        {
            var transitions = new double[N, N];
            var stream = Stream.ToArray();
            for (var i = 0; i < stream.Length - 1; i++)
            {
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

        public abstract double[] Predict();
    }
}