using BSS.Threading;
using BSS.Logging;
using BSS.Random;
using System;
using System.Net;
using System.Net.Sockets;

namespace Server
{
    internal static partial class Worker
    {
        private static Boolean _protectionEnabled;

        private static Socket? _secureListener;
        private static Socket? _redirectListener;

        internal static void CloseListeners()
        {
            _secureListener?.Close();
            _redirectListener?.Close();
        }

        internal unsafe static Boolean Initialize(ConfigurationLoader.Configuration* configuration)
        {
            Log.FastLog("Initializing", LogSeverity.Info, "Init");

            _protectionEnabled = configuration->EnableServiceProtection;

            if (HWRandom.GetSupportedInstructions() != HWRandom.SupportedInstructions.All)
            {
                Log.FastLog("CPU not compatible! RDSEED instruction not found!\nShutting down..", LogSeverity.Critical, "HWRandom");
                return false;
            }

            if (HTTP.ERRORS.IsInitialized)
            {
                Log.FastLog("Initialized static content", LogSeverity.Info, "Init");
            }
            else
            {
                Log.FastLog("HTTP_ERRORS not initialized", LogSeverity.Error, "Init");
                return false;
            }

            SecureSocket.SetPfxOptions(configuration->PfxPath, configuration->PfxPassphrase);
            if (!SecureSocket.LoadCertificate()) return false;

            if (configuration->EnableReload) IPC.StartServerModule();

            if (!ThreadPoolX.Initialize(configuration->ThreadPoolThreads)) return false;

            if (!UserDB.IsInitialized)
            {
                Log.FastLog("Database error, shutting down", LogSeverity.Error, "Init");
                return false;
            }

            if (!CookieDB.IsInitialized)
            {
                Log.FastLog("Session Cookie Database error, shutting down", LogSeverity.Error, "Init");
                return false;
            }

            try
            {
                _secureListener = new(SocketType.Stream, ProtocolType.Tcp);
                _secureListener.NoDelay = true;
                _secureListener.ReceiveTimeout = 5120;
                _secureListener.SendTimeout = 5120;
                _secureListener.Bind(new IPEndPoint(configuration->ListenerAddress, configuration->HttpsListenerPort));
                _secureListener.Listen(4);
                Log.FastLog("Successfully bound secure secure listener to: " + configuration->ListenerAddress.ToString() + ":" + configuration->HttpsListenerPort, LogSeverity.Info, "Init->Socket");

                if (configuration->EnableHttpRedirector)
                {
                    _redirectListener = new(SocketType.Stream, ProtocolType.Tcp);
                    _redirectListener.NoDelay = true;
                    _redirectListener.ReceiveTimeout = 5120;
                    _redirectListener.SendTimeout = 5120;
                    _redirectListener.Bind(new IPEndPoint(configuration->ListenerAddress, configuration->HttpListenerPort));
                    _redirectListener.Listen(4);
                    Log.FastLog("Successfully bound http redirect listener to: " + configuration->ListenerAddress.ToString() + ":" + configuration->HttpListenerPort, LogSeverity.Info, "Init->Socket");
                }
            }
            catch (Exception exception)
            {
                if (configuration->EnableHttpRedirector) Log.FastLog("Unable to bind to " + configuration->ListenerAddress.ToString() + " " + configuration->HttpsListenerPort + "/" + configuration->HttpListenerPort + " >> " + exception.Message, LogSeverity.Error, "Init->Socket");
                else Log.FastLog("Unable to bind to: " + configuration->ListenerAddress.ToString() + ":" + configuration->HttpsListenerPort +" >> " + exception.Message, LogSeverity.Error, "Init->Socket");
                return false;
            }

            return true;
        }

#pragma warning disable IDE0051
        private static void SetSocketKeepAlive(Socket connection)
        {
            Byte[] keepAlive = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
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