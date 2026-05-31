using System.Threading.Channels;
using App.Services;
using AutoMapper;
using Domain.Entities;
using Infrastructure;
using Infrastructure.Entities;
using Infrastructure.Repositories;

namespace Handler.Background;

public class TickProcessor : BackgroundService
{
    private readonly ChannelReader<Tick> _reader;
    private readonly TickRepository _repository;
    private readonly Deduplicator _deduplicator;
    private readonly IMapper _mapper;
    private long _processed;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var tick in _reader.ReadAllAsync(stoppingToken))
        {
            if (_deduplicator.IsDuplicate(tick))
                continue;
            
            var tickEntity = _mapper.Map<TickEntity>(tick);
            await _repository.SaveAsync(tickEntity, stoppingToken);

            var count = Interlocked.Increment(ref _processed);

            if (count % 100 == 0)
            {
                Console.WriteLine($"Processed: {count}");
            }
        }
    }
}