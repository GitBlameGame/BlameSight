namespace BlameSightBackend.Models
{
    public class newBlame
    {
        public string Path { get; set; }
        public string Comment { get; set; }
        public int Urgency { get; set; }
        public int LineNum { get; set; } = 1;
        public string Branch { get; set; } = "main";
    }
}
