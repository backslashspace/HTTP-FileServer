using System;

namespace Server
{
    internal static partial class HTML
    {
        internal static partial class HTML_STATIC
        {
            /// <summary>Too Many Requests - Connection: close</summary>
            internal static Byte[] _429_response;

            private static readonly String _429_body =
    @"<!DOCTYPE html>
<html>

    <head>
	    <meta charset=""utf-8"">
	    <title>Too Many Requests 429</title>
    </head>

    <center>
        <body style=""background-color:#00000a;font-family:Helvetica;color:#fbfbfb;"">
            <h1>429 Too Many Requests</h1>
            <p>Server has reached maximum number of concurrent connections</p>
        </body>
    </center>

</html>";
        }
    }
}