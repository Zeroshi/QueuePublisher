using Interfaces;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Runtime;
using Polly;
using System;
using System.Threading.Tasks;

namespace Client
{
    public class Program
    {
        private static int Main(string[] args)
        {
            return RunMainAsync().GetAwaiter().GetResult();
        }

        private static async Task<int> RunMainAsync()
        {
            try
            {
                using (var client = StartClient())
                {
                    int reps = 25;
                    int counter = 0;

                    while (reps <= counter)
                    {
                        counter++;

                        var message = new Message { SendingApplication = "clientLogging", Payload = $"Test: {counter}" };
                        var grain = client.GetGrain<IPublishMessage>(message.Queue);
                        var response = await grain.PublishMessageAck(message);
                    }

                    Console.ReadLine();
                }

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return -1;
            }
        }

        private static IClusterClient StartClient()
        {
            return Policy<IClusterClient>
                .Handle<SiloUnavailableException>()
                .Or<OrleansMessageRejectionException>()
                .WaitAndRetry(
                    new[] {
                            TimeSpan.FromSeconds(2),
                            TimeSpan.FromSeconds(4),
                            TimeSpan.FromSeconds(6)
                        }).Execute(() =>
                            {
                                //Azure Setup
                                var client = new ClientBuilder()
                                    .Configure<ClusterOptions>(options =>
                                    {
                                        options.ClusterId = "dev";
                                        options.ServiceId = "QueuePublisher";
                                    })
                                    .UseAzureStorageClustering(options => options.ConnectionString = Environment.GetEnvironmentVariable("ClusterConnectionString"))
                                    .ConfigureLogging(builder => builder.SetMinimumLevel(LogLevel.Warning).AddConsole())
                                    .Build();

                                Console.WriteLine("Connecting the Client");
                                client.Connect().GetAwaiter().GetResult();
                                Console.WriteLine("Client connected");


                                return client;
                            });
        }

        internal class Message : IMessage
        {
            public string SendingApplication { get; set; }
            public string Payload { get; set; }
            public string Queue { get; set; }
        }
    }
}