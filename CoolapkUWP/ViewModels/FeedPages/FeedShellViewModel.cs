using CoolapkUWP.Controls;
using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using CoolapkUWP.Models.Feeds;
using CoolapkUWP.Models.Users;
using CoolapkUWP.ViewModels.DataSource;
using CoolapkUWP.ViewModels.Providers;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;

namespace CoolapkUWP.ViewModels.FeedPages
{
    public abstract partial class FeedShellViewModel : ObservableObject, IViewModel
    {
        protected string ID { get; set; }

        [ObservableProperty]
        public partial string Title { get; protected set; } = string.Empty;

        [ObservableProperty]
        public partial FeedDetailModel FeedDetail { get; protected set; }

        [ObservableProperty]
        public partial List<ShyHeaderItem> ItemSource { get; protected set; }

        protected FeedShellViewModel(string id)
        {
            if (string.IsNullOrEmpty(id)) { throw new ArgumentException(nameof(id)); }
            ID = id;
        }

        protected virtual async Task<FeedDetailModel> GetFeedDetailAsync(string id)
        {
            (bool isSucceed, JsonNode result) = id.Contains("changeHistoryDetail") ? await RequestHelper.GetDataAsync(new Uri(UriHelper.BaseUri.ToString() + "v6/feed/" + id), true) : await RequestHelper.GetDataAsync(UriHelper.GetUri(UriType.GetFeedDetail, id), true);
            if (!isSucceed) { return null; }

            JsonObject detail = result.AsObject();
            return detail != null ? new FeedDetailModel(detail) : null;
        }

        public abstract Task Refresh(bool reset = false);

        bool IViewModel.IsEqual(IViewModel other) => other is FeedShellViewModel model && IsEqual(model);

        public bool IsEqual(FeedShellViewModel other) => ID == other.ID;
    }

}
