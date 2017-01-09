using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Detector
{
    public class DataReader : IDisposable
    {
        private readonly string _path;

        public DataReader(string path)
        {
            _path = path;
        }

        public double[] GetLines()
        {
            var list = new List<double>();
            using (var sr = new StreamReader(_path))
            {
                var head = sr.ReadLine().Split(',').ToList();
                var valueColumnIndex = head.IndexOf("value");
                if (valueColumnIndex == -1) throw new FormatException("\"value\" column does not exist.");
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine().Split(',');
                    var value = double.Parse(line[valueColumnIndex]);
                    list.Add(value);
                }
            }
            return list.ToArray();
        }

        public void Dispose()
        {
        }
    }
}