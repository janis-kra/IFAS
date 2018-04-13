using System;
namespace dotneteventelasticbridge
{
  public class ExperimentParticipatedEvent: Event
  {
    public new TutorialEventData Data { get; set; }

    public class TutorialEventData: EventData
    {
      public string group { get; set; }
      public string decision { get; set; }
    }
  }
}
