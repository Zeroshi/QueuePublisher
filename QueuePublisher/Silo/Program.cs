﻿using Grains;
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
                Console.WriteLine("Initiating Silo");
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
            Console.WriteLine("Setting Silo Properties");
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

                //.Configure<GrainVersioningOptions>(options =>
                //{
                //    options.DefaultCompatibilityStrategy = nameof(BackwardCompatible);
                //    options.DefaultVersionSelectorStrategy = nameof(AllVersionsCompatible);
                //})

                .UseDashboard()

                .UseAzureStorageClustering(options => { options.ConnectionString = Environment.GetEnvironmentVariable("ClusterConnectionString"); })

                // Application parts: just reference one of the grain implementations that we use
                .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(MessagePublishing).Assembly).WithReferences());

            Console.WriteLine("Building Silo");
            host = siloHostBuilder.Build();
            Console.WriteLine("Starting Silo");
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