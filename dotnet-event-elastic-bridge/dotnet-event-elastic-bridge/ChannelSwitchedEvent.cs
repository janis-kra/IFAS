using System;
namespace dotneteventelasticbridge
{
  public class ChannelSwitchedEvent: Event
  {
    public new ChannelSwitchedEventData Data { get; set; }

    public class ChannelSwitchedEventData: EventData
    {
      public string via { get; set; }
      public new WithId user { get; set; }
      public WithId channel { get; set; }
    }
  }
}
