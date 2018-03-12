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

    static void Main(string[] args)
    {
      var eventsLock = new Object();

      var stream = Environment.GetEnvironmentVariable("STREAM") ?? "mattermost";
      var group = Environment.GetEnvironmentVariable("GROUP") ?? "analytics";
      var elasticsearchName = Environment.GetEnvironmentVariable("ELASTIC_NAME") ?? "localhost";
      var eventstoreName = Environment.GetEnvironmentVariable("EVENTSTORE_NAME") ?? "localhost";
      var elasticsearchIndex = Environment.GetEnvironmentVariable("ES_INDEX") ?? stream;
      var delay = Int32.Parse(Environment.GetEnvironmentVariable("DELAY") ?? "1000");
      var expectedAmount = Int32.Parse(Environment.GetEnvironmentVariable("EXPECTED_AMOUNT") ?? "150");
      var totalAmount = 0;

      var elasticAddress = "http://" + elasticsearchName + ":9200/";
      var nestNode = new Uri(elasticAddress);
      var nestSettings = new Nest.ConnectionSettings(nestNode);
      nestSettings.DefaultIndex(elasticsearchIndex);
      var nestClient = new ElasticClient(nestSettings);
      var nestLowLevelClient = new ElasticLowLevelClient(nestSettings);

      //uncommet to enable verbose logging in client.
      var settings = EventStore.ClientAPI.ConnectionSettings.Create();//.EnableVerboseLogging().UseConsoleLogger();
      var evtStAddress = new IPEndPoint(Dns.GetHostAddresses(eventstoreName)[0], DEFAULTPORT);

      var start = DateTime.Now;

      using (var conn = EventStoreConnection.Create(settings, evtStAddress))
      {
        conn.ConnectAsync().Wait();
        Timer timer = null;
        var events = new List<UserClickedEvent>();

        TaskCompletionSource<string> tsc = new TaskCompletionSource<string>();
        Task<string> t = tsc.Task;
        Task.Factory.StartNew(() =>
        {
          try
          {
            var sub = conn.ConnectToPersistentSubscription(stream, group, (_, x) =>
            {
              // Console.WriteLine("Received: " + x.Event.EventStreamId + ":" + x.Event.EventNumber);
              var json = Encoding.ASCII.GetString(x.Event.Data);
              var data = JsonConvert.DeserializeObject<UserClickedEvent.UserClickedEventData>(json);
              // var eventData = JObject.Parse(json);
              // var converter = new ExpandoObjectConverter();
              // dynamic obj = JsonConvert.DeserializeObject<ExpandoObject>(json, converter);
              DateTimeOffset ts;
              if (data.timestamp == null)
              {
                ts = new DateTimeOffset();
              }
              else
              {
                try
                {
                  ts = DateTimeOffset.Parse(data.timestamp);
                }
                catch (Exception)
                {
                  var ms = long.Parse(data.timestamp);
                  ts = DateTimeOffset.FromUnixTimeMilliseconds(ms);
                }
              }
              var evt = new UserClickedEvent
              {
                Data = JsonConvert.DeserializeObject<UserClickedEvent.UserClickedEventData>(json),
                EventType = x.Event.EventType,
                Timestamp = ts.UtcDateTime
              };
              lock (eventsLock)
              {
                events.Add(evt);
              }
              //var data = new Dictionary<string, object>();
              //data.Add("Data", eventData);
              //data.Add("EventType", x.Event.EventType.ToString());
              //data.Add("@timestamp", eventData["@timestamp"]);
              //var json = JsonConvert.SerializeObject(data);
              // var indexAction = new { 
              //   index = new { _index = elasticsearchIndex, _type = "data" } 
              // };
              // events.Add(indexAction);
              // events.Add(evt);



              if (timer == null)
              {
                timer = new Timer((_x) =>
                {
                  // var response = nestLowLevelClient.Bulk<StringResponse>(PostData.MultiJson(events));
                  List<UserClickedEvent> send;
                  int amount;
                  lock (eventsLock)
                  {
                    send = events;
                    events = new List<UserClickedEvent>();
                    amount = send.Count;
                    totalAmount += amount;
                  }
                  var response = nestClient.IndexMany(send);
                  if (response.IsValid)
                  // if (response.Success)
                  {
                    Console.WriteLine($"Successfully sent {amount} events to elasticsearch");
                  }
                  else 
                  {
                    Console.WriteLine($"Error while sending {amount} events to elasticsearch");
                  }
                  timer.Dispose();
                  timer = null;
                  
                  if (totalAmount >= expectedAmount)
                  {
                    tsc.SetResult($"Finished reading {expectedAmount} events (read {totalAmount})");
                  }
                },
                null, delay, 0);
              }
            });

            Console.WriteLine($"waiting for events on {evtStAddress} to post to {elasticAddress}.");
          }
          catch (Exception e)
          {
            tsc.SetResult(e.Message);
          }
        });

        Console.WriteLine("Result: " + t.Result);
        var end = DateTime.Now;
        Console.WriteLine($"Took {end - start}");
      }
    }
  }
}
