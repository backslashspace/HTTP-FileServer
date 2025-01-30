using System;

namespace Server
{
    internal static partial class Worker
    {
        private static Boolean ParseCredentials(String credentials, out String urlEncodedUsername, out String urlEncodedPassword)
        {
            if (credentials == null || credentials.Length < 17 || credentials.Length > 527)
            {
                urlEncodedUsername = null!;
                urlEncodedPassword = null!;
                return false;
            }

            Int32 contentLength = credentials.Length;
            Int32 usernameLength = 0;

            if (credentials[0] != 'u'
                || credentials[1] != 's'
                || credentials[2] != 'e'
                || credentials[3] != 'r'
                || credentials[4] != '=')
            {
                urlEncodedUsername = null!;
                urlEncodedPassword = null!;
                return false;
            }

            for (Int32 i = 5; i < contentLength; ++i)
            {
                if (credentials[i] == '&')
                {
                    usernameLength = i - 5;
                    break;
                }
            }

            if (usernameLength == 0 || contentLength < usernameLength + 16)
            {
                urlEncodedUsername = null!;
                urlEncodedPassword = null!;
                return false;
            }

            if (credentials[usernameLength + 6] != 'p'
                || credentials[usernameLength + 7] != 'a'
                || credentials[usernameLength + 8] != 's'
                || credentials[usernameLength + 9] != 's'
                || credentials[usernameLength + 10] != 'w'
                || credentials[usernameLength + 11] != 'o'
                || credentials[usernameLength + 12] != 'r'
                || credentials[usernameLength + 13] != 'd'
                || credentials[usernameLength + 14] != '=')
            {
                urlEncodedUsername = null!;
                urlEncodedPassword = null!;
                return false;
            }

            urlEncodedUsername = credentials.Substring(5, usernameLength);
            urlEncodedPassword = credentials.Substring(15 + usernameLength, contentLength - usernameLength - 15);

            return true;
        }
    }
}