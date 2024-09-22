using System;

namespace Server
{
    internal static partial class HTML
    {
        internal static partial class STATIC
        {
            /// <summary>Forbidden</summary>
            internal static readonly Byte[] _403_response;

            private const String _403_body =
    @"<!DOCTYPE html>
<html>

    <head>
	    <meta charset=""utf-8"">
	    <title>403 Forbidden</title>
    </head>

    <center>
        <body style=""background-color:#00000a;font-family:Helvetica;color:#fbfbfb;"">
            <h1>403 Forbidden</h1>
        </body>
    </center>

</html>";
        }
    }
}