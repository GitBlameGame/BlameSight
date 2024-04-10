using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VeryUsefulServer.Models
{

    public class DiscordMessage
    {
        public string content { get; set; }

        public DiscordMessage(string content)
        {
            this.content = content;
        }
    }
    
}
