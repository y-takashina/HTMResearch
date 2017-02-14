using System;
using System.Collections.Generic;
using System.Linq;
using static MatViz.MatViz;
using static PipExtensions.PipExtensions;

namespace Detector
{
    public class RelationAnalyzer
    {
        private double[,] relationships;
        private readonly List<int[]> _seriesSet = new List<int[]>();

        public void AddSeries(int[] series)
        {
            if (!_seriesSet.Any()) _seriesSet.Add(series);
            else
            {
                if (_seriesSet.First().Length == series.Length)
                {
                    _seriesSet.Add(series);
                }
                else
                {
                    Console.WriteLine("series size mismatch");
                }
            }
        }

        public void CalcMutualInformation()
        {
            var n = _seriesSet.Count;
            Console.WriteLine(n);
            relationships = new double[n, n];
            for (var i = 0; i < n; i++)
            {
                for (var j = 0; j < n; j++)
                {
                    relationships[i, j] = MutualInformation(_seriesSet[i], _seriesSet[j]);
                }
            }
        }

        public void SaveRelationImage()
        {
            if (relationships != null) SaveMatrixImage(relationships, "relations");
        }
    }
}