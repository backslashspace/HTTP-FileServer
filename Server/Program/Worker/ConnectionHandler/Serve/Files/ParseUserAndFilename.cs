using System;
using System.Web;

namespace Server
{
    internal static partial class Worker
    {
        private static Boolean ParseUserAndFilename(String urlEncodedContent, out String decodedLoginUsername, out String decodedFilename)
        {
            Int32 contentLength = urlEncodedContent.Length;

            if (contentLength < 44 && contentLength > 1024)
            {
                decodedLoginUsername = null!;
                decodedFilename = null!;
                return false;
            }

            Int32 filenameStartIndex = 0;
            Int32 filenameLength = 0;

            Int32 usernameStartIndex = 0;
            Int32 usernameLength = 0;

            for (Int32 i = 0; i < contentLength; ++i)
            {
                if (filenameLength == 0 && i + 5 <= contentLength)
                {
                    if (urlEncodedContent[i] == 'n'
                        && urlEncodedContent[i + 1] == 'a'
                        && urlEncodedContent[i + 2] == 'm'
                        && urlEncodedContent[i + 3] == 'e'
                        && urlEncodedContent[i + 4] == '=')
                    {
                        filenameStartIndex = i + 5;

                        for (Int32 j = filenameStartIndex; i < contentLength; ++j)
                        {
                            if (contentLength == j || urlEncodedContent[j] == '&')
                            {
                                filenameLength = j - filenameStartIndex;
                                i = j;
                                goto OUTER;
                            }
                        }
                    }
                }

                if (usernameLength == 0 && i + 5 <= contentLength)
                {
                    if (urlEncodedContent[i] == 'u'
                        && urlEncodedContent[i + 1] == 's'
                        && urlEncodedContent[i + 2] == 'e'
                        && urlEncodedContent[i + 3] == 'r'
                        && urlEncodedContent[i + 4] == '=')
                    {
                        usernameStartIndex = i + 5;

                        for (Int32 j = usernameStartIndex; i < contentLength; ++j)
                        {
                            if (contentLength == j || urlEncodedContent[j] == '&')
                            {
                                usernameLength = j - usernameStartIndex;
                                i = j;
                                goto OUTER;
                            }
                        }
                    }
                }

            OUTER:;
            }

            if (filenameLength == 0 || usernameLength == 0)
            {
                decodedLoginUsername = null!;
                decodedFilename = null!;
                return false;
            }

            decodedLoginUsername = HttpUtility.UrlDecode(urlEncodedContent.Substring(usernameStartIndex, usernameLength));
            decodedFilename = HttpUtility.UrlDecode(urlEncodedContent.Substring(filenameStartIndex, filenameLength));

            return true;
        }
    }
}