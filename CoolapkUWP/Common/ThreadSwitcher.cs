using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;

namespace CoolapkUWP.Common
{
    public readonly struct DispatcherThreadSwitcher : INotifyCompletion
    {
        private readonly DispatcherQueue dispatcher;

        public bool IsCompleted => dispatcher?.HasThreadAccess != false;

        internal DispatcherThreadSwitcher(DispatcherQueue dispatcher) => this.dispatcher = dispatcher;

        public void GetResult() { }

        public DispatcherThreadSwitcher GetAwaiter() => this;

        public void OnCompleted(Action continuation) => _ = dispatcher.TryEnqueue(DispatcherQueuePriority.Normal, () => continuation());
    }

    public readonly struct ThreadPoolThreadSwitcher : INotifyCompletion
    {
        public bool IsCompleted => SynchronizationContext.Current == null;

        public void GetResult() { }

        public ThreadPoolThreadSwitcher GetAwaiter() => this;

        public void OnCompleted(Action continuation) => Task.Run(continuation);
    }

    public static class ThreadSwitcher
    {
        public static DispatcherThreadSwitcher ResumeForegroundAsync(this DispatcherQueue dispatcher) => new DispatcherThreadSwitcher(dispatcher);

        public static ThreadPoolThreadSwitcher ResumeBackgroundAsync() => new ThreadPoolThreadSwitcher();

        public static Task AwaitableRunAsync(this DispatcherQueue dispatcher, Action callback, DispatcherQueuePriority priority = DispatcherQueuePriority.Normal)
        {
            var tcs = new TaskCompletionSource<bool>();
            dispatcher.TryEnqueue(priority, () =>
            {
                try { callback(); tcs.SetResult(true); }
                catch (Exception ex) { tcs.SetException(ex); }
            });
            return tcs.Task;
        }

        public static Task<T> AwaitableRunAsync<T>(this DispatcherQueue dispatcher, Func<T> callback, DispatcherQueuePriority priority = DispatcherQueuePriority.Normal)
        {
            var tcs = new TaskCompletionSource<T>();
            dispatcher.TryEnqueue(priority, () =>
            {
                try { tcs.SetResult(callback()); }
                catch (Exception ex) { tcs.SetException(ex); }
            });
            return tcs.Task;
        }

        public static Task AwaitableRunAsync(this DispatcherQueue dispatcher, Func<Task> callback, DispatcherQueuePriority priority = DispatcherQueuePriority.Normal)
        {
            var tcs = new TaskCompletionSource<bool>();
            dispatcher.TryEnqueue(priority, async () =>
            {
                try { await callback(); tcs.SetResult(true); }
                catch (Exception ex) { tcs.SetException(ex); }
            });
            return tcs.Task;
        }

        public static Task<T> AwaitableRunAsync<T>(this DispatcherQueue dispatcher, Func<Task<T>> callback, DispatcherQueuePriority priority = DispatcherQueuePriority.Normal)
        {
            var tcs = new TaskCompletionSource<T>();
            dispatcher.TryEnqueue(priority, async () =>
            {
                try { tcs.SetResult(await callback()); }
                catch (Exception ex) { tcs.SetException(ex); }
            });
            return tcs.Task;
        }
    }
}
