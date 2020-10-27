using BenchmarkDotNet.Running;

namespace Lagoon.Benchmarks
{
    internal static class Program
    {
        private static void Main()
        {
            var summary = BenchmarkRunner.Run<Benchmarks>();
        }
    }
}
