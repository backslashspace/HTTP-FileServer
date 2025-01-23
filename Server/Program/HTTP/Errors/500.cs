using System;

namespace Server
{
    internal static partial class HTTP
    {
        internal static partial class ERRORS
        {
            /// <summary>Internal Server Error</summary>
            internal static readonly Byte[] _500_response;

            private const String _500_body =
    @"<!DOCTYPE html>
<html>

    <head>
	    <meta charset=""utf-8"">
	    <title>500 Internal Server Error</title>
    </head>

    <center>
        <body style=""background-color:#00000a;font-family:Helvetica;color:#fbfbfb;"">
            <h1>500 Internal Server Error</h1>
        </body>
    </center>

</html>";
        }
    }
}