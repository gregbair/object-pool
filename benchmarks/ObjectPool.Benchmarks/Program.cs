using BenchmarkDotNet.Running;

namespace ObjectPool.Benchmarks
{
    internal static class Program
    {
        private static void Main()
        {
            var summary = BenchmarkRunner.Run<Benchmarks>();
        }
    }
}
