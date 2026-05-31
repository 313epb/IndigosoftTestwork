using System.Threading.Channels;
using Domain.Entities;

namespace Domain.Interfaces;

public interface IExchangeClient
{
    Task StartAsync(ChannelWriter<Tick> writer, CancellationToken ct);
}