using System;
using System.Dynamic;

namespace dotneteventelasticbridge
{
  public class EventData
    {
        public string timestamp { get; set; }
        public WithId user { get; set; }
        public string AAGroup { get; set; }
    }
}
