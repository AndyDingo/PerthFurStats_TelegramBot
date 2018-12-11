using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nwTelegramBot
{
    public class Event
    {
        public long id { get; set; }
        public string url { get; set; }
        public string title { get; set; }
        public string location { get; set; }
        public string start { get; set; }
        public string end { get; set; }
        public long authorid { get; set; }
        public string host { get; set; }
        public string created { get; set; }
    }
}
