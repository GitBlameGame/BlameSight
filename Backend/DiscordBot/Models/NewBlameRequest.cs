using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Models
{
    public class NewBlameRequest
    {
        string? Path { get; set; }
        int? LineNum { get; set; }
        int? Urgency { get; set; }
        string? Comment { get; set; }
        string? Branch { get; set; }

        public NewBlameRequest() { }

        public NewBlameRequest(Dictionary<string, string> parameters)
        {
            this.Path = parameters.GetValueOrDefault("Path");
            this.LineNum = int.Parse(parameters.GetValueOrDefault("LineNum"));
            this.Urgency = int.Parse(parameters.GetValueOrDefault("Urgency"));
            this.Comment = parameters.GetValueOrDefault("Comment");
            this.Branch = parameters.GetValueOrDefault("Branch");
        }
        public NewBlameRequest(string path, int lineNum, int urgency, string comment, string branch)
        {
            Path = path;
            LineNum = lineNum;
            Urgency = urgency;
            Comment = comment;
            Branch = branch;
        }

        public List<string> getKeys()
        {
            var keys = new List<string>();
            keys.Add("Path");
            keys.Add("LineNum");
            keys.Add("Urgency");
            keys.Add("Comment");
            keys.Add("Branch");

            return keys;
        }
    }
}
