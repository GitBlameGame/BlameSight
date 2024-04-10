using DiscordBot.Models;

using System.Net.Http.Headers;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using static DiscordBot.Services.DiscordClient;


namespace DiscordBot.Services
{


    public class DiscordClient
    {
        static string gitHubLoginUrl = "https://github.com/login/device";
        static string baseUrl = Environment.GetEnvironmentVariable("API_ENDPOINT");

        private readonly HttpClient discordHTTPClient;
        private readonly HttpClient backendHTTPClient;


        List<UserState> _users;
        UserStateManager _userStateManager;
        CommandHandler _commandHandler;
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
            _userStateManager = new UserStateManager();
            _commandHandler = new();
            ws = new();
            handler = new();
            this.token = token;
            _discordBaseEndpoint = "https://discord.com/api/v9";
            discordHTTPClient = new HttpClient();
            backendHTTPClient = new HttpClient();
            discordHTTPClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bot", token);


            source = new CancellationTokenSource();

        }
        // helper thread for heartbeat

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
                    //Console.WriteLine($"Unrefined: {receivedMessage}\n\n\n");
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
                                if (gatewayEvent.d.content.Equals("/blame login"))
                                {
                                    await login(gatewayEvent.d.channel_id, gatewayEvent.d.author.id);
                                }
                                if (gatewayEvent.d.content.Equals("/blame confirm"))
                                {
                                    await confirm(gatewayEvent.d.channel_id, gatewayEvent.d.author.id);
                                    await GetHelloWorld(gatewayEvent);
                                }
                                if (gatewayEvent.d.content.Equals("/blame help"))
                                {
                                    await sendHelp(gatewayEvent.d.channel_id);
                                }
                                if (Regex.IsMatch(gatewayEvent.d.content, @"^/blame newBlame\b"))
                                {
                                    await NewBlame(gatewayEvent);
                                }
                                if (Regex.IsMatch(gatewayEvent.d.content, @"^/blame shame\b"))
                                {
                                    await GetBlameShame(gatewayEvent);
                                }
                                if (Regex.IsMatch(gatewayEvent.d.content, @"^/blame openBlames\b"))
                                {
                                    await getMyOpenBlames(gatewayEvent);
                                }
                                if (Regex.IsMatch(gatewayEvent.d.content, @"^/blame myBlames\b"))
                                {
                                    await getMyCreatedBlames(gatewayEvent);
                                }
                                if (Regex.IsMatch(gatewayEvent.d.content, @"^/blame begoneBlame\b"))
                                {
                                    await SetBlameComplete(gatewayEvent);
                                }
                                //_commandHandler.Handle(gatewayEvent);
                                //Console.WriteLine(receivedMessage);
                                //await sendMessage(new DiscordMessage("Hello there " + gatewayEvent.d.author.username), gatewayEvent.d.channel_id);
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
            if (_userStateManager.userStateExists(discord_id))
            {
                UserState user = _userStateManager.getUserState(discord_id);
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
                _userStateManager.putUserState(user);


            }

        }
        public async Task login(string channel_id, string discord_id)
        {

            var deviceCodeResponse = await GetDeviceCode(baseUrl, backendHTTPClient);
            string deviceCode = deviceCodeResponse["device_code"];
            string userCode = deviceCodeResponse["user_code"];
            UserState newuser = new(discord_id);
            newuser.deviceCode = deviceCode;

            _userStateManager.putUserState(newuser);
            DiscordMessage msg = new($"\nPlease visit {gitHubLoginUrl} and enter the code : {userCode}\n\n Type '/blame confirm' after you have authorized.");
            await sendMessage(msg, channel_id);


        }



        //Duplicated code:
        public class RankUser
        {
            public string name { get; set; }
            public int blamePoints { get; set; }
        }

        public class Blame
        {
            public int id { get; set; }
            public string name { get; set; }
            public string path { get; set; }
            public string comment { get; set; }
            public string urgencyDescriptor { get; set; }
            public int lineNum { get; set; }
            public bool blameViewed { get; set; }
            public bool blameComplete { get; set; }

            public override string ToString()
            {
                string blameString = "";
                blameString +=$"ID: {id}\n";
                blameString +=$"Name: {name}\n";
                blameString +=$"Path: {path}\n";
                blameString +=$"Comment: {comment}\n";
                blameString +=$"Urgency: {urgencyDescriptor}\n";
                blameString +=$"Line Number: {lineNum}\n";
                blameString +=$"Blame Viewed: {blameViewed}\n";
                blameString +=$"Blame Complete: {blameComplete}\n";
                
                return blameString;
            }
        }
        public async Task DisplayGreetingAsync(Message message)
        {

            string welcomeMessage = "Welcome to BlameSight!\n\n For more information, type 'blame help'.\n🔥 This Week's Board of Shame 🔥\n";
            DiscordMessage msg = new(welcomeMessage);
            await GetBlameShame(message);
        }

        public async Task sendHelp(string channelId)
        {
            string commands = "";
            

            commands +="╔══════════════════════════════ Help Screen ══════════════════════════════╗\n";
            commands +="║                                                                         ║\n";
            commands +="║ blame help            Display this help screen.                         ║\n";
            commands +="║ blame login           Login to BlameSight.                              ║\n";
            commands +="║ blame newBlame        Blame a user on a GitHub repository.              ║\n";
            commands +="║ blame myBlames        View blames where you were the author.            ║\n";
            commands +="║ blame openBlames      View open blames that you initiated.              ║\n";
            commands +="║ blame begoneBlame     Mark a blame as resolved.                         ║\n";
            commands +="║ clear                 Clear the terminal                                ║\n";
            commands +="║                                                                         ║\n";
            commands +="║Commands are case insensitive.                                           ║\n";
            commands +="╚═════════════════════════════════════════════════════════════════════════╝\n";

            await sendMessage(new DiscordMessage(commands), channelId);
        }

        // =================================
        //            QUERIES
        // =================================

        public async Task SetBlameComplete(Message message)
        {
            if (_userStateManager.userStateExists(message.d.author.id) && _userStateManager.getUserState(message.d.author.id).currentState == State.LOGGED_IN)
            {
                // Set the Authorization header with the JWT token
                backendHTTPClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _userStateManager.getUserState(message.d.author.id).jwt);

                var commandDict = _commandHandler.ExtractParameterBody(
                    ["id"],
                    message.d.content);
                // Send the HTTP request
                HttpResponseMessage response = await backendHTTPClient.GetAsync(baseUrl + $"/api/Blames/blameBegone/{commandDict.GetValueOrDefault("id")}");

                // Check if the request was successful
                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Server response: {responseBody}");
                    await sendMessage(new DiscordMessage(responseBody), message.d.channel_id);
                }
                else
                {
                    // Print out the detailed error reason if available
                    string errorMessage = response.ReasonPhrase; // Default to the reason phrase
                    if (response.Content != null)
                    {
                        errorMessage = await response.Content.ReadAsStringAsync();
                    }
                    Console.WriteLine($"\nError ({response.StatusCode}): {errorMessage}");
                    return;
                }
            }
            else
            {
                Console.WriteLine("JWT token is missing. Please log in first.");
            }
        }
        public async Task getMyOpenBlames(Message message)
        {
            if (_userStateManager.userStateExists(message.d.author.id) && _userStateManager.getUserState(message.d.author.id).currentState == State.LOGGED_IN)
            {
                // Set the Authorization header with the JWT token
                backendHTTPClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _userStateManager.getUserState(message.d.author.id).jwt);


                using (var cts = new CancellationTokenSource())
                {
                    

                    HttpResponseMessage response = await backendHTTPClient.GetAsync(baseUrl + "/api/Blames/openBlames");
                    
                    

                    if (response.IsSuccessStatusCode)
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();
                        // Deserialize the JSON response into a list of Blame objects
                        if (responseBody.Contains("You have no pending blames"))
                        {
                            await sendMessage(new DiscordMessage("You have no pending blames. Congrats! 🎊"), message.d.channel_id);
                            return; // Return an empty list
                        }
                        Console.WriteLine(responseBody);

                        var blames = JsonSerializer.Deserialize<List<Blame>>(responseBody);

                        string result = string.Empty;
                        Console.WriteLine("\nBlames:");
                        foreach (var blame in blames)
                        {
                            result += blame.ToString();
                        }
                        await sendMessage(new DiscordMessage(result), message.d.channel_id);
                    }
                    else
                    {
                        // Print out the detailed error reason if available
                        string errorMessage = response.ReasonPhrase; // Default to the reason phrase
                        if (response.Content != null)
                        {
                            errorMessage = await response.Content.ReadAsStringAsync();
                        }
                        Console.WriteLine($"\nError ({response.StatusCode}): {errorMessage}");
                        return;
                    }

                }
            }
            else
            {
                Console.WriteLine("JWT token is missing. Please log in first.");
            }
        }

        private async Task GetHelloWorld(Message message)
        {
            if (_userStateManager.userStateExists(message.d.author.id) && _userStateManager.getUserState(message.d.author.id).currentState == State.LOGGED_IN)
            {
                // Set the Authorization header with the JWT token
                backendHTTPClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _userStateManager.getUserState(message.d.author.id).jwt);

                HttpResponseMessage response = await backendHTTPClient.GetAsync(baseUrl + "/api/hello");

                // Check if the request was successful
                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    await sendMessage(new DiscordMessage(responseBody), message.d.channel_id);
                }
                else
                {
                    Console.WriteLine($"Error: {response.StatusCode}");
                }
            }
            else
            {
                await sendMessage(new DiscordMessage("Unauthorized, please use: /blame login"), message.d.channel_id);
                Console.WriteLine("JWT token is missing. Please log in first.");
            }
        }
        public async Task GetBlameShame(Message message)

        {
                try
                {
                    HttpResponseMessage response = await backendHTTPClient.GetAsync(baseUrl + "/api/Blames/blameShame");

                    if (response.IsSuccessStatusCode)
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();

                        var blameShame = JsonSerializer.Deserialize<RankUser[]>(responseBody);
                        string result = "";
                        foreach (var user in blameShame)
                        {
                            result += $"Name: {user.name}, Blame Points: {user.blamePoints}\n";
                        }
                        await sendMessage(new DiscordMessage(result), message.d.channel_id);
                    }
                    else
                    {
                        Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                }

        }

        public  async Task getMyCreatedBlames(Message message)
        {
            if (_userStateManager.userStateExists(message.d.author.id) && _userStateManager.getUserState(message.d.author.id).currentState == State.LOGGED_IN)
            {
                // Set the Authorization header with the JWT token
                backendHTTPClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _userStateManager.getUserState(message.d.author.id).jwt);


                using (var cts = new CancellationTokenSource())
                {
                    

                    HttpResponseMessage response = await backendHTTPClient.GetAsync(baseUrl + "/api/Blames/myBlames");
                    


                    if (response.IsSuccessStatusCode)
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();
                        // Deserialize the JSON response into a list of Blame objects
                        try 
                        {
                            var blames = JsonSerializer.Deserialize<List<Blame>>(responseBody);
                            string result = string.Empty;
                            Console.WriteLine("\nBlames:");
                            foreach (var blame in blames)
                            {
                                result += blame.ToString();
                            }
                            await sendMessage(new DiscordMessage(result), message.d.channel_id);
                        }
                        catch
                        {
                            await sendMessage(new DiscordMessage(responseBody), message.d.channel_id);
                        }
                        
                        
                    }
                    else
                    {
                        // Print out the detailed error reason if available
                        string errorMessage = response.ReasonPhrase; // Default to the reason phrase
                        if (response.Content != null)
                        {
                            errorMessage = await response.Content.ReadAsStringAsync();
                        }
                        Console.WriteLine($"\nError ({response.StatusCode}): {errorMessage}");
                        return;
                    }

                }
            }
            else
            {
                Console.WriteLine("JWT token is missing. Please log in first.");
            }
        }
        public async Task NewBlame(Message message)
        {
            if (_userStateManager.userStateExists(message.d.author.id) && _userStateManager.getUserState(message.d.author.id).currentState == State.LOGGED_IN)
            {
                StringContent? httpContent;

                // Set the Authorization header with the JWT token
                backendHTTPClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _userStateManager.getUserState(message.d.author.id).jwt);
                try{
                var commandDict = _commandHandler.ExtractParameterBody(
                    (new NewBlameRequest().getKeys()),
                    message.d.content);

                NewBlameRequest newBlameRequest = new(commandDict);

                // Serialize the request body to JSON
                var jsonString = JsonSerializer.Serialize(newBlameRequest);
                 httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
}
                catch
                {
                    await sendMessage(new DiscordMessage($"Sorry! You're input doesn't seem to be correct, please try again."), message.d.channel_id);
                    return;
                }
                // Start loader animation
                using (var cts = new CancellationTokenSource())
                {
                    HttpResponseMessage response;
                    response = await backendHTTPClient.PutAsync(baseUrl + "/api/Blames/newBlame", httpContent);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"\nServer response: {responseBody}");
                        //TODO: Send response back to discord channelk
                        await sendMessage(new DiscordMessage(responseBody), message.d.channel_id);
                    }
                    else
                    {
                        // Print out the detailed error reason if available
                        string errorMessage = response.ReasonPhrase; // Default to the reason phrase
                        if (response.Content != null)
                        {
                            errorMessage = await response.Content.ReadAsStringAsync();
                        }
                        Console.WriteLine($"\nError ({response.StatusCode}): {errorMessage}");
                        await sendMessage(new DiscordMessage($"Sorry! Something seems to have gone wrong."), message.d.channel_id);
                        return;
                    }

                }
            }
            else
            {
                Console.WriteLine("JWT token is missing. Please log in first.");
                await sendMessage(new DiscordMessage($"JWT token is missing. Please log in first."), message.d.channel_id);
            }
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
