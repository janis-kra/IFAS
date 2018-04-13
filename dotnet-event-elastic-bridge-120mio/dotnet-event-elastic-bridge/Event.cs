using System;
using System.Dynamic;

namespace dotneteventelasticbridge
{
  public class Event
    {
        public string EventType { get; set; }
        public EventData Data { get; set; }
        /** The timestamp as calculated by this program, NOT the one that comes from the client side */
        public DateTime Timestamp { get; set; }
    }
}
