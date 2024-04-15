using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrontEndShared.Models
{

    public class DiscordMessage
    {
        public string content { get; set; }
        public string channel { get; set; }

        public DiscordMessage(string content, string channel)
        {
            this.content = content;
            this.channel = channel;
        }
    }
    
}
