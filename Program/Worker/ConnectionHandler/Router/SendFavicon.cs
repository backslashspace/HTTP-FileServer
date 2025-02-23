using BSS.Logging;
using System;

namespace Server
{
    internal static partial class Worker
    {
        private static void SendFavicon(SecureSocket connection)
        {
            Int64 fileSize = GetFileSize(Program.AssemblyPath + "html\\favicon.ico");

            if (fileSize < 1)
            {
                Log.FastLog("Unable to load favicon.ico file size -> 500", LogSeverity.Error, "Handler");
                HTTP.ERRORS.Send_500(connection);
                connection.Close();
                return;
            }

            Span<Byte> fileBuffer = stackalloc Byte[(Int32)fileSize];
            if (!LoadStackFile(Program.AssemblyPath + "html\\favicon.ico", fileBuffer))
            {
                Log.FastLog("Unable to load favicon.ico from disk -> 500", LogSeverity.Error, "Handler");
                HTTP.ERRORS.Send_500(connection);
                connection.Close();
                return;
            }

            HTTP.HeaderOptions headerOptions = new(HTTP.ResponseType.HTTP_200, new(HTTP.ContentType.Icon), fileSize);
            HTTP.CraftHeader(headerOptions, out Byte[] headerBuffer);

            connection.SslStream!.Write(headerBuffer, 0, headerBuffer.Length);
            connection.SslStream!.Write(fileBuffer);
            connection.Close();
        }
    }
}