using System;

namespace Server
{
    internal static partial class HTTP
    {
        internal static partial class ERRORS
        {
            /// <summary>Conflict</summary>
            internal static readonly Byte[] _409_response;

            private const String _409_body =
    @"<!DOCTYPE html>
<html>

    <head>
	    <meta charset=""utf-8"">
	    <title>409 Conflict</title>
    </head>

    <center>
        <body style=""background-color:#00000a;font-family:Helvetica;color:#fbfbfb;"">
            <h1>409 Conflict</h1>
            <p>Client - Server state mismatch</p>
        </body>
    </center>

</html>";
        }
    }
}