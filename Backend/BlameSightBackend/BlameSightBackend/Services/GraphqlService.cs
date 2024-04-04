using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

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

public class GraphField
{
    string name;
    Attributes? attributes;
    GraphObject? graphObject;

    public GraphField(string name, Attributes? attributes = null, GraphObject? graphObject = null)
    {
        this.name = name;
        this.attributes = attributes;
        this.graphObject = graphObject;
    }

    public override string ToString()
    {
        string result = name;
        result += attributes == null ? "" : $"({attributes})";
        result += graphObject == null ? "" : $"{{ {graphObject} }}";

        return result;
    }

}

public class GraphObject
{
    List<GraphField> fields;

    public GraphObject(List<GraphField> list)
    {
        this.fields = list;
    }

    public override string ToString()
    {
        string result = string.Empty;
        foreach (GraphField field in fields)
        {
            result += $"{field} \n";
        }
        return result;
    }

}

public class GraphQuery
{
    public List<GraphField>? fields;

    public GraphQuery() { }

    public void add(GraphField field)
    {
        if (fields == null)
        {
            fields = [field];
        }
        else
        {
            fields.Add(field);
        }
    }



    public override string ToString()
    {
        string result = "";

        if (fields != null)
        {
            foreach (GraphField item in fields)
            {
                result += $"{item} \n";
            }
        }
        string json = JsonSerializer.Serialize($"{{ {result} }}");
        return $"{{ \"query\" :  {json} }}";
    }
}


public class Attributes
{
    List<KeyValuePair<string, string>>? keyValuePairs;

    public Attributes() { }
    public Attributes(List<KeyValuePair<String, String>> keyValuePairs)
    {
        this.keyValuePairs = keyValuePairs;
    }

    public void add(string filtername, string value)
    {
        KeyValuePair<string, string> temp = new(filtername, value);
        if (keyValuePairs != null)
        {
            keyValuePairs.Add(temp);
        }
        else
        {
            keyValuePairs = new List<KeyValuePair<string, string>>([temp]);
        }
    }
    public override string ToString()
    {
        string result = "";
        if (keyValuePairs != null)
        {
            foreach (KeyValuePair<string, string> item in keyValuePairs)
            {
                result += $"{item.Key}:\"{item.Value}\",";
            }
        }
        return result.Remove(result.Length - 1);
    }
}
