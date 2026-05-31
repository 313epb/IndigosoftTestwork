using System.Threading.Channels;
using Domain.Entities;
using Domain.Interfaces;
using Handler;
using Infrastructure;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();
builder.Services.AddSingleton(Channel.CreateBounded<Tick>(new BoundedChannelOptions(10000)
{
    FullMode = BoundedChannelFullMode.Wait,
}));

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("Postgres"));
});

builder.Services.AddScoped<ITickRepo, TickRepository>();

var host = builder.Build();
host.Run();


