using System;
namespace dotneteventelasticbridge
{
  public class MessageSentEvent: Event
  {
    public new MessageSentEventData Data { get; set; }

    public class MessageSentEventData: EventData
    {
      public string message { get; set; }
      public string sentVia { get; set; }
      public new WithId user { get; set; }
    }
  }
}
