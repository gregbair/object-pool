using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Moq;
using Optional.Unsafe;
using Xunit;

namespace ObjectPool.Tests
{
    public class DefaultObjectPoolTests
    {
        [Fact]
        public void ConstructorThrowsNullFactory()
        {
            Action act = () => new DefaultObjectPool<IDisposable>(null!);
            act.Should().ThrowExactly<ArgumentNullException>().Which.ParamName.Should().Be("factory");
        }

        [Fact]
        public async Task ActivateTrueActivates()
        {
            var mockFoo = new Mock<IFoo>();
            var factory = new Mock<IObjectPoolFactory<IFoo>>();
            factory.Setup(x => x.Create()).Returns(mockFoo.Object);

            var sut = new DefaultObjectPool<IFoo>(factory.Object, new ObjectPoolOptions(), _ => true);

            await sut.GetObjectAsync();

            factory.Verify(x => x.ActivateAsync(mockFoo.Object), Times.Once);
        }

        [Fact]
        public async Task ActivateFalseDoesNotActivate()
        {
            var mockFoo = new Mock<IFoo>();
            var factory = new Mock<IObjectPoolFactory<IFoo>>();
            factory.Setup(x => x.Create()).Returns(mockFoo.Object);

            var sut = new DefaultObjectPool<IFoo>(factory.Object, new ObjectPoolOptions(), _ => false);

            await sut.GetObjectAsync();

            factory.Verify(x => x.ActivateAsync(mockFoo.Object), Times.Never);
        }

        [Fact]
        public async Task CancelledTokenReturnsNone()
        {
            var mockFoo = new Mock<IFoo>();
            var factory = new Mock<IObjectPoolFactory<IFoo>>();
            factory.Setup(x => x.Create()).Returns(mockFoo.Object);

            var sut = new DefaultObjectPool<IFoo>(factory.Object);

            var source = new CancellationTokenSource();
            source.Cancel();
            var result = await sut.GetObjectAsync(source.Token);
            result.HasValue.Should().BeFalse();
        }

        [Fact]
        public void ReturnThrowsNullProxy()
        {
            var sut = new DefaultObjectPool<IFoo>(new Mock<IObjectPoolFactory<IFoo>>().Object);
            Action act = () => sut.ReturnObject(null!);
            act.Should().ThrowExactly<ArgumentNullException>().Which.ParamName.Should().Be("proxyObj");
        }

        [Fact]
        public async Task OneAvailableGetsOneFromPool()
        {
            var mockFactory = new Mock<IObjectPoolFactory<IFoo>>();
            var mockFoo = new Mock<IFoo>();
            mockFactory.Setup(x => x.Create()).Returns(mockFoo.Object);

            var sut = new DefaultObjectPool<IFoo>(mockFactory.Object);

            sut.ActiveCount.Should().Be(0);
            sut.AvailableCount.Should().Be(0);
            var first = (await sut.GetObjectAsync()).ValueOrFailure();
            sut.ActiveCount.Should().Be(1);
            sut.AvailableCount.Should().Be(0);
            first.Dispose();
            sut.AvailableCount.Should().Be(1);
            sut.ActiveCount.Should().Be(0);
            await sut.GetObjectAsync();
            sut.AvailableCount.Should().Be(0);
            sut.ActiveCount.Should().Be(1);
        }

        public interface IFoo : IDisposable
        {

        }

    }
}