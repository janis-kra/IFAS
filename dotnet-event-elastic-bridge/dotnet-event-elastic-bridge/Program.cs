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
    const int DELAY = 3000;

    static void Main(string[] args)
    {
      string stream = Environment.GetEnvironmentVariable("STREAM") ?? "mattermost";
      string group = Environment.GetEnvironmentVariable("GROUP") ?? "analytics";
      var elasticsearchName = Environment.GetEnvironmentVariable("ELASTIC_NAME") ?? "localhost";
      var eventstoreName = Environment.GetEnvironmentVariable("EVENTSTORE_NAME") ?? "localhost";

      var elasticAddress = "http://" + elasticsearchName + ":9200/" + stream + "/data/?pretty";
      var nestNode = new Uri(elasticAddress);
      var nestSettings = new Nest.ConnectionSettings(nestNode);
      nestSettings.DefaultIndex(stream + "-test");
      var nestClient = new ElasticClient(nestSettings);

      //uncommet to enable verbose logging in client.
      var settings = EventStore.ClientAPI.ConnectionSettings.Create();//.EnableVerboseLogging().UseConsoleLogger();
      var evtStAddress = new IPEndPoint(Dns.GetHostAddresses(eventstoreName)[0], DEFAULTPORT);
      using (var conn = EventStoreConnection.Create(settings, evtStAddress))
      {
        conn.ConnectAsync().Wait();
        var events = new List<Event>();

        TaskCompletionSource<string> tsc = new TaskCompletionSource<string>();
        Task<string> t = tsc.Task;
        Task.Factory.StartNew(() =>
        {
          try
          {
            var sub = conn.ConnectToPersistentSubscription(stream, group, (_, x) =>
            {
                Console.WriteLine("Received: " + x.Event.EventStreamId + ":" + x.Event.EventNumber);
                var eventData = JObject.Parse(Encoding.ASCII.GetString(x.Event.Data));
                var ms = long.Parse(eventData["@timestamp"].ToString());
                var evt = new Event
                {
                    Data = Encoding.ASCII.GetString(x.Event.Data),
                    EventType = x.Event.EventType,
                    Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(ms).UtcDateTime
                };
                //var data = new Dictionary<string, object>();
                //data.Add("Data", eventData);
                //data.Add("EventType", x.Event.EventType.ToString());
                //data.Add("@timestamp", eventData["@timestamp"]);
                //var json = JsonConvert.SerializeObject(data);
                events.Add(evt);

                //if (timer == null)
                //{
                //    timer = new Timer((_x) =>
                //    {
                var response = nestClient.IndexMany(events);
                events.Clear();
                //        timer.Dispose();
                //        timer = null;
                //    },
                //    null, DELAY, 0);
                //}
            });

            Console.WriteLine($"waiting for events on {evtStAddress} to post to {elasticAddress}.");
          }
          catch (Exception e)
          {
            tsc.SetResult(e.Message);
          }
        });

        Console.WriteLine("Result: " + t.Result);
      }
    }
  }
}
