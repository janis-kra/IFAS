using System;
namespace dotneteventelasticbridge
{
  public class UserClickedEvent: Event
  {
    public new UserClickedEventData Data { get; set; }

    public class UserClickedEventData: EventData
    {
      public Click click { get; set; }
      public Screen screen { get; set; }
      public string owner { get; set; }
    }

    public class Click
    {
      public double x { get; set; }
      public double y { get; set; }
      public Target target { get; set; }
    }

    public class Screen
    {
      public int height { get; set; }
      public int width { get; set; }
    }

    public class Target
    {
      public string name { get; set; }
      public string text { get; set; }
    }
  }
}
