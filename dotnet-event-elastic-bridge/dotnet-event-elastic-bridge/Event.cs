using System;
using System.Dynamic;

namespace dotneteventelasticbridge
{
  public class Event
    {
        public string EventType { get; set; }
        public ExpandoObject Data { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
