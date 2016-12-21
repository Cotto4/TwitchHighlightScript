using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchHighlightBot
{
    class MessageMeta
    {
        public string type { get; set; }
        public string id { get; set; }
        public MessageDetails attributes { get; set; }
    }
}
