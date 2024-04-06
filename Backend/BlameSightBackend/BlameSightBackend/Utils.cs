using Microsoft.AspNetCore.Mvc;

namespace BlameSightBackend
{
    public static class Utils
    {
        public static string getConnectionString()
        {
            string dbUsername = Environment.GetEnvironmentVariable("DB_USERNAME");
            string dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD"); // Ensure this is securely handled
            string dbUrl = Environment.GetEnvironmentVariable("DB_URL");

            // Parse the DB_URL to extract the database address and name


            return $"Host={dbUrl};Port=5432;Database=blamesightdb;Username={dbUsername};Password={dbPassword}";
        }

        public static (string repositoryOwner, string repositoryName, string filePath) ParsePath(string path)
        {
            var segments = path.TrimStart('/').Split('/');
            var repositoryOwner = segments.Length > 0 ? segments[0] : string.Empty;
            var repositoryName = segments.Length > 1 ? segments[1] : string.Empty;
            var filePath = segments.Length > 2 ? string.Join("/", segments.Skip(2)) : string.Empty;

            return (repositoryOwner, repositoryName, filePath);
        }
     
    }
}
