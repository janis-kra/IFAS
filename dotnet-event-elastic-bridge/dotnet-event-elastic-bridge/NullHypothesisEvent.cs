using System;
namespace dotneteventelasticbridge
{
  public class NullHypothesisEvent: Event
  {
    public new NullHypothesisEventData Data { get; set; }

    public class NullHypothesisEventData: EventData
    {
      public string group { get; set; }
    }
  }
}
