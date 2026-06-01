using System.Threading.Channels;
using Domain.Entities;
using Domain.Interfaces;

namespace Handler.Background;

public class ExchangeWorker(
    IEnumerable<IExchangeClient> clients,
    Channel<Tick> channel)
    : BackgroundService
{
    protected override Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        return Task.WhenAll(
            clients.Select(x =>
                x.StartAsync(
                    channel.Writer,
                    stoppingToken)));
    }
}