using BSS.Logging;
using BSS.Threading;
using System.Data.SQLite;

namespace Server
{
    internal static partial class Worker
    {
        internal static void Shutdown()
        {
            CookieDB.Shutdown();
            SQLiteConnection.ClearAllPools();

            if (!ThreadPoolFast.Shutdown())
            {
                Log.FastLog("ThreadPool shutdown finished with errors", LogSeverity.Critical, "ThreadPool");
            }
        }
    }
}