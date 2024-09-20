using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    internal static partial class Worker
    {
        private static void ValidateLogin(Socket connection, String header)
        {
            CloseConnection(connection);
        }
    }
}