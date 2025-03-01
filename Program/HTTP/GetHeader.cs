using System;
using System.Text;
using BSS.Logging;
using System.Threading;

namespace Server
{
    internal static partial class HTTP
    {
        internal unsafe static Boolean GetHeader(SecureSocket connection, out String headerString)
        {
            Span<Byte> receiveBuffer = stackalloc Byte[4069];

            for (Int32 i = 0; i < 4069; ++i)
            {
                try
                {
                    connection.SslStream!.ReadExactly(receiveBuffer.Slice(i, 1));
                }
                catch (Exception exception)
                {
                    Log.FastLog("Unable to parse request header: " + exception.Message + " -> sending 431", LogSeverity.Error, "GetHeader()");
                    HTTP.ERRORS.Send_431(connection);

                    headerString = null!;
                    return false;
                }
                
                if (24 < i                             // min size
                    && receiveBuffer[i - 3] == 0x0D    // first CR
                    && receiveBuffer[i - 2] == 0x0A    // first LF
                    && receiveBuffer[i - 1] == 0x0D    // second CR
                    && receiveBuffer[i - 0] == 0x0A)   // second LF)
                {
                    headerString = Encoding.UTF8.GetString(receiveBuffer[..i]);
                    return true;
                }
            }

            Log.FastLog("Unable to parse request header - reached buffer end before finding CRLF -> sending 431", LogSeverity.Error, "GetHeader()");
            HTTP.ERRORS.Send_431(connection);

            headerString = null!;
            return false;
        }
    }
}