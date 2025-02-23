using System;

namespace Server
{
    internal static partial class Worker
    {
        private static void FileSharingBaseHandler(SecureSocket connection, String header)
        {
            if (!AuthenticationBarrier(connection, header, out UserDB.User invokingUser)) return;

            //

            HTTP.RedirectOptions redirectOptions;
            if (invokingUser.IsAdministrator) redirectOptions = new HTTP.RedirectOptions(HTTP.ResponseType.HTTP_303, "/fileSharing/controlPanel");
            else redirectOptions = new HTTP.RedirectOptions(HTTP.ResponseType.HTTP_303, "/fileSharing/files");
            HTTP.CraftHeader(new HTTP.HeaderOptions(redirectOptions), out Byte[] rawResponse);

            connection.SslStream!.Write(rawResponse, 0, rawResponse.Length);
            connection.Close();
        }
    }
}