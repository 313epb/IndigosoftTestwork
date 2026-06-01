using System.Threading.Channels;
using App.Services;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Settings;
using Handler.Background;
using Infrastructure;
using Infrastructure.Mapping;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddMemoryCache();

builder.Services.AddAutoMapper(typeof(TickProfile).Assembly);

builder.Services.AddSingleton(Channel.CreateBounded<Tick>(new BoundedChannelOptions(10000)
{
    FullMode = BoundedChannelFullMode.Wait,
}));
builder.Services.AddScoped<ITickRepository, TickRepository>();
builder.Services.AddSingleton<IExchangeClient, ClientX>();
builder.Services.AddSingleton<IExchangeClient, ClientY>();
builder.Services.AddSingleton<IExchangeClient, ClientZ>();
builder.Services.AddSingleton<Deduplicator>();
builder.Services.AddHostedService<ExchangeWorker>();
builder.Services.AddHostedService<TickProcessor>();

builder.Services.Configure<ClientConnectionStrings>( x => 
    builder.Configuration.GetSection(nameof(ClientConnectionStrings)).Bind(x));

builder.Services.AddDbContextFactory<AppDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("Postgres"));
});

var host = builder.Build();
host.Run();