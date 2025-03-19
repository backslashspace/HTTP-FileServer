using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using BSS.Logging;
using System.Threading;

namespace Server
{
    internal static partial class Program
    {
        internal static String? AssemblyPath;

        internal static readonly ManualResetEvent Exit = new(false);

        private static void Main(String[] args)
        {
            AssemblyPath = AppContext.BaseDirectory;

            if (args.Length == 1 && args[0] == "/reload")
            {
                Log.Initialize(new(AssemblyPath), false);
                Int32 exitCode = IPC.SendReloadSignal();
                Environment.Exit(exitCode);
                return;
            }

            Log.Initialize(new(AssemblyPath), true);

            HostApplicationBuilderSettings hostApplicationBuilderSettings = new();
            hostApplicationBuilderSettings.ApplicationName = "File Server";

            HostApplicationBuilder builder = Host.CreateEmptyApplicationBuilder(hostApplicationBuilderSettings);
            builder.Services.AddSingleton<Service>();
            builder.Services.AddHostedService<Service>();
            builder.Services.AddWindowsService(options =>
            {
                options.ServiceName = "File Server";
            });

            IHost host = builder.Build();
            host.Run();

            Exit.WaitOne();
        }
    }
}