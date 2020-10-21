using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
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
            act.Should().ThrowExactly<ArgumentNullException>().Which.ParamName.Should().Be("wrapper");
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

        [Fact]
        public async Task DisposeThrowsActiveObjects()
        {
            var mockFactory = new Mock<IObjectPoolFactory<IFoo>>();
            var mockFoo = new Mock<IFoo>();
            mockFactory.Setup(x => x.Create()).Returns(mockFoo.Object);

            var sut = new DefaultObjectPool<IFoo>(mockFactory.Object);

            await sut.GetObjectAsync();

            Action act = () => sut.Dispose();

            act.Should().ThrowExactly<InvalidOperationException>().Which.Message.Should()
                .ContainEquivalentOf("1 active object(s)");
        }

        [Fact]
        public async Task DisposeDisposesObjects()
        {
            var mockFactory = new Mock<IObjectPoolFactory<IFoo>>();
            var mockFoo = new Mock<IFoo>();
            mockFactory.Setup(x => x.Create()).Returns(mockFoo.Object);

            var sut = new DefaultObjectPool<IFoo>(mockFactory.Object, objectPassivator: x => true);

            var proxy = (await sut.GetObjectAsync()).ValueOrFailure().As<IFoo>();
            proxy.Dispose();

            sut.Dispose();

            mockFoo.Verify(x => x.Dispose(), Times.Once);
        }

        [Fact]
        public async Task DisposePassivatorFalseDoesNotDispose()
        {
            var mockFactory = new Mock<IObjectPoolFactory<IFoo>>();
            var mockFoo = new Mock<IFoo>();
            mockFactory.Setup(x => x.Create()).Returns(mockFoo.Object);

            var sut = new DefaultObjectPool<IFoo>(mockFactory.Object, objectPassivator: x => false);

            var proxy = (await sut.GetObjectAsync()).ValueOrFailure().As<IFoo>();
            proxy.Dispose();

            sut.Dispose();

            mockFoo.Verify(x => x.Dispose(), Times.Never);
        }

        [Fact]
        public async Task GetObjectThrowsWhenFactoryThrows()
        {
            var mockFactory = new Mock<IObjectPoolFactory<IFoo>>();
            mockFactory.Setup(x => x.Create()).Throws<InvalidOperationException>();

            var sut = new DefaultObjectPool<IFoo>(mockFactory.Object);

            Func<Task> act = async () => await sut.GetObjectAsync();

            (await act.Should().ThrowExactlyAsync<PoolException>())
                .WithInnerExceptionExactly<InvalidOperationException>();
        }

        [Fact]
        public async Task TimeoutExpires()
        {
            var mockFactory = new Mock<IObjectPoolFactory<IFoo>>();
            var mockObj = new Mock<IFoo>();
            mockFactory.Setup(x => x.Create()).Returns(mockObj.Object);

            var options = new ObjectPoolOptions
            {
                MaxObjects = 1,
                AcquisitionTimeout = TimeSpan.FromMilliseconds(500),
            };

            var sut = new DefaultObjectPool<IFoo>(mockFactory.Object, options);

            using var obj = (await sut.GetObjectAsync()).ValueOrFailure();

            Func<Task> act = async () => await sut.GetObjectAsync();

            act.Should().ThrowExactly<PoolExhaustedException>();
        }
    }

    public interface IFoo : IDisposable
    {

    }

}
