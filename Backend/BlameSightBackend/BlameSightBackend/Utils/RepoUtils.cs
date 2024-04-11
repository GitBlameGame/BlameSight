namespace BlameSightBackend.Utils
{
    public static class RepoUtils
    {
    

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
