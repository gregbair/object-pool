using System;
using System.Threading.Tasks;

namespace ObjectPool.Benchmarks
{
    public interface IFoo : IDisposable
    {
        Guid Id { get; }
        Task Initialize();
    }
}