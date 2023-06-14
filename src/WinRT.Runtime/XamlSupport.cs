// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace WinRT
{
#if EMBED
    internal
#else
    public
#endif
    enum XamlNamespace
    {
        /// <summary>
        /// Microsoft.UI.Xaml
        /// </summary>
        MUX,
        /// <summary>
        /// Windows.UI.Xaml
        /// </summary>
        WUX
    }

#if EMBED
    internal
#else
    public
#endif
    static class XamlSupport
    {
        internal static XamlNamespace XamlNamespace = XamlNamespace.MUX;

        /// <summary>
        /// Set the XAML namespace for use with WinRT.
        /// </summary>
        /// <param name="xamlNamespace">The XAML namespace for use with WinRT.</param>
        public static void SetXamlNamespace(XamlNamespace xamlNamespace)
        {
            if (XamlNamespace == xamlNamespace)
                return;

            XamlNamespace = xamlNamespace;
        }
    }
}
