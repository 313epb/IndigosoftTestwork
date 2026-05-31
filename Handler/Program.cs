using System.Threading.Channels;
using Domain.Entities;
using Handler;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();
builder.Services.AddSingleton(Channel.CreateBounded<Tick>(new BoundedChannelOptions(10000)
{
    FullMode = BoundedChannelFullMode.Wait,
}));

var host = builder.Build();
host.Run();
