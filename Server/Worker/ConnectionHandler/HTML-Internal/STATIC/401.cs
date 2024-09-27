using System;

namespace Server
{
    internal static partial class HTML
    {
        internal static partial class STATIC
        {
            /// <summary>Unauthorized</summary>
            internal static readonly Byte[] _401_response;

            private const String _401_body =
    @"<!DOCTYPE html>
<html>

    <head>
	    <meta charset=""utf-8"">
	    <title>401 Unauthorized</title>
    </head>

    <center>
        <body style=""background-color:#00000a;font-family:Helvetica;color:#fbfbfb;"">
            <h1>401 Unauthorized</h1>
        </body>
    </center>

</html>";
        }
    }
}