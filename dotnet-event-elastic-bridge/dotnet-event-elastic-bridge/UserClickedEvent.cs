using System;
namespace dotneteventelasticbridge
{
  public class UserClickedEvent
  {
    public string EventType { get; set; }
    public UserClickedEventData Data { get; set; }
    public DateTime Timestamp { get; set; }

    public class UserClickedEventData
    {
      public Click click { get; set; }
      public Screen screen { get; set; }
      public string owner { get; set; }
      public string timestamp { get; set; }
      public User user { get; set; }
    }

    public class User
    {
      public string id { get; set; }
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
      public string id { get; set; }
      [Newtonsoft.Json.JsonProperty("class")]
      public string Class { get; set; }
      public string name { get; set; }
      public string text { get; set; }
    }
  }
}
