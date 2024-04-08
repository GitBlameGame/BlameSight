using DiscordBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Services
{
    public class CommandHandler
    {

        Dictionary<string, string>? commands;
        public CommandHandler() { }

        public void Handle(Message message)
        {
            string[] inputParts = message.d!.content.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (inputParts.Length == 1) 
            { 
                Console.WriteLine("Enter a non-empty command. Type 'blame help' for a list of commands"); 
            }
            Console.WriteLine("First word after /blame : " + inputParts[1]);

        }
    }
}
