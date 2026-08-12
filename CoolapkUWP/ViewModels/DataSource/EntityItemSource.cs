using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using CoolapkUWP.ViewModels.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;

namespace CoolapkUWP.ViewModels.DataSource
{
    public abstract partial class EntityItemSource : DataSourceBase<Entity>
    {
        protected CoolapkListProvider Provider;
        protected CoolapkListProvider SubProvider;

        /// <summary>当前列表所属的实体 ID，用于区分不同数据源。</summary>
        public string ID { get; }

        /// <summary>是否在添加条目时展开子提供器（首页 Tab 卡片等），列表型数据源应关闭。</summary>
        private readonly bool useSubProvider;

        public EntityItemSource() : this(App.MainWindow.DispatcherQueue) { }

        public EntityItemSource(DispatcherQueue dispatcher) : base(dispatcher) { }

        protected EntityItemSource(string id) : this(App.MainWindow.DispatcherQueue)
        {
            ID = id;
        }

        protected EntityItemSource(string id, CoolapkListProvider provider, bool useSubProvider = true)
            : this(App.MainWindow.DispatcherQueue)
        {
            ID = id;
            Provider = provider ?? throw new ArgumentNullException(nameof(provider));
            this.useSubProvider = useSubProvider;
        }

        protected override async Task<IList<Entity>> LoadItemsAsync(uint count)
        {
            List<Entity> Models = new List<Entity>();
            while (Models.Count < count)
            {
                int temp = Models.Count;
                if (Models.Count > 0) { _currentPage++; }
                if (SubProvider == null)
                {
                    await Provider?.GetEntity(Models, _currentPage);
                }
                else
                {
                    await SubProvider.GetEntity(Models, _currentPage);
                }
                if (Models.Count <= 0 || Models.Count <= temp) { break; }
            }
            return Models;
        }

        protected override Task AddItemsAsync(IList<Entity> items)
        {
            if (items == null) { return Task.CompletedTask; }
            foreach (Entity item in items)
            {
                if (!(item is NullEntity))
                {
                    Add(item);
                    if (useSubProvider) { AddSubProvider(item); }
                }
            }
            return Task.CompletedTask;
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

        public override async Task Reset()
        {
            //reset
            _currentPage = 1;
            _hasMoreItems = true;

            Clear();
            SubProvider = null;
            _ = await LoadMoreItemsAsync(20);
        }

        protected virtual void AddSubProvider(Entity item)
        {
            if (item is IndexPageHasEntitiesModel model
                && model.EntitiesType == EntityType.TabLink)
            {
                IndexPageModel indexPage = model.Entities.Where((x) => x is IndexPageModel).FirstOrDefault() as IndexPageModel;
                if (indexPage == null) { return; }
                string Uri = UriHelper.NormalizePageUri(indexPage.Url);
                SubProvider = new CoolapkListProvider(
                    (p, _, __) => UriHelper.GetUri(UriType.GetIndexPage, Uri, Uri.Contains("?") ? "&" : "?", p),
                    Provider.GetEntities,
                    "entityId");
                _currentPage = 1;
            }
        }
    }
}
