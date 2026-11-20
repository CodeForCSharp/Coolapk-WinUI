using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using CoolapkUWP.ViewModels.Providers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;

namespace CoolapkUWP.ViewModels.DataSource
{
    public abstract partial class EntityItemSource : DataSourceBase
    {
        protected CoolapkListProvider Provider;

        /// <summary>当前列表所属的实体 ID，用于区分不同数据源。</summary>
        public string ID { get; }

        public EntityItemSource() : this(App.MainWindow.DispatcherQueue) { }

        public EntityItemSource(DispatcherQueue dispatcher) : base(dispatcher) { }

        protected EntityItemSource(string id) : this(App.MainWindow.DispatcherQueue)
        {
            ID = id;
        }

        protected EntityItemSource(string id, CoolapkListProvider provider)
            : this(App.MainWindow.DispatcherQueue)
        {
            ID = id;
            // 没有列表地址的标签传 null，加载时直接返回空。
            Provider = provider;
        }

        protected override async Task<IList<Entity>> LoadItemsAsync(uint count)
        {
            // 一页回来就交给列表显示。凑满 count 再返回会把第二页的网络等待算进首屏。
            List<Entity> Models = new List<Entity>();
            await Provider?.GetEntity(Models, _currentPage);
            return Models;
        }

        public virtual async Task Refresh(bool reset = false)
        {
            if (reset)
            {
                await Reset();
            }
            else
            {
                _ = await LoadMoreItemsAsync(20);
            }
        }
    }
}
