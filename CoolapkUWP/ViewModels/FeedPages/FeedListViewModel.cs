using CoolapkUWP.Controls;
using CoolapkUWP.Controls.DataTemplates;
using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using CoolapkUWP.Models.Pages;
using CoolapkUWP.Pages.FeedPages;
using CoolapkUWP.ViewModels.DataSource;
using CoolapkUWP.ViewModels.Providers;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json.Nodes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoolapkUWP.ViewModels.FeedPages
{
    public abstract partial class FeedListViewModel : ObservableObject, IViewModel
    {
        protected const string idName = "id";

        public string ID { get; }
        private FeedListType ListType { get; }

        [ObservableProperty]
        public partial string Title { get; protected set; }

        [ObservableProperty]
        public partial List<ShyHeaderItem> ItemSource { get; protected set; }

        [ObservableProperty]
        public partial FeedListDetailBase Detail { get; protected set; }

        protected FeedListViewModel(string id, FeedListType type)
        {
            ID = string.IsNullOrEmpty(id)
                ? throw new ArgumentException(nameof(id))
                : id;
            ListType = type;
        }

        public static FeedListViewModel GetProvider(FeedListType type, string id)
        {
            if (string.IsNullOrEmpty(id) || id == "0") { return null; }
            switch (type)
            {
                case FeedListType.UserPageList: return new UserViewModel(id);
                case FeedListType.TagPageList: return new TagViewModel(id);
                case FeedListType.DyhPageList: return new DyhViewModel(id);
                case FeedListType.ProductPageList: return new ProductViewModel(id);
                case FeedListType.CollectionPageList: return new CollectionViewModel(id);
                default: return null;
            }
        }

        public void ChangeCopyMode(bool mode)
        {
            if (Detail != null)
            {
                Detail.IsCopyEnabled = mode;
            }
        }

        public abstract Task<FeedListDetailBase> GetDetail();

        public abstract Task Refresh(bool reset = false);

        bool IViewModel.IsEqual(IViewModel other) => other is FeedListViewModel model && IsEqual(model);

        public bool IsEqual(FeedListViewModel other) => ListType == other.ListType && ID == other.ID;

        private IEnumerable<Entity> GetEntities(JsonObject jo)
        {
            yield return EntityTemplateSelector.GetEntity(jo);
        }
    }
}
