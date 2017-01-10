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
        public void DistanceFromSingleToClusterTest()
        {
            var single = Cluster(1);
            var cluster = Cluster(Cluster(Cluster(60), Cluster(Cluster(68), Cluster(31))), Cluster(Cluster(99), Cluster(19)));
            var d = DistanceFromSingleToCluster(single, cluster, (x, y) => Math.Abs(x - y));
            Assert.AreEqual(d, 18);
        }

        [TestMethod()]
        public void DistanceFromClusterToClusterTest()
        {
            var cluster = Cluster(Cluster(Cluster(60), Cluster(Cluster(68), Cluster(31))), Cluster(Cluster(99), Cluster(19)));
            var largeCluster = Cluster(Cluster(35), Cluster(Cluster(Cluster(Cluster(77), Cluster(Cluster(34), Cluster(22))), Cluster(Cluster(Cluster(69),
                Cluster(Cluster(69), Cluster(33))), Cluster(Cluster(95), Cluster(22)))), Cluster(Cluster(Cluster(8), Cluster(38)), Cluster(72))));
            var d = DistanceFromClusterToCluster(largeCluster, cluster, (x, y) => Math.Abs(x - y));
            Assert.AreEqual(d, 1);
        }

        [TestMethod()]
        public void SingleLinkageTest()
        {
        }

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
        }
    }
}