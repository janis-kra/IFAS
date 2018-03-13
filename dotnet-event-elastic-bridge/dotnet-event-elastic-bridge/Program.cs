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

      var semaphore = new SemaphoreSlim(1, 1);

      var stream = Environment.GetEnvironmentVariable("STREAM") ?? "mattermost";
      var group = Environment.GetEnvironmentVariable("GROUP") ?? "analytics";
      var elasticsearchName = Environment.GetEnvironmentVariable("ELASTIC_NAME") ?? "localhost";
      var eventstoreName = Environment.GetEnvironmentVariable("EVENTSTORE_NAME") ?? "localhost";
      var elasticsearchIndex = Environment.GetEnvironmentVariable("ES_INDEX") ?? stream;
      var delay = Int32.Parse(Environment.GetEnvironmentVariable("DELAY") ?? "1000");
      var bufferSize = Int32.Parse(Environment.GetEnvironmentVariable("BUFFER_SIZE") ?? "10");
      expectedAmount = Int32.Parse(Environment.GetEnvironmentVariable("EXPECTED_AMOUNT") ?? "150");

      var elasticAddress = "http://" + elasticsearchName + ":9200/";
      var nestNode = new Uri(elasticAddress);
      var nestSettings = new Nest.ConnectionSettings(nestNode);
      nestSettings.DefaultIndex(elasticsearchIndex);
      var nestClient = new ElasticClient(nestSettings);
      var nestLowLevelClient = new ElasticLowLevelClient(nestSettings);

      //uncommet to enable verbose logging in client.
      var settings = EventStore.ClientAPI.ConnectionSettings.Create();//.EnableVerboseLogging().UseConsoleLogger();
      var evtStAddress = new IPEndPoint(Dns.GetHostAddresses(eventstoreName)[0], DEFAULTPORT);

      var start = ToTimestamp(DateTime.UtcNow);

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
              // Console.WriteLine($"Received event {x.OriginalEventNumber}");
              var evt = CreateEvent(x);
              semaphore.Wait();
              try
              {
                events.Add(evt);
                // Console.WriteLine($"Enqueued event {x.OriginalEventNumber}");
                if (!timerStarted)
                {
                  timerStarted = true;
                  System.Threading.Timer timer = null;
                  timer = new System.Threading.Timer((obj) =>
                    {
                      ReadOnlyCollection<Event> sendEvents = null;
                      semaphore.Wait();
                      try
                      {
                        timerStarted = false;
                        sendEvents = events.AsReadOnly();
                      }
                      finally
                      {
                        semaphore.Release();
                      }
                      events = new List<Event>();
                      WriteResults(semaphore, sendEvents, nestClient, tsc, delay, start);
                      timer.Dispose();
                    },
                    null, delay, System.Threading.Timeout.Infinite);
                }
              }
              finally
              {
                semaphore.Release();
              }
            }, bufferSize: bufferSize);

            Console.WriteLine($";;;waiting for events on {evtStAddress} to post to {elasticAddress}.");
          }
          catch (Exception e)
          {
            tsc.SetResult(e.Message);
          }
        });

        Console.WriteLine(";;;Result: " + t.Result);
        var end = ToTimestamp(DateTime.UtcNow);
        Console.WriteLine($";;;Took {end - start}ms");
      }
    }

    private static async void WriteResults(
      SemaphoreSlim semaphore,
      ReadOnlyCollection<Event> events,
      ElasticClient nestClient,
      TaskCompletionSource<string> tsc,
      int delay,
      double start
      )
    {
      await Task.Delay(delay);
      int amount;

      await semaphore.WaitAsync();
      // Console.WriteLine($"Sending {events.Count} events");
      try
      {
        amount = events.Count;
        totalAmount += amount;
        var now = ToTimestamp(DateTime.UtcNow);
        Console.WriteLine($"{amount}; {totalAmount}; {now};");
      }
      finally
      {
        semaphore.Release();
      }
      var response = nestClient.IndexMany(events, type: "_doc"); // type is always doc
      if (response.IsValid)
      {
      }
      else
      {
        Console.WriteLine($";;;Error while sending {amount} events to elasticsearch");
        throw new Exception(response.ServerError.ToString());
      }

      if (totalAmount >= expectedAmount)
      {
        tsc.SetResult($"Finished reading {expectedAmount} events (read {totalAmount})");
      }
    }

    private static UserClickedEvent CreateEvent(ResolvedEvent evt)
    {
      var json = Encoding.ASCII.GetString(evt.Event.Data);
      var data = JsonConvert.DeserializeObject<UserClickedEvent.UserClickedEventData>(json);
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
      return new UserClickedEvent
      {
        Data = JsonConvert.DeserializeObject<UserClickedEvent.UserClickedEventData>(json),
        EventType = evt.Event.EventType,
        Timestamp = ts.UtcDateTime
      };
    }

    private static double ToTimestamp (DateTime time) {
      return (time.Subtract(new DateTime(1970, 1, 1))).TotalMilliseconds;
    }
  }
}
