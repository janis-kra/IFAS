using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace event_generator
{
  class Program
  {
    const int DEFAULTPORT = 1113;
    const int TRANSACTION_SIZE = 50;

    static void Main(string[] args)
    {
      var size = Int32.Parse(Environment.GetEnvironmentVariable("EXPERIMENT_SIZE") ?? "150");
      string stream = $"performance-{size}";
      var eventstoreName = Environment.GetEnvironmentVariable("EVENTSTORE_NAME") ?? "localhost";
      var liveBufferSize = Int32.Parse(Environment.GetEnvironmentVariable("LIVE_BUFFER_SIZE") ?? "500");
      var bufferSize = Int32.Parse(Environment.GetEnvironmentVariable("BUFFER_SIZE") ?? "500");
      var readBatch = Int32.Parse(Environment.GetEnvironmentVariable("READ_BATCH") ?? "40");
      string pw = Environment.GetEnvironmentVariable("PW") ?? "changeit";
      //uncommet to enable verbose logging in client.
      var settings = ConnectionSettings.Create();//.EnableVerboseLogging().UseConsoleLogger();
      var evtStAddress = new IPEndPoint(Dns.GetHostAddresses(eventstoreName)[0], DEFAULTPORT);

        Thread.Sleep(15 * 1000);

      var start = DateTime.Now;

      using (var conn = EventStoreConnection.Create(settings, evtStAddress))
      {
        Console.WriteLine("Connected to Event Store");
        conn.ConnectAsync().Wait();
        CreateSubscription(conn, stream, "performance", bufferSize, liveBufferSize, readBatch, pw);

        EventStoreTransaction transaction = conn.StartTransactionAsync(stream, ExpectedVersion.Any).Result;
        var eventData = new EventData[size < TRANSACTION_SIZE ? size : TRANSACTION_SIZE];
        var rnd = new Random();
        var now = DateTime.Now;

        for (var i = 0; i < size; i++)
        {
          if (i != 0 && i % TRANSACTION_SIZE == 0)
          {
            transaction.WriteAsync(eventData).Wait();
            double progress = i * 100 / size;
            Console.WriteLine($"{progress}%");
          }
          var eventType = "TutorialExperimentParticipated";
          var obj = new
          {
            group = rnd.NextDouble() < 0.5 ? "control" : "treatment",
            decision = rnd.NextDouble() < 0.5 ? "TutorialSkipped" : "TutorialStarted",
            timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
            user = new { id = "0" }
          };
          string json = JsonConvert.SerializeObject(obj);
          var evt = new EventData(Guid.NewGuid(), eventType, true,
                  Encoding.UTF8.GetBytes(json),
                  null);

          eventData[i % TRANSACTION_SIZE] = evt;
        }
        transaction.WriteAsync(eventData).Wait();
        transaction.CommitAsync().Wait();
        Console.WriteLine("Done");
        var end = DateTime.Now;
        Console.WriteLine($"Took {end - start}");
      }
    }

    private static void CreateSubscription(
        IEventStoreConnection conn,
        string stream,
        string group,
        int buffer,
        int liveBuffer,
        int read,
        string pw
    )
    {
      var credentials = new UserCredentials("admin", pw);
      PersistentSubscriptionSettings settings = PersistentSubscriptionSettings.Create()
          .DoNotResolveLinkTos()
          .WithBufferSizeOf(buffer)
          .WithLiveBufferSizeOf(liveBuffer)
          .WithReadBatchOf(read)
          .StartFromCurrent(); // strat from current, such that existing events do not mess with the result of the performance test

      try
      {
        conn.DeletePersistentSubscriptionAsync(stream, group, credentials).Wait();
      }
      catch (Exception)
      {
        // just assume the subscriptions did not exist yet, move on
      }

      try
      {
        conn.CreatePersistentSubscriptionAsync(stream, group, settings, credentials).Wait();
        Console.WriteLine($"Created persistent subscription {stream} of group {group}");
      }
      catch (AggregateException ex)
      {
        if (ex.InnerException.GetType() != typeof(InvalidOperationException)
            && ex.InnerException?.Message != $"Subscription group {group} on stream {stream} already exists")
        {
          throw;
        }
      }
    }
  }
}
