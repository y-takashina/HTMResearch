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
        private readonly Node _tree;

        public NodeTests()
        {
            _node = new LeafNode(new[] {3, 4, 5, 4, 3, 4, 5, 8, 0, 0}, null, 2);
            _node.Learn();
            //                   0  0  0  0  0  0  0  0  0  0  1  1  1  0
            var stream1 = new[] {0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 2, 2, 2, 0};
            //                   0  0  0  0  1  1  1  1  1  1  0  0  0  0
            var stream2 = new[] {0, 1, 0, 1, 2, 3, 2, 3, 2, 3, 0, 1, 0, 0};
            //                   0  0  0  0  0  1  0  1  0  1  1  1  1  0
            var stream3 = new[] {0, 0, 0, 0, 0, 1, 0, 1, 0, 1, 1, 1, 1, 0};
            //                   000, 010, 011, 101
            //                   0  0  0  0  1  2  1  2  1  2  3  3  3  0
            _tree = new InternalNode(new[]
            {
                new LeafNode(stream1, stream1, 2),
                new LeafNode(stream2, stream2, 2),
                new LeafNode(stream3, stream3, 2),
            }, 2);
            _tree.Learn();
        }

        [TestMethod()]
        public void ForwardHardTest()
        {
            var inputs = new[]
            {
                new[] {1, 0, 0, 0, 0},
                new[] {0, 1, 0, 0, 0},
                new[] {0, 0, 1, 0, 0},
                new[] {0, 0, 0, 1, 0},
                new[] {0, 0, 0, 0, 1},
            };
            var answers = new[,] {{0, 1}, {0, 1}, {0, 1}, {1, 0}, {1, 0}};
            for (var i = 0; i < 5; i++)
            {
                var output = _node.Forward(inputs[i]);
                for (var j = 0; j < 2; j++)
                {
                    Assert.AreEqual(answers[i, j], output[j]);
                }
            }
        }

        [TestMethod()]
        public void ForwardSoftTest()
        {
            var inputs = new[]
            {
                new[] {0.2, 0.7, 0.0, 0.0, 0.1},
                new[] {0.0, 0.0, 0.2, 0.6, 0.2},
                new[] {0.2, 0.2, 0.2, 0.2, 0.2},
            };
            var answers = new[,] {{0.125, 0.875}, {0.75, 0.25}, {0.5, 0.5}};
            for (var i = 0; i < 3; i++)
            {
                var output = _node.Forward(inputs[i]);
                for (var j = 0; j < 2; j++)
                {
                    Assert.AreEqual(answers[i, j], output[j], 1e-6);
                }
            }
        }

        [TestMethod()]
        public void BackwardHardTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void BackwardSoftTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void LearnTest()
        {
            var answers = new[,] {{0, 1}, {0, 1}, {0, 1}, {1, 0}, {1, 0}};
            for (var i = 0; i < 5; i++)
            {
                for (var j = 0; j < 2; j++)
                {
                    Assert.AreEqual(answers[i, j], _node.Membership[i, j]);
                }
            }
        }

        [TestMethod()]
        public void LearnInternalNodeTest()
        {
            var answers = new[,] {{0, 1}, {1, 0}, {1, 0}, {0, 1}};
            for (var i = 0; i < 4; i++)
            {
                for (var j = 0; j < 2; j++)
                {
                    Assert.AreEqual(answers[i, j], _tree.Membership[i, j]);
                }
            }
        }

        [TestMethod()]
        public void PredictTest()
        {
            Console.WriteLine(_tree.Predict());
            Console.WriteLine(_tree.Predict());
            Console.WriteLine(_tree.Predict());
            Console.WriteLine(_tree.Predict());
            Console.WriteLine(_tree.Predict());
            Console.WriteLine(_tree.Predict());
            Console.WriteLine(_tree.Predict());
            Console.WriteLine(_tree.Predict());
            Console.WriteLine(_tree.Predict());
            Console.WriteLine(_tree.Predict());
            Console.WriteLine(_tree.Predict());
            Console.WriteLine(_tree.Predict());
            Console.WriteLine(_tree.Predict());
            Console.WriteLine(_tree.Predict());
        }
    }
}