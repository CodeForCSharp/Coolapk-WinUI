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
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(Any)));
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
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(IsLoading)));
                }
            }
        }

        public DataSourceBase() : this(App.MainWindow.DispatcherQueue) { }

        public DataSourceBase(DispatcherQueue dispatcher) => Dispatcher = dispatcher;

        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            // 加载进行中时交回同一次加载：若立即返回 0 条，ListView 在滚动位置再次变化前不会重新请求，停在底部就再也不加载。
            if (_busy && _loading != null)
            {
                return _loading.AsAsyncOperation();
            }

            _busy = true;
            _loading = LoadMoreItemsAsync(CancellationToken.None, count);
            return _loading.AsAsyncOperation();
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
            // 先等进行中的加载结束，否则它的结果会在清空之后写进来，页码也会接着旧的走。
            if (_busy && _loading != null)
            {
                _ = await _loading;
            }

            _currentPage = 1;
            _hasMoreItems = true;

            Clear();
            await LoadMoreItemsAsync(20);
        }

        protected int _currentPage = 1;
        protected bool _hasMoreItems = true;
        protected bool _busy = false;
        private Task<LoadMoreItemsResult> _loading;
    }
}
