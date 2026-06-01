using System.Threading.Channels;
using App.Services;
using Domain.Entities;
using Domain.Interfaces;
using FluentAssertions;
using Handler.Background;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.Unit;

public class UnitTests
{
    [Fact]
    public async Task Processor_Should_Save_Batch_When_100_Items_Reached()
    {
        // Arrange

        var repository =
            new Mock<ITickRepository>();
        var channel =
            Channel.CreateUnbounded<Tick>();
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var dedupLogger= new Mock<ILogger<Deduplicator>>();
        var deduplicator = new Deduplicator(memoryCache,  dedupLogger.Object);
        var logger= new Mock<ILogger<TickProcessor>>();

        var processor =
            new TickProcessor(
                channel,
                repository.Object,
                deduplicator,
                logger.Object);
        
        //Act
        await processor.StartAsync(CancellationToken.None);
        for(int i = 0; i < 110; i++)
        {
            await channel.Writer.WriteAsync(
                new Tick
                {
                    Symbol = $"BTC{i}"
                });
        }
        await Task.Delay(500);

        // Assert

        repository.Verify(
            x => x.SaveBatchAsync(
                It.IsAny<IReadOnlyCollection<Tick>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
    
    [Fact]
    public void Should_Return_True_For_Duplicate()
    {
        // Arrange

        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var logger= new Mock<ILogger<Deduplicator>>();
        var deduplicator =
            new Deduplicator(memoryCache,  logger.Object);

        var tick =
            new Tick
            {
                Source = "Binance",
                Symbol = "BTC",
                Price = 100
            };

        // Act

        var first =
            deduplicator.IsDuplicate(tick);
        var second =
            deduplicator.IsDuplicate(tick);

        // Assert

        first.Should().BeFalse();
        second.Should().BeTrue();
    }
}