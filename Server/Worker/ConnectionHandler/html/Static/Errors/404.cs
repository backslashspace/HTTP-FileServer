using System;

namespace Server
{
    internal static partial class HTML
    {
        internal static partial class HTML_STATIC
        {
            /// <summary>Not Found</summary>
            internal static Byte[] _404_response;

            private static readonly String _404_body =
    @"<!DOCTYPE html>
<html>

    <head>
	    <meta charset=""utf-8"">
	    <title>404 Not Found</title>
    </head>

    <center>
        <body style=""background-color:#00000a;font-family:Helvetica;color:#fbfbfb;"">
            <h1>404 Not Found</h1>
            <p>Invalid endpoint at current connection state</p>
        </body>
    </center>

</html>";
        }
    }
}