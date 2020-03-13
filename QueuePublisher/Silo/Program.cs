using Grains;
using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using System;
using System.Net;
using System.Threading.Tasks;

namespace SiloHost
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            return await RunSilo();
        }

        private static async Task<int> RunSilo()
        {
            try
            {
                await StartSilo();
                Console.WriteLine("Silo started");

                Console.WriteLine("Press enter to terminate");
                Console.ReadLine();

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

            var host = builder.Build();
            await host.StartAsync();
            return host;
        }
    }
}