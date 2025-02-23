using BSS.Logging;
using System;
using System.Net;
using System.Text;

namespace Server
{
    internal static partial class ConfigurationLoader
    {
        private unsafe static Boolean Parse(ref Span<Byte> buffer, Configuration* configuration)
        {
            if (buffer.Length < 85)
            {
                Log.FastLog("Config file was less than 85 chars", LogSeverity.Error, LOG_WORD);
                return false;
            }

            Int32 bufferLength = buffer.Length;

            // # # # # # # # # # # # # # # # # # # # # # # # # # # # # # #

            Span<Byte> ipBuffer = null;
            Span<Byte> securePortBuffer = null;
            Span<Byte> enableRedirectBuffer = null;
            Span<Byte> redirectPortBuffer = null;
            Span<Byte> threadsBuffer = null;
            Span<Byte> protectionBuffer = null;

            Int32 startIndex = 0;

            for (Int32 i = 0; i < bufferLength; ++i)
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
                    startIndex = i + 9;
                    i = startIndex;

                    for (; i < bufferLength; ++i)
                    {
                        if (buffer[i] == '\r' || buffer[i] == ' ' || buffer[i] == 'n')
                        {
                            ipBuffer = buffer[startIndex..i];
                            if (bufferLength > i + 1 && buffer[i + 1] == '\n') ++i;
                            goto OUTER;
                        }
                        else if (bufferLength == i + 1)
                        {
                            ipBuffer = buffer[startIndex..(i + 1)];
                            goto OUTER;
                        }
                    }
                }

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
                    startIndex = i + 11;
                    i = startIndex;

                    for (; i < bufferLength; ++i)
                    {
                        if (buffer[i] == '\r' || buffer[i] == ' ' || buffer[i] == 'n')
                        {
                            securePortBuffer = buffer[startIndex..i];
                            if (bufferLength > i + 1 && buffer[i + 1] == '\n') ++i;
                            goto OUTER;
                        }
                        else if (bufferLength == i + 1)
                        {
                            securePortBuffer = buffer[startIndex..(i + 1)];
                            goto OUTER;
                        }
                    }
                }

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
                    startIndex = i + 15;
                    i = startIndex;

                    for (; i < bufferLength; ++i)
                    {
                        if (buffer[i] == '\r' || buffer[i] == ' ' || buffer[i] == 'n')
                        {
                            enableRedirectBuffer = buffer[startIndex..i];
                            if (bufferLength > i + 1 && buffer[i + 1] == '\n') ++i;
                            goto OUTER;
                        }
                        else if (bufferLength == i + 1)
                        {
                            enableRedirectBuffer = buffer[startIndex..(i + 1)];
                            goto OUTER;
                        }
                    }
                }

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
                    startIndex = i + 13;
                    i = startIndex;

                    for (; i < bufferLength; ++i)
                    {
                        if (buffer[i] == '\r' || buffer[i] == ' ' || buffer[i] == 'n')
                        {
                            redirectPortBuffer = buffer[startIndex..i];
                            if (bufferLength > i + 1 && buffer[i + 1] == '\n') ++i;
                            goto OUTER;
                        }
                        else if (bufferLength == i + 1)
                        {
                            redirectPortBuffer = buffer[startIndex..(i + 1)];
                            goto OUTER;
                        }
                    }
                }

                if (buffer[i] == 't'
                && buffer[i + 1] == 'h'
                && buffer[i + 2] == 'r'
                && buffer[i + 3] == 'e'
                && buffer[i + 4] == 'a'
                && buffer[i + 5] == 'd'
                && buffer[i + 6] == 's'
                && buffer[i + 7] == '=')
                {
                    startIndex = i + 8;
                    i = startIndex;

                    for (; i < bufferLength; ++i)
                    {
                        if (buffer[i] == '\r' || buffer[i] == ' ' || buffer[i] == 'n')
                        {
                            threadsBuffer = buffer[startIndex..i];
                            if (bufferLength > i + 1 && buffer[i + 1] == '\n') ++i;
                            goto OUTER;
                        }
                        else if (bufferLength == i + 1)
                        {
                            threadsBuffer = buffer[startIndex..(i + 1)];
                            goto OUTER;
                        }
                    }
                }

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
                    startIndex = i + 11;
                    i = startIndex;

                    for (; i < bufferLength; ++i)
                    {
                        if (buffer[i] == '\r' || buffer[i] == ' ' || buffer[i] == 'n')
                        {
                            protectionBuffer = buffer[startIndex..i];
                            if (bufferLength > i + 1 && buffer[i + 1] == '\n') ++i;
                            goto OUTER;
                        }
                        else if (bufferLength == i + 1)
                        {
                            protectionBuffer = buffer[startIndex..(i + 1)];
                            goto OUTER;
                        }
                    }
                }

            OUTER:;

                if (!ipBuffer.IsEmpty
                    && !securePortBuffer.IsEmpty
                    && !enableRedirectBuffer.IsEmpty
                    && !redirectPortBuffer.IsEmpty
                    && !threadsBuffer.IsEmpty
                    && !protectionBuffer.IsEmpty) break;
            }

            //

            if (ipBuffer.IsEmpty
                    || securePortBuffer.IsEmpty
                    || enableRedirectBuffer.IsEmpty
                    || redirectPortBuffer.IsEmpty
                    || threadsBuffer.IsEmpty
                    || protectionBuffer.IsEmpty)
            {
                Log.FastLog("Config incomplete or invalid", LogSeverity.Error, LOG_WORD);
                return false;
            }

            // # # # # # # # # # # # # # # # # # # # # # # # # # # # # # #

            Span<Char> ip = stackalloc Char[ipBuffer.Length];
            for (Int32 i = 0; i < ipBuffer.Length; ++i)
            {
                ip[i] = (Char)ipBuffer[i];
            }
            if (!IPAddress.TryParse(ip, out IPAddress? parsedIP))
            {
                Log.FastLog("listenIP was invalid, expected ip, found: " + ip.ToString(), LogSeverity.Error, LOG_WORD);
                return false;
            }
            configuration->ListenerAddress = parsedIP!;

            if (!GetUInt16(ref securePortBuffer, out configuration->HttpsListenerPort))
            {
                Log.FastLog("securePort was invalid, expected number from 0 to 65535, found: " + Encoding.ASCII.GetString(securePortBuffer), LogSeverity.Error, LOG_WORD);
                return false;
            }
            if (configuration->HttpsListenerPort == 0)
            {
                Log.FastLog("securePort was invalid, expected number from 1 to 65535, port was 0", LogSeverity.Error, LOG_WORD);
                return false;
            }

            *(Byte*)&configuration->EnableHttpRedirector = (Char)enableRedirectBuffer[0] switch
            {
                't' => 1,
                'T' => 1,
                '1' => 1,
                'f' => 0,
                'F' => 0,
                '0' => 0,
                _ => 2
            };
            if (*(Byte*)&configuration->EnableHttpRedirector == 2)
            {
                Log.FastLog("enableRedirect was invalid, expected t,T,1,f,F,0 - found: " + (Char)enableRedirectBuffer[0], LogSeverity.Error, LOG_WORD);
                configuration->EnableHttpRedirector = false;
                return false;
            }

            if (!GetUInt16(ref redirectPortBuffer, out configuration->HttpListenerPort))
            {
                Log.FastLog("redirectPort was invalid, expected number from 0 to 65535, found: " + Encoding.ASCII.GetString(redirectPortBuffer), LogSeverity.Error, LOG_WORD);
                return false;
            }
            if (configuration->HttpListenerPort == 0)
            {
                Log.FastLog("redirectPort was invalid, expected number from 1 to 65535, port was 0", LogSeverity.Error, LOG_WORD);
                return false;
            }

            if (!GetUInt16(ref threadsBuffer, out configuration->ThreadPoolThreads))
            {
                Log.FastLog("threads was invalid, expected number from 0 to 65535, found: " + Encoding.ASCII.GetString(threadsBuffer), LogSeverity.Error, LOG_WORD);
                return false;
            }
            if (configuration->ThreadPoolThreads == 0)
            {
                Log.FastLog("threads was invalid, expected number from 1 to 65535, port was 0", LogSeverity.Error, LOG_WORD);
                return false;
            }

            *(Byte*)&configuration->EnableServiceProtection = (Char)protectionBuffer[0] switch
            {
                't' => 1,
                'T' => 1,
                '1' => 1,
                'f' => 0,
                'F' => 0,
                '0' => 0,
                _ => 2
            };
            if (*(Byte*)&configuration->EnableServiceProtection == 2)
            {
                Log.FastLog("protection was invalid, expected t,T,1,f,F,0 - found: " + (Char)protectionBuffer[0], LogSeverity.Error, LOG_WORD);
                configuration->EnableServiceProtection = false;
                return false;
            }

            return true;
        }

        private static Boolean GetUInt16(ref Span<Byte> buffer, out UInt16 value)
        {
            Int32 internalValue= 0;

            for (Int32 i = 0; i < buffer.Length; i++)
            {
                // shift whole to left by doing x10, then add the new number
                internalValue = (internalValue * 10) + (buffer[i] - 48);
            }

            if (internalValue < 1 && internalValue > UInt16.MaxValue)
            {
                value = 0;
                return false;
            }

            value = (UInt16)internalValue;
            return true;
        }
    }
}