using System;

namespace Server
{
    internal static partial class HTML
    {
        internal static partial class HTML_STATIC
        {
            /// <summary>Bad Request</summary>
            internal static Byte[] _400_response;

            private static readonly String _400_body =
    @"<!DOCTYPE html>
<html>

    <head>
	    <meta charset=""utf-8"">
	    <title>400 Bad Request</title>
    </head>

    <center>
        <body style=""background-color:#00000a;font-family:Helvetica;color:#fbfbfb;"">
            <h1>404 Bad Request</h1>
        </body>
    </center>

</html>";
        }
    }
}