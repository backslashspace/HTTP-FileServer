using System;

#pragma warning disable CS8625

namespace Server
{
    internal static partial class Worker
    {
        private static Boolean ParseCredentials(String credentials, out String urlEncodedUserName, out String urlEncodedPassword)
        {
            if (credentials == null || credentials.Length < 17 || credentials.Length > 527)
            {
                urlEncodedUserName = null;
                urlEncodedPassword = null;
                return false;
            }

            Int32 contentLength = credentials.Length;
            Int32 userNameLength = 0;

            if (credentials[0] != 'u'
                || credentials[1] != 's'
                || credentials[2] != 'e'
                || credentials[3] != 'r'
                || credentials[4] != '=')
            {
                urlEncodedUserName = null;
                urlEncodedPassword = null;
                return false;
            }

            for (Int32 i = 5; i < contentLength; ++i)
            {
                if (credentials[i] == '&')
                {
                    userNameLength = i - 5;
                    break;
                }
            }

            if (userNameLength == 0 || contentLength < userNameLength + 16)
            {
                urlEncodedUserName = null;
                urlEncodedPassword = null;
                return false;
            }

            if (credentials[userNameLength + 6] != 'p'
                || credentials[userNameLength + 7] != 'a'
                || credentials[userNameLength + 8] != 's'
                || credentials[userNameLength + 9] != 's'
                || credentials[userNameLength + 10] != 'w'
                || credentials[userNameLength + 11] != 'o'
                || credentials[userNameLength + 12] != 'r'
                || credentials[userNameLength + 13] != 'd'
                || credentials[userNameLength + 14] != '=')
            {
                urlEncodedUserName = null;
                urlEncodedPassword = null;
                return false;
            }

            urlEncodedUserName = credentials.Substring(5, userNameLength);
            urlEncodedPassword = credentials.Substring(15 + userNameLength, contentLength - userNameLength - 15);

            return true;
        }
    }
}