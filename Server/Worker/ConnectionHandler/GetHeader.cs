﻿using BSS.Logging;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Server
{
    internal static partial class Worker
    {
        private static Boolean GetHeader(Socket connection, out String header)
        {
            (header, Boolean success, Boolean wasHeaderError) = ReceiveRequestHeader(connection);

            if (!success)
            {
                if (wasHeaderError) Log.FastLog("Unable to parse request header, sending 431 and closing connection", LogSeverity.Error, "ReceiveRequestHeader()");

                try
                {
                    if (wasHeaderError) connection.Send(HTML.HTML_STATIC._431_response, 0, HTML.HTML_STATIC._431_response.Length, SocketFlags.None);
                }
                catch { }

                CloseConnection(connection);

                return false;
            }

            xDebug.WriteLine($"{Thread.CurrentThread.Name} => {header.Split('\r')[0]}");

            return true;
        }

        // todo: verify header to long request
        private static ValueTuple<String, Boolean, Boolean> ReceiveRequestHeader(Socket connection)
        {
            _receiveBuffer = new Byte[2048];
            _receiveBufferIterator = new Byte[1];

            for (UInt16 i = 0; i < 2048; ++i)
            {
                if (connection.Receive(_receiveBufferIterator, 0, 1, SocketFlags.None) == 0)
                {
                    connection.Shutdown(SocketShutdown.Both);
                    connection.Close();

                    return (null, false, false);
                }

                _receiveBuffer[i] = _receiveBufferIterator[0];

                // check for header end
                if (24 < i                             // min size
                    && _receiveBuffer[i - 3] == 0x0D    // first CR
                    && _receiveBuffer[i - 2] == 0x0A    // first LF
                    && _receiveBuffer[i - 1] == 0x0D    // second CR
                    && _receiveBuffer[i - 0] == 0x0A)   // second LF)
                {
                    return (Encoding.UTF8.GetString(_receiveBuffer, 0, i), true, false);
                }
            }

            return (null, false, true);
        }
    }
}