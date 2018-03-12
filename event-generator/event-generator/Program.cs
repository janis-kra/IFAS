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
            //uncommet to enable verbose logging in client.
            var settings = ConnectionSettings.Create();//.EnableVerboseLogging().UseConsoleLogger();
            var evtStAddress = new IPEndPoint(Dns.GetHostAddresses(eventstoreName)[0], DEFAULTPORT);

            var start = DateTime.Now;

            using (var conn = EventStoreConnection.Create(settings, evtStAddress))
            {
                Console.WriteLine("Connected to Event Store");
                conn.ConnectAsync().Wait();
                CreateSubscription(conn, stream, "performance");

                EventStoreTransaction transaction = conn.StartTransactionAsync(stream, ExpectedVersion.Any).Result;
                var eventData = new EventData[TRANSACTION_SIZE];
                var rnd = new Random();

                for (var i = 0; i < size; i++) {
                    if (i != 0 && i % TRANSACTION_SIZE == 0) {
                        transaction.WriteAsync(eventData).Wait();
                        double progress = i * 100 / size;
                        Console.WriteLine($"{progress}%");
                    }
                    var now = DateTime.Now.ToUniversalTime();
                    var obj = new {
                        click = new {
                            x = (rnd.NextDouble() * 1920),
                            y = (rnd.NextDouble() * 1080)
                        },
                        screen = new {
                            width = 1920,
                            height = 1080
                        },
                        timestamp = now
                    };
                    string json = JsonConvert.SerializeObject(obj);
                    var evt = new EventData(Guid.NewGuid(), "UserClicked", true,
                            Encoding.UTF8.GetBytes(json),
                            Encoding.UTF8.GetBytes("some metadata"));

                    eventData[i % TRANSACTION_SIZE] = evt;
                }
                transaction.WriteAsync(eventData).Wait();
                transaction.CommitAsync().Wait();
                Console.WriteLine("Done");
                var end = DateTime.Now;
                Console.WriteLine($"Took {end - start}");
            }
        }

    private static void CreateSubscription(IEventStoreConnection conn, string stream, string group)
        {
            var credentials = new UserCredentials("admin", "changeit");
            PersistentSubscriptionSettings settings = PersistentSubscriptionSettings.Create()
                .DoNotResolveLinkTos()
                .StartFromCurrent(); // strat from current, such that existing events do not mess with the result of the performance test

            try
            {
                conn.DeletePersistentSubscriptionAsync(stream, group, credentials);
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
