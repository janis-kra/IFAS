using System;
namespace dotneteventelasticbridge
{
  public class PostCreatedEvent: Event
  {
    public new PostCreatedEventData Data { get; set; }

    public class PostCreatedEventData
    {
      public Msg msg { get; set; }
      public Post post { get; set; }
    }

    public class Msg
    {
      public MsgData data { get; set; }
      [Newtonsoft.Json.JsonProperty("event")]
      public string _event { get; set; }
      public int seq { get; set; }

    }

    public class MsgData
    {
      public string channel_display_name { get; set; }
      public string channel_name { get; set; }
      public string channel_type { get; set; }
      public object post { get; set; }
      public string sender_name { get; set; }
      public string team_id { get; set; } 
    }

    public class Post
    {
      public string channel_id { get; set; }
      public long create_at { get; set; }
      public long delete_at { get; set; }
      public long edit_at { get; set; }
      public object hashtags { get; set; }
      public string id { get; set; }
      public bool is_pinned { get; set; }
      public string message { get; set; }
      public string original_id { get; set; }
      public string parent_id { get; set; }
      public string pending_post_id { get; set; }
      public string root_id { get; set; }
      public string type { get; set; }
      public long update_at { get; set; }
      public string user_id { get; set; }
    }
  }
}
