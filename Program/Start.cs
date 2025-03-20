using System;
using BSS.Threading;
using System.Threading;
using System.Data.SQLite;
using BSS.Logging;

namespace Server
{
    internal static partial class Program
    {
        internal unsafe static void Run()
        {
            Int32 exitCode = 0;
            Thread redirector = null!;

            ConfigurationLoader.Configuration configuration = default;
            if (!ConfigurationLoader.Load(AssemblyPath!, &configuration))
            {
                exitCode = -1;
                goto SHUTDOWN;
            }

            if (!Worker.Initialize(&configuration)) 
            {
                exitCode = -1;
                goto SHUTDOWN;
            }

            if (configuration.EnableHttpRedirector)
            {
                redirector = new(Worker.Redirector!);
                redirector.Name = "HTTP -> HTTPS";
                redirector.Start(configuration.EnableServiceProtection);
            }

            Log.FastLog("Done", LogSeverity.Info, "Init");

            Worker.Run();

        SHUTDOWN:

            redirector?.Join();

            CookieDB.Shutdown();
            SQLiteConnection.ClearAllPools();

            ThreadPoolX.Shutdown();

            if (exitCode != 0) Environment.Exit(exitCode);
        }
    }
}