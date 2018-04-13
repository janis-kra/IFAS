using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nest;
using System.Threading;
using Newtonsoft.Json.Converters;
using System.Dynamic;
using Elasticsearch.Net;
using System.Collections.ObjectModel;

namespace dotneteventelasticbridge
{
  /*
  * This example sets up a volatile subscription to a test stream.
  * 
  * As written it will use the default ipaddress (loopback) and the default tcp port 1113 of the event
  * store. In order to run the application bring up the event store in another window (you can use
  * default arguments eg EventStore.ClusterNode.exe) then you can run this application with it. Once 
  * this program is running you can run the WritingEvents sample to write some events to the stream
  * and they will appear over the catch up subscription. You can also run many concurrent instances of this
  * program and each will receive the events over the subscription.
  * 
  */
  class Program
  {

    const int DEFAULTPORT = 1113;
    const int BATCH_SIZE = 100;
    static int expectedAmount = 0;
    static int totalAmount = 0;
    static bool timerStarted = false;

    static void Main(string[] args)
    {
      Console.WriteLine("amount; totalAmount; timestamp; comment");

      var elasticsearchName = Environment.GetEnvironmentVariable("ELASTIC_NAME") ?? "localhost";
      var password = Environment.GetEnvironmentVariable("PW") ?? "changeit";
      expectedAmount = 12000000;

      var elasticAddress = "http://" + elasticsearchName + ":9200/";
      var nestNode = new Uri(elasticAddress);
      var nestSettings = new Nest.ConnectionSettings(nestNode);
      nestSettings.DisableDirectStreaming();
      var nestClient = new ElasticClient(nestSettings);
      var nestLowLevelClient = new ElasticLowLevelClient(nestSettings);
      var index = "performance-120mio";
      
      var start = ToTimestamp(DateTime.UtcNow);

      var events = new List<Event>();
      var ran = new Random();

      for (var i = 0; i < expectedAmount; i++)
      {
        var now = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        var evt = new ExperimentParticipatedEvent()
        {
          Data = new ExperimentParticipatedEvent.TutorialEventData()
          {
            group = ran.NextDouble() > 0.5 ? "control" : "treatment",
            decision = ran.NextDouble() > 0.5 ? "TutorialStarted" : "TutorialSkipped",
            timestamp = now.ToString(),
            user = new WithId() { id = "1" }
          },
          EventType = "ExperimentParticipatedEvent",
          Timestamp = DateTime.Now
        };
        events.Add(evt);
        if (events.Count % 10000 == 0)
        {
          var send = events;
          events = new List<Event>();
          WriteResults(send, nestClient, index);
        }
      }
    }

    private static void WriteResults(
      List<Event> events,
      ElasticClient nestClient,
      string index
      )
    {
      var amount = events.Count;
      // Console.WriteLine($"Sending {events.Count} events");
      totalAmount += amount;
      var now = ToTimestamp(DateTime.UtcNow);
      Console.WriteLine($"{amount}; {totalAmount}; {now};");
    

        var eventType = "ExperimentParticipatedEvent";
        var response = nestClient.IndexMany(events, $"{index}_{eventType.ToLower()}");
        if (!response.IsValid)
        {
          Console.WriteLine($";;;Error while sending {amount} events to elasticsearch");
          throw new Exception(response.DebugInformation.ToString());
        }
    }

    private static double ToTimestamp (DateTime time) {
      return (time.Subtract(new DateTime(1970, 1, 1))).TotalMilliseconds;
    }
  }
}
