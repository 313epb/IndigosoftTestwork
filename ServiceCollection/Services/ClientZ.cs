using System.Net.WebSockets;
using System.Text;
using System.Threading.Channels;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Settings;
using Microsoft.Extensions.Logging;

namespace App.Services;

public class ClientZ(ILogger<ClientZ> logger, ClientConnectionStrings  connection) : IExchangeClient
{
    public async Task StartAsync(ChannelWriter<Tick> writer, CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                using (var socket= new ClientWebSocket())
                {
                    await socket.ConnectAsync(new Uri(connection.ClientZ!), ct);
                    logger.LogInformation($"{nameof(ClientZ)} connected");
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
                logger.LogError(ex, ex.Message);
                await Task.Delay(1000, ct);
            }
        }
    }
    
    private Tick Parse(string json)
    {
        //Тут в зависимости от формата  входной строки парсим в тик. 
        return new Tick
        {
            Source = nameof(ClientZ),
            Timestamp =  DateTime.UtcNow
        };
    }
}