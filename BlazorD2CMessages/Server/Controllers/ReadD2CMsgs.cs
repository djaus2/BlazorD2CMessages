using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


using Microsoft.Azure.EventHubs;
using System.Threading;
using System.Text;
using BlazorD2CMessages.Shared;
using Newtonsoft;

using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace BlazorApp1.Data
{

    public static class ReadDeviceToCloudMessages
    {

        public static IConfiguration Configuration;
        private static bool show_system_properties = true;
        // Event Hub-compatible endpoint
        // az iot hub show --query properties.eventHubEndpoints.events.endpoint --name {your IoT Hub name}
        ////private readonly static string s_eventHubsCompatibleEndpoint = "{your Event Hubs compatible endpoint}";

        private static string s_eventHubsCompatibleEndpoint;


        // Event Hub-compatible name
        // az iot hub show --query properties.eventHubEndpoints.events.path --name {your IoT Hub name}
        //// private readonly static string s_eventHubsCompatiblePath = "{your Event Hubs compatible name}";

        private static string s_eventHubsCompatiblePath;


        // az iot hub policy show --name service --query primaryKey --hub-name {your IoT Hub name}
        //private readonly static string s_iotHubSasKey = "{your service primary key}";
        //private readonly static string s_iotHubSasKeyName = "service";

        private static string s_iotHubSasKey;
        private static string s_iotHubSasKeyName;


        private static EventHubClient s_eventHubClient;

        // Asynchronously create a PartitionReceiver for a partition and then start 
        // reading any messages sent from the simulated client.
        private static async Task ReceiveMessagesFromDeviceAsync(string partition, CancellationToken ct)
        {
            // Create the receiver using the default consumer group.
            // For the purposes of this sample, read only messages sent since 
            // the time the receiver is created. Typically, you don't want to skip any messages.
            var eventHubReceiver = s_eventHubClient.CreateReceiver("$Default", partition, EventPosition.FromEnqueuedTime(DateTime.Now));
            System.Diagnostics.Debug.WriteLine("Create receiver on partition: " + partition);
            while (true)
            {
                if (ct.IsCancellationRequested) break;
                System.Diagnostics.Debug.WriteLine("Listening for messages on: " + partition);
                // Check for EventData - this methods times out if there is nothing to retrieve.
                var events = await eventHubReceiver.ReceiveAsync(100);

                // If there is data in the batch, process it.
                if (events == null) continue;

                foreach (EventData eventData in events)
                {
                    string data = Encoding.UTF8.GetString(eventData.Body.Array);
                    System.Diagnostics.Debug.WriteLine("Message received on partition {0}:", partition);
                    System.Diagnostics.Debug.WriteLine("  {0}:", data);
                    System.Diagnostics.Debug.WriteLine("Application properties (set by device):");

                Sensor sensor = JsonConvert.DeserializeObject<Sensor>(data);
                Sensors.Enqueue(sensor);

                foreach (var prop in eventData.Properties)
                    {
                        System.Diagnostics.Debug.WriteLine("  {0}: {1}", prop.Key, prop.Value);
                    }
                    if (show_system_properties)
                    {
                        System.Diagnostics.Debug.WriteLine("System properties (set by IoT Hub):");
                        foreach (var prop in eventData.SystemProperties)
                        {
                            System.Diagnostics.Debug.WriteLine("  {0}: {1}", prop.Key, prop.Value);
                        }
                    }
                }
            }
        }

        public static Queue<Sensor> Sensors = new Queue<Sensor>();

        public static async Task DoRD2C(string[] args)
        {

            //_Sensors = new List<Sensor>();
            s_eventHubsCompatibleEndpoint = Configuration["EVENT_HUBS_COMPATIBILITY_ENDPOINT"];
            s_eventHubsCompatiblePath = Configuration["EVENT_HUBS_COMPATIBILITY_PATH"];
            s_iotHubSasKey = Configuration["EVENT_HUBS_SAS_KEY"];// "{your service primary key}";
            s_iotHubSasKeyName = Configuration["SHARED_ACCESS_KEY_NAME"];// "service, iothubowner";

            System.Diagnostics.Debug.WriteLine("IoT Hub Quickstarts - Read device to cloud messages. Ctrl-C to exit.\n");


            System.Diagnostics.Debug.WriteLine("Using Env Var EVENT_HUBS_COMPATIBILITY_ENDPOINT = " + s_eventHubsCompatibleEndpoint);
            System.Diagnostics.Debug.WriteLine("Using Env Var EVENT_HUBS_COMPATIBILITY_PATH = " + s_eventHubsCompatiblePath);
            System.Diagnostics.Debug.WriteLine("Using Env Var EVENT_HUBS_SAS_KEY = " + s_iotHubSasKey);
            System.Diagnostics.Debug.WriteLine("Using Env Var SHARED_ACCESS_KEY_NAME = " + s_iotHubSasKeyName);

            //System.Diagnostics.Debug.WriteLine("\nDo you want to Hide System Properties sent by IoT Hub? [Y]es Default No");
            //var ch = Console.ReadKey();
            //if ((ch.KeyChar == 'Y') || (ch.KeyChar == 'y'))
            //{
            //    show_system_properties = false;
            //}

        show_system_properties = false;
        System.Diagnostics.Debug.WriteLine("Press Enter to continue when the Simulated-Device is sending messages.");
            //Console.ReadLine();

            // Create an EventHubClient instance to connect to the
            // IoT Hub Event Hubs-compatible endpoint.
            var connectionString = new EventHubsConnectionStringBuilder(new Uri(s_eventHubsCompatibleEndpoint), s_eventHubsCompatiblePath, s_iotHubSasKeyName, s_iotHubSasKey);



            s_eventHubClient = EventHubClient.CreateFromConnectionString(connectionString.ToString());

        // Create a PartitionReciever for each partition on the hub.
        //var xx = await s_eventHubClient.GetPartitionRuntimeInformationAsync("1");
            var runtimeInfo = await s_eventHubClient.GetRuntimeInformationAsync();
            var d2cPartitions = runtimeInfo.PartitionIds;

            CancellationTokenSource cts = new CancellationTokenSource();

            Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
                System.Diagnostics.Debug.WriteLine("Exiting...");
            };

            var tasks = new List<Task>();
            foreach (string partition in d2cPartitions)
            {
                tasks.Add(ReceiveMessagesFromDeviceAsync(partition, cts.Token));
            }

            // Wait for all the PartitionReceivers to finsih.
            Task.WaitAll(tasks.ToArray());
        }

    }
}

