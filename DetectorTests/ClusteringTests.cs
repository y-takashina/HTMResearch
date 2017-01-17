using Microsoft.VisualStudio.TestTools.UnitTesting;
using Detector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Detector.Clustering;

namespace Detector.Tests
{
    [TestClass()]
    public class ClusteringTests
    {
        [TestMethod()]
        public void ExtractTest()
        {
            var clusters = Cluster(Cluster(Cluster(60), Cluster(Cluster(68), Cluster(31))), Cluster(Cluster(99), Cluster(19))).Extract(2);
            var c1 = Cluster(Cluster(60), Cluster(Cluster(68), Cluster(31)));
            var c2 = Cluster(Cluster(99), Cluster(19));
            Assert.AreEqual(clusters[0].ToString(), c1.ToString());
            Assert.AreEqual(clusters[1].ToString(), c2.ToString());
        }

        [TestMethod()]
        public void GetMembersTest()
        {
            // 目視する必要がある。
            var cluster = Cluster(Cluster(Cluster(60), Cluster(Cluster(68), Cluster(31))), Cluster(Cluster(99), Cluster(19)));
            var members = cluster.GetMembers();
            members.ForEach(Console.WriteLine);
        }

        [TestMethod()]
        public void ClusterTest()
        {
        }

        [TestMethod()]
        public void DistanceFromSingleToClusterTest()
        {
        }

        [TestMethod()]
        public void DistanceFromClusterToClusterTest()
        {
        }

        [TestMethod()]
        public void ShortestDistanceFromSingleToClusterTest()
        {
            var single = Cluster(1);
            var cluster = Cluster(Cluster(Cluster(60), Cluster(Cluster(68), Cluster(31))), Cluster(Cluster(99), Cluster(19)));
            var largeCluster = Cluster(Cluster(35), Cluster(Cluster(Cluster(Cluster(77), Cluster(Cluster(34), Cluster(22))), Cluster(Cluster(Cluster(69),
                Cluster(Cluster(69), Cluster(33))), Cluster(Cluster(95), Cluster(22)))), Cluster(Cluster(Cluster(8), Cluster(38)), Cluster(72))));
            var d = ShortestDistanceFromSingleToCluster(single, cluster, (x, y) => Math.Abs(x - y));
            Assert.AreEqual(d, 18);
            d = ShortestDistanceFromSingleToCluster(single, largeCluster, (x, y) => Math.Abs(x - y));
            Assert.AreEqual(d, 7);
        }

        [TestMethod()]
        public void ShortestDistanceFromClusterToClusterTest()
        {
            var cluster = Cluster(Cluster(Cluster(60), Cluster(Cluster(68), Cluster(31))), Cluster(Cluster(99), Cluster(19)));
            var largeCluster = Cluster(Cluster(35), Cluster(Cluster(Cluster(Cluster(77), Cluster(Cluster(34), Cluster(22))), Cluster(Cluster(Cluster(69),
                Cluster(Cluster(69), Cluster(33))), Cluster(Cluster(95), Cluster(22)))), Cluster(Cluster(Cluster(8), Cluster(38)), Cluster(72))));
            var d = ShortestDistanceFromClusterToCluster(cluster, largeCluster, (x, y) => Math.Abs(x - y));
            Assert.AreEqual(d, 1);
            d = ShortestDistanceFromClusterToCluster(largeCluster, cluster, (x, y) => Math.Abs(x - y));
            Assert.AreEqual(d, 1);
        }

        [TestMethod()]
        public void LongestDistanceFromSingleToClusterTest()
        {
            var single = Cluster(1);
            var cluster = Cluster(Cluster(Cluster(60), Cluster(Cluster(68), Cluster(31))), Cluster(Cluster(99), Cluster(19)));
            var largeCluster = Cluster(Cluster(35), Cluster(Cluster(Cluster(Cluster(77), Cluster(Cluster(34), Cluster(22))), Cluster(Cluster(Cluster(69),
                Cluster(Cluster(69), Cluster(33))), Cluster(Cluster(95), Cluster(22)))), Cluster(Cluster(Cluster(8), Cluster(38)), Cluster(72))));
            var d = LongestDistanceFromSingleToCluster(single, cluster, (x, y) => Math.Abs(x - y));
            Assert.AreEqual(d, 98);
            d = LongestDistanceFromSingleToCluster(single, largeCluster, (x, y) => Math.Abs(x - y));
            Assert.AreEqual(d, 94);
        }

        [TestMethod()]
        public void LongestDistanceFromClusterToClusterTest()
        {
            var single = Cluster(1);
            var cluster = Cluster(Cluster(Cluster(60), Cluster(Cluster(68), Cluster(31))), Cluster(Cluster(99), Cluster(19)));
            var largeCluster = Cluster(Cluster(35), Cluster(Cluster(Cluster(Cluster(77), Cluster(Cluster(34), Cluster(22))), Cluster(Cluster(Cluster(69),
                Cluster(Cluster(69), Cluster(33))), Cluster(Cluster(95), Cluster(22)))), Cluster(Cluster(Cluster(8), Cluster(38)), Cluster(72))));
            var d1 = LongestDistanceFromClusterToCluster(single, cluster, (x, y) => Math.Abs(x - y));
            Assert.AreEqual(d1, 98);
            var d2 = LongestDistanceFromClusterToCluster(single, largeCluster, (x, y) => Math.Abs(x - y));
            Assert.AreEqual(d2, 94);
            var d3 = LongestDistanceFromClusterToCluster(largeCluster, cluster, (x, y) => Math.Abs(x - y));
            Assert.AreEqual(d3, 91);
            var d4 = LongestDistanceFromClusterToCluster(cluster, largeCluster, (x, y) => Math.Abs(x - y));
            Assert.AreEqual(d4, 91);
        }

        [TestMethod()]
        public void GroupAverageDistanceFromSingleToClusterTest()
        {
            var single = Cluster(1);
            var cluster = Cluster(Cluster(Cluster(60), Cluster(Cluster(68), Cluster(31))), Cluster(Cluster(99), Cluster(19)));
            var largeCluster = Cluster(Cluster(35), Cluster(Cluster(Cluster(Cluster(77), Cluster(Cluster(34), Cluster(22))), Cluster(Cluster(Cluster(69),
                Cluster(Cluster(69), Cluster(33))), Cluster(Cluster(95), Cluster(22)))), Cluster(Cluster(Cluster(8), Cluster(38)), Cluster(72))));
            var d1 = GroupAverageDistanceFromSingleToCluster(single, cluster, (x, y) => Math.Abs(x - y));
            Assert.AreEqual(d1, 54.4, 1e-6);
            var d2 = GroupAverageDistanceFromSingleToCluster(single, largeCluster, (x, y) => Math.Abs(x - y));
            Assert.AreEqual(d2, 46.8333333, 1e-6);
        }

        [TestMethod()]
        public void GroupAverageDistanceFromClusterToClusterTest()
        {
        }

        [TestMethod()]
        public void AggregativeHierarchicalClusteringTest()
        {
        }
    }
}