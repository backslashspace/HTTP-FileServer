using BSS.Logging;
using System;

namespace Server
{
    internal static partial class Worker
    {
        private static void SendStackFile(SecureSocket connection, String subPath)
        {
            Int64 fileSize = Tools.GetFileSize(Program.AssemblyPath + "html\\" + subPath);

            if (fileSize < 1)
            {
                Log.FastLog("Unable to load html\\" + subPath + "file size -> 500", LogSeverity.Error, "StackFile");
                HTTP.ERRORS.Send_500(connection);
                connection.Close();
                return;
            }

            Span<Byte> fileBuffer = stackalloc Byte[(Int32)fileSize];
            if (!Tools.LoadStackFile(Program.AssemblyPath + "html\\" + subPath, fileBuffer))
            {
                Log.FastLog("Unable to load html\\" + subPath + "file size -> 500", LogSeverity.Error, "StackFile");
                HTTP.ERRORS.Send_500(connection);
                connection.Close();
                return;
            }

            HTTP.HeaderOptions headerOptions = new(HTTP.ResponseType.HTTP_200, new(HTTP.ContentType.HTML), fileSize);
            HTTP.CraftHeader(headerOptions, out Byte[] headerBuffer);

            connection.SslStream!.Write(headerBuffer, 0, headerBuffer.Length);
            connection.SslStream!.Write(fileBuffer);
            connection.Close();
        }
    }
}