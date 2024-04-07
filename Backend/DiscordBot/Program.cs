using DiscordBot.Services;

string token =  Environment.GetEnvironmentVariable("DISCORD_BOT_TOKEN");
DiscordClient client = new(token);

await client.start();
















