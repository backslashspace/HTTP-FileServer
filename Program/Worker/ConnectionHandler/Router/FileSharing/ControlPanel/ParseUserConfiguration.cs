using System;

namespace Server
{
    internal static partial class Worker
    {
        private ref struct UserConfiguration
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

            internal Boolean IsEnabled;
            internal Boolean Write;
            internal Boolean Read;
        }

        private unsafe static Boolean ParseUserConfiguration(ref Span<Byte> buffer, out UserConfiguration userConfiguration)
        {
            Int32 length = buffer.Length;

            String loginUsername = null!;
            String displayUsername = null!;
            String password = null!;

            Int32 isEnabled = 2;
            Int32 write = 2;
            Int32 read = 2;

            Int32 currentStartIndex = 0;

            for (Int32 i = 0; i < length; ++i)
            {
            OUTER:

                if (loginUsername == null && i + 3 < length)
                {
                    if (buffer[i] == 'l'
                        && buffer[i + 1] == 'U'
                        && buffer[i + 2] == '='
                        && buffer[i + 3] != '&')
                    {
                        currentStartIndex = i += 3;

                        for (++i; i < length; ++i)
                        {
                            if (buffer[i] == '&')
                            {
                                Int32 sliceLength = i - currentStartIndex;
                                loginUsername = new('\0', sliceLength);

                                fixed (Char* loginUsernamePointer = &loginUsername.GetPinnableReference())
                                {
                                    for (Int32 j = 0; j < sliceLength; ++j)
                                    {
                                        loginUsernamePointer[j] = (Char)buffer[currentStartIndex + j];
                                    }
                                }

                                ++i;
                                goto OUTER;
                            }
                        }

                        if (i == length)
                        {
                            Int32 sliceLength = i - currentStartIndex;
                            loginUsername = new('\0', sliceLength);

                            fixed (Char* loginUsernamePointer = &loginUsername.GetPinnableReference())
                            {
                                for (Int32 j = 0; j < sliceLength; ++j)
                                {
                                    loginUsernamePointer[j] = (Char)buffer[currentStartIndex + j];
                                }
                            }

                            break;
                        }
                    }
                }

                if (displayUsername == null && i + 3 < length)
                {
                    if (buffer[i] == 'd'
                        && buffer[i + 1] == 'U'
                        && buffer[i + 2] == '='
                        && buffer[i + 3] != '&')
                    {
                        currentStartIndex = i += 3;

                        for (++i; i < length; ++i)
                        {
                            if (buffer[i] == '&')
                            {
                                Int32 sliceLength = i - currentStartIndex;
                                displayUsername = new('\0', sliceLength);

                                fixed (Char* displayUsernamePointer = &displayUsername.GetPinnableReference())
                                {
                                    for (Int32 j = 0; j < sliceLength; ++j)
                                    {
                                        displayUsernamePointer[j] = (Char)buffer[currentStartIndex + j];
                                    }
                                }

                                ++i;
                                goto OUTER;
                            }
                        }

                        if (i == length)
                        {
                            Int32 sliceLength = i - currentStartIndex;
                            displayUsername = new('\0', sliceLength);

                            fixed (Char* displayUsernamePointer = &displayUsername.GetPinnableReference())
                            {
                                for (Int32 j = 0; j < sliceLength; ++j)
                                {
                                    displayUsernamePointer[j] = (Char)buffer[currentStartIndex + j];
                                }
                            }

                            break;
                        }
                    }
                }

                if (password == null && i + 3 < length)
                {
                    if (buffer[i] == 'p'
                        && buffer[i + 1] == 'w'
                        && buffer[i + 2] == '='
                        && buffer[i + 3] != '&')
                    {
                        currentStartIndex = i += 3;

                        for (++i; i < length; ++i)
                        {
                            if (buffer[i] == '&')
                            {
                                Int32 sliceLength = i - currentStartIndex;
                                password = new('\0', sliceLength);

                                fixed (Char* passwordPointer = &password.GetPinnableReference())
                                {
                                    for (Int32 j = 0; j < sliceLength; ++j)
                                    {
                                        passwordPointer[j] = (Char)buffer[currentStartIndex + j];
                                    }
                                }

                                ++i;
                                goto OUTER;
                            }
                        }

                        if (i == length)
                        {
                            Int32 sliceLength = i - currentStartIndex;
                            password = new('\0', sliceLength);

                            fixed (Char* passwordPointer = &password.GetPinnableReference())
                            {
                                for (Int32 j = 0; j < sliceLength; ++j)
                                {
                                    passwordPointer[j] = (Char)buffer[currentStartIndex + j];
                                }
                            }

                            break;
                        }
                    }
                }

                if (isEnabled == 2 && i + 4 < length)
                {
                    if (buffer[i] == 'a'
                         && buffer[i + 1] == 'c'
                         && buffer[i + 2] == '='
                         && buffer[i + 3] != '&')
                    {
                        currentStartIndex = i += 4;

                        isEnabled = buffer[i] == 'n' ? 1 : 0;

                        for (++i; i < length; ++i)
                        {
                            if (buffer[i] == '&')
                            {
                                ++i;
                                goto OUTER;
                            }
                        }
                    }
                }

                if (write == 2 && i + 4 < length)
                {
                    if (buffer[i] == 'w'
                         && buffer[i + 1] == 'r'
                         && buffer[i + 2] == '='
                         && buffer[i + 3] != '&')
                    {
                        currentStartIndex = i += 4;

                        write = buffer[i] == 'n' ? 1 : 0;

                        for (++i; i < length; ++i)
                        {
                            if (buffer[i] == '&')
                            {
                                ++i;
                                goto OUTER;
                            }
                        }
                    }
                }

                if (read == 2 && i + 4 < length)
                {
                    if (buffer[i] == 'r'
                         && buffer[i + 1] == 'd'
                         && buffer[i + 2] == '='
                         && buffer[i + 3] != '&')
                    {
                        currentStartIndex = i += 4;

                        read = buffer[i] == 'n' ? 1 : 0;

                        for (++i; i < length; ++i)
                        {
                            if (buffer[i] == '&')
                            {
                                ++i;
                                goto OUTER;
                            }
                        }
                    }
                }
            }

            userConfiguration = new(loginUsername!, displayUsername!, password!, *(Boolean*)&isEnabled, *(Boolean*)&write, *(Boolean*)&read);
            return true;
        }
    }
}