using System;
using FluentAssertions;
using Moq;
using Xunit;

namespace ObjectPool.Tests
{
    public class PooledObjectInterceptorTests
    {
        [Fact]
        public void ConstructorThrowsNullPool()
        {
            var mockDisposable = new Mock<IDisposable>();
            Action act = () => new PooledObjectInterceptor<IDisposable>(null!, mockDisposable.Object);
            act.Should().ThrowExactly<ArgumentNullException>().Which.ParamName.Should().Be("pool");
        }

        [Fact]
        public void ConstructorThrowsNullActual()
        {
            var pool = new Mock<IObjectPool<IDisposable>>();
            Action act = () => new PooledObjectInterceptor<IDisposable>(pool.Object, null!);
            act.Should().ThrowExactly<ArgumentNullException>().Which.ParamName.Should().Be("actual");
        }
    }
}