namespace BlameSightBackend.Utils
{
    public static class DbUtils
    {
        public static string getConnectionString()
        {
            string dbUsername = Environment.GetEnvironmentVariable("DB_USERNAME");
            string dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD"); // Ensure this is securely handled
            string dbUrl = Environment.GetEnvironmentVariable("DB_URL");

            return $"Host={dbUrl};Port=5432;Database=blamesightdb;Username={dbUsername};Password={dbPassword}";
        }
    }
}
