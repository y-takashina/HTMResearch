using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        public static int IndexOf<T>(this IEnumerable<T> source, T value) where T : IEnumerable<int>
        {
            var array = source.ToArray();
            for (var i = 0; i < array.Length; i++) if (array[i].SequenceEqual(value)) return i;
            return -1;
        }

        public static IEnumerable<T> Distinct<T>(this IEnumerable<T> source, Func<T, T, bool> comparer)
        {
            return source.Distinct(new DelegateComparer<T>(comparer));
        }

        public static IEnumerable<T> Distinct<T>(this IEnumerable<T> source, Func<T, T, bool> comparer, Func<T, int> hashMethod)
        {
            return source.Distinct(new DelegateComparer<T>(comparer, hashMethod));
        }

        public class DelegateComparer<T> : IEqualityComparer<T>
        {
            private readonly Func<T, T, bool> _equals;
            private readonly Func<T, int> _getHashCode;

            public DelegateComparer(Func<T, T, bool> equals)
            {
                _equals = equals;
            }

            public DelegateComparer(Func<T, T, bool> equals, Func<T, int> getHashCode)
            {
                _equals = equals;
                _getHashCode = getHashCode;
            }

            public bool Equals(T a, T b)
            {
                return _equals(a, b);
            }

            public int GetHashCode(T a)
            {
                return _getHashCode?.Invoke(a) ?? a.GetHashCode();
            }
        }
    }
}