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

        private static Boolean ParseUserConfiguration(String content, out UserConfiguration userConfiguration)
        {
            Int32 contentLength = content.Length;

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
                    if (content[i] == 'l'
                        && content[i + 1] == 'o'
                        && content[i + 2] == 'g'
                        && content[i + 3] == 'i'
                        && content[i + 4] == 'n'
                        && content[i + 5] == 'U'
                        && content[i + 6] == 's'
                        && content[i + 7] == 'e'
                        && content[i + 8] == 'r'
                        && content[i + 9] == 'n'
                        && content[i + 10] == 'a'
                        && content[i + 11] == 'm'
                        && content[i + 12] == 'e'
                        && content[i + 13] == '=')
                    {
                        loginUsernameStartIndex = i + 14;

                        for (Int32 j = loginUsernameStartIndex; i < contentLength; ++j)
                        {
                            if (contentLength == j || content[j] == '&')
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
                    if (content[i] == 'd'
                        && content[i + 1] == 'i'
                        && content[i + 2] == 's'
                        && content[i + 3] == 'p'
                        && content[i + 4] == 'l'
                        && content[i + 5] == 'a'
                        && content[i + 6] == 'y'
                        && content[i + 7] == 'U'
                        && content[i + 8] == 's'
                        && content[i + 9] == 'e'
                        && content[i + 10] == 'r'
                        && content[i + 11] == 'n'
                        && content[i + 12] == 'a'
                        && content[i + 13] == 'm'
                        && content[i + 14] == 'e'
                        && content[i + 15] == '=')
                    {
                        displayUsernameStartIndex = i + 16;

                        for (Int32 j = displayUsernameStartIndex; i < contentLength; ++j)
                        {
                            if (contentLength == j || content[j] == '&')
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
                    if (content[i] == 'p'
                        && content[i + 1] == 'a'
                        && content[i + 2] == 's'
                        && content[i + 3] == 's'
                        && content[i + 4] == 'w'
                        && content[i + 5] == 'o'
                        && content[i + 6] == 'r'
                        && content[i + 7] == 'd'
                        && content[i + 8] == '=')
                    {
                        passwordStartIndex = i + 9;

                        for (Int32 j = passwordStartIndex; i < contentLength; ++j)
                        {
                            if (contentLength == j || content[j] == '&')
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
                    if (content[i] == 'i'
                        && content[i + 1] == 's'
                        && content[i + 2] == 'E'
                        && content[i + 3] == 'n'
                        && content[i + 4] == 'a'
                        && content[i + 5] == 'b'
                        && content[i + 6] == 'l'
                        && content[i + 7] == 'e'
                        && content[i + 8] == 'd'
                        && content[i + 9] == '=')
                    {
                        isEnabledStartIndex = i + 10;

                        for (Int32 j = isEnabledStartIndex; i < contentLength; ++j)
                        {
                            if (contentLength == j || content[j] == '&')
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
                    if (content[i] == 'w'
                        && content[i + 1] == 'r'
                        && content[i + 2] == 'i'
                        && content[i + 3] == 't'
                        && content[i + 4] == 'e'
                        && content[i + 5] == '=')
                    {
                        writeStartIndex = i + 6;

                        for (Int32 j = writeStartIndex; i < contentLength; ++j)
                        {
                            if (contentLength == j || content[j] == '&')
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
                    if (content[i] == 'r'
                        && content[i + 1] == 'e'
                        && content[i + 2] == 'a'
                        && content[i + 3] == 'd'
                        && content[i + 4] == '=')
                    {
                        readStartIndex = i + 5;

                        for (Int32 j = readStartIndex; i < contentLength; ++j)
                        {
                            if (contentLength == j || content[j] == '&')
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

            if (loginUsernameLength == 0 || displayUsernameLength == 0 || passwordLength == 0)
            {
                userConfiguration = new();
                return false;
            }

            String loginUsername = content.Substring(loginUsernameStartIndex, loginUsernameLength);
            String displayUsername = content.Substring(displayUsernameStartIndex, displayUsernameLength);
            String password = content.Substring(passwordStartIndex, passwordLength);

            Boolean isEnabled = false;
            Boolean read = false;
            Boolean write = false;

            if (isEnabledLength != 0)
            {
                if (content[isEnabledStartIndex] == 'o' && content[isEnabledStartIndex + 1] == 'n')
                {
                    isEnabled = true;
                }
            }

            if (readLength != 0)
            {
                if (content[readStartIndex] == 'o' && content[readStartIndex + 1] == 'n')
                {
                    read = true;
                }
            }

            if (writeLength != 0)
            {
                if (content[writeStartIndex] == 'o' && content[writeStartIndex + 1] == 'n')
                {
                    write = true;
                }
            }

            userConfiguration = new(HttpUtility.UrlDecode(loginUsername), HttpUtility.UrlDecode(displayUsername), HttpUtility.UrlDecode(password), isEnabled, read, write);
            return true;
        }
    }
}