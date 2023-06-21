// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.ComponentModel;
using System.Collections;
using System.Collections.Specialized;
using System.Windows.Input;

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

            if (xamlNamespace == XamlNamespace.WUX)
            {
                Projections.ReregisterCustomAbiTypeMappingNoLock(typeof(PropertyChangedEventArgs), typeof(ABI.System.ComponentModel.WUX.PropertyChangedEventArgs), "Windows.UI.Xaml.Data.PropertyChangedEventArgs", isRuntimeClass: true);
                Projections.ReregisterCustomAbiTypeMappingNoLock(typeof(PropertyChangedEventHandler), typeof(ABI.System.ComponentModel.WUX.PropertyChangedEventHandler), "Windows.UI.Xaml.Data.PropertyChangedEventHandler");
                Projections.ReregisterCustomAbiTypeMappingNoLock(typeof(INotifyPropertyChanged), typeof(ABI.System.ComponentModel.WUX.INotifyPropertyChanged), "Windows.UI.Xaml.Data.INotifyPropertyChanged");
                Projections.ReregisterCustomAbiTypeMappingNoLock(typeof(INotifyCollectionChanged), typeof(ABI.System.Collections.Specialized.WUX.INotifyCollectionChanged), "Windows.UI.Xaml.Interop.INotifyCollectionChanged");
                Projections.ReregisterCustomAbiTypeMappingNoLock(typeof(NotifyCollectionChangedAction), typeof(ABI.System.Collections.Specialized.WUX.NotifyCollectionChangedAction), "Windows.UI.Xaml.Interop.NotifyCollectionChangedAction");
                Projections.ReregisterCustomAbiTypeMappingNoLock(typeof(NotifyCollectionChangedEventArgs), typeof(ABI.System.Collections.Specialized.WUX.NotifyCollectionChangedEventArgs), "Windows.UI.Xaml.Interop.NotifyCollectionChangedEventArgs", isRuntimeClass: true);
                Projections.ReregisterCustomAbiTypeMappingNoLock(typeof(NotifyCollectionChangedEventHandler), typeof(ABI.System.Collections.Specialized.WUX.NotifyCollectionChangedEventHandler), "Windows.UI.Xaml.Interop.NotifyCollectionChangedEventHandler");
                Projections.ReregisterCustomAbiTypeMappingNoLock(typeof(IEnumerable), typeof(ABI.System.Collections.IEnumerable), "Windows.UI.Xaml.Interop.IBindableIterable");
                Projections.ReregisterCustomAbiTypeMappingNoLock(typeof(IList), typeof(ABI.System.Collections.IList), "Windows.UI.Xaml.Interop.IBindableVector");
                Projections.ReregisterCustomAbiTypeMappingNoLock(typeof(ICommand), typeof(ABI.System.Windows.Input.ICommand), "Windows.UI.Xaml.Interop.ICommand");

                Projections.UnregisterCustomAbiTypeMappingNoLock(typeof(DataErrorsChangedEventArgs), isRuntimeClass: true);
                Projections.UnregisterCustomAbiTypeMappingNoLock(typeof(INotifyDataErrorInfo));
                Projections.UnregisterCustomAbiTypeMappingNoLock(typeof(IServiceProvider));
            }
            else
            {
                Projections.RegisterCustomAbiTypeMappingNoLock(typeof(DataErrorsChangedEventArgs), typeof(ABI.System.ComponentModel.MUX.DataErrorsChangedEventArgs), "Microsoft.UI.Xaml.Data.DataErrorsChangedEventArgs", isRuntimeClass: true);
                Projections.RegisterCustomAbiTypeMappingNoLock(typeof(INotifyDataErrorInfo), typeof(ABI.System.ComponentModel.MUX.INotifyDataErrorInfo), "Microsoft.UI.Xaml.Data.INotifyDataErrorInfo");
                Projections.RegisterCustomAbiTypeMappingNoLock(typeof(IServiceProvider), typeof(ABI.System.MUX.IServiceProvider), "Microsoft.UI.Xaml.IXamlServiceProvider");

                Projections.ReregisterCustomAbiTypeMappingNoLock(typeof(PropertyChangedEventArgs), typeof(ABI.System.ComponentModel.MUX.PropertyChangedEventArgs), "Microsoft.UI.Xaml.Data.PropertyChangedEventArgs", isRuntimeClass: true);
                Projections.ReregisterCustomAbiTypeMappingNoLock(typeof(PropertyChangedEventHandler), typeof(ABI.System.ComponentModel.MUX.PropertyChangedEventHandler), "Microsoft.UI.Xaml.Data.PropertyChangedEventHandler");
                Projections.ReregisterCustomAbiTypeMappingNoLock(typeof(INotifyPropertyChanged), typeof(ABI.System.ComponentModel.MUX.INotifyPropertyChanged), "Microsoft.UI.Xaml.Data.INotifyPropertyChanged");
                Projections.ReregisterCustomAbiTypeMappingNoLock(typeof(ICommand), typeof(ABI.System.Windows.Input.ICommand), "Microsoft.UI.Xaml.Interop.ICommand");
                Projections.ReregisterCustomAbiTypeMappingNoLock(typeof(IEnumerable), typeof(ABI.System.Collections.IEnumerable), "Microsoft.UI.Xaml.Interop.IBindableIterable");
                Projections.ReregisterCustomAbiTypeMappingNoLock(typeof(IList), typeof(ABI.System.Collections.IList), "Microsoft.UI.Xaml.Interop.IBindableVector");
                Projections.ReregisterCustomAbiTypeMappingNoLock(typeof(INotifyCollectionChanged), typeof(ABI.System.Collections.Specialized.MUX.INotifyCollectionChanged), "Microsoft.UI.Xaml.Interop.INotifyCollectionChanged");
                Projections.ReregisterCustomAbiTypeMappingNoLock(typeof(NotifyCollectionChangedAction), typeof(ABI.System.Collections.Specialized.MUX.NotifyCollectionChangedAction), "Microsoft.UI.Xaml.Interop.NotifyCollectionChangedAction");
                Projections.ReregisterCustomAbiTypeMappingNoLock(typeof(NotifyCollectionChangedEventArgs), typeof(ABI.System.Collections.Specialized.MUX.NotifyCollectionChangedEventArgs), "Microsoft.UI.Xaml.Interop.NotifyCollectionChangedEventArgs", isRuntimeClass: true);
                Projections.ReregisterCustomAbiTypeMappingNoLock(typeof(NotifyCollectionChangedEventHandler), typeof(ABI.System.Collections.Specialized.MUX.NotifyCollectionChangedEventHandler), "Microsoft.UI.Xaml.Interop.NotifyCollectionChangedEventHandler");
            }

            XamlNamespace = xamlNamespace;
        }
    }
}
