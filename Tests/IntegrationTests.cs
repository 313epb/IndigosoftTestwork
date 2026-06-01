using AutoMapper;
using Domain.Entities;
using FluentAssertions;
using Infrastructure;
using Infrastructure.Mapping;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Testcontainers.PostgreSql;

namespace Tests;

public class IntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres =
        new PostgreSqlBuilder()
            .WithDatabase("testdb")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

    private AppDbContext _db = null!;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        var options =
            new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql(_postgres.GetConnectionString())
                .Options;

        _db = new AppDbContext(options);

        await _db.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
    }

    [Fact]
    public async Task SaveBatchAsync_Should_Save_Ticks()
    {
        // Arrange
        var mapperConfig = new MapperConfiguration(e => e.AddProfile(typeof(TickProfile)), new LoggerFactory());
        var mapper = new Mapper(mapperConfig);
        var repository =
            new TickRepository(_db, mapper);

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
            await _db.Ticks.ToListAsync();
        saved.Should().HaveCount(1);
        saved[0].Symbol.Should().Be("BTCUSDT");
    }
}