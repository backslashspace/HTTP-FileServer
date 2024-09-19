using System.Net.Sockets;

namespace Server
{
    internal static partial class HTML
    {
        internal static partial class STATIC
        {
            /// <summary>Bad Request</summary>
            internal static void Send_400(Socket connection)
            {
                connection.Send(_400_response, 0, _400_response.Length, SocketFlags.None);
                Worker.CloseConnection(connection);
            }

            /// <summary>Not Found</summary>
            internal static void Send_404(Socket connection)
            {
                connection.Send(_404_response, 0, _404_response.Length, SocketFlags.None);
                Worker.CloseConnection(connection);
            }

            /// <summary>Too Many Requests</summary>
            internal static void Send_429(Socket connection)
            {
                connection.Send(_429_response, 0, _429_response.Length, SocketFlags.None);
                Worker.CloseConnection(connection);
            }

            /// <summary>Request Header Fields Too Large</summary>
            internal static void Send_431(Socket connection)
            {
                connection.Send(_431_response, 0, _431_response.Length, SocketFlags.None);
                Worker.CloseConnection(connection);
            }
        }
    }
}