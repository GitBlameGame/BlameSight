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

        public static string GetGitHubPathWithoutBranch(string url)
        {
            string gitHubDomain = "https://github.com/";
            string blobPart = "/blob/";

            if (url.StartsWith(gitHubDomain))
            {
                url = url.Substring(gitHubDomain.Length);
            }

            int blobIndex = url.IndexOf(blobPart);
            if (blobIndex != -1)
            {
                int nextSlashIndex = url.IndexOf('/', blobIndex + blobPart.Length);
                url = url.Substring(0, blobIndex) + url.Substring(nextSlashIndex);
            }
            url = url.Replace("%20", " ");
            return url;
        }

    }
}
