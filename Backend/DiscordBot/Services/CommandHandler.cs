using DiscordBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DiscordBot.Services
{
    public class CommandHandler
    {

        public List<string> splitCommand(string command)
        {
            return command.Split("-").ToList();
        }


        public Dictionary<string, string> ExtractParameterBody(List<string> keys, List<string> arguments)
        {
            Dictionary<string, string> result = new();
            foreach (string key in keys)
            {
                string pattern = $"^{key.ToUpper()}";
                foreach (var item in arguments)
                {
                    if (Regex.IsMatch(item.ToUpper(), pattern))
                    {
                        Console.WriteLine(item.Remove(0, key.Length).Trim());
                        result.Add(key, item.Remove(0, key.Length).Trim());
                    }
                }

            }
            return result;
        }

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
