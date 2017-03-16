using Microsoft.VisualStudio.TestTools.UnitTesting;
using Detector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PipExtensions;

namespace Detector.Tests
{
    [TestClass()]
    public class NodeTests
    {
        private readonly Node _node;

        public NodeTests()
        {
            _node = new Node(new List<int> {3, 4, 5, 4, 3, 4, 5, 8, 0, 0}, 2);
            _node.Learn();
        }

        [TestMethod()]
        public void ForwardTest() {}

        [TestMethod()]
        public void ForwardTest1() {}

        [TestMethod()]
        public void BackwardTest() {}

        [TestMethod()]
        public void BackwardTest1() {}

        [TestMethod()]
        public void LearnTest()
        {
            _node.SpatialPooler.ForEach(Console.WriteLine);
            MatViz.MatViz.SaveMatrixImage(_node.Membership.Cast(v => (double) v), "membership_test");
        }

        [TestMethod()]
        public void PredictTest() {}
    }
}