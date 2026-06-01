using System.Threading.Channels;
using Domain.Entities;

namespace Tests.Utility;

public static class TickProducer
{
    public static async Task ProduceTicks(
        ChannelWriter<Tick> writer,
        string source,
        int count)
    {
        for (var i = 0; i < count; i++)
        {
            await writer.WriteAsync(
                new Tick
                {
                    Source = source,
                    Symbol = $"BTC{i}",
                    Price = i,
                    Volume = 1,
                    Timestamp = DateTime.UtcNow
                });
        }
    }
}