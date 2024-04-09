using DiscordBot.Models;
using DiscordBot.Services;
using System.Text.RegularExpressions;

string token = Environment.GetEnvironmentVariable("DISCORD_BOT_TOKEN");
DiscordClient client = new(token);

await client.start();

















