using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using BSS.Logging;
using BSS.Random;
using BSS.Threading;

#pragma warning disable IDE0079
#pragma warning disable CS8618

namespace Server
{
    internal static partial class Worker
    {
        internal const String WEB_ROOT = "fileSharing";
        internal const String WEB_ROOT_LOWERCAPS = "filesharing";

        internal static volatile Boolean ShutdownPending = false;
        internal static Socket Listener;
        internal static String AssemblyPath;

        internal static void Initialize()
        {
            AssemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            Log.Initialize(AssemblyPath);
            Log.FastLog("Initializing", LogSeverity.Info, "Init");

            if (!HWRandom.HardwareRandomIsPresent())
            {
                Log.FastLog("CPU not compatible! RDSEED and or RDRAND instruction not found!\nShutting down..", LogSeverity.Critical, "CPU");
                Environment.Exit(-1);
            }

            (IPAddress interfaceIP, UInt16 interfacePort, UInt16 maximumConcurrentConnections) = ValidateSettings();

            if (!UserDB.IsInitialized)
            {
                Log.FastLog("Database error, shutting down", LogSeverity.Error, "Init");
                Environment.Exit(-1);
            }

            if (!CookieDB.IsInitialized)
            {
                Log.FastLog("Session Cookie Database error, shutting down", LogSeverity.Error, "Init");
                Environment.Exit(-1);
            }

            ThreadPoolFast.Initialize(maximumConcurrentConnections, () =>
            {
                // init thread static variables
                _receiveBuffer = new Byte[2048];
                _receiveBufferIterator = new Byte[1];
            });

            if (HTML.STATIC.IsInitialized)
            {
                Log.FastLog("Initialized static content", LogSeverity.Info, "Init");
            }
            else
            {
                Log.FastLog("HTTP_ERRORS not initialized", LogSeverity.Info, "Init");
                Environment.Exit(-1);
            }

            Listener = BindSocket(interfaceIP, interfacePort);

            Log.FastLog("Initialization complete", LogSeverity.Info, "Init");

            StartNewConnectionHandler();

            // main worker thread will end here
        }

        // # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # #

        private static ValueTuple<IPAddress, UInt16, UInt16> ValidateSettings()
        {
            if (!IPAddress.TryParse(Properties.Settings.Default.ListenInterface, out IPAddress interfaceIP))
            {
                Log.FastLog($"Unable to parse ip from settings file, ip was: {Properties.Settings.Default.ListenInterface}", LogSeverity.Error, "Load");
                Environment.Exit(-1);
            }

            if (interfaceIP.AddressFamily != AddressFamily.InterNetwork)
            {
                Log.FastLog($"Only IPv4 supported, ip was: {Properties.Settings.Default.ListenInterface}", LogSeverity.Error, "Load");
                Environment.Exit(-1);
            }

            if (!UInt16.TryParse(Properties.Settings.Default.ListenPort, out UInt16 interfacePort))
            {
                Log.FastLog($"Unable to parse port from settings file, port was: {Properties.Settings.Default.ListenPort}", LogSeverity.Error, "Load");
                Environment.Exit(-1);
            }

            if (!UInt16.TryParse(Properties.Settings.Default.MaximumConcurrentConnections, out UInt16 maximumConcurrentConnections))
            {
                Log.FastLog($"Unable to parse 'MaximumConcurrentConnections' from settings file, value was: {Properties.Settings.Default.MaximumConcurrentConnections}", LogSeverity.Error, "Load");
                Environment.Exit(-1);
            }

            Log.FastLog("Successfully loaded configuration", LogSeverity.Info, "Init");

            return (interfaceIP, interfacePort, maximumConcurrentConnections);
        }

        private static Socket BindSocket(IPAddress interfaceIP, UInt16 interfacePort)
        {
            Socket listener = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listener.NoDelay = true;
            listener.ReceiveTimeout = 0;
            listener.SendTimeout = 5120;

            try
            {
                listener.Bind(new IPEndPoint(interfaceIP, interfacePort));
                Log.FastLog($"Successfully bound to: {interfaceIP}:{interfacePort}", LogSeverity.Info, "Init->Socket");
            }
            catch
            {
                Log.FastLog($"Unable to bind to: {interfaceIP}:{interfacePort}", LogSeverity.Error, "Init->Socket");
                Environment.Exit(-1);
            }

            //try
            //{
            //    SetSocketKeepAlive(listener);
            //    Log.FastLog($"Successfully enabled socket keep-alive: 15000, 1000", LogSeverity.Info, "Init->Socket");
            //}
            //catch
            //{
            //    Log.FastLog($"Failed to enable socket keep-alive", LogSeverity.Error, "Init->Socket");
            //    Environment.Exit(-1);
            //}

            return listener;
        }

#pragma warning disable IDE0051

        private static void SetSocketKeepAlive(Socket connection)
        {
            Byte[] keepAlive = new Byte[12];
            // keepalive on / off
            Buffer.BlockCopy(BitConverter.GetBytes((UInt32)1), 0, keepAlive, 0, 4);
            // keepAlive time in ms
            Buffer.BlockCopy(BitConverter.GetBytes((UInt32)16384), 0, keepAlive, 4, 4);
            // keepAlive timeout in ms
            Buffer.BlockCopy(BitConverter.GetBytes((UInt32)1536), 0, keepAlive, 8, 4);

            connection.IOControl(IOControlCode.KeepAliveValues, keepAlive, null);
        }
    }
}