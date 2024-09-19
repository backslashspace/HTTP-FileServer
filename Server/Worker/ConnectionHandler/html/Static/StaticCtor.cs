using System;

#pragma warning disable CS8618

namespace Server
{
    internal static partial class HTML
    {
        internal static partial class HTML_STATIC
        {
            internal static Boolean IsInitialized { get; private set; }

            static HTML_STATIC()
            {
                InitializeErrors();

                LoadFavicon();

                IsInitialized = true;
            }
        }
    }   
}