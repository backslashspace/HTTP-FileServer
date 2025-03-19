using System;
using System.Text;
using System.Web;

namespace Server
{
    internal static partial class Worker
    {
        private readonly ref struct UserConfiguration
        {
            internal UserConfiguration(String loginUsername, String displayUsername, String password, Boolean isEnabled, Boolean write, Boolean read)
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
            internal readonly Boolean Write;
            internal readonly Boolean Read;
        }

        private static Boolean ParseUserConfiguration(Span<Byte> buffer, out UserConfiguration userConfiguration)
        {
            Int32 length = buffer.Length;

            if (length < 32)
            {
                userConfiguration = new();
                return false;
            }

            String loginUsername = null!;
            Int32 loginUsernameStartIndex;

            String displayUsername = null!;
            Int32 displayUsernameStartIndex;

            String password = null!;
            Int32 passwordStartIndex;

            Boolean? isEnabled = null!;
            Boolean? read = null!;
            Boolean? write = null!;

            for (Int32 i = 0; i < length;)
            {
                if (loginUsername == null && i + 3 < length)
                {
                    if (buffer[i] == 'l' && buffer[i + 1] == 'U' && buffer[i + 2] == '=')
                    {
                        loginUsernameStartIndex = i + 3;

                        for (; i < length; ++i)
                        {
                            if (buffer[i] == '&')
                            {
                                loginUsername = Encoding.UTF8.GetString(buffer[loginUsernameStartIndex..i]);
                                ++i;
                                break;
                            }
                        }
                    }
                }

                if (displayUsername == null && i + 3 < length)
                {
                    if (buffer[i] == 'd' && buffer[i + 1] == 'U' && buffer[i + 2] == '=')
                    {
                        displayUsernameStartIndex = i + 3;

                        for (; i < length; ++i)
                        {
                            if (buffer[i] == '&')
                            {
                                displayUsername = Encoding.UTF8.GetString(buffer[displayUsernameStartIndex..i]);
                                ++i;
                                break;
                            }
                        }
                    }
                }

                if (password == null && i + 3 < length)
                {
                    if (buffer[i] == 'p' && buffer[i + 1] == 'w' && buffer[i + 2] == '=')
                    {
                        passwordStartIndex = i + 3;

                        for (; i < length; ++i)
                        {
                            if (buffer[i] == '&')
                            {
                                password = Encoding.UTF8.GetString(buffer[passwordStartIndex..i]);
                                ++i;
                                break;
                            }
                        }
                    }
                }

                if (isEnabled == null && i + 4 < length)
                {
                    if (buffer[i] == 'a' && buffer[i + 1] == 'c' && buffer[i + 2] == '=')
                    {
                        i += 3;

                        isEnabled = buffer[i] == 'o' && buffer[i + 1] == 'n';

                        while (i < length)
                        {
                            if (buffer[i++] == '&') break;
                        }
                    }
                }

                if (write == null && i + 4 < length)
                {
                    if (buffer[i] == 'w' && buffer[i + 1] == 'r' && buffer[i + 2] == '=')
                    {
                        i += 3;

                        write = buffer[i] == 'o' && buffer[i + 1] == 'n';

                        while (i < length)
                        {
                            if (buffer[i++] == '&') break;
                        }
                    }
                }

                if (read == null && i + 4 < length)
                {
                    if (buffer[i] == 'r' && buffer[i + 1] == 'd' && buffer[i + 2] == '=')
                    {
                        i += 3;

                        read = buffer[i] == 'o' && buffer[i + 1] == 'n';

                        while (i < length)
                        {
                            if (buffer[i++] == '&') break;
                        }
                    }
                }
            }

            if (loginUsername == null || displayUsername == null || password == null || isEnabled == null || write == null || read == null)
            {
                userConfiguration = new();
                return false;
            }

            loginUsername = HttpUtility.UrlDecode(loginUsername);
            displayUsername = HttpUtility.UrlDecode(displayUsername);
            password = HttpUtility.UrlDecode(password);

            userConfiguration = new(loginUsername!, displayUsername!, password!, (Boolean)isEnabled!, (Boolean)write!, (Boolean)read!);
            return true;
        }
    }
}