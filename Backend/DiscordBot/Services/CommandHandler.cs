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

        


        public Dictionary<string, string> ExtractParameterBody(List<string> keys, string body)
        {
            /*
             * Regex to find parameters in a command according to the following rules:
                - Parameter names will start with a hyphen, followed by the name eg: -path
                - String parameter bodies will be enclosed by "
                - Int parameter bodies do not need to be enclosed 
             * 
             */
            string pattern = @"-(\w+)(?:\s*?+""([^""]+)"" | (\s*?[0-9]+) )?";
            string pattern2 = @"-(\w+)\s*(""(.*?)""|(\d+))";

            MatchCollection matches = Regex.Matches(body, pattern2);
            Dictionary<string, string> result = new();
            foreach (Match match in matches)
            {
                string parameter = match.Groups[1].Value;
                string value = match.Groups[2].Value.Replace("\"",  "");
                Console.WriteLine($"Parameter: {parameter}, Body: {body}");
                result.Add( parameter.ToLower(), value );
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
