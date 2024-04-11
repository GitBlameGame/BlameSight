using BlameSightBackend.Models;
using BlameSightBackend.Utils;
using System.Net.Http.Headers;

public class GraphQLClient
{
    private readonly HttpClient _httpClient;
    private readonly string _graphqlEndpoint;

    public GraphQLClient(string graphqlEndpoint, String token)
    {
        _graphqlEndpoint = graphqlEndpoint;
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", token);
        _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("AppName", "1.0"));
    }
    public GraphQLClient(String token)
    {
        _graphqlEndpoint = "https://api.github.com/graphql";
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", token);
        _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("AppName", "1.0"));
    }
    public async Task<string> SendQueryAsync(string query)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, _graphqlEndpoint);
        request.Content = new StringContent(query, System.Text.Encoding.UTF8, "application/json");
        var response = await _httpClient.SendAsync(request);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsStringAsync();
        }
        else
        {
            throw new HttpRequestException($"GraphQL request failed with status code {(int)response.StatusCode} - {response.ReasonPhrase}");
        }
    }
    public string generateBlameQL(newBlame blameInput)
    {
        var (repositoryOwner, repositoryName, filePath) = RepoUtils.ParsePath(blameInput.Path);
        var author = new GraphField("author", graphObject: new([new GraphField("name")]));
        var commit = new GraphField("commit", graphObject: new([author]));
        var ranges = new GraphField("ranges", graphObject: new([new GraphField("startingLine"), new GraphField("endingLine"), commit]));
        Attributes pathAt = new();
        pathAt.add("path", filePath);
        GraphField blame = new GraphField("blame", pathAt, graphObject: new([ranges]));
        var onCommit = new GraphField("... on Commit", graphObject: new([blame]));
        Attributes expressionAt = new();
        expressionAt.add("expression", blameInput.Branch);
        GraphField objectGf = new GraphField("object", expressionAt, graphObject: new([onCommit]));
        Attributes repoAt = new();
        repoAt.add("owner", repositoryOwner);
        repoAt.add("name", repositoryName);
        GraphField graphField = new GraphField("repository", repoAt, graphObject: new([objectGf]));
        GraphQuery query = new();
        query.add(graphField);
        return query.ToString();
    }
}
