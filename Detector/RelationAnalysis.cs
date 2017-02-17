using System;
using System.Collections.Generic;
using System.Linq;
using static MatViz.MatViz;
using static PipExtensions.PipExtensions;

namespace Detector
{
    public static class RelationAnalysis
    {
        public static double[,] CalcMutualInformationMatrix(params int[][] series)
        {
            var n = series.Length;
            var matrix = new double[n, n];
            for (var i = 0; i < n; i++)
            {
                for (var j = 0; j < n; j++)
                {
                    matrix[i, j] = MutualInformation(series[i], series[j]);
                }
            }
            return matrix;
        }
    }
}