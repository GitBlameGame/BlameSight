using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VeryUsefulServer.Models
{
    public class InitialPayload
    {
        public int op { get; set; }
        public InitialData? d { get; set; }
    }

    public class InitialData
    {
        public string token { get; set; }
        public int intents { get; set; }
        public InitialProperties properties { get; set; }
    }

    public class InitialProperties
    {
        public string os { get; set; }
        public string browser { get; set; }
        public string device { get; set; }
    }
}
