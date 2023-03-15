using BenchmarkDotNet.Running;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BenchmarkATR
{
    public static class Runner
    {
        public static void Main(params string[] Args)
        {
            BenchmarkRunner.Run<DataModelTests>();
            Console.ReadLine();
        }
    }
}
