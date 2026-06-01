using System.Threading.Channels;
using App.Services;
using AutoMapper;
using Domain.Entities;
using FluentAssertions;
using Handler.Background;
using Infrastructure.Mapping;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Tests.Utility;

namespace Tests.Integration;

public class PipelineTest:BaseIntegrationTest
{
    [Fact]
    public async Task Pipeline_Should_Process_3000_Ticks()
    {
        // Arrange
        var mapperConfig = new MapperConfiguration(e => e.AddProfile(typeof(TickProfile)), new LoggerFactory());
        var mapper = new Mapper(mapperConfig);
        var repository =
            new TickRepository(context, mapper);

        var channel =
            Channel.CreateUnbounded<Tick>();

        var cache =
            new MemoryCache(
                new MemoryCacheOptions());

        var deduplicator =
            new Deduplicator(
                cache,
                Mock.Of<ILogger<Deduplicator>>());

        var processor =
            new TickProcessor(
                channel,
                repository,
                deduplicator,
                Mock.Of<ILogger<TickProcessor>>());

        await processor.StartAsync(
            CancellationToken.None);

        // Act
        var producer1 =
            TickProducer.ProduceTicks(
                channel.Writer,
                "ClientX",
                1000);

        var producer2 =
            TickProducer.ProduceTicks(
                channel.Writer,
                "ClientY",
                1000);

        var producer3 =
            TickProducer.ProduceTicks(
                channel.Writer,
                "ClientZ",
                1000);

        await Task.WhenAll(
            producer1,
            producer2,
            producer3);

        await Task.Delay(3000);

        // Assert

        var count =
            await context.Ticks.CountAsync();

        count.Should().Be(3000);
    }
}