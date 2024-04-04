using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

class UserInput
{
    static string gitHubLoginUrl = "https://github.com/login/device";
    static string? jwt;
    static string baseUrl = "http://localhost:5000";

    static async Task Main(string[] args)
    {

        HttpClient client = new HttpClient();

        Console.Write("Welcome to GitBlame!\n");
        Console.WriteLine("For more information, type 'blame help'.\n");

        while (true)
        {

            string input = Console.ReadLine()?.ToLower();

            // Split the input string by space and take the first word
            string[] inputParts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            string firstWord = inputParts.Length > 0 ? inputParts[0] : "";

            if (firstWord == "blame")
            {
                // Process the command
                string command = inputParts.Length > 1 ? inputParts[1] : ""; // Get the second word as the command
                switch (command)
                {
                    case "login":
                        await Login(client);
                        break;
                    case "help":
                        DisplayBlameHelp();
                        break;
                    default:
                        Console.WriteLine("Invalid command. Please try again.");
                        break;
                }
            }
            else
            {
                Console.WriteLine("Command must start with 'Blame'. Please try again.");
            }
        }
    }

    static void DisplayBlameHelp()
    {
        Console.WriteLine("\nHelp Screen:########################################################################");
        Console.WriteLine("blame help                          Display this help screen.\n");
        Console.WriteLine("blame login                         Login to GitBlame.\n");
        Console.WriteLine("blame newBlame <path> <message>     Blame a user on a GitHub repository.\n");
        Console.WriteLine("blame myBlames                      View blames where you were the author.\n");
        Console.WriteLine("blame openBlames                    View open blames that you initiated.\n");
        Console.WriteLine("blame begoneBlame <blameID>         Mark a blame as resolved.\n");
    }

    static async Task Login(HttpClient client)
    {
        var deviceCodeResponse = await GetDeviceCode(baseUrl, client);
        string deviceCode = deviceCodeResponse["device_code"];
        string userCode = deviceCodeResponse["user_code"];

        // Display user instructions
        Console.Write("\nPlease visit ");

        Console.ForegroundColor = ConsoleColor.Blue;

        Console.Write(gitHubLoginUrl);

        Console.ResetColor();

        Console.WriteLine($" and enter the user code: {userCode}");
        Console.WriteLine("Press Enter after you have authorized the application.");
        Console.ReadLine();

        //Exchange Device Code for Access Token
        var accessTokenResponse = await ExchangeDeviceCodeForAccessToken(baseUrl, client, deviceCode);

        if (!accessTokenResponse.ContainsKey("access_token"))
        {
            Console.WriteLine("Authorization process was not completed. Please try again.");
            await Login(client); // Restart the login process
            return;
        }
        // Extract access token
        string accessToken = accessTokenResponse["access_token"];

        //Use Access Token to obtain JWT
        var jwtResponse = await GetJwtFromBackend(baseUrl, client, accessToken);
        jwt = jwtResponse;
        Console.WriteLine("You have successfully logged in to Git Blame!");

    }


    static async Task<Dictionary<string, string>> GetDeviceCode(string baseUrl, HttpClient client)
    {
        var clientId = "Iv1.8586ee55069c437b";
        var response = await client.PostAsync("https://github.com/login/device/code", new StringContent($"client_id={clientId}", System.Text.Encoding.UTF8, "application/x-www-form-urlencoded"));
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        return ParseResponse(responseBody);
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

    static async Task GetHelloWorld(HttpClient client)
    {
        if (!string.IsNullOrEmpty(jwt))
        {
            // Set the Authorization header with the JWT token
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

            HttpResponseMessage response = await client.GetAsync(baseUrl + "/api/hello");

            // Check if the request was successful
            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Server response: {responseBody}");
            }
            else
            {
                Console.WriteLine($"Error: {response.StatusCode}");
            }
        }
        else
        {
            Console.WriteLine("JWT token is missing. Please log in first.");
        }
    }

}
