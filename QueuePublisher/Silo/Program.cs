using Grains;
using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Versions.Compatibility;
using System;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

[assembly: AssemblyVersion("1.0.0")]

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
            var siloHostBuilder = new SiloHostBuilder()

                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = "dev";
                    options.ServiceId = "QueuePublisher";
                })

                //// Clustering provider
                //.UseLocalhostClustering()

                .Configure<EndpointOptions>(options =>
                {
                    options.SiloPort = 11111;
                    options.GatewayPort = 30000;
                    options.AdvertisedIPAddress = IPAddress.Parse("172.17.0.5");
                    options.GatewayListeningEndpoint = new IPEndPoint(IPAddress.Any, 40000);
                    options.SiloListeningEndpoint = new IPEndPoint(IPAddress.Any, 50000);
                })

                .Configure<GrainVersioningOptions>(options =>
                {
                    options.DefaultCompatibilityStrategy = nameof(BackwardCompatible);
                    options.DefaultVersionSelectorStrategy = nameof(AllVersionsCompatible);
                })

                .UseDashboard()

                .UseAzureStorageClustering(options => { options.ConnectionString = "DefaultEndpointsProtocol=https;AccountName=queueclusterstorage;AccountKey=bfMxQptp+yUBZtEBdrKKoe7jQiS3D96/7EWn38jwpsVWsrNMexpkuc8vX7+5Bov4KL19463Ca6fBEBxtB9v2og==;EndpointSuffix=core.windows.net"; })

                // Application parts: just reference one of the grain implementations that we use
                .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(MessagePublishing).Assembly).WithReferences());

            host = siloHostBuilder.Build();
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