using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Detector
{
    public static class Extensions
    {
        public delegate string Indexer<in T>(T obj);

        public static string Concatenate<T>(this IEnumerable<T> collection, char separator = ',')
        {
            var sb = new StringBuilder();
            foreach (var t in collection) sb.Append(t).Append(separator);
            return sb.Remove(sb.Length - 1, 1).ToString();
        }

    }
}