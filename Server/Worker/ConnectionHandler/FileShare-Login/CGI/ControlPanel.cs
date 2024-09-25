using System;
using System.Net.Sockets;

namespace Server
{
    internal static partial class HTML
    {
        internal static partial class CGI
        {
            internal static void SendControlPanel(Socket connection)
            {
                /*
                 * 
                 * make html
                 * 
                 */

                Worker.CloseConnection(connection);
            }
        }
    }
}