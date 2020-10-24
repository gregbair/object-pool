using System;
using FluentAssertions;
using Moq;
using Xunit;

namespace ObjectPool.Tests
{
    public class PooledObjectProxyTests
    {
        [Fact]
        public void ConstructorThrowsNullActual()
        {
            Action act = () =>
                new PooledObjectWrapper<ISomeInterface>(new Mock<IObjectPool<ISomeInterface>>().Object, null!);
            //act.Should().ThrowExactly<ArgumentNullException>().Which.ParamName.Should().Be("actual");
        }

        public interface ISomeInterface : IDisposable
        {
        }
    }
}