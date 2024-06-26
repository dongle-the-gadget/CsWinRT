﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using WinRT;
using WinRT.Interop;

namespace WinRT.Interop
{
    /// <summary>
    /// An interface for managed objects representing activation factories for WinRT types.
    /// </summary>
    [WindowsRuntimeType]
    [Guid("00000035-0000-0000-C000-000000000046")]
    [WindowsRuntimeHelperType(typeof(global::ABI.WinRT.Interop.IActivationFactory))]
#if EMBED
    internal
#else
    public
#endif
    interface IActivationFactory
    {
        /// <summary>
        /// Activates an instance of the target WinRT type.
        /// </summary>
        /// <returns>The resulting instance.</returns>
        /// <exception cref="NotImplementedException">
        /// Thrown if the operation is not available on the activation factory type in use. For instance,
        /// that is the case if the associated type is static or does not have a default constructor.
        /// </exception>
        IntPtr ActivateInstance();
    }
}

namespace ABI.WinRT.Interop
{
#if EMBED 
    internal
#else
    public
#endif
    static class IActivationFactoryMethods
    {
        public static global::System.Guid IID => global::WinRT.Interop.IID.IID_IActivationFactory;

        public static IntPtr AbiToProjectionVftablePtr => IActivationFactory.Vftbl.AbiToProjectionVftablePtr;

#if !NET
        /// <summary>
        /// Activates an instance from a given activation factory <see cref="IObjectReference"/> instance.
        /// </summary>
        /// <param name="objectReference">The input activation factory <see cref="IObjectReference"/> instance to use.</param>
        /// <returns>The resulting <see cref="IObjectReference"/> instance created from <paramref name="objectReference"/>.</returns>
        /// <remarks>
        /// <para>This method assumes <paramref name="objectReference"/> is wrapping an <c>IActivationFactory</c> instance (with no validation).</para>
        /// <para>This method is only meant to be used by the generated projections, and not by consumers of CsWinRT directly.</para>
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static unsafe IObjectReference ActivateInstanceUnsafe(IObjectReference objectReference)
        {
            IntPtr thisPtr = objectReference.ThisPtr;
            IntPtr instancePtr;

            ExceptionHelpers.ThrowExceptionForHR((*(delegate* unmanaged[Stdcall]<IntPtr, IntPtr*, int>**)thisPtr)[6](thisPtr, &instancePtr));

            GC.KeepAlive(objectReference);

            try
            {
                return ComWrappersSupport.GetObjectReferenceForInterface<IUnknownVftbl>(instancePtr, global::WinRT.Interop.IID.IID_IInspectable, requireQI: false);
            }
            finally
            {
                MarshalInspectable<object>.DisposeAbi(instancePtr);
            }
        }
#else
        /// <summary>
        /// Activates an instance from a given activation factory <see cref="IObjectReference"/> instance with the result
        /// pointing to the interface for the given iid.
        /// </summary>
        /// <param name="objectReference">The input activation factory <see cref="IObjectReference"/> instance to use.</param>
        /// <param name="iid">The iid for the interface which the resulting <see cref="IObjectReference"/> will be for.</param>
        /// <returns>The resulting <see cref="IObjectReference"/> instance created from <paramref name="objectReference"/> and QI to the given <paramref name="iid"/>.</returns>
        /// <remarks>
        /// <para>This method assumes <paramref name="objectReference"/> is wrapping an <c>IActivationFactory</c> instance (with no validation).</para>
        /// <para>This method is only meant to be used by the generated projections, and not by consumers of CsWinRT directly.</para>
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static unsafe IObjectReference ActivateInstanceUnsafe(IObjectReference objectReference, Guid iid)
        {
            IntPtr thisPtr = objectReference.ThisPtr;
            IntPtr instancePtr;

            ExceptionHelpers.ThrowExceptionForHR((*(delegate* unmanaged[Stdcall]<IntPtr, IntPtr*, int>**)thisPtr)[6](thisPtr, &instancePtr));

            GC.KeepAlive(objectReference);

            try
            {
                return ComWrappersSupport.GetObjectReferenceForInterface<IUnknownVftbl>(instancePtr, iid, requireQI: true);
            }
            finally
            {
                MarshalInspectable<object>.DisposeAbi(instancePtr);
            }
        }
#endif
    }

#if !NET
    [global::WinRT.ObjectReferenceWrapper(nameof(_obj))]
#endif
    [Guid("00000035-0000-0000-C000-000000000046")]
#if EMBED
    internal
#else
    public 
#endif
    unsafe class IActivationFactory : global::WinRT.Interop.IActivationFactory
    {
        [Guid("00000035-0000-0000-C000-000000000046")]
        public struct Vftbl
        {
            internal IInspectable.Vftbl IInspectableVftbl;
#if !NET
            private delegate int ActivateInstance_Delegate(IntPtr thisPtr, IntPtr* pobj);
            private void* _ActivateInstance_0;
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr*, int> ActivateInstance_0 { get => (delegate* unmanaged[Stdcall]<IntPtr, IntPtr*, int>)_ActivateInstance_0; set => _ActivateInstance_0 = value; }
#else
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr*, int> ActivateInstance_0;
#endif
            public static readonly IntPtr AbiToProjectionVftablePtr;

#if !NET
            private static readonly Delegate[] DelegateCache = new Delegate[1];
#endif
            static unsafe Vftbl()
            {
                AbiToProjectionVftablePtr = ComWrappersSupport.AllocateVtableMemory(typeof(Vftbl), sizeof(global::WinRT.IInspectable.Vftbl) + sizeof(IntPtr) * 1);
                (*(Vftbl*)AbiToProjectionVftablePtr) = new Vftbl
                {
                    IInspectableVftbl = global::WinRT.IInspectable.Vftbl.AbiToProjectionVftable,
#if !NET
                    _ActivateInstance_0 = (void*)Marshal.GetFunctionPointerForDelegate(DelegateCache[0] = new ActivateInstance_Delegate(Do_Abi_ActivateInstance_0))
#else
                    ActivateInstance_0 = &Do_Abi_ActivateInstance_0
#endif
                };
            }

#if NET
            [UnmanagedCallersOnly(CallConvs = new [] { typeof(CallConvStdcall) })]
#endif
            private static unsafe int Do_Abi_ActivateInstance_0(IntPtr thisPtr, IntPtr* result)
            {
                IntPtr __result = default;

                *result = default;

                try
                {
                    __result = global::WinRT.ComWrappersSupport.FindObject<global::WinRT.Interop.IActivationFactory>(thisPtr).ActivateInstance();
                    *result = __result;
                }
                catch (Exception __exception__)
                {
                    global::WinRT.ExceptionHelpers.SetErrorInfo(__exception__);
                    return global::WinRT.ExceptionHelpers.GetHRForException(__exception__);
                }
                return 0;
            }
        }

#if !NET
        public static implicit operator IActivationFactory(IObjectReference obj) => (obj != null) ? new IActivationFactory(obj) : null;
#endif
#if NET
        protected readonly ObjectReference<IUnknownVftbl> _obj;
#else
        protected readonly ObjectReference<Vftbl> _obj;
#endif
        public IObjectReference ObjRef { get => _obj; }
        public IntPtr ThisPtr => _obj.ThisPtr;

        public IActivationFactory(IObjectReference obj)
        {
#if NET
            _obj = obj.As<IUnknownVftbl>(IID.IID_IActivationFactory);
#else
            _obj = obj.As<Vftbl>(IID.IID_IActivationFactory);
#endif
        }

        public unsafe IntPtr ActivateInstance()
        {
            IntPtr __retval = default;
            try
            {
#if NET
                IntPtr thisPtr = ThisPtr;
                Marshal.ThrowExceptionForHR(((delegate* unmanaged[Stdcall]<IntPtr, IntPtr*, int>)(*(void***)thisPtr)[6])(thisPtr, &__retval));
#else
                global::WinRT.ExceptionHelpers.ThrowExceptionForHR(_obj.Vftbl.ActivateInstance_0(ThisPtr, &__retval));
#endif
                return __retval;
            }
            finally
            {
            }
        }
    }
}
