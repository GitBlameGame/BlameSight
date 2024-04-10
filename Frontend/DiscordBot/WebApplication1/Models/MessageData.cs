using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VeryUsefulServer.Models
{
    

    public class MessageData
    {
        public int type { get; set; }
        public bool tts { get; set; }
        public DateTime timestamp { get; set; }
        public string id { get; set; }
        public int flags { get; set; }
        public object edited_timestamp { get; set; }
        public string content { get; set; }
        public Author author { get; set; }
        public string channel_id { get; set; }
        public int? heartbeat_interval { get; set; }

    }
}
