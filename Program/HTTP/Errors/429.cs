﻿using System;

namespace Server
{
    internal static partial class HTTP
    {
        internal static partial class ERRORS
        {
            /// <summary>Too Many Requests</summary>
            internal static readonly Byte[] _429_response;

            private const String _429_body =
    @"<!DOCTYPE html>
<html>

    <head>
	    <meta charset=""utf-8"">
	    <title>429 Too Many Requests</title>
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