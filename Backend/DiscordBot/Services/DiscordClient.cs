using DiscordBot.Models;

using System.Net.Http.Headers;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;


namespace DiscordBot.Services
{
    public class DiscordClient
    {
        private readonly HttpClient _httpClient;


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
            ws = new();
            handler = new();
            this.token = token;
            _discordBaseEndpoint = "https://discord.com/api/v9";
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bot", token);

            source = new CancellationTokenSource();

        }
        // helper thread for heartbeat

        public async Task<string> sendMessage(DiscordMessage discordMessage, string channelId)
        {
            Console.Write("ChannelID : " + channelId);
            var request = new HttpRequestMessage(HttpMethod.Post, _discordBaseEndpoint + $"/channels/{channelId}/messages");
            var temp = JsonSerializer.Serialize(discordMessage);
            request.Content = new StringContent(temp, System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.SendAsync(request);
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
                            if (gatewayEvent.d.content.Contains("/blame"))
                            {
                                Console.WriteLine(receivedMessage);
                                await sendMessage(new DiscordMessage("Hello there " + gatewayEvent.d.author.username), gatewayEvent.d.channel_id);
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
    }


}
