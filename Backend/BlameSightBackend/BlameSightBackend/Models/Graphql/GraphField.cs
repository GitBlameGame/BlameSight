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