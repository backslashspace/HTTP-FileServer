using System;
using System.Text;
using System.Web;

namespace Server
{
    internal static partial class Worker
    {
        private static Boolean ParseCredentials(Span<Byte> credentials, out String urlDecodedUsername, out String urlDecodedPassword)
        {
            Int32 contentLength = credentials.Length;
            Int32 usernameLength = 0;

            if (credentials[0] != 'u'
                || credentials[1] != 's'
                || credentials[2] != 'e'
                || credentials[3] != 'r'
                || credentials[4] != '=')
            {
                urlDecodedUsername = null!;
                urlDecodedPassword = null!;
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
                urlDecodedUsername = null!;
                urlDecodedPassword = null!;
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
                urlDecodedUsername = null!;
                urlDecodedPassword = null!;
                return false;
            }

            urlDecodedUsername = Encoding.UTF8.GetString(credentials.Slice(5, usernameLength));
            urlDecodedPassword = Encoding.UTF8.GetString(credentials.Slice(15 + usernameLength, contentLength - usernameLength - 15));

            urlDecodedUsername = HttpUtility.UrlDecode(urlDecodedUsername);
            urlDecodedPassword = HttpUtility.UrlDecode(urlDecodedPassword);

            return true;
        }
    }
}