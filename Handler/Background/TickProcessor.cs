using System.Threading.Channels;
using App.Services;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure;
using Infrastructure.Entities;
using Infrastructure.Repositories;

namespace Handler.Background;

public class TickProcessor(
    Channel<Tick> channel,
    ITickRepository tickRepository,
    Deduplicator deduplicator,
    ILogger<TickProcessor> logger)
    : BackgroundService
{
    private const int BatchSize = 100;

    private long _processedTicks;
    private long _duplicates;

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        var batch = new List<Tick>();

        try
        {
            await foreach (
                var tick in channel.Reader.ReadAllAsync(stoppingToken))
            {
                if (deduplicator.IsDuplicate(tick))
                {
                    Interlocked.Increment(ref _duplicates);
                    continue;
                }

                batch.Add(tick);

                var processed =
                    Interlocked.Increment(ref _processedTicks);

                if (processed % 1000 == 0)
                {
                    logger.LogInformation(
                        "Processed={Processed}, Duplicates={Duplicates}",
                        _processedTicks,
                        _duplicates);
                }

                if (batch.Count >= BatchSize)
                {
                    await FlushBatchAsync(
                        batch,
                        stoppingToken);
                }
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation(
                "TickProcessor stopping");
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error while processing ticks");
        }
        finally
        {
            if (batch.Count > 0)
            {
                await FlushBatchAsync(
                    batch,
                    CancellationToken.None);
            }
        }
    }

    private async Task FlushBatchAsync(
        List<Tick> batch,
        CancellationToken cancellationToken)
    {
        if (batch.Count == 0)
        {
            return;
        }

        try
        {
            await tickRepository.SaveBatchAsync(
                batch,
                cancellationToken);

            logger.LogDebug(
                "Saved batch: {Count}",
                batch.Count);

            batch.Clear();
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error while saving batch");
        }
    }
}