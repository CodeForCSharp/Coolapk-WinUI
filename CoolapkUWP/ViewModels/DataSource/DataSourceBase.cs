using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Data;
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

namespace CoolapkUWP.ViewModels.DataSource
{
    /// <summary>
    /// 酷安列表数据源基类：基于 <see cref="ObservableCollection{T}"/>，支持分页增量加载（<see cref="ISupportIncrementalLoading"/>）。
    /// </summary>
    public abstract class DataSourceBase : ObservableCollection<Entity>, ISupportIncrementalLoading
    {
        public DispatcherQueue Dispatcher { get; }

        public bool HasMoreItems => _hasMoreItems;

        private bool any;
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

        private bool isLoading;
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

        public DataSourceBase() : this(App.MainWindow.DispatcherQueue) { }

        public DataSourceBase(DispatcherQueue dispatcher) => Dispatcher = dispatcher;

        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            if (_busy)
            {
                return Task.FromResult(new LoadMoreItemsResult { Count = 0 }).AsAsyncOperation();
            }

            _busy = true;

            // ISupportIncrementalLoading 要求返回 IAsyncOperation，故用 AsyncInfo.Run 包装。
            return AsyncInfo.Run((c) => LoadMoreItemsAsync(c, count));
        }

        private async Task<LoadMoreItemsResult> LoadMoreItemsAsync(CancellationToken c, uint count)
        {
            try
            {
                IsLoading = true;
                LoadMoreStarted?.Invoke();

                // 加载（含同步 JSON 解析）在后台线程执行。
                IList<Entity> items = await Task.Run(() => LoadItemsAsync(count));
                if (items != null)
                {
                    _currentPage++;
                }
                _hasMoreItems = items != null && items.Count > 0;

                AddItems(items);

                return new LoadMoreItemsResult { Count = items == null ? 0 : (uint)items.Count };
            }
            catch (OperationCanceledException)
            {
                return new LoadMoreItemsResult { Count = 0 };
            }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.CreateLogger(nameof(DataSourceBase)).LogError(ex, ex.ExceptionToMessage());
                return new LoadMoreItemsResult { Count = 0 };
            }
            finally
            {
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

        public event EventHandler LoadMoreStarted;
        public event EventHandler LoadMoreCompleted;

        /// <summary>
        /// 追加新条目（已过滤 <see cref="NullEntity"/>）。子类可在调用基类前做额外处理。
        /// </summary>
        protected virtual void AddItems(IList<Entity> items)
        {
            if (items == null || items.Count == 0) { return; }

            List<Entity> filtered = new List<Entity>(items.Count);
            foreach (Entity item in items)
            {
                if (item is not NullEntity) { filtered.Add(item); }
            }
            if (filtered.Count == 0) { return; }

            // Items 是底层原始列表，Items.Add 不会触发 CollectionChanged，
            // 故静默加入后再用一次 Add 事件整批通知（带起始索引），ListView 只实例化新容器、不丢已有容器。
            int startIndex = Count;
            CheckReentrancy();
            foreach (Entity item in filtered) { Items.Add(item); }
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, filtered, startIndex));
        }

        /// <summary>
        /// 子类实现的实际分页加载逻辑，返回本页新增条目。
        /// </summary>
        protected abstract Task<IList<Entity>> LoadItemsAsync(uint count);

        /// <summary>
        /// 清空当前条目，并从第一页重新加载。
        /// </summary>
        public virtual async Task Reset()
        {
            _currentPage = 1;
            _hasMoreItems = true;

            Clear();
            await LoadMoreItemsAsync(20);
        }

        protected int _currentPage = 1;
        protected bool _hasMoreItems = true;
        protected bool _busy = false;
    }
}
