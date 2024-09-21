﻿using System;

namespace Server
{
    internal static partial class HTML
    {
        internal static partial class STATIC
        {
            /// <summary>Request Header Fields Too Large</summary>
            internal static readonly Byte[] _431_response;

            private const String _431_body =
    @"<!DOCTYPE html>
<html>

    <head>
	    <meta charset=""utf-8"">
	    <title>Request Header Fields Too Large 431</title>
    </head>

    <center>
        <body style=""background-color:#00000a;font-family:Helvetica;color:#fbfbfb;"">
            <h1>431 Request Header Fields Too Large</h1>
            <p>Header size exceeded 2048 bytes</p>
        </body>
    </center>

</html>";
        }
    }
}