using System;

namespace Server
{
    internal static partial class Tools
    {
        internal static Boolean IsNumberOrLetter(String text)
        {
            Int32 length = text.Length;
            for (Int32 i = 0; i < length; i++)
            {
                if ((text[i] > 'z' || text[i] < 'a')
                    && (text[i] > 'Z' || text[i] < 'A')
                    && (text[i] > '9' || text[i] < '0'))
                {
                    return false;
                }
            }
            return true;
        }
    }
}