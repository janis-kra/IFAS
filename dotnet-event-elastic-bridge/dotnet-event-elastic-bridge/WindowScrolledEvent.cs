using System;
namespace dotneteventelasticbridge
{
  public class WindowScrolledEvent: Event
  {
    public new WindowScrolledEventData Data { get; set; }

    public class WindowScrolledEventData: EventData
    {
      public long delta { get; set; }
      public new WithId user { get; set; }
      public string owner { get; set; }
      public long duration { get; set; }
    }
  }
}
