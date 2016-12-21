using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchHighlightBot
{
    class MessageDetails
    {
        public string command { get; set; }
        public string room { get; set; }
        public long timestamp { get; set; }
        public bool deleted { get; set; }
        public string message { get; set; }
        public string from { get; set; }


    }
}
