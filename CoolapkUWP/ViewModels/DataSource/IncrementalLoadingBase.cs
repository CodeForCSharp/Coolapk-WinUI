using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Data;
using CoolapkUWP.Helpers;
using Microsoft.Extensions.Logging;

namespace CoolapkUWP.ViewModels.DataSource
{
    /// <summary>
    /// A incremental loading class base on the data binding sample on
    /// <see cref="MSDN" href="https://code.msdn.microsoft.com/windowsapps/Data-Binding-7b1d67b5/"/>
    /// , but using ObservableCollection to contain data and notify changes. <br/>
    /// If you want to use incremental loading in MVVM pattern, you can use this as a collection,
    /// and add a constructor with a delegate to load data,
    /// so that you can load different data in your view model, refer this blog for detail
    /// <see href="http://blogs.msdn.com/b/devosaure/archive/2012/10/15/isupportincrementalloading-loading-a-subsets-of-data.aspx"/>
    /// </summary>
    public abstract partial class IncrementalLoadingBase<T> : ObservableCollection<T>, ISupportIncrementalLoading
    {
        #region ISupportIncrementalLoading

        public bool HasMoreItems => HasMoreItemsOverride();

        /// <summary>
        /// Load more items, this is invoked by Controls like ListView.
        /// </summary>
        /// <param name="count">How many new items want to load.</param>
        /// <returns>Item count actually loaded.</returns>
        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            if (_busy)
            {
                return Task.FromResult(new LoadMoreItemsResult { Count = 0 }).AsAsyncOperation();
            }

            _busy = true;

            // We need to use AsyncInfo.Run to invoke async operation, as this method cannot return a Task.
            return AsyncInfo.Run((c) => LoadMoreItemsAsync(c, count));
        }

        #endregion

        public DispatcherQueue Dispatcher { get; }

        private bool any = false;
        public bool Any
        {
            get => any;
            set
            {
                if (any != value)
                {
                    any = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private bool isLoading = false;
        public bool IsLoading
        {
            get => isLoading;
            set
            {
                if (isLoading != value)
                {
                    isLoading = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        protected override event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChangedEvent([CallerMemberName] string name = null)
        {
            if (name != null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }

        public IncrementalLoadingBase(DispatcherQueue dispatcher) => Dispatcher = dispatcher;

        /// <summary>
        /// We use this method to load data and add to self.
        /// </summary>
        /// <param name="c">Cancellation Token</param>
        /// <param name="count">How many want to load.</param>
        /// <returns>Item count actually loaded.</returns>
        protected async Task<LoadMoreItemsResult> LoadMoreItemsAsync(CancellationToken c, uint count)
        {
            try
            {
                // We are going to load more.
                IsLoading = true;
                LoadMoreStarted?.Invoke();

                // Data loading happens on a background thread (includes synchronous JSON parsing).
                IList<T> items = await Task.Run(() => LoadMoreItemsOverrideAsync(c, count));

                await AddItemsAsync(items);

                return new LoadMoreItemsResult { Count = items == null ? 0 : (uint)items.Count };
            }
            catch (OperationCanceledException)
            {
                return new LoadMoreItemsResult { Count = 0 };
            }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.CreateLogger(nameof(IncrementalLoadingBase<T>)).LogError(ex, ex.ExceptionToMessage());
                return new LoadMoreItemsResult { Count = 0 };
            }
            finally
            {
                // We finished (or failed) the loading operation.
                IsLoading = false;
                LoadMoreCompleted?.Invoke();
                _busy = false;
            }
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnCollectionChanged(e);
            Any = Count > 0;
        }

        public delegate void EventHandler();
        public delegate void EventHandler<TEventArgs>(TEventArgs e);

        public event EventHandler LoadMoreStarted;
        public event EventHandler LoadMoreCompleted;

        #region Overridable methods

        /// <summary>
        /// Append items to list.
        /// </summary>
        protected virtual Task AddItemsAsync(IList<T> items)
        {
            if (items == null || items.Count == 0) { return Task.CompletedTask; }

            CheckReentrancy();
            foreach (T item in items)
            {
                Items.Add(item);
            }
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

            return Task.CompletedTask;
        }

        protected abstract Task<IList<T>> LoadMoreItemsOverrideAsync(CancellationToken c, uint count);

        protected abstract bool HasMoreItemsOverride();

        #endregion

        protected bool _busy = false;
    }
}
