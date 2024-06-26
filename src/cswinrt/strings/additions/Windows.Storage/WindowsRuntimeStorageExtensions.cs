// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.IO
{
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Microsoft.Win32.SafeHandles;
    using global::Windows.Storage;
    using global::Windows.Storage.FileProperties;
    using global::Windows.Storage.Streams;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Contains extension methods that provide convenience helpers for WinRT IO.
    /// </summary>
#if EMBED 
    internal 
#else 
    public 
#endif 
    static class WindowsRuntimeStorageExtensions
    {
        // Net5-specific extension methods
#if NET
        [global::System.Runtime.Versioning.SupportedOSPlatform("windows10.0.10240.0")]
        public static Task<Stream> OpenStreamForReadAsync(this IStorageFile windowsRuntimeFile)
        {
            if (windowsRuntimeFile == null)
                throw new ArgumentNullException(nameof(windowsRuntimeFile));

            return OpenStreamForReadAsyncCore(windowsRuntimeFile);
        }

        [global::System.Runtime.Versioning.SupportedOSPlatform("windows10.0.10240.0")]
        private static async Task<Stream> OpenStreamForReadAsyncCore(this IStorageFile windowsRuntimeFile)
        {
            Debug.Assert(windowsRuntimeFile != null);

            try
            {
                IRandomAccessStream windowsRuntimeStream = await windowsRuntimeFile.OpenAsync(FileAccessMode.Read)
                                                                 .AsTask().ConfigureAwait(continueOnCapturedContext: false);
                Stream managedStream = windowsRuntimeStream.AsStreamForRead();
                return managedStream;
            }
            catch (Exception ex)
            {
                // From this API, managed dev expect IO exceptions for "something wrong":
                WinRtIOHelper.NativeExceptionToIOExceptionInfo(ex).Throw();
                return null;
            }
        }

        [global::System.Runtime.Versioning.SupportedOSPlatform("windows10.0.10240.0")]
        public static Task<Stream> OpenStreamForWriteAsync(this IStorageFile windowsRuntimeFile)
        {
            if (windowsRuntimeFile == null)
                throw new ArgumentNullException(nameof(windowsRuntimeFile));

            return OpenStreamForWriteAsyncCore(windowsRuntimeFile, 0);
        }

        [global::System.Runtime.Versioning.SupportedOSPlatform("windows10.0.10240.0")]
        private static async Task<Stream> OpenStreamForWriteAsyncCore(this IStorageFile windowsRuntimeFile, long offset)
        {
            Debug.Assert(windowsRuntimeFile != null);
            Debug.Assert(offset >= 0);

            try
            {
                IRandomAccessStream windowsRuntimeStream = await windowsRuntimeFile.OpenAsync(FileAccessMode.ReadWrite)
                                                                 .AsTask().ConfigureAwait(continueOnCapturedContext: false);
                Stream managedStream = windowsRuntimeStream.AsStreamForWrite();
                managedStream.Position = offset;
                return managedStream;
            }
            catch (Exception ex)
            {
                // From this API, managed dev expect IO exceptions for "something wrong":
                WinRtIOHelper.NativeExceptionToIOExceptionInfo(ex).Throw();
                return null;
            }
        }

        [global::System.Runtime.Versioning.SupportedOSPlatform("windows10.0.10240.0")]
        public static Task<Stream> OpenStreamForReadAsync(this IStorageFolder rootDirectory, string relativePath)
        {
            if (rootDirectory == null)
                throw new ArgumentNullException(nameof(rootDirectory));

            if (relativePath == null)
                throw new ArgumentNullException(nameof(relativePath));

            if (string.IsNullOrWhiteSpace(relativePath))
                throw new ArgumentException(global::Windows.Storage.SR.Argument_RelativePathMayNotBeWhitespaceOnly, nameof(relativePath));

            return OpenStreamForReadAsyncCore(rootDirectory, relativePath);
        }

        [global::System.Runtime.Versioning.SupportedOSPlatform("windows10.0.10240.0")]
        private static async Task<Stream> OpenStreamForReadAsyncCore(this IStorageFolder rootDirectory, string relativePath)
        {
            Debug.Assert(rootDirectory != null);
            Debug.Assert(!string.IsNullOrWhiteSpace(relativePath));

            try
            {
                IStorageFile windowsRuntimeFile = await rootDirectory.GetFileAsync(relativePath)
                                                        .AsTask().ConfigureAwait(continueOnCapturedContext: false);
                Stream managedStream = await windowsRuntimeFile.OpenStreamForReadAsync()
                                             .ConfigureAwait(continueOnCapturedContext: false);
                return managedStream;
            }
            catch (Exception ex)
            {
                // From this API, managed dev expect IO exceptions for "something wrong":
                WinRtIOHelper.NativeExceptionToIOExceptionInfo(ex).Throw();
                return null;
            }
        }

        [global::System.Runtime.Versioning.SupportedOSPlatform("windows10.0.10240.0")]
        public static Task<Stream> OpenStreamForWriteAsync(this IStorageFolder rootDirectory, string relativePath,
                                                           CreationCollisionOption creationCollisionOption)
        {
            if (rootDirectory == null)
                throw new ArgumentNullException(nameof(rootDirectory));

            if (relativePath == null)
                throw new ArgumentNullException(nameof(relativePath));

            if (string.IsNullOrWhiteSpace(relativePath))
                throw new ArgumentException(global::Windows.Storage.SR.Argument_RelativePathMayNotBeWhitespaceOnly, nameof(relativePath));

            return OpenStreamForWriteAsyncCore(rootDirectory, relativePath, creationCollisionOption);
        }

        [global::System.Runtime.Versioning.SupportedOSPlatform("windows10.0.10240.0")]
        private static async Task<Stream> OpenStreamForWriteAsyncCore(this IStorageFolder rootDirectory, string relativePath,
                                                                      CreationCollisionOption creationCollisionOption)
        {
            Debug.Assert(rootDirectory != null);
            Debug.Assert(!string.IsNullOrWhiteSpace(relativePath));

            Debug.Assert(creationCollisionOption == CreationCollisionOption.FailIfExists
                                    || creationCollisionOption == CreationCollisionOption.GenerateUniqueName
                                    || creationCollisionOption == CreationCollisionOption.OpenIfExists
                                    || creationCollisionOption == CreationCollisionOption.ReplaceExisting,
                              "The specified creationCollisionOption has a value that is not a value we considered when devising the"
                            + " policy about Append-On-OpenIfExists used in this method. Apparently a new enum value was added to the"
                            + " CreationCollisionOption type and we need to make sure that the policy still makes sense.");

            try
            {
                // Open file and set up default options for opening it:

                IStorageFile windowsRuntimeFile = await rootDirectory.CreateFileAsync(relativePath, creationCollisionOption)
                                                                     .AsTask().ConfigureAwait(continueOnCapturedContext: false);
                long offset = 0;

                // If the specified creationCollisionOption was OpenIfExists, then we will try to APPEND, otherwise we will OVERWRITE:

                if (creationCollisionOption == CreationCollisionOption.OpenIfExists)
                {
                    BasicProperties fileProperties = await windowsRuntimeFile.GetBasicPropertiesAsync()
                                                           .AsTask().ConfigureAwait(continueOnCapturedContext: false);
                    ulong fileSize = fileProperties.Size;

                    Debug.Assert(fileSize <= long.MaxValue, ".NET streams assume that file sizes are not larger than Int64.MaxValue,"
                                                              + " so we are not supporting the situation where this is not the case.");
                    offset = checked((long)fileSize);
                }

                // Now open a file with the correct options:

                Stream managedStream = await OpenStreamForWriteAsyncCore(windowsRuntimeFile, offset).ConfigureAwait(continueOnCapturedContext: false);
                return managedStream;
            }
            catch (Exception ex)
            {
                // From this API, managed dev expect IO exceptions for "something wrong":
                WinRtIOHelper.NativeExceptionToIOExceptionInfo(ex).Throw();
                return null;
            }
        }
#endif

        public static SafeFileHandle CreateSafeFileHandle(
            this IStorageFile windowsRuntimeFile,
            FileAccess access = FileAccess.ReadWrite,
            FileShare share = FileShare.Read,
            FileOptions options = FileOptions.None)
        {
            if (windowsRuntimeFile == null)
                throw new ArgumentNullException(nameof(windowsRuntimeFile));

            HANDLE_ACCESS_OPTIONS accessOptions = FileAccessToHandleAccessOptions(access);
            HANDLE_SHARING_OPTIONS sharingOptions = FileShareToHandleSharingOptions(share);
            HANDLE_OPTIONS handleOptions = FileOptionsToHandleOptions(options);

            return global::ABI.Windows.Storage.IStorageItemHandleAccessMethods.Create(
                windowsRuntimeFile,
                accessOptions,
                sharingOptions,
                handleOptions,
                IntPtr.Zero);
        }

        public static SafeFileHandle CreateSafeFileHandle(
            this IStorageFolder rootDirectory,
            string relativePath,
            FileMode mode)
        {
            return rootDirectory.CreateSafeFileHandle(relativePath, mode, (mode == FileMode.Append ? FileAccess.Write : FileAccess.ReadWrite));
        }

        public static SafeFileHandle CreateSafeFileHandle(
            this IStorageFolder rootDirectory,
            string relativePath,
            FileMode mode,
            FileAccess access,
            FileShare share = FileShare.Read,
            FileOptions options = FileOptions.None)
        {
            if (rootDirectory == null)
                throw new ArgumentNullException(nameof(rootDirectory));
            if (relativePath == null)
                throw new ArgumentNullException(nameof(relativePath));

            HANDLE_CREATION_OPTIONS creationOptions = FileModeToCreationOptions(mode);
            HANDLE_ACCESS_OPTIONS accessOptions = FileAccessToHandleAccessOptions(access);
            HANDLE_SHARING_OPTIONS sharingOptions = FileShareToHandleSharingOptions(share);
            HANDLE_OPTIONS handleOptions = FileOptionsToHandleOptions(options);

            return global::ABI.Windows.Storage.IStorageFolderHandleAccessMethods.Create(
                rootDirectory,
                relativePath,
                creationOptions,
                accessOptions,
                sharingOptions,
                handleOptions,
                IntPtr.Zero);
        }

        private static HANDLE_ACCESS_OPTIONS FileAccessToHandleAccessOptions(FileAccess access) =>
            access switch
            {
                FileAccess.ReadWrite => HANDLE_ACCESS_OPTIONS.HAO_READ | HANDLE_ACCESS_OPTIONS.HAO_WRITE,
                FileAccess.Read => HANDLE_ACCESS_OPTIONS.HAO_READ,
                FileAccess.Write => HANDLE_ACCESS_OPTIONS.HAO_WRITE,
                _ => throw new ArgumentOutOfRangeException(nameof(access), access, null),
            };

        private static HANDLE_SHARING_OPTIONS FileShareToHandleSharingOptions(FileShare share)
        {
            if ((share & FileShare.Inheritable) != 0)
                throw new NotSupportedException(global::Windows.Storage.SR.NotSupported_Inheritable);
            if (share < FileShare.None || share > (FileShare.ReadWrite | FileShare.Delete))
                throw new ArgumentOutOfRangeException(nameof(share), share, null);

            HANDLE_SHARING_OPTIONS sharingOptions = HANDLE_SHARING_OPTIONS.HSO_SHARE_NONE;
            if ((share & FileShare.Read) != 0)
                sharingOptions |= HANDLE_SHARING_OPTIONS.HSO_SHARE_READ;
            if ((share & FileShare.Write) != 0)
                sharingOptions |= HANDLE_SHARING_OPTIONS.HSO_SHARE_WRITE;
            if ((share & FileShare.Delete) != 0)
                sharingOptions |= HANDLE_SHARING_OPTIONS.HSO_SHARE_DELETE;

            return sharingOptions;
        }

        private static HANDLE_OPTIONS FileOptionsToHandleOptions(FileOptions options)
        {
            if ((options & FileOptions.Encrypted) != 0)
                throw new NotSupportedException(global::Windows.Storage.SR.NotSupported_Encrypted);
            if (options != FileOptions.None && (options &
                ~(FileOptions.WriteThrough | FileOptions.Asynchronous | FileOptions.RandomAccess | FileOptions.DeleteOnClose |
                  FileOptions.SequentialScan | (FileOptions)0x20000000 /* NoBuffering */)) != 0)
                throw new ArgumentOutOfRangeException(nameof(options), options, null);

            return (HANDLE_OPTIONS)options;
        }

        private static HANDLE_CREATION_OPTIONS FileModeToCreationOptions(FileMode mode)
        {
            if (mode < FileMode.CreateNew || mode > FileMode.Append)
                throw new ArgumentOutOfRangeException(nameof(mode), mode, null);

            if (mode == FileMode.Append)
                return HANDLE_CREATION_OPTIONS.HCO_CREATE_ALWAYS;

            return (HANDLE_CREATION_OPTIONS)mode;
        }
    }
}
