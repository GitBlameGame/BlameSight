using static VeryUsefulServer.Services.DiscordClient;
using VeryUsefulServer.Models;
using VeryUsefulServer.Services;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;

public class QueryService
{
    public delegate Task AuthDelegate();
    public delegate void HTTPResponseDelegate();
    public delegate Task SendMessageDelegate(DiscordMessage message, string channelId);
    HttpClient backendHTTPClient;
    CommandHandler commandHandler;
    UserStateManager userStateManager;
    string baseUrl;
    SendMessageDelegate sendMessage;


    public QueryService(
            HttpClient client, 
            CommandHandler handler,
            UserStateManager manager,
            string url,
            SendMessageDelegate snd
        )
    {
        this.backendHTTPClient = client;
        this.commandHandler = handler;
        this.userStateManager = manager;
        this.baseUrl = url;
        this.sendMessage = snd;
    }


    public async Task SetBlameComplete(Message message)
    {

        AuthDelegate auth = async () =>
        {
            backendHTTPClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userStateManager.getUserState(message.d.author.id).jwt);

            var commandDict = commandHandler.ExtractParameterBody(
                ["id"],
                message.d.content);
            // Send the HTTP request
            HttpResponseMessage response = await backendHTTPClient.GetAsync(baseUrl + $"/api/Blames/blameBegone/{commandDict.GetValueOrDefault("id")}");

            HTTPResponseDelegate hTTPResponseDelegate = async () =>
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Server response: {responseBody}");
                await sendMessage(new DiscordMessage(responseBody), message.d.channel_id);
            };

            await HTTPResponseWrapper(response, message.d.channel_id, hTTPResponseDelegate);
        };

        await AuthWrapper(message, auth);

    }

    public async Task getMyOpenBlames(Message message)
    {

        AuthDelegate test = async () =>
        {
            backendHTTPClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userStateManager.getUserState(message.d.author.id).jwt);

            var commandDict = commandHandler.ExtractParameterBody(
                ["id"],
                message.d.content);
            // Send the HTTP request
            HttpResponseMessage response = await backendHTTPClient.GetAsync(baseUrl + "/api/Blames/openBlames");

            HTTPResponseDelegate hTTPResponseDelegate = async () =>
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
            };

            await HTTPResponseWrapper(response, message.d.channel_id, hTTPResponseDelegate);
        };

        await AuthWrapper(message, test);



    }

    public async Task GetHelloWorld(Message message)
    {

        AuthDelegate auth = async () =>
        {
            backendHTTPClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userStateManager.getUserState(message.d.author.id).jwt);

            // Send the HTTP request
            HttpResponseMessage response = await backendHTTPClient.GetAsync(baseUrl + "/api/hello");

            HTTPResponseDelegate hTTPResponseDelegate = async () =>
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                await sendMessage(new DiscordMessage(responseBody), message.d.channel_id);
            };

            await HTTPResponseWrapper(response, message.d.channel_id, hTTPResponseDelegate);
        };

        await AuthWrapper(message, auth);
    }
    public async Task GetBlameShame(Message message)

    {
        HttpResponseMessage response = await backendHTTPClient.GetAsync(baseUrl + "/api/Blames/blameShame");
        HTTPResponseDelegate hTTPResponseDelegate = async () =>
        {
            string responseBody = await response.Content.ReadAsStringAsync();

            var blameShame = JsonSerializer.Deserialize<RankUser[]>(responseBody);
            string result = "# Wall of Shame\n";
            foreach (var user in blameShame)
            {
                result += $"***Name***: {user.name}, Blame Points: {user.blamePoints}\n";
            }
            await sendMessage(new DiscordMessage(result), message.d.channel_id);

        };
        await HTTPResponseWrapper(response, message.d.channel_id, hTTPResponseDelegate);


    }

    public async Task getMyCreatedBlames(Message message)
    {
        AuthDelegate auth = async () =>
        {
            backendHTTPClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userStateManager.getUserState(message.d.author.id).jwt);

            // Send the HTTP request
            HttpResponseMessage response = await backendHTTPClient.GetAsync(baseUrl + "/api/Blames/myBlames");
            string responseBody = await response.Content.ReadAsStringAsync();

            HTTPResponseDelegate hTTPResponseDelegate = async () =>
            {
                var blames = JsonSerializer.Deserialize<List<Blame>>(responseBody);
                string result = string.Empty;
                Console.WriteLine("\nBlames:");
                foreach (var blame in blames)
                {
                    result += blame.ToString();
                }
                await sendMessage(new DiscordMessage(result), message.d.channel_id);
            };

            await HTTPResponseWrapper(response, message.d.channel_id, hTTPResponseDelegate);
        };

        await AuthWrapper(message, auth);

    }
    public async Task NewBlame(Message message)
    {

        AuthDelegate auth = async () =>
        {
            backendHTTPClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userStateManager.getUserState(message.d.author.id).jwt);

            // Send the HTTP request
            HttpResponseMessage response = await backendHTTPClient.GetAsync(baseUrl + "/api/hello");
            StringContent? httpContent;
            try
            {
                var commandDict = commandHandler.ExtractParameterBody(
                    (new NewBlameRequest().getKeys()),
                    message.d.content);

                NewBlameRequest newBlameRequest = new(commandDict);

                // Serialize the request body to JSON
                var jsonString = JsonSerializer.Serialize(newBlameRequest);
                httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
                response = await backendHTTPClient.PutAsync(baseUrl + "/api/Blames/newBlame", httpContent);

            }
            catch
            {
                await sendMessage(new DiscordMessage($"Sorry! You're input doesn't seem to be correct, please try again."), message.d.channel_id);
                return;
            }
            HTTPResponseDelegate hTTPResponseDelegate = async () =>
            {


                string responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"\nServer response: {responseBody}");
                //TODO: Send response back to discord channelk
                await sendMessage(new DiscordMessage(responseBody), message.d.channel_id);
            };

            await HTTPResponseWrapper(response, message.d.channel_id, hTTPResponseDelegate);
        };

        await AuthWrapper(message, auth);
    }


    private async Task AuthWrapper(Message message, AuthDelegate action)
    {
        if (userStateManager.userStateExists(message.d.author.id) && userStateManager.getUserState(message.d.author.id).currentState == State.LOGGED_IN)
        {
            await action();
        }
        else
        {
            await sendMessage(new DiscordMessage("You are not authenticated, begone! (Please run /blame login)"), message.d.channel_id);

        }
    }

    private async Task HTTPResponseWrapper(HttpResponseMessage response, string channelId, HTTPResponseDelegate responseDelegate)
    {
        if (response.IsSuccessStatusCode)
        {
            // Deserialize the JSON response into a list of Blame objects
            try
            {
                responseDelegate();
            }
            catch
            {
                await sendMessage(new DiscordMessage("Something went wrong with the request!"), channelId);
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
            await sendMessage(new DiscordMessage($"\n**Error ({response.StatusCode})**: {errorMessage}"), channelId);
            return;
        }
    }
}