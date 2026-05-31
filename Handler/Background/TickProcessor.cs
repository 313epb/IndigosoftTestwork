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
    ITickRepo repository,
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
        var flushTimer = new PeriodicTimer(TimeSpan.FromMilliseconds(500));

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var readTask = channel.Reader.ReadAsync(stoppingToken).AsTask();
                var timerTask = flushTimer.WaitForNextTickAsync(stoppingToken).AsTask();

                var completed = await Task.WhenAny(readTask, timerTask);

                if (completed == readTask)
                {
                    var tick = await readTask;

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
                else
                {
                    if (batch.Count > 0)
                    {
                        await FlushBatchAsync(
                            batch,
                            stoppingToken);
                    }
                }
            }
        }
        finally
        {
            if (batch.Count > 0)
            {
                await FlushBatchAsync(
                    batch,
                    CancellationToken.None);
            }

            flushTimer.Dispose();
        }
    }

    private async Task FlushBatchAsync(
        List<Tick> batch,
        CancellationToken cancellationToken)
    {
        await repository.SaveBatchAsync(
            batch,
            cancellationToken);

        logger.LogDebug(
            "Saved batch: {Count}",
            batch.Count);

        batch.Clear();
    }
}