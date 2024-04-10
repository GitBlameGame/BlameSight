using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VeryUsefulServer.Models
{
    public class NewBlameRequest
    {
        public string Path { get; set; }
        public int lineNum { get; set; }
        public int Urgency { get; set; }
        public string Comment { get; set; }
        public string branch { get; set; }

        public NewBlameRequest() { }

        public NewBlameRequest(Dictionary<string, string> parameters)
        {
            this.Path = parameters.GetValueOrDefault("Path".ToLower());
            this.lineNum = int.Parse(parameters.GetValueOrDefault("LineNum".ToLower()));
            this.Urgency = int.Parse(parameters.GetValueOrDefault("Urgency".ToLower()));
            this.Comment = parameters.GetValueOrDefault("Comment".ToLower());
            this.branch = parameters.GetValueOrDefault("Branch".ToLower());
        }
        public NewBlameRequest(string path, int lineNum, int urgency, string comment, string branch)
        {
            Path = path;
            lineNum = lineNum;
            Urgency = urgency;
            Comment = comment;
            branch = branch;
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
