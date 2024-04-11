class UserInterface
{
    private BlameApiClient apiClient;

    public UserInterface()
    {
        apiClient = new BlameApiClient();
    }

    public async Task RunAsync()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        await DisplayGreetingAsync(apiClient);
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
                        await apiClient.Login();
                        break;
                    case "help":
                        DisplayBlameHelp();
                        break;
                    case "new":
                        await apiClient.NewBlame();
                        break;
                    case "byme":
                        await apiClient.getMyCreatedBlames();
                        break;
                    case "atme":
                        await apiClient.getMyOpenBlames();
                        break;
                    case "begone":
                        await apiClient.SetBlameComplete();
                        break;
                    case "shame":
                        await apiClient.GetBlameShame();
                        break;
                    case "hello":
                        await apiClient.GetHelloWorld();
                        break;
                    default:
                        Console.WriteLine("Invalid command. Please try again.");
                        break;
                }
            }
            else if (firstWord == "clear")
            {
                Console.Clear();
                await DisplayGreetingAsync(apiClient);
            }
            else
            {
                Console.WriteLine("Command must start with 'Blame'. Please try again.");
            }
        }
    }
    static async Task DisplayGreetingAsync(BlameApiClient apiClient)
    {
        Console.Write("Welcome to BlameSight!\n");
        Console.WriteLine("For more information, type 'blame help'.\n");

        Console.WriteLine("🔥 This Week's Board of Shame 🔥\n");

        await apiClient.GetBlameShame();
    }
    static void DisplayBlameHelp()
    {
        Console.WriteLine();
        Console.WriteLine("╔══════════════════════════════ Help Screen ══════════════════════════════╗");
        Console.WriteLine("║                                                                         ║");
        Console.WriteLine("║ blame help            Display this help screen.                         ║");
        Console.WriteLine("║ blame login           Login to BlameSight.                              ║");
        Console.WriteLine("║ blame new             Blame a user on a GitHub repository.              ║");
        Console.WriteLine("║ blame byMe            View blames where you were the author.            ║");
        Console.WriteLine("║ blame atMe            View open blames that you initiated.              ║");
        Console.WriteLine("║ blame begone          Mark a blame as resolved.                         ║");
        Console.WriteLine("║ blame shame           View this week's board of shame.                  ║");
        Console.WriteLine("║ clear                 Clear the terminal                                ║");
        Console.WriteLine("║                                                                         ║");
        Console.WriteLine("║Commands are case insensitive.                                           ║");
        Console.WriteLine("╚═════════════════════════════════════════════════════════════════════════╝");
    }
}