using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Detector
{
    public abstract class Node<T>
    {
        public IEnumerable<T> SpatialPooler { get; set; }
        public IEnumerable<T> TemporalPooler { get; set; }
        public IEnumerable<Node<T>> ChildNodes { get; set; }
        public int[,] Membership { get; set; }

        public abstract void Forward();
        public abstract void Backward();
        public abstract void Learn();
        public abstract void Predict();
    }

    public class LeafNode<T> : Node<T>
    {
        public LeafNode(IEnumerable<T> inputStream)
        {
            ChildNodes = new Node<T>[] {};
            SpatialPooler = inputStream.Distinct();
        }

        public override void Forward() {}
        public override void Backward() {}
        public override void Learn() {}
        public override void Predict() {}
    }
}