class Program
{
    static async Task Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        var apiClient = new BlameApiClient();
        var ui = new UserInterface();
        await ui.RunAsync();
    }
}