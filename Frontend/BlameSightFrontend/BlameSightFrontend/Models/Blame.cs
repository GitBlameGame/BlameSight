public class Blame
{
    public int id { get; set; }
    public string? name { get; set; }
    public string? path { get; set; }
    public string? comment { get; set; }
    public string? urgencyDescriptor { get; set; }
    public int lineNum { get; set; }
    public bool blameViewed { get; set; }
    public bool blameComplete { get; set; }
}