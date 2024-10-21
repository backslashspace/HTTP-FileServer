using System;
using System.Web;

namespace Server
{
    internal static partial class Worker
    {
        private readonly ref struct UserConfiguration
        {
            internal UserConfiguration(String loginUsername, String displayUsername, String password, Boolean isEnabled, Boolean read, Boolean write)
            {
                LoginUsername = loginUsername;
                DisplayUsername = displayUsername;
                Password = password;

                IsEnabled = isEnabled;
                Read = read;
                Write = write;
            }

            internal readonly String LoginUsername;
            internal readonly String DisplayUsername;
            internal readonly String Password;

            internal readonly Boolean IsEnabled;
            internal readonly Boolean Read;
            internal readonly Boolean Write;
        }

        private static Boolean ParseUserConfiguration(String urlEncodedContent, out UserConfiguration userConfiguration, Boolean updateMode = false)
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

            Int32 isEnabledStartIndex = 0;
            Int32 isEnabledLength = 0;

            Int32 readStartIndex = 0;
            Int32 readLength = 0;

            Int32 writeStartIndex = 0;
            Int32 writeLength = 0;

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

                //

                if (isEnabledLength == 0 && i + 10 <= contentLength)
                {
                    if (urlEncodedContent[i] == 'i'
                        && urlEncodedContent[i + 1] == 's'
                        && urlEncodedContent[i + 2] == 'E'
                        && urlEncodedContent[i + 3] == 'n'
                        && urlEncodedContent[i + 4] == 'a'
                        && urlEncodedContent[i + 5] == 'b'
                        && urlEncodedContent[i + 6] == 'l'
                        && urlEncodedContent[i + 7] == 'e'
                        && urlEncodedContent[i + 8] == 'd'
                        && urlEncodedContent[i + 9] == '=')
                    {
                        isEnabledStartIndex = i + 10;

                        for (Int32 j = isEnabledStartIndex; i < contentLength; ++j)
                        {
                            if (contentLength == j || urlEncodedContent[j] == '&')
                            {
                                isEnabledLength = j - isEnabledStartIndex;
                                i = j;
                                goto OUTER;
                            }
                        }
                    }
                }

                if (writeLength == 0 && i + 6 <= contentLength)
                {
                    if (urlEncodedContent[i] == 'w'
                        && urlEncodedContent[i + 1] == 'r'
                        && urlEncodedContent[i + 2] == 'i'
                        && urlEncodedContent[i + 3] == 't'
                        && urlEncodedContent[i + 4] == 'e'
                        && urlEncodedContent[i + 5] == '=')
                    {
                        writeStartIndex = i + 6;

                        for (Int32 j = writeStartIndex; i < contentLength; ++j)
                        {
                            if (contentLength == j || urlEncodedContent[j] == '&')
                            {
                                writeLength = j - writeStartIndex;
                                i = j;
                                goto OUTER;
                            }
                        }
                    }
                }

                if (readLength == 0 && i + 5 <= contentLength)
                {
                    if (urlEncodedContent[i] == 'r'
                        && urlEncodedContent[i + 1] == 'e'
                        && urlEncodedContent[i + 2] == 'a'
                        && urlEncodedContent[i + 3] == 'd'
                        && urlEncodedContent[i + 4] == '=')
                    {
                        readStartIndex = i + 5;

                        for (Int32 j = readStartIndex; i < contentLength; ++j)
                        {
                            if (contentLength == j || urlEncodedContent[j] == '&')
                            {
                                readLength = j - readStartIndex;
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

            if (!updateMode)
            {
                if (loginUsernameLength == 0 || displayUsernameLength == 0 || passwordLength == 0)
                {
                    userConfiguration = new();
                    return false;
                }

                loginUsername = urlEncodedContent.Substring(loginUsernameStartIndex, loginUsernameLength);
                displayUsername = urlEncodedContent.Substring(displayUsernameStartIndex, displayUsernameLength);
                password = urlEncodedContent.Substring(passwordStartIndex, passwordLength);
            }
            else
            {
                if (loginUsernameLength == 0)
                {
                    userConfiguration = new();
                    return false;
                }

                loginUsername = urlEncodedContent.Substring(loginUsernameStartIndex, loginUsernameLength);

                if (displayUsernameLength != 0) displayUsername = urlEncodedContent.Substring(displayUsernameStartIndex, displayUsernameLength);
                else displayUsername = null;

                if (passwordLength != 0) password = urlEncodedContent.Substring(passwordStartIndex, passwordLength);
                else password = null;
            }

            Boolean isEnabled = false;
            Boolean read = false;
            Boolean write = false;

            if (isEnabledLength != 0)
            {
                if (urlEncodedContent[isEnabledStartIndex] == 'o' && urlEncodedContent[isEnabledStartIndex + 1] == 'n')
                {
                    isEnabled = true;
                }
            }

            if (readLength != 0)
            {
                if (urlEncodedContent[readStartIndex] == 'o' && urlEncodedContent[readStartIndex + 1] == 'n')
                {
                    read = true;
                }
            }

            if (writeLength != 0)
            {
                if (urlEncodedContent[writeStartIndex] == 'o' && urlEncodedContent[writeStartIndex + 1] == 'n')
                {
                    write = true;
                }
            }

            userConfiguration = new(HttpUtility.UrlDecode(loginUsername), HttpUtility.UrlDecode(displayUsername), HttpUtility.UrlDecode(password), isEnabled, read, write);
            return true;
        }
    }
}