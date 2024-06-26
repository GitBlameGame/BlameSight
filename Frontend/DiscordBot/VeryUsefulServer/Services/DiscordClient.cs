﻿using VeryUsefulServer.Models;

using System.Net.Http.Headers;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using static VeryUsefulServer.Services.DiscordClient;
using System.Net.Http;
using Microsoft.AspNetCore.Http;


namespace VeryUsefulServer.Services
{


    public class DiscordClient
    {
        static string gitHubLoginUrl = "https://github.com/login/device";
        static string baseUrl = "http://" + Environment.GetEnvironmentVariable("API_ENDPOINT");

        private readonly HttpClient discordHTTPClient;
        private readonly HttpClient backendHTTPClient;

        QueryService queryService;

        List<UserState> users;
        UserStateManager userStateManager;
        CommandHandler commandHandler;
        private readonly string _discordBaseEndpoint;
        SocketsHttpHandler handler;
        ClientWebSocket ws;
        private int interval = 0;
        string token;
        Uri uri = new("ws://gateway.discord.gg");
        CancellationTokenSource source;
        Thread? heartbeatThread;

        public DiscordClient(string token)
        {
            userStateManager = new UserStateManager();
            commandHandler = new();
            ws = new();
            handler = new();
            this.token = token;
            _discordBaseEndpoint = "https://discord.com/api/v9";
            discordHTTPClient = new HttpClient();
            backendHTTPClient = new HttpClient();
            discordHTTPClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bot", token);


            source = new CancellationTokenSource();

            queryService = new(
                backendHTTPClient,
                commandHandler,
                userStateManager,
                baseUrl,
                sendMessage
                );

        }
        

        public async Task<string> sendMessage(DiscordMessage discordMessage, string channelId)
        {

            Console.Write("ChannelID : " + channelId);
            var request = new HttpRequestMessage(HttpMethod.Post, _discordBaseEndpoint + $"/channels/{channelId}/messages");
            var temp = JsonSerializer.Serialize(discordMessage);
            request.Content = new StringContent(temp, System.Text.Encoding.UTF8, "application/json");
            var response = await discordHTTPClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            else
            {
                throw new HttpRequestException($"Discord api request failed with status code {(int)response.StatusCode} - {response.ReasonPhrase}");
            }
        }

        public async Task sendHello()
        {
            var initial_payload = new InitialPayload
            {
                op = 2,
                d = new InitialData
                {
                    token = token,
                    intents = 513,
                    properties = new InitialProperties
                    {
                        os = "Windows",
                        browser = "chrome",
                        device = "chrome"
                    }
                }
            };

            ArraySegment<byte> bytesToSend = new ArraySegment<byte>(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(initial_payload)));
            await ws.SendAsync(bytesToSend, WebSocketMessageType.Text, true, source.Token);
        }

        public async Task start()
        {
            await ws.ConnectAsync(uri, new HttpMessageInvoker(handler), source.Token);
            await sendHello();
            while (ws.State == WebSocketState.Open)
            {
                ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[4096]);
                WebSocketReceiveResult response = await ws.ReceiveAsync(buffer, source.Token);

                if (response.MessageType == WebSocketMessageType.Text)
                {
                    string receivedMessage = System.Text.Encoding.UTF8.GetString(buffer.Array, 0, response.Count);
                    Console.WriteLine($"Unrefined: {receivedMessage}\n\n\n");
                    try
                    {

                        Message gatewayEvent = JsonSerializer.Deserialize<Message>(receivedMessage);

                        if (gatewayEvent.op == 10 && gatewayEvent.d.heartbeat_interval != null)
                        {

                            interval = (int)gatewayEvent.d.heartbeat_interval;

                            startHeartbeat(ws);

                        }

                        if (gatewayEvent.t != null && gatewayEvent.t.Equals("MESSAGE_CREATE"))
                        {
                            Console.WriteLine($"Received message from discord: {gatewayEvent.d.content}");
                            if (Regex.IsMatch(gatewayEvent.d.content, @"^/blame\b"))
                            {
                                if (Regex.IsMatch(gatewayEvent.d.content, @"^/blame login\b"))
                                {
                                    await login(gatewayEvent.d.channel_id, gatewayEvent.d.author.id);
                                }
                                else if (Regex.IsMatch(gatewayEvent.d.content, @"^/blame confirm\b"))
                                {
                                    await confirm(gatewayEvent.d.channel_id, gatewayEvent.d.author.id);
                                    await queryService.GetHelloWorld(gatewayEvent);
                                }
                                else if (Regex.IsMatch(gatewayEvent.d.content, @"^/blame help\b"))
                                {
                                    await sendHelp(gatewayEvent.d.channel_id);
                                }
                                else if (Regex.IsMatch(gatewayEvent.d.content, @"^/blame new\b"))
                                {
                                    await queryService.NewBlame(gatewayEvent);
                                }
                                else if (Regex.IsMatch(gatewayEvent.d.content, @"^/blame shame\b"))
                                {
                                    await queryService.GetBlameShame(gatewayEvent);
                                }
                                else if (Regex.IsMatch(gatewayEvent.d.content, @"^/blame atMe\b"))
                                {
                                    await queryService.getMyOpenBlames(gatewayEvent);
                                }
                                else if (Regex.IsMatch(gatewayEvent.d.content, @"^/blame byMe\b"))
                                {
                                    await queryService.getMyCreatedBlames(gatewayEvent);

                                }
                                else if (Regex.IsMatch(gatewayEvent.d.content, @"^/blame begone\b"))
                                {
                                    await queryService.SetBlameComplete(gatewayEvent);
                                }
                                else
                                {
                                    await sendHelp(gatewayEvent.d.channel_id);
                                }
                            }
                        }
                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine(ex.Message);
                    }




                }
                else if (response.MessageType == WebSocketMessageType.Close)
                {
                    await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "", source.Token);
                }
            }
        }

        public void startHeartbeat(ClientWebSocket ws)
        {
            heartbeatThread = new Thread(() =>
            {
                while (true)
                {
                    Console.WriteLine($"Sent heartbeat");
                    Thread.CurrentThread.IsBackground = true;
                    System.Threading.Thread.Sleep(interval);
                    ArraySegment<byte> heartbeat = new ArraySegment<byte>(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new InitialPayload { op = 1, d = null })));
                    ws.SendAsync(heartbeat, WebSocketMessageType.Text, true, CancellationToken.None);

                }
            });
            heartbeatThread.Start();
        }
        //Refactor this:

        public async Task confirm(string channel_id, string discord_id)
        {
            if (userStateManager.userStateExists(discord_id))
            {
                UserState user = userStateManager.getUserState(discord_id);
                var deviceCode = user.deviceCode ?? throw new Exception("No device code for this user, start at /blame login please.");
                var accessTokenResponse = await ExchangeDeviceCodeForAccessToken(baseUrl, backendHTTPClient, deviceCode);

                if (!accessTokenResponse.ContainsKey("access_token"))
                {
                    Console.WriteLine("Authorization process was not completed. Please try again.");
                    await login(channel_id, discord_id); // Restart the login process
                    return;
                }

                // Extract access token
                user.accessToken = accessTokenResponse["access_token"];
                string accessToken = accessTokenResponse["access_token"];

                //Use Access Token to obtain JWT
                var jwtResponse = await GetJwtFromBackend(baseUrl, backendHTTPClient, accessToken);
                user.jwt = jwtResponse;
                userStateManager.putUserState(user);


            }

        }
        public async Task login(string channel_id, string discord_id)
        {

            var deviceCodeResponse = await GetDeviceCode(baseUrl, backendHTTPClient);
            string deviceCode = deviceCodeResponse["device_code"];
            string userCode = deviceCodeResponse["user_code"];
            UserState newuser = new(discord_id);
            newuser.deviceCode = deviceCode;

            userStateManager.putUserState(newuser);
            DiscordMessage msg = new($"\nPlease visit {gitHubLoginUrl} and enter the code : {userCode}\n\n Type '/blame confirm' after you have authorized.");
            await sendMessage(msg, channel_id);


        }


        public async Task sendHelp(string channelId)
        {
            string commands = "";


            commands += "╔══════════════════════════════ Help Screen ══════════════════════════════╗\n";
            commands += "║                                                                         ║\n";
            commands += "║ /blame help            Display this help screen.                        ║\n";
            commands += "║ /blame login           Login to BlameSight.                             ║\n";
            commands += "║ /blame new             Blame a user on a GitHub repository.             ║\n";
            commands += "║                        -> -path Owner/Repo/File <\"string\">              ║\n";
            commands += "║                        -> -lineNum Line number of code to blame <int>   ║\n";
            commands += "║                        -> -Comment Blame comment <\"string\">             ║\n";
            commands += "║                        -> -Urgency 1 to 5                               ║\n";
            commands += "║                        -> -branch Branch name <\"string\">                ║\n";
            commands += "║ /blame byMe            View blames where you were the author.           ║\n";
            commands += "║ /blame atMe            View open blames that you initiated.             ║\n";
            commands += "║ /blame begone          Mark a blame as resolved.                        ║\n";
            commands += "║                        -> -id  ID of Blame <int>                        ║\n";
            commands += "║ /blame shame           Shows the blame leaderboard (of shame).          ║\n";
            commands += "║                                                                         ║\n";
            commands += "║        Commands are case ... SENSITIVE.                                 ║\n";
            commands += "╚═════════════════════════════════════════════════════════════════════════╝\n";

            await sendMessage(new DiscordMessage($"```{commands}```"), channelId);
        }



        /*++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
         *                  HELPER FUNCTIONS
         * +++++++++++++++++++++++++++++++++++++++++++++++++++++++++
         */


        static async Task<Dictionary<string, string>> GetDeviceCode(string baseUrl, HttpClient client)
        {
            var clientId = "Iv1.8586ee55069c437b";
            var response = await client.PostAsync("https://github.com/login/device/code", new StringContent($"client_id={clientId}", System.Text.Encoding.UTF8, "application/x-www-form-urlencoded"));
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync();
            return ParseResponse(responseBody);
        }

        static Dictionary<string, string> ParseResponse(string response)
        {
            var parts = response.Split('&');
            var result = new Dictionary<string, string>();
            foreach (var part in parts)
            {
                var keyValue = part.Split('=');
                if (keyValue.Length == 2) // Check if the array has at least 2 elements
                {
                    result[keyValue[0]] = keyValue[1];
                }
            }
            return result;
        }

        static async Task<Dictionary<string, string>> ExchangeDeviceCodeForAccessToken(string baseUrl, HttpClient client, string deviceCode)
        {
            var clientId = "Iv1.8586ee55069c437b";
            var grantType = "urn:ietf:params:oauth:grant-type:device_code";

            var requestBody = $"client_id={clientId}&grant_type={grantType}&device_code={deviceCode}";

            var response = await client.PostAsync("https://github.com/login/oauth/access_token/", new StringContent(requestBody, System.Text.Encoding.UTF8, "application/x-www-form-urlencoded"));

            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();

            return ParseResponse(responseBody);
        }

        static async Task<string> GetJwtFromBackend(string baseUrl, HttpClient client, string accessToken)
        {
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Authorization", accessToken);

            HttpResponseMessage response = await client.GetAsync($"{baseUrl}/api/login");

            response.EnsureSuccessStatusCode();
            string jwt = await response.Content.ReadAsStringAsync();
            return jwt;
        }



    }

}
