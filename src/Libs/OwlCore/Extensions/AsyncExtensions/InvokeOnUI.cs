﻿using System;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Helpers;

namespace OwlCore.Extensions.AsyncExtensions
{
    public static partial class AsyncExtensions
    {
        /// <summary>
        /// Invokes a task on the UI thread.
        /// </summary>
        /// <param name="task">The task to invoke.</param>
        public static async Task InvokeOnUI(this Task task)
        {
            var originalContext = SynchronizationContext.Current;

            if (Threading.UISyncContext is null)
                throw new InvalidOperationException($"UI context not found. {nameof(Threading.SetUISynchronizationContext)} was not called.");

            SynchronizationContext.SetSynchronizationContext(Threading.UISyncContext);

            await task;

            SynchronizationContext.SetSynchronizationContext(originalContext);
        }
    }
}