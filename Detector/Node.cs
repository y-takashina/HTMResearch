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
    public abstract class Node<T>
    {
        public IEnumerable<T> Stream { get; set; }
        public List<T> SpatialPooler { get; set; }
        public List<T> TemporalPooler { get; set; }
        public IEnumerable<Node<T>> ChildNodes { get; set; }
        public int[,] Membership { get; set; }

        public abstract int Forward(T input);
        public abstract T Backward(int input);
        public abstract void Learn();
        public abstract void Predict();
    }

    public class LeafNode<T> : Node<T>
    {
        public LeafNode(IEnumerable<T> inputStream, int numberTemporalGroup)
        {
            Stream = inputStream;
            TemporalPooler = new T[numberTemporalGroup].ToList();
        }

        public override int Forward(T input)
        {
            throw new NotImplementedException();
        }

        public override T Backward(int input)
        {
            throw new NotImplementedException();
        }

        public override void Learn()
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

        public override void Predict() {}
    }
}