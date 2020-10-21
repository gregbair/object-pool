using System;
using Castle.DynamicProxy;
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

        [Fact]
        public void InterceptThrowsNullInvocation()
        {
            var pool = new Mock<IObjectPool<IFoo>>();
            var sut = new PooledObjectInterceptor<IFoo>(pool.Object, new Mock<IFoo>().Object);
            Action act = () => sut.Intercept(null!);
            act.Should().ThrowExactly<ArgumentNullException>().Which.ParamName.Should().Be("invocation");
        }

        [Fact]
        public void InterceptRedirectsToObject()
        {
            var mockFoo = new Mock<IFoo>();
            var pool = new Mock<IObjectPool<IFoo>>();

            var generator = new ProxyGenerator();
            var obj = generator.CreateInterfaceProxyWithTarget(mockFoo.Object,
                new PooledObjectInterceptor<IFoo>(pool.Object, mockFoo.Object));

            obj.DoThing("bar");

            mockFoo.Verify(x=>x.DoThing("bar"), Times.Once);
        }

        [Fact]
        public void InterceptDisposeRedirectsToPool()
        {
            var mockFoo = new Mock<IFoo>();
            var pool = new Mock<IObjectPool<IFoo>>();

            var generator = new ProxyGenerator();
            var obj = generator.CreateInterfaceProxyWithTarget(mockFoo.Object,
                new PooledObjectInterceptor<IFoo>(pool.Object, mockFoo.Object));

            obj.Dispose();

            pool.Verify(x => x.ReturnObject(mockFoo.Object), Times.Once);
        }

        public interface IFoo : IDisposable
        {
            void DoThing(string xyz);
        }
    }
}