using System;

namespace Server
{
    internal static partial class Tools
    {
        internal static Boolean GetUInt16(Span<Byte> buffer, out UInt16 value)
        {
            Int32 length = buffer.Length;
            Int32 internalValue = 0;

            for (Int32 i = 0; i < length; i++)
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