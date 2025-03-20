using System;

namespace Server
{
    internal static partial class Tools
    {
        internal static Boolean IsNumberOrLetterOrDashOrUnderscore(String text)
        {
            Int32 length = text.Length;
            for (Int32 i = 0; i < length; i++)
            {
                if ((text[i] > 'z' || text[i] < 'a')
                    && (text[i] > 'Z' || text[i] < 'A')
                    && (text[i] > '_' || text[i] < '_')
                    && (text[i] > '9' || text[i] < '0')
                    && (text[i] > '-' || text[i] < '-'))
                {
                    return false;
                }
            }
            return true;
        }
    }
}