using System;
using FluentAssertions;
using Moq;
using Xunit;

namespace ObjectPool.Tests
{
    public class PooledObjectProxyTests
    {
        [Fact]
        public void ConstructorThrowsNullInterceptor()
        {
            Action act = () => new PooledObjectProxy<IFoo>(null!, new Mock<IFoo>().Object);
            act.Should().ThrowExactly<ArgumentNullException>().Which.ParamName.Should().Be("interceptor");
        }

        [Fact]
        public void ConstructorThrowsNullObject()
        {
            Action act = () =>
                new PooledObjectProxy<IFoo>(
                    new PooledObjectInterceptor<IFoo>(new Mock<IObjectPool<IFoo>>().Object, new Mock<IFoo>().Object),
                    null!);
            act.Should().ThrowExactly<ArgumentNullException>().Which.ParamName.Should().Be("obj");
        }

        public interface IFoo : IDisposable
        {

        }
    }
}