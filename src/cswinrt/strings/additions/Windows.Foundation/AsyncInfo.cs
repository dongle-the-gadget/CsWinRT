// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Runtime.InteropServices.WindowsRuntime
{
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using global::Windows.Foundation;

    /// <summary><p>Provides factory methods to construct WinRT-compatible representations of asynchronous operations.</p>
    /// <p>The factory methods take as inputs functions (delegates) that provide managed Task objects;
    /// Different factory methods return different sub-interfaces of <code>Windows.Foundation.IAyncInfo</code>.
    /// When an asynchronous operation created by this factory is actually started (by calling <code>Start()</code>),
    /// the specified <code>Task</code>-provider delegate will be invoked to create the <code>Task</code> that will
    /// be wrapped by the to-WinRT adapter.</p> </summary>
#if NET
    [global::System.Runtime.Versioning.SupportedOSPlatform("windows10.0.10240.0")]
#endif
#if EMBED
    internal
#else
    public
#endif
    static class AsyncInfo
    {
        #region Factory methods for creating "normal" IAsyncInfo instances backed by a Task created by a pastProvider delegate

        /// <summary>
        /// Creates and starts an <see cref="IAsyncAction"/> instance from a function that generates
        /// a <see cref="System.Threading.Tasks.Task"/>.
        /// Use this overload if your task supports cancellation in order to hook-up the <code>Cancel</code>
        /// mechanism exposed by the created asynchronous action and the cancellation of your task.</summary>
        /// <param name="taskProvider">The function to invoke to create the task when the IAsyncInfo is started.
        /// The function is passed a <see cref="System.Threading.CancellationToken"/> that the task may monitor
        /// to be notified of a cancellation request;
        /// you may ignore the <code>CancellationToken</code> if your task does not support cancellation.</param>
        /// <returns>An unstarted <see cref="IAsyncAction"/> instance. </returns>
        public static IAsyncAction Run(Func<CancellationToken, Task> taskProvider)
        {
            if (taskProvider == null)
                throw new ArgumentNullException(nameof(taskProvider));

            return new TaskToAsyncActionAdapter(taskProvider);
        }


        /// <summary>
        /// Creates and starts an <see cref="IAsyncActionWithProgress{TProgress}"/> instance from a function
        /// that generates a <see cref="System.Threading.Tasks.Task"/>.
        /// Use this overload if your task supports cancellation and progress monitoring is order to:
        /// (1) hook-up the <code>Cancel</code> mechanism of the created asynchronous action and the cancellation of your task,
        /// and (2) hook-up the <code>Progress</code> update delegate exposed by the created async action and the progress updates
        /// published by your task.</summary>
        /// <param name="taskProvider">The function to invoke to create the task when the IAsyncInfo is started.
        /// The function is passed a <see cref="System.Threading.CancellationToken"/> that the task may monitor
        /// to be notified of a cancellation request;
        /// you may ignore the <code>CancellationToken</code> if your task does not support cancellation.
        /// It is also passed a <see cref="System.IProgress{TProgress}"/> instance to which progress updates may be published;
        /// you may ignore the <code>IProgress</code> if your task does not support progress reporting.</param>
        /// <returns>An unstarted <see cref="IAsyncActionWithProgress{TProgress}"/> instance.</returns>
        public static IAsyncActionWithProgress<TProgress> Run<TProgress>(Func<CancellationToken, IProgress<TProgress>, Task> taskProvider)
        {
            if (taskProvider == null)
                throw new ArgumentNullException(nameof(taskProvider));

            return new TaskToAsyncActionWithProgressAdapter<TProgress>(taskProvider);
        }


        /// <summary>
        /// Creates and starts  an <see cref="IAsyncOperation{TResult}"/> instance from a function
        /// that generates a <see cref="System.Threading.Tasks.Task{TResult}"/>.
        /// Use this overload if your task supports cancellation in order to hook-up the <code>Cancel</code>
        /// mechanism exposed by the created asynchronous operation and the cancellation of your task.</summary>
        /// <param name="taskProvider">The function to invoke to create the task when the IAsyncInfo is started.
        /// The function is passed a <see cref="System.Threading.CancellationToken"/> that the task may monitor
        /// to be notified of a cancellation request;
        /// you may ignore the <code>CancellationToken</code> if your task does not support cancellation.</param>
        /// <returns>An unstarted <see cref="IAsyncOperation{TResult}"/> instance.</returns>
        public static IAsyncOperation<TResult> Run<TResult>(Func<CancellationToken, Task<TResult>> taskProvider)
        {
            // This is only internal to reduce the number of public overloads.
            // Code execution flows through this method when the method above is called. We can always make this public.

            if (taskProvider == null)
                throw new ArgumentNullException(nameof(taskProvider));

            return new TaskToAsyncOperationAdapter<TResult>(taskProvider);
        }


        /// <summary>
        /// Creates and starts  an <see cref="IAsyncOperationWithProgress{TResult, TProgress}"/> instance
        /// from a function that generates a <see cref="System.Threading.Tasks.Task{TResult}"/>.<br />
        /// Use this overload if your task supports cancellation and progress monitoring is order to:
        /// (1) hook-up the <code>Cancel</code> mechanism of the created asynchronous operation and the cancellation of your task,
        /// and (2) hook-up the <code>Progress</code> update delegate exposed by the created async operation and the progress
        /// updates published by your task.</summary>
        /// <typeparam name="TResult">The result type of the task.</typeparam>
        /// <typeparam name="TProgress">The type used for progress notifications.</typeparam>
        /// <param name="taskProvider">The function to invoke to create the task when the IAsyncOperationWithProgress is started.<br />
        /// The function is passed a <see cref="System.Threading.CancellationToken"/> that the task may monitor
        /// to be notified of a cancellation request;
        /// you may ignore the <code>CancellationToken</code> if your task does not support cancellation.
        /// It is also passed a <see cref="System.IProgress{TProgress}"/> instance to which progress updates may be published;
        /// you may ignore the <code>IProgress</code> if your task does not support progress reporting.</param>
        /// <returns>An unstarted <see cref="IAsyncOperationWithProgress{TResult, TProgress}"/> instance.</returns>
        public static IAsyncOperationWithProgress<TResult, TProgress> Run<TResult, TProgress>(
                                                                            Func<CancellationToken, IProgress<TProgress>, Task<TResult>> taskProvider)
        {
            if (taskProvider == null)
                throw new ArgumentNullException(nameof(taskProvider));

            return new TaskToAsyncOperationWithProgressAdapter<TResult, TProgress>(taskProvider);
        }

        #endregion Factory methods for creating "normal" IAsyncInfo instances backed by a Task created by a pastProvider delegate


        #region Factory methods for creating IAsyncInfo instances that have already completed synchronously

        public static IAsyncAction CompletedAction()
        {
            var asyncInfo = new TaskToAsyncActionAdapter(isCanceled: false);
            return asyncInfo;
        }


        public static IAsyncActionWithProgress<TProgress> CompletedActionWithProgress<TProgress>()
        {
            var asyncInfo = new TaskToAsyncActionWithProgressAdapter<TProgress>(isCanceled: false);
            return asyncInfo;
        }


        public static IAsyncOperation<TResult> FromResult<TResult>(TResult synchronousResult)
        {
            var asyncInfo = new TaskToAsyncOperationAdapter<TResult>(synchronousResult);
            return asyncInfo;
        }


        public static IAsyncOperationWithProgress<TResult, TProgress> FromResultWithProgress<TResult, TProgress>(TResult synchronousResult)
        {
            var asyncInfo = new TaskToAsyncOperationWithProgressAdapter<TResult, TProgress>(synchronousResult);
            return asyncInfo;
        }

        #endregion Factory methods for creating IAsyncInfo instances that have already completed synchronously

        #region Factory methods for creating IAsyncInfo instances that have already completed synchronously with an error

        public static IAsyncAction FromException(Exception error)
        {
            if (error == null)
                throw new ArgumentNullException(nameof(error));

            var asyncInfo = new TaskToAsyncActionAdapter(isCanceled: false);

            asyncInfo.DangerousSetError(error);
            Debug.Assert(asyncInfo.Status == AsyncStatus.Error);

            return asyncInfo;
        }


        public static IAsyncActionWithProgress<TProgress> FromExceptionWithProgress<TProgress>(Exception error)
        {
            if (error == null)
                throw new ArgumentNullException(nameof(error));

            var asyncInfo = new TaskToAsyncActionWithProgressAdapter<TProgress>(isCanceled: false);

            asyncInfo.DangerousSetError(error);
            Debug.Assert(asyncInfo.Status == AsyncStatus.Error);

            return asyncInfo;
        }


        public static IAsyncOperation<TResult> FromException<TResult>(Exception error)
        {
            if (error == null)
                throw new ArgumentNullException(nameof(error));

            var asyncInfo = new TaskToAsyncOperationAdapter<TResult>(default(TResult));

            asyncInfo.DangerousSetError(error);
            Debug.Assert(asyncInfo.Status == AsyncStatus.Error);

            return asyncInfo;
        }


        public static IAsyncOperationWithProgress<TResult, TProgress> FromExceptionWithProgress<TResult, TProgress>(Exception error)
        {
            if (error == null)
                throw new ArgumentNullException(nameof(error));

            var asyncInfo = new TaskToAsyncOperationWithProgressAdapter<TResult, TProgress>(default(TResult));

            asyncInfo.DangerousSetError(error);
            Debug.Assert(asyncInfo.Status == AsyncStatus.Error);

            return asyncInfo;
        }

        #endregion Factory methods for creating IAsyncInfo instances that have already completed synchronously with an error

        #region Factory methods for creating IAsyncInfo instances that have already been canceled synchronously

        public static IAsyncAction CanceledAction()
        {
            var asyncInfo = new TaskToAsyncActionAdapter(isCanceled: true);
            return asyncInfo;
        }


        public static IAsyncActionWithProgress<TProgress> CanceledActionWithProgress<TProgress>()
        {
            var asyncInfo = new TaskToAsyncActionWithProgressAdapter<TProgress>(isCanceled: true);
            return asyncInfo;
        }


        public static IAsyncOperation<TResult> CanceledOperation<TResult>()
        {
            var asyncInfo = new TaskToAsyncOperationAdapter<TResult>(isCanceled: true);
            return asyncInfo;
        }


        public static IAsyncOperationWithProgress<TResult, TProgress> CanceledOperationWithProgress<TResult, TProgress>()
        {
            var asyncInfo = new TaskToAsyncOperationWithProgressAdapter<TResult, TProgress>(isCanceled: true);
            return asyncInfo;
        }

        #endregion Factory methods for creating IAsyncInfo instances that have already been canceled synchronously

    }  // class AsyncInfo
}  // namespace
