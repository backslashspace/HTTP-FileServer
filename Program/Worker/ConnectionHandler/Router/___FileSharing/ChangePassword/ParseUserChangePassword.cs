using System;
using System.Web;

namespace Server
{
    internal static partial class Worker
    {
        private readonly ref struct UserChangePassword
        {
            internal UserChangePassword(String loginUsername, String displayUsername, String password)
            {
                LoginUsername = loginUsername;
                DisplayUsername = displayUsername;
                Password = password;
            }

            internal readonly String LoginUsername;
            internal readonly String DisplayUsername;
            internal readonly String Password;
        }

        private static Boolean ParseUserChangePassword(String urlEncodedContent, out UserChangePassword userConfiguration)
        {
            Int32 contentLength = urlEncodedContent.Length;

            if (contentLength < 44 && contentLength > 1024)
            {
                userConfiguration = new();
                return false;
            }

            Int32 loginUsernameStartIndex = 0;
            Int32 loginUsernameLength = 0;

            Int32 displayUsernameStartIndex = 0;
            Int32 displayUsernameLength = 0;

            Int32 passwordStartIndex = 0;
            Int32 passwordLength = 0;

            for (Int32 i = 0; i < contentLength; ++i)
            {
                if (loginUsernameLength == 0 && i + 14 <= contentLength)
                {
                    if (urlEncodedContent[i] == 'l'
                        && urlEncodedContent[i + 1] == 'o'
                        && urlEncodedContent[i + 2] == 'g'
                        && urlEncodedContent[i + 3] == 'i'
                        && urlEncodedContent[i + 4] == 'n'
                        && urlEncodedContent[i + 5] == 'U'
                        && urlEncodedContent[i + 6] == 's'
                        && urlEncodedContent[i + 7] == 'e'
                        && urlEncodedContent[i + 8] == 'r'
                        && urlEncodedContent[i + 9] == 'n'
                        && urlEncodedContent[i + 10] == 'a'
                        && urlEncodedContent[i + 11] == 'm'
                        && urlEncodedContent[i + 12] == 'e'
                        && urlEncodedContent[i + 13] == '=')
                    {
                        loginUsernameStartIndex = i + 14;

                        for (Int32 j = loginUsernameStartIndex; i < contentLength; ++j)
                        {
                            if (contentLength == j || urlEncodedContent[j] == '&')
                            {
                                loginUsernameLength = j - loginUsernameStartIndex;
                                i = j;
                                goto OUTER;
                            }
                        }
                    }
                }

                if (displayUsernameLength == 0 && i + 16 <= contentLength)
                {
                    if (urlEncodedContent[i] == 'd'
                        && urlEncodedContent[i + 1] == 'i'
                        && urlEncodedContent[i + 2] == 's'
                        && urlEncodedContent[i + 3] == 'p'
                        && urlEncodedContent[i + 4] == 'l'
                        && urlEncodedContent[i + 5] == 'a'
                        && urlEncodedContent[i + 6] == 'y'
                        && urlEncodedContent[i + 7] == 'U'
                        && urlEncodedContent[i + 8] == 's'
                        && urlEncodedContent[i + 9] == 'e'
                        && urlEncodedContent[i + 10] == 'r'
                        && urlEncodedContent[i + 11] == 'n'
                        && urlEncodedContent[i + 12] == 'a'
                        && urlEncodedContent[i + 13] == 'm'
                        && urlEncodedContent[i + 14] == 'e'
                        && urlEncodedContent[i + 15] == '=')
                    {
                        displayUsernameStartIndex = i + 16;

                        for (Int32 j = displayUsernameStartIndex; i < contentLength; ++j)
                        {
                            if (contentLength == j || urlEncodedContent[j] == '&')
                            {
                                displayUsernameLength = j - displayUsernameStartIndex;
                                i = j;
                                goto OUTER;
                            }
                        }
                    }
                }

                if (passwordLength == 0 && i + 9 <= contentLength)
                {
                    if (urlEncodedContent[i] == 'p'
                        && urlEncodedContent[i + 1] == 'a'
                        && urlEncodedContent[i + 2] == 's'
                        && urlEncodedContent[i + 3] == 's'
                        && urlEncodedContent[i + 4] == 'w'
                        && urlEncodedContent[i + 5] == 'o'
                        && urlEncodedContent[i + 6] == 'r'
                        && urlEncodedContent[i + 7] == 'd'
                        && urlEncodedContent[i + 8] == '=')
                    {
                        passwordStartIndex = i + 9;

                        for (Int32 j = passwordStartIndex; i < contentLength; ++j)
                        {
                            if (contentLength == j || urlEncodedContent[j] == '&')
                            {
                                passwordLength = j - passwordStartIndex;
                                i = j;
                                goto OUTER;
                            }
                        }
                    }
                }

            OUTER:;
            }

            // ## ## ## ## ## ## ## ## ## ## ## ## ## ## ## ##

            String loginUsername;
            String displayUsername;
            String password;

            if (loginUsernameLength == 0)
            {
                userConfiguration = new();
                return false;
            }

            loginUsername = urlEncodedContent.Substring(loginUsernameStartIndex, loginUsernameLength);

            if (displayUsernameLength != 0) displayUsername = urlEncodedContent.Substring(displayUsernameStartIndex, displayUsernameLength);
            else displayUsername = null!;

            if (passwordLength != 0) password = urlEncodedContent.Substring(passwordStartIndex, passwordLength);
            else password = null!;

            userConfiguration = new(HttpUtility.UrlDecode(loginUsername), HttpUtility.UrlDecode(displayUsername), HttpUtility.UrlDecode(password));
            return true;
        }
    }
}