namespace BlameSightBackend
{
    public class ConnectionStringBuilder
    {
        public ConnectionStringBuilder() { }
        public static string getConnectionString()
        {
            string dbUsername = Environment.GetEnvironmentVariable("DB_USERNAME");
            string dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD"); // Ensure this is securely handled
            string dbUrl = Environment.GetEnvironmentVariable("DB_URL");

            // Parse the DB_URL to extract the database address and name

           
            return $"Host={dbUrl};Port=5432;Database=blamesightdb;Username={dbUsername};Password={dbPassword}";
        }
    }
}
