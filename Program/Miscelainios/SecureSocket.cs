using BSS.Logging;
using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

#pragma warning disable IDE0032

namespace Server
{
    internal sealed class SecureSocket
    {
        internal static Boolean Initialized { get => _initialized; }
        private static Boolean _initialized = false;

        private static X509Certificate2? _serverCertificate;
        private static SslServerAuthenticationOptions? _sslServerAuthenticationOptions;

        internal Socket? Socket;
        private NetworkStream? _networkStream;
        internal SslStream? SslStream;

        // # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # #

        /// <summary>
        /// Error handled method
        /// </summary>
        /// <param name="pfxPath">Path to the certificate and private key</param>
        /// <param name="password">Optional password for the PFX file</param>
        /// <returns></returns>
        internal static Boolean LoadCertificate(String pfxPath, String? password)
        {
            try
            {
                _serverCertificate = X509CertificateLoader.LoadPkcs12FromFile(pfxPath, password, X509KeyStorageFlags.DefaultKeySet);
            }
            catch (Exception exception)
            {
                Log.FastLog("Failed to load PFX certificate: " + exception.Message, LogSeverity.Error, "SecureSocket");

                _initialized = false;
                _serverCertificate = null;
                _sslServerAuthenticationOptions = null;

                return false;
            }

            _sslServerAuthenticationOptions = new();
            _sslServerAuthenticationOptions.AllowTlsResume = false;
            _sslServerAuthenticationOptions.AllowRenegotiation = false;
            _sslServerAuthenticationOptions.CertificateRevocationCheckMode = X509RevocationMode.Online;
            // todo: _sslServerAuthenticationOptions.ServerCertificateContext = 
            //sslServerAuthenticationOptions.CipherSuitesPolicy = new([TlsCipherSuite.TLS_AES_256_GCM_SHA384]);
            _sslServerAuthenticationOptions.EncryptionPolicy = EncryptionPolicy.RequireEncryption;
            _sslServerAuthenticationOptions.EnabledSslProtocols = SslProtocols.Tls13;
            _sslServerAuthenticationOptions.ServerCertificate = _serverCertificate;

            _initialized = true;

            Log.FastLog("Successfully loaded certificate", LogSeverity.Info, "SecureSocket");

            return true;
        }

        internal static Boolean CreateSecureConnection(Socket socket, out SecureSocket secureSocket)
        {
            if (!_initialized)
            {
                Log.FastLog("TLS not initialized - call static initializer -> load certificate", LogSeverity.Error, "SecureSocket");
                secureSocket = null!;
                return false;
            }

            secureSocket = new();

            return secureSocket.InternalCreate(socket, out secureSocket);
        }

        private Boolean InternalCreate(Socket socket, out SecureSocket secureSocket)
        {
            if (!_initialized)
            {
                secureSocket = null!;
                return false;
            }

            if (socket == null || !socket.Connected)
            {
                Log.FastLog("Unable to create secure connection - socket was null or not connected", LogSeverity.Error, "SecureSocket");
                secureSocket = null!;
                return false;
            }

            Socket = socket;
            _networkStream = new(Socket, true);
            SslStream = new(_networkStream, false, null, null, EncryptionPolicy.RequireEncryption);

            try
            {
                SslStream.AuthenticateAsServer(_sslServerAuthenticationOptions!);
            }
            catch (Exception exception)
            {
                Log.FastLog("Unable to create a secure connection - authentication failed: " + exception.Message, LogSeverity.Error, "SecureSocket");
                secureSocket = null!;
                return false;
            }

            if (!SslStream.IsAuthenticated || !SslStream.IsEncrypted || !SslStream.IsSigned)
            {
                Log.FastLog("Unable to create a secure connection - IsAuthenticated: " + SslStream.IsAuthenticated + ", IsEncrypted: " + SslStream.IsEncrypted + ", IsSigned: " + SslStream.IsSigned, LogSeverity.Error, "SecureSocket");
                secureSocket = null!;
                return false;
            }

            if (SslStream.NegotiatedCipherSuite != TlsCipherSuite.TLS_AES_256_GCM_SHA384)
            {
                Log.FastLog("Unable to create a sufficiently secure connection - required cipher is TLS_AES_256_GCM_SHA384, currently connected using: " + SslStream.NegotiatedCipherSuite, LogSeverity.Error, "SecureSocket");
                secureSocket = null!;
                return false;
            }

            secureSocket = this;
            return true;
        }

        internal void Close()
        {
            if (SslStream == null) Log.Debug("SslStream was null", "SecureSocket->Close()");
            if (_networkStream == null) Log.Debug("_networkStream was null", "SecureSocket->Close()");
            if (Socket == null) Log.Debug("_socket was null", "SecureSocket->Close()");

            SslStream?.Close();
            _networkStream?.Close();
            Socket?.Close();
        }
    }
}