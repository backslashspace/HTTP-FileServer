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

            /// <summary>Unauthorized</summary>
            internal static void Send_401(Socket connection)
            {
                connection.Send(_401_response, 0, _401_response.Length, SocketFlags.None);
                Worker.CloseConnection(connection);
            }

            /// <summary>Forbidden</summary>
            internal static void Send_403(Socket connection)
            {
                connection.Send(_403_response, 0, _403_response.Length, SocketFlags.None);
                Worker.CloseConnection(connection);
            }

            /// <summary>Not Found</summary>
            internal static void Send_404(Socket connection)
            {
                connection.Send(_404_response, 0, _404_response.Length, SocketFlags.None);
                Worker.CloseConnection(connection);
            }

            /// <summary>Conflict</summary>
            internal static void Send_409(Socket connection)
            {
                connection.Send(_409_response, 0, _409_response.Length, SocketFlags.None);
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

            /// <summary>Internal Server Error</summary>
            internal static void Send_500(Socket connection)
            {
                connection.Send(_500_response, 0, _500_response.Length, SocketFlags.None);
                Worker.CloseConnection(connection);
            }

            /// <summary>Not Implemented</summary>
            internal static void Send_501(Socket connection)
            {
                connection.Send(_501_response, 0, _501_response.Length, SocketFlags.None);
                Worker.CloseConnection(connection);
            }

            /// <summary>Insufficient Storage</summary>
            internal static void Send_507(Socket connection)
            {
                connection.Send(_507_response, 0, _507_response.Length, SocketFlags.None);
                Worker.CloseConnection(connection);
            }
        }
    }
}