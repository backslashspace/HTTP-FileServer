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

        private static void Main()
        {
            AssemblyPath = AppContext.BaseDirectory;
            Log.Initialize(new(AssemblyPath));

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