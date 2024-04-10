using System.Net.WebSockets;
using System.Text.Json;
using System.Text;
using VeryUsefulServer.Models;
using VeryUsefulServer.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();


var parasite = new Thread(async () =>
{
    string token = Environment.GetEnvironmentVariable("DISCORD_BOT_TOKEN");
    DiscordClient client = new(token);

    await client.start();
});
parasite.Start();

app.Run();