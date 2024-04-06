using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

/*

The basic building blocks for a graphql query are fields.

To create a query with this implementation you'll need to do the following:

Create your GraphQL fields you would like to query, this includes nested ones (via the GraphObject):
- eg Simple single field:
      GraphField simple = new("firstname");
- eg Attributed field (filtered):
      Attributes attr = new();
      attr.add("filtername", "filtervalue");
      GraphField example = new("fieldname", attributes: attr)
-eg Nested field:
      GraphField nestedField = new("firstname");
      GraphObject nestedObject = new([nestedField]);
      Attributes attr = new();
      attr.add("filtername", "filtervalue");
      GraphField example = new("fieldname", attributes: attr, graphObject: nestedObject)


Then simply create a GraphQuery class and add your fields to it:
      GraphQuery query = new();
      query.addField(example); // using the example field from above

Now when the ToString method is called on the GraphQuery class you'll be able to pass it to the GraphQLClient.SendQueryAsync function.
*/

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
}
