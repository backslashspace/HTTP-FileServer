namespace Server
{
    internal static partial class HTTP
    {
        internal static partial class ERRORS
        {
            /// <summary>Bad Request</summary>
            internal static void Send_400(SecureSocket connection)
            {
                connection.SslStream!.Write(_400_response, 0, _400_response.Length);
                connection.Close();
            }

            /// <summary>Unauthorized</summary>
            internal static void Send_401(SecureSocket connection)
            {
                connection.SslStream!.Write(_401_response, 0, _401_response.Length);
                connection.Close();
            }

            /// <summary>Forbidden</summary>
            internal static void Send_403(SecureSocket connection)
            {
                connection.SslStream!.Write(_403_response, 0, _403_response.Length);
                connection.Close();
            }

            /// <summary>Not Found</summary>
            internal static void Send_404(SecureSocket connection)
            {
                connection.SslStream!.Write(_404_response, 0, _404_response.Length);
                connection.Close();
            }

            /// <summary>Conflict</summary>
            internal static void Send_409(SecureSocket connection)
            {
                connection.SslStream!.Write(_409_response, 0, _409_response.Length);
                connection.Close();
            }

            /// <summary>Too Many Requests</summary>
            internal static void Send_429(SecureSocket connection)
            {
                connection.SslStream!.Write(_429_response, 0, _429_response.Length);
                connection.Close();
            }

            /// <summary>Request Header Fields Too Large</summary>
            internal static void Send_431(SecureSocket connection)
            {
                connection.SslStream!.Write(_431_response, 0, _431_response.Length);
                connection.Close();
            }

            /// <summary>Internal Server Error</summary>
            internal static void Send_500(SecureSocket connection)
            {
                connection.SslStream!.Write(_500_response, 0, _500_response.Length);
                connection.Close();
            }

            /// <summary>Not Implemented</summary>
            internal static void Send_501(SecureSocket connection)
            {
                connection.SslStream!.Write(_501_response, 0, _501_response.Length);
                connection.Close();
            }

            /// <summary>Insufficient Storage</summary>
            internal static void Send_507(SecureSocket connection)
            {
                connection.SslStream!.Write(_507_response, 0, _507_response.Length);
                connection.Close();
            }
        }
    }
}