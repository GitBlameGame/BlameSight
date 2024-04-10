using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VeryUsefulServer.Models
{
    public class Message
    {
        public int op { get; set; }
        public string? t { get; set; }
        public int? s { get; set; }

        public MessageData? d { get; set; }
    }
}
