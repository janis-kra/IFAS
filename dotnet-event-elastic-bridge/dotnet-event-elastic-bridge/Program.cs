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

namespace dotnet_event_elastic_bridge
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
    const string GROUP = "analytics";
    const int DEFAULTPORT = 1113;

    static void Main(string[] args)
    {

      string stream = Environment.GetEnvironmentVariable("INDEX") ?? "analytics";
      var elasticsearchName = Environment.GetEnvironmentVariable("ELASTIC_NAME") ?? "localhost";
      var eventstoreName = Environment.GetEnvironmentVariable("EVENTSTORE_NAME") ?? "localhost";
      var client = new HttpClient();
      var uri = "http://" + elasticsearchName + ":9200/" + stream + "/data/?pretty";
      //uncommet to enable verbose logging in client.
      var settings = ConnectionSettings.Create();//.EnableVerboseLogging().UseConsoleLogger();
      var address = new IPEndPoint(Dns.GetHostAddresses(eventstoreName)[0], DEFAULTPORT);
      using (var conn = EventStoreConnection.Create(settings, address))
      {
        conn.ConnectAsync().Wait();

        TaskCompletionSource<string> tsc = new TaskCompletionSource<string>();
        Task<string> t = tsc.Task;
        Task.Factory.StartNew(() =>
        {
          try
          {
            var sub = conn.ConnectToPersistentSubscription(stream, GROUP, (_, x) =>
            {
              var data = Encoding.ASCII.GetString(x.Event.Data);
              Console.WriteLine("Received: " + x.Event.EventStreamId + ":" + x.Event.EventNumber);
              Console.WriteLine(data);
              client.PostAsync(uri, new StringContent(data, Encoding.UTF8, "application/json"));
            });

            // does not work when Console.ReadLine() and tsc.SetResult is commented out
            Console.WriteLine("waiting for events.");
            // Console.ReadLine();
            // tsc.SetResult("ok");
          }
          catch (System.Exception e)
          {
            tsc.SetResult(e.Message);
          }
        });

        Console.WriteLine("Result: " + t.Result);
      }
    }
  }
}
