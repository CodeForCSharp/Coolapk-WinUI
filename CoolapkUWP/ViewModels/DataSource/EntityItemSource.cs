using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using CoolapkUWP.ViewModels.Providers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;

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
            Provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        protected override async Task<IList<Entity>> LoadItemsAsync(uint count)
        {
            List<Entity> Models = new List<Entity>();
            while (Models.Count < count)
            {
                int temp = Models.Count;
                if (Models.Count > 0) { _currentPage++; }
                await Provider?.GetEntity(Models, _currentPage);
                if (Models.Count <= 0 || Models.Count <= temp) { break; }
            }
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
