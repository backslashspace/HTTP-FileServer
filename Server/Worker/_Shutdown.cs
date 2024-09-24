using BSS.Logging;
using BSS.Threading;
using System.Data.SQLite;

namespace Server
{
    internal static partial class Worker
    {
        internal static void Shutdown()
        {
            try
            {
                _databaseConnection?.Close();
                _databaseConnection?.Dispose();
                SQLiteConnection.ClearAllPools();
            }
            catch
            {
                Log.FastLog("DB shutdown finished with errors", LogSeverity.Critical, "Shutdown()");
            }

            if (!ThreadPoolFast.Shutdown())
            {
                Log.FastLog("ThreadPool shutdown finished with errors", LogSeverity.Critical, "ThreadPool");
            }
            
        }
    }
}