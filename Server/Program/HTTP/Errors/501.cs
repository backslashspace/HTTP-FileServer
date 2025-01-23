using System;

namespace Server
{
    internal static partial class HTTP
    {
        internal static partial class ERRORS
        {
            /// <summary>Not Implemented</summary>
            internal static readonly Byte[] _501_response;

            private const String _501_body =
    @"<!DOCTYPE html>
<html>

    <head>
	    <meta charset=""utf-8"">
	    <title>501 Not Implemented</title>
    </head>

    <center>
        <body style=""background-color:#00000a;font-family:Helvetica;color:#fbfbfb;"">
            <h1>501 Not Implemented</h1>
            <p>This section should have never been reached..</p>
            <p>The sun must be at fault</p>
        </body>
    </center>

</html>";
        }
    }
}