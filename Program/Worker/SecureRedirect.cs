using BSS.Logging;
using System;
using System.Net.Sockets;
using System.Text;

namespace Server
{
    internal static partial class Worker
    {
        internal static void Redirector(Object parameter)
        {
            Boolean protection = (Boolean)parameter;

            while (!Service.Shutdown)
            {
                Socket connection;

                try
                {
                    connection = _redirectListener!.Accept();
                }
                catch (SocketException)
                {
                    Log.FastLog("HTTP -> HTTPS listener closed, shutting down", LogSeverity.Info, "Redirector");
                    return;
                }
                catch (Exception exception)
                {
                    Log.FastLog("Unknown listener socket error: " + exception.Message, LogSeverity.Critical, "Redirector");
                    Service.InternalShutdown();
                    return;
                }

                try
                {
                    if (!Redirect(connection))
                    {
                        Log.FastLog("Http request was longer than 4096 bytes - closing connection", LogSeverity.Warning, "Redirector");
                    }

                    // todo: protection

                    connection.Shutdown(SocketShutdown.Both);
                    connection.Close();
                }
                catch { }
            }
        }

        private static Boolean Redirect(Socket connection)
        {
            Int32 headerLength = 0;
            Span<Byte> headerBuffer = stackalloc Byte[4069];

            try
            {
                for (Int32 i = 0; i < 4069; ++i)
                {
                    if (connection.Receive(headerBuffer.Slice(i, 1), SocketFlags.None) == 0) return false;

                    if (24 < i                            // min size
                        && headerBuffer[i - 3] == 0x0D    // first CR
                        && headerBuffer[i - 2] == 0x0A    // first LF
                        && headerBuffer[i - 1] == 0x0D    // second CR
                        && headerBuffer[i - 0] == 0x0A)   // second LF)
                    {
                        headerLength = i;
                        break;
                    }
                }

                if (headerLength == 0) return false;

                // #  #  #  #  #  #  #  #  #  #  #  #  #  #  #  #  #  #  #  #

                Int32 hostStartIndex = 0;
                Int32 hostEndIndex = 0;

                for (Int32 i = 0; i < headerLength; ++i)
                {
                    if (!(headerBuffer[i] == 'H' || headerBuffer[i] == 'h')
                        || headerBuffer[i + 1] != 'o'
                        || headerBuffer[i + 2] != 's'
                        || headerBuffer[i + 3] != 't'
                        || headerBuffer[i + 4] != ':') continue;

                    if (headerBuffer[i + 5] == ' ') hostStartIndex = i + 6;
                    else hostStartIndex = i + 5;

                    break;
                }

                if (hostStartIndex == 0) return false;

                for (Int32 i = hostStartIndex; i < headerLength; ++i)
                {
                    if (headerBuffer[i] != '\r') continue;

                    hostEndIndex = i;
                    break;
                }

                if (hostEndIndex == 0) return false;

                // #  #  #  #  #  #  #  #  #  #  #  #  #  #  #  #  #  #  #  #

                String host = Encoding.UTF8.GetString(headerBuffer[hostStartIndex..hostEndIndex]);

                HTTP.RedirectOptions redirectOptions = new(HTTP.ResponseType.HTTP_303, "https://" + host);
                HTTP.CraftHeader(new HTTP.HeaderOptions(redirectOptions), out Byte[] rawResponse);
                connection.Send(rawResponse, 0, rawResponse.Length, SocketFlags.None);

                Log.FastLog("Redirected client from http to https", LogSeverity.Info, "Redirector");

                return true;
            }
            catch
            {
                return false;
            }            
        }
    }
}