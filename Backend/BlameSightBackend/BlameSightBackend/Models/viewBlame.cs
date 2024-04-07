namespace BlameSightBackend.Models
{
    public class viewBlame
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string Comment { get; set; }
        public string UrgencyDescriptor { get; set; }
        public int LineNum { get; set; } = 1;
        public bool blameViewed { get; set; }
        public bool blameComplete {  get; set; }
    }
}
