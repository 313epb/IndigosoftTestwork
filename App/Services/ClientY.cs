using System.Net.WebSockets;
using System.Text;
using System.Threading.Channels;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace App.Services;

public class ClientY : IExchangeClient
{
    private readonly ILogger<ClientY> _logger;

    public async Task StartAsync(ChannelWriter<Tick> writer, CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                using (var socket= new ClientWebSocket())
                {
                    await socket.ConnectAsync(new Uri("ws://localhost:8080"), ct);
                    _logger.LogInformation($"{nameof(ClientY)} connected");
                    var buffer= new byte[2048];
                    while (socket.State == WebSocketState.Open)
                    {
                        var result = await socket.ReceiveAsync(buffer, ct);

                        var json = Encoding.UTF8.GetString(
                            buffer,
                            0,
                            result.Count);

                        var tick = Parse(json);
                        await writer.WriteAsync(tick, ct);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                await Task.Delay(1000, ct);
            }
        }
    }
    
    private Tick Parse(string json)
    {
        //Тут в зависимости от формата  входной строки парсим в тик. 
        return new Tick
        {
            Timestamp =  DateTime.Now
        };
    }
}