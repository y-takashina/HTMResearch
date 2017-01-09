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
    public class ClusterTests
    {
        [TestMethod()]
        public void PrintTest()
        {
            var cluster = Cluster(Cluster(35), Cluster(Cluster(Cluster(Cluster(77), Cluster(Cluster(34), Cluster(22))), Cluster(Cluster(Cluster(69),
                Cluster(Cluster(69), Cluster(33))), Cluster(Cluster(95), Cluster(22)))), Cluster(Cluster(Cluster(8), Cluster(38)), Cluster(72))));
            cluster.Print();
        }
    }
}