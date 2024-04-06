using System.Text.Json;

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