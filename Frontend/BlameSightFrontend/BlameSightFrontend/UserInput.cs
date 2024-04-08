using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

class UserInput
{
    static string gitHubLoginUrl = "https://github.com/login/device";
    static string? jwt;
    static string baseUrl = "http://api-env.eba-kcb3b8tc.eu-west-1.elasticbeanstalk.com";

    static async Task Main(string[] args)
    {

        HttpClient client = new HttpClient();

        await DisplayGreetingAsync(client);
        while (true)
        {
            string? input = Console.ReadLine()?.ToLower();

            string[] inputParts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            string firstWord = inputParts.Length > 0 ? inputParts[0] : "";

            if (firstWord == "blame")
            {
                string command = inputParts.Length > 1 ? inputParts[1] : ""; // Get the second word as the command
                switch (command)
                {
                    case "login":
                        await Login(client);
                        break;
                    case "help":
                        DisplayBlameHelp();
                        break;
                    case "newblame":
                        await NewBlame(client);
                        break;
                    case "myblames":
                        await getMyCreatedBlames(client);
                        break;
                    case "openblames":
                        await getMyOpenBlames(client);
                        break;
                    case "begoneblame":
                        await SetBlameComplete(client);
                        break;
                    case "shame":
                        await GetBlameShame(client);
                        break;
                    case "hello":
                        await GetHelloWorld(client);
                        break;
                    default:
                        Console.WriteLine("Invalid command. Please try again.");
                        break;
                }
            }
            else if (firstWord == "clear")
            {
                Console.Clear();
                await DisplayGreetingAsync(client);
            }
            else
            {
                Console.WriteLine("Command must start with 'Blame'. Please try again.");
            }
        }
    }
    static async Task DisplayGreetingAsync(HttpClient client)
    {
        Console.Write("Welcome to BlameSight!\n");
        Console.WriteLine("For more information, type 'blame help'.\n");

        Console.WriteLine("🔥 This Week's Board of Shame 🔥\n");

        await GetBlameShame(client);
    }
    static void DisplayBlameHelp()
    {
        Console.WriteLine();
        Console.WriteLine("╔══════════════════════════════ Help Screen ══════════════════════════════╗");
        Console.WriteLine("║                                                                         ║");
        Console.WriteLine("║ blame help            Display this help screen.                         ║");
        Console.WriteLine("║ blame login           Login to BlameSight.                              ║");
        Console.WriteLine("║ blame newBlame        Blame a user on a GitHub repository.              ║");
        Console.WriteLine("║ blame myBlames        View blames where you were the author.            ║");
        Console.WriteLine("║ blame openBlames      View open blames that you initiated.              ║");
        Console.WriteLine("║ blame begoneBlame     Mark a blame as resolved.                         ║");
        Console.WriteLine("║ clear                 Clear the terminal                                ║");
        Console.WriteLine("║                                                                         ║");
        Console.WriteLine("║Commands are case insensitive.                                           ║");
        Console.WriteLine("╚═════════════════════════════════════════════════════════════════════════╝");
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

    static async Task StartLoader(CancellationToken cancellationToken)
    {
        Console.Write("Waiting for server response ");
        var loaderChars = new[] { '/', '-', '\\', '|' };
        var a = 0;

        Console.ForegroundColor = ConsoleColor.Blue;

        while (!cancellationToken.IsCancellationRequested)
        {
            int left = Console.CursorLeft > 0 ? Console.CursorLeft - 1 : 0;
            Console.SetCursorPosition(left, Console.CursorTop);
            Console.Write(loaderChars[a++]);
            a = a == loaderChars.Length ? 0 : a;
            await Task.Delay(300); // Use Task.Delay instead of Thread.Sleep
        }

        Console.ResetColor();
        Console.WriteLine();
    }



    static async Task NewBlame(HttpClient client)
    {
        if (!string.IsNullOrEmpty(jwt))
        {
            // Set the Authorization header with the JWT token
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

            // Prompt the user for input
            Console.Write("Enter the path (owner/repo/pathtofile): ");
            string path = Console.ReadLine();

            Console.Write("Enter the branch: ");
            string branch = Console.ReadLine();

            Console.Write("Enter the line number: ");
            int lineNumber;
            while (!int.TryParse(Console.ReadLine(), out lineNumber) || lineNumber <= 0)
            {
                Console.Write("Please enter a valid positive integer for the line number: ");
            }

            Console.Write("Enter the urgency (1 to 5): ");
            int urgency;
            while (!int.TryParse(Console.ReadLine(), out urgency) || urgency < 1 || urgency > 5)
            {
                Console.Write("Please enter a valid integer between 1 and 5 for the urgency: ");
            }

            Console.Write("Enter the comment: ");
            string comment = Console.ReadLine();

            // Prepare the request body
            var content = new
            {
                Path = path,
                LineNum = lineNumber,
                Urgency = urgency,
                Comment = comment,
                Branch = branch,
            };

            // Serialize the request body to JSON
            var jsonString = JsonSerializer.Serialize(content);
            var httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");

            // Start loader animation
            using (var cts = new CancellationTokenSource())
            {
                var loaderTask = Task.Run(() => StartLoader(cts.Token));

                HttpResponseMessage response;
                try
                {
                    response = await client.PutAsync(baseUrl + "/api/Blames/newBlame", httpContent);
                }
                finally
                {
                    // Stop loader animation
                    cts.Cancel();
                    await loaderTask;
                }

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"\nServer response: {responseBody}");
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

    static async Task getMyCreatedBlames(HttpClient client)
    {
        if (!string.IsNullOrEmpty(jwt))
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);


            using (var cts = new CancellationTokenSource())
            {
                var loaderTask = Task.Run(() => StartLoader(cts.Token));

                HttpResponseMessage response;
                try
                {
                    response = await client.GetAsync(baseUrl + "/api/Blames/myBlames");
                }
                finally
                {
                    // Stop loader animation
                    cts.Cancel();
                    await loaderTask;
                }

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    // Deserialize the JSON response into a list of Blame objects
                    var blames = JsonSerializer.Deserialize<List<Blame>>(responseBody);

                    Console.WriteLine("\nBlames:");
                    foreach (var blame in blames)
                    {
                        Console.WriteLine($"ID: {blame.id}");
                        Console.WriteLine($"Name: {blame.name}");
                        Console.WriteLine($"Path: {blame.path}");
                        Console.WriteLine($"Comment: {blame.comment}");
                        Console.WriteLine($"Urgency: {blame.urgencyDescriptor}");
                        Console.WriteLine($"Line Number: {blame.lineNum}");
                        Console.WriteLine($"Blame Viewed: {blame.blameViewed}");
                        Console.WriteLine($"Blame Complete: {blame.blameComplete}");
                        Console.WriteLine();
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

    static async Task getMyOpenBlames(HttpClient client)
    {
        if (!string.IsNullOrEmpty(jwt))
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);


            using (var cts = new CancellationTokenSource())
            {
                var loaderTask = Task.Run(() => StartLoader(cts.Token));

                HttpResponseMessage response;
                try
                {
                    response = await client.GetAsync(baseUrl + "/api/Blames/openBlames");
                }
                finally
                {
                    // Stop loader animation
                    cts.Cancel();
                    await loaderTask;
                }

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    // Deserialize the JSON response into a list of Blame objects
                    if (responseBody.Contains("You have no pending blames"))
                    {
                        Console.WriteLine("You have no pending blames. Congrats! 🎊");
                        return; // Return an empty list
                    }
                    Console.WriteLine(responseBody);

                    var blames = JsonSerializer.Deserialize<List<Blame>>(responseBody);

                    Console.WriteLine("\nBlames:");
                    foreach (var blame in blames)
                    {
                        Console.WriteLine($"ID: {blame.id}");
                        Console.WriteLine($"Name: {blame.name}");
                        Console.WriteLine($"Path: {blame.path}");
                        Console.WriteLine($"Comment: {blame.comment}");
                        Console.WriteLine($"Urgency: {blame.urgencyDescriptor}");
                        Console.WriteLine($"Line Number: {blame.lineNum}");
                        Console.WriteLine($"Blame Viewed: {blame.blameViewed}");
                        Console.WriteLine($"Blame Complete: {blame.blameComplete}");
                        Console.WriteLine();
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

    static async Task SetBlameComplete(HttpClient client)
    {
        if (!string.IsNullOrEmpty(jwt))
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

            // Prompt the user for input
            Console.Write("Enter the ID of the blame to mark as complete: ");
            int blameId;
            while (!int.TryParse(Console.ReadLine(), out blameId) || blameId <= 0)
            {
                Console.Write("Please enter a valid positive integer for the blame ID: ");
            }

            // Send the HTTP request
            HttpResponseMessage response = await client.GetAsync(baseUrl + $"/api/Blames/blameBegone/{blameId}");

            // Check if the request was successful
            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Server response: {responseBody}");
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
    static async Task GetBlameShame(HttpClient client)

    {
        try
        {
            HttpResponseMessage response = await client.GetAsync(baseUrl + "/api/Blames/blameShame");

            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();

                var blameShame = JsonSerializer.Deserialize<RankUser[]>(responseBody);

                foreach (var user in blameShame)
                {
                    Console.WriteLine($"Name: {user.name}, Blame Points: {user.blamePoints}");
                }
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine($"Error: {response.StatusCode}");
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }


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
    }
}

