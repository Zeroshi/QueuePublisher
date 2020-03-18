using Grains;
using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SiloHost
{
    public class Program
    {
        private static readonly ManualResetEvent _siloStopped = new ManualResetEvent(false);
        private static bool siloStopping;
        private static readonly object syncLock = new object();
        private static ISiloHost host;

        public static async Task<int> Main(string[] args)
        {
            return await RunSilo();
        }

        private static async Task<int> RunSilo()
        {
            try
            {
                SetupApplicationShutdown();

                var silo = await StartSilo();
                Console.WriteLine("Silo started");

                Console.WriteLine("Press enter to terminate");
                Console.ReadLine();

                _siloStopped.WaitOne();

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Silo encountered an error.");
                Console.WriteLine(ex);
                return -1;
            }
        }

        private static async Task<ISiloHost> StartSilo()
        {
            var builder = new SiloHostBuilder()

                // Clustering information
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = "dev";
                    options.ServiceId = "QueuePublisher";
                })

                // Clustering provider
                .UseLocalhostClustering()

                .Configure<EndpointOptions>(options =>
                {
                    options.SiloPort = 11111;
                    options.GatewayPort = 30000;
                    options.AdvertisedIPAddress = IPAddress.Loopback;
                })

                .UseDashboard()

                //.UseAzureStorageClustering(options => options.ConnectionString = connectionString)

                // Application parts: just reference one of the grain implementations that we use
                .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(MessagePublishing).Assembly).WithReferences());

            /* Azure Setup
                // TODO replace with your connection string
                const string connectionString = "YOUR_CONNECTION_STRING_HERE";
                var silo = new SiloHostBuilder()
                    .Configure<ClusterOptions>(options =>
                    {
                        options.ClusterId = "Cluster42";
                        options.ServiceId = "MyAwesomeService";
                    })
                    .UseAzureStorageClustering(options => options.ConnectionString = connectionString)
                    .ConfigureEndpoints(siloPort: 11111, gatewayPort: 30000)
                    .ConfigureLogging(builder => builder.SetMinimumLevel(LogLevel.Warning).AddConsole())
                    .Build();
            */

            host = builder.Build();
            await host.StartAsync();
            return host;
        }

        private static void SetupApplicationShutdown()
        {
            Console.CancelKeyPress += (s, a) =>
            {
                a.Cancel = true;
                lock (syncLock)
                {
                    if (siloStopping)
                    {
                        siloStopping = true;
                        Task.Run(StopSilo).Ignore();
                    }
                }
            };
        }

        private static async Task StopSilo()
        {
            await host.StopAsync();
            _siloStopped.Set();
        }
    }
}