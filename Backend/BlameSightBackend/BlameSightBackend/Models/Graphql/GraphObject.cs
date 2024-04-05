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