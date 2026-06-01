using AutoMapper;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Mapping;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Tests.Integration;

public class DbTest : BaseIntegrationTest
{
    [Fact]
    public async Task SaveBatchAsync_Should_Save_Ticks()
    {
        // Arrange
        var mapperConfig = new MapperConfiguration(e => e.AddProfile(typeof(TickProfile)), new LoggerFactory());
        var mapper = new Mapper(mapperConfig);
        var repository =
            new TickRepository(context, mapper);

        var ticks =
            new List<Tick>
            {
                new()
                {
                    Source = "Binance",
                    Symbol = "BTCUSDT",
                    Price = 100,
                    Volume = 1,
                    Timestamp = DateTime.UtcNow
                }
            };

        // Act

        await repository.SaveBatchAsync(
            ticks,
            CancellationToken.None);

        // Assert
        var saved =
            await context.Ticks.ToListAsync();
        saved.Should().HaveCount(1);
        saved[0].Symbol.Should().Be("BTCUSDT");
    }
}