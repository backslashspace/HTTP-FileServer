using System;
using System.Net;
using BSS.Logging;

namespace Server
{
    internal static partial class ConfigurationLoader
    {
        private unsafe static Boolean Parse(ref Span<Byte> buffer, Configuration* configuration)
        {
            Int32 length = buffer.Length;
            Int32 newLineWidth = 0;
            Char newLineStart = '\0';

            for (Int32 i = 0; i < length; ++i)
            {
                if (buffer[i] == '\r')
                {
                    if (i + 1 < length && buffer[i] == '\n')
                    {
                        // CRLF -> Windows
                        newLineWidth = 2;
                        newLineStart = '\r';
                        break;
                    }
                    else
                    {
                        // CR -> Mac
                        newLineWidth = 1;
                        newLineStart = '\r';
                        break;
                    }
                }

                if (buffer[i] == '\n')
                {
                    // LF -> Linux
                    newLineWidth = 1;
                    newLineStart = '\n';
                    break;
                }
            }

            if (newLineWidth == 0) return false;

            IPAddress listenIP = null!;
            UInt16 httpsPort = 0;
            UInt16 redirectPort = 0;
            Int32 enableRedirect = 2;
            UInt16 threads = 0;
            Int32 enableProtection = 2;
            Int32 enableReload = 2;
            String pfxPath = null!;
            String pfxPassphrase = null!;

            Int32 currentStartIndex = 0;
            Boolean increment = true;

            for (Int32 i = 0; i < length; i = increment ? ++i : i)
            {
            OUTER:
                if (buffer[i] == '#')
                {
                    for (++i; i < length; ++i)
                    {
                        if (buffer[i] == newLineStart)
                        {
                            i += newLineWidth;
                            goto OUTER;
                        }
                    }
                }

                if (buffer[i] == newLineStart)
                {
                    i += newLineWidth;
                    goto OUTER;
                }

                //

                increment = true;

                if (listenIP == null && i + 9 < length)
                {
                    if (buffer[i] == 'l'
                        && buffer[i + 1] == 'i'
                        && buffer[i + 2] == 's'
                        && buffer[i + 3] == 't'
                        && buffer[i + 4] == 'e'
                        && buffer[i + 5] == 'n'
                        && buffer[i + 6] == 'I'
                        && buffer[i + 7] == 'P'
                        && buffer[i + 8] == '=')
                    {
                        increment = false;
                        currentStartIndex = i += 9;

                        for (++i; i < length; ++i)
                        {
                            if (buffer[i] == newLineStart)
                            {
                                Int32 sliceLength = i - currentStartIndex;
                                Span<Char> charCopy = stackalloc Char[sliceLength];

                                for (Int32 j = 0; j < sliceLength; ++j)
                                {
                                    charCopy[j] = (Char)buffer[currentStartIndex + j];
                                }

                                if (!IPAddress.TryParse(charCopy, out listenIP!))
                                {
                                    Log.FastLog("listenIP was invalid: " + charCopy.ToString(), LogSeverity.Error, LOG_WORD);
                                    return false;
                                }

                                i += newLineWidth;
                                goto OUTER;
                            }
                        }

                        if (i == length)
                        {
                            Int32 sliceLength = i - currentStartIndex;
                            Span<Char> charCopy = stackalloc Char[sliceLength];

                            for (Int32 j = 0; j < sliceLength; ++j)
                            {
                                charCopy[j] = (Char)buffer[currentStartIndex + j];
                            }

                            if (!IPAddress.TryParse(charCopy, out listenIP!))
                            {
                                Log.FastLog("listenIP was invalid: " + charCopy.ToString(), LogSeverity.Error, LOG_WORD);
                                return false;
                            }

                            break;
                        }
                    }
                }

                if (httpsPort == 0 && i + 11 < length)
                {
                    if (buffer[i] == 's'
                        && buffer[i + 1] == 'e'
                        && buffer[i + 2] == 'c'
                        && buffer[i + 3] == 'u'
                        && buffer[i + 4] == 'r'
                        && buffer[i + 5] == 'e'
                        && buffer[i + 6] == 'P'
                        && buffer[i + 7] == 'o'
                        && buffer[i + 8] == 'r'
                        && buffer[i + 9] == 't'
                        && buffer[i + 10] == '=')
                    {
                        increment = false;
                        currentStartIndex = i += 11;

                        for (++i; i < length; ++i)
                        {
                            if (buffer[i] == newLineStart)
                            {
                                if (!Tools.GetUInt16(buffer[currentStartIndex..i], out httpsPort))
                                {
                                    Log.FastLog("securePort (https) was invalid: " + buffer[currentStartIndex..i].ToString(), LogSeverity.Error, LOG_WORD);
                                    return false;
                                }

                                i += newLineWidth;
                                goto OUTER;
                            }
                        }

                        if (i == length)
                        {
                            if (!Tools.GetUInt16(buffer[currentStartIndex..i], out httpsPort))
                            {
                                Log.FastLog("securePort (https) was invalid: " + buffer[currentStartIndex..i].ToString(), LogSeverity.Error, LOG_WORD);
                                return false;
                            }

                            break;
                        }
                    }
                }

                if (redirectPort == 0 && i + 13 < length)
                {
                    if (buffer[i] == 'r'
                        && buffer[i + 1] == 'e'
                        && buffer[i + 2] == 'd'
                        && buffer[i + 3] == 'i'
                        && buffer[i + 4] == 'r'
                        && buffer[i + 5] == 'e'
                        && buffer[i + 6] == 'c'
                        && buffer[i + 7] == 't'
                        && buffer[i + 8] == 'P'
                        && buffer[i + 9] == 'o'
                        && buffer[i + 10] == 'r'
                        && buffer[i + 11] == 't'
                        && buffer[i + 12] == '=')
                    {
                        increment = false;
                        currentStartIndex = i += 13;

                        for (++i; i < length; ++i)
                        {
                            if (buffer[i] == newLineStart)
                            {
                                if (!Tools.GetUInt16(buffer[currentStartIndex..i], out redirectPort))
                                {
                                    Log.FastLog("redirectPort (http) was invalid: " + buffer[currentStartIndex..i].ToString(), LogSeverity.Error, LOG_WORD);
                                    return false;
                                }

                                i += newLineWidth;
                                goto OUTER;
                            }
                        }

                        if (i == length)
                        {
                            if (!Tools.GetUInt16(buffer[currentStartIndex..i], out redirectPort))
                            {
                                Log.FastLog("redirectPort (http) was invalid: " + buffer[currentStartIndex..i].ToString(), LogSeverity.Error, LOG_WORD);
                                return false;
                            }

                            break;
                        }
                    }
                }

                if (enableRedirect == 2 && i + 15 < length)
                {
                    if (buffer[i] == 'e'
                        && buffer[i + 1] == 'n'
                        && buffer[i + 2] == 'a'
                        && buffer[i + 3] == 'b'
                        && buffer[i + 4] == 'l'
                        && buffer[i + 5] == 'e'
                        && buffer[i + 6] == 'R'
                        && buffer[i + 7] == 'e'
                        && buffer[i + 8] == 'd'
                        && buffer[i + 9] == 'i'
                        && buffer[i + 10] == 'r'
                        && buffer[i + 11] == 'e'
                        && buffer[i + 12] == 'c'
                        && buffer[i + 13] == 't'
                        && buffer[i + 14] == '=')
                    {
                        increment = false;
                        currentStartIndex = i += 15;

                        enableRedirect = buffer[i] == 't' ? 1 : 0;

                        for (++i; i < length; ++i)
                        {
                            if (buffer[i] == newLineStart)
                            {
                                i += newLineWidth;
                                goto OUTER;
                            }
                        }
                    }
                }

                if (threads == 0 && i + 8 < length)
                {
                    if (buffer[i] == 't'
                        && buffer[i + 1] == 'h'
                        && buffer[i + 2] == 'r'
                        && buffer[i + 3] == 'e'
                        && buffer[i + 4] == 'a'
                        && buffer[i + 5] == 'd'
                        && buffer[i + 6] == 's'
                        && buffer[i + 7] == '=')
                    {
                        increment = false;
                        currentStartIndex = i += 8;

                        for (++i; i < length; ++i)
                        {
                            if (buffer[i] == newLineStart)
                            {
                                if (!Tools.GetUInt16(buffer[currentStartIndex..i], out threads))
                                {
                                    Log.FastLog("threads was invalid: " + buffer[currentStartIndex..i].ToString(), LogSeverity.Error, LOG_WORD);
                                    return false;
                                }

                                i += newLineWidth;
                                goto OUTER;
                            }
                        }

                        if (i == length)
                        {
                            if (!Tools.GetUInt16(buffer[currentStartIndex..i], out threads))
                            {
                                Log.FastLog("threads was invalid: " + buffer[currentStartIndex..i].ToString(), LogSeverity.Error, LOG_WORD);
                                return false;
                            }

                            break;
                        }
                    }
                }

                if (enableProtection == 2 && i + 11 < length)
                {
                    if (buffer[i] == 'p'
                        && buffer[i + 1] == 'r'
                        && buffer[i + 2] == 'o'
                        && buffer[i + 3] == 't'
                        && buffer[i + 4] == 'e'
                        && buffer[i + 5] == 'c'
                        && buffer[i + 6] == 't'
                        && buffer[i + 7] == 'i'
                        && buffer[i + 8] == 'o'
                        && buffer[i + 9] == 'n'
                        && buffer[i + 10] == '=')
                    {
                        increment = false;
                        currentStartIndex = i += 11;

                        enableProtection = buffer[i] == 't' ? 1 : 0;

                        for (++i; i < length; ++i)
                        {
                            if (buffer[i] == newLineStart)
                            {
                                i += newLineWidth;
                                goto OUTER;
                            }
                        }
                    }
                }

                if (enableReload == 2 && i + 13 < length)
                {
                    if (buffer[i] == 'e'
                        && buffer[i + 1] == 'n'
                        && buffer[i + 2] == 'a'
                        && buffer[i + 3] == 'b'
                        && buffer[i + 4] == 'l'
                        && buffer[i + 5] == 'e'
                        && buffer[i + 6] == 'R'
                        && buffer[i + 7] == 'e'
                        && buffer[i + 8] == 'l'
                        && buffer[i + 9] == 'o'
                        && buffer[i + 10] == 'a'
                        && buffer[i + 11] == 'd'
                        && buffer[i + 12] == '=')
                    {
                        increment = false;
                        currentStartIndex = i += 13;

                        enableReload = buffer[i] == 't' ? 1 : 0;

                        for (++i; i < length; ++i)
                        {
                            if (buffer[i] == newLineStart)
                            {
                                i += newLineWidth;
                                goto OUTER;
                            }
                        }
                    }
                }

                if (pfxPath == null && i + 8 < length)
                {
                    if (buffer[i] == 'p'
                        && buffer[i + 1] == 'f'
                        && buffer[i + 2] == 'x'
                        && buffer[i + 3] == 'P'
                        && buffer[i + 4] == 'a'
                        && buffer[i + 5] == 't'
                        && buffer[i + 6] == 'h'
                        && buffer[i + 7] == '=')
                    {
                        increment = false;
                        currentStartIndex = i += 8;

                        for (++i; i < length; ++i)
                        {
                            if (buffer[i] == newLineStart)
                            {
                                Int32 sliceLength = i - currentStartIndex;
                                pfxPath = new('\0', sliceLength);

                                fixed (Char* pfxPathPointer = &pfxPath.GetPinnableReference())
                                {
                                    for (Int32 j = 0; j < sliceLength; ++j)
                                    {
                                        pfxPathPointer[j] = (Char)buffer[currentStartIndex + j];
                                    }
                                }

                                i += newLineWidth;
                                goto OUTER;
                            }
                        }

                        if (i == length)
                        {
                            Int32 sliceLength = i - currentStartIndex;
                            pfxPath = new('\0', sliceLength);

                            fixed (Char* pfxPathPointer = &pfxPath.GetPinnableReference())
                            {
                                for (Int32 j = 0; j < sliceLength; ++j)
                                {
                                    pfxPathPointer[j] = (Char)buffer[currentStartIndex + j];
                                }
                            }

                            break;
                        }
                    }
                }

                if (pfxPassphrase == null && i + 12 < length)
                {
                    if (buffer[i] == 'p'
                        && buffer[i + 1] == 'f'
                        && buffer[i + 2] == 'x'
                        && buffer[i + 3] == 'P'
                        && buffer[i + 4] == 'a'
                        && buffer[i + 5] == 's'
                        && buffer[i + 6] == 's'
                        && buffer[i + 7] == 'w'
                        && buffer[i + 8] == 'o'
                        && buffer[i + 9] == 'r'
                        && buffer[i + 10] == 'd'
                        && buffer[i + 11] == '=')
                    {
                        increment = false;
                        currentStartIndex = i += 12;

                        for (++i; i < length; ++i)
                        {
                            if (buffer[i] == newLineStart)
                            {
                                Int32 sliceLength = i - currentStartIndex;
                                pfxPassphrase = new('\0', sliceLength);

                                fixed (Char* pfxPassphrasePointer = &pfxPassphrase.GetPinnableReference())
                                {
                                    for (Int32 j = 0; j < sliceLength; ++j)
                                    {
                                        pfxPassphrasePointer[j] = (Char)buffer[currentStartIndex + j];
                                    }
                                }

                                i += newLineWidth;
                                goto OUTER;
                            }
                        }

                        if (i == length)
                        {
                            Int32 sliceLength = i - currentStartIndex;
                            pfxPassphrase = new('\0', sliceLength);

                            fixed (Char* pfxPassphrasePointer = &pfxPassphrase.GetPinnableReference())
                            {
                                for (Int32 j = 0; j < sliceLength; ++j)
                                {
                                    pfxPassphrasePointer[j] = (Char)buffer[currentStartIndex + j];
                                }
                            }

                            break;
                        }
                    }
                }
            }

            configuration->ListenerAddress = listenIP!;
            configuration->HttpsListenerPort = httpsPort;
            configuration->HttpListenerPort = redirectPort;
            configuration->EnableHttpRedirector = *(Boolean*)&enableRedirect;
            configuration->ThreadPoolThreads = threads;
            configuration->EnableServiceProtection = *(Boolean*)&enableProtection;
            configuration->EnableReload = *(Boolean*)&enableReload;
            configuration->PfxPath = pfxPath!;
            configuration->PfxPassphrase = pfxPassphrase!;

            return true;
        }
    }
}