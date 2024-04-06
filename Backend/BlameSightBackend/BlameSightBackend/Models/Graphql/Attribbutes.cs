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
