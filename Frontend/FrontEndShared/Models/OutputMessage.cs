using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrontEndShared.Models
{
    public class OutputMessage
    {
        public OutputType Type { get; set;}
        public DiscordMessage? DiscordMessage { get; set; }
        public string? CLIMessage { get; set; }

        public OutputMessage(DiscordMessage? discordMessage)
        {
            Type = OutputType.Discord;
            this.DiscordMessage = discordMessage;
        }

        public OutputMessage(string msg)
        {
            Type = OutputType.CLI;
           this. CLIMessage = msg;
        }

        
    }

}
