using System;
using System.Net.Sockets;

namespace Server
{
    internal static partial class Worker
    {
        private static void Login(Socket connection, String header)
        {
            switch (GetRequestMethode(header))
            {
                case RequestMethode.GET:
                    HTML.SendLoginPage(connection);
                    return;

                case RequestMethode.POST:
                    PerformLogin(connection, header);
                    return;

                default:
                    HTML.STATIC.Send_400(connection);
                    return;
            }
        }

        private static void PerformLogin(Socket connection, String header)
        {
            if (GetContent(header, connection, out String content))
            {
                
            }


            HTML.STATIC.Send_401(connection);
        }














        public enum RequestMethode : UInt16
        {
            Invalid = 0,
            GET = 1,
            POST = 2
        }

        private static RequestMethode GetRequestMethode(String header)
        {
            if (header == null || header.Length < 8) return RequestMethode.Invalid;

            if (header[0] == 'G' || header[0] == 'g')
            {
                return RequestMethode.GET;
            }
            if ((header[0] == 'P' || header[0] == 'p') && (header[1] == 'O' || header[1] == 'o'))
            {
                return RequestMethode.POST;
            }

            return RequestMethode.Invalid;
        }





        
    }
}