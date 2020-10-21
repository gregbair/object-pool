using System;
using FluentAssertions;
using Xunit;

namespace ObjectPool.Tests
{
    public class PooledObjectProxyTests
    {
        [Fact]
        public void ConstructorThrowsNullActual()
        {
            Action act = () => new PooledObjectProxy<ISomeInterface>(null!);
            act.Should().ThrowExactly<ArgumentNullException>().Which.ParamName.Should().Be("actual");
        }

        public interface ISomeInterface : IDisposable
        {
        }
    }
}