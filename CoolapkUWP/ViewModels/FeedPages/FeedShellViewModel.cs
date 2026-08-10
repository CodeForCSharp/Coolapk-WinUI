using CoolapkUWP.Controls;
using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using CoolapkUWP.Models.Feeds;
using CoolapkUWP.Models.Users;
using CoolapkUWP.ViewModels.DataSource;
using CoolapkUWP.ViewModels.Providers;
using System.Text.Json.Nodes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;

namespace CoolapkUWP.ViewModels.FeedPages
{
    public abstract class FeedShellViewModel : IViewModel, INotifyPropertyChanged
    {
        protected string ID { get; set; }

        private string title = string.Empty;
        public string Title
        {
            get => title;
            protected set
            {
                if (title != value)
                {
                    title = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private FeedDetailModel feedDetail;
        public FeedDetailModel FeedDetail
        {
            get => feedDetail;
            protected set
            {
                if (feedDetail != value)
                {
                    feedDetail = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private List<ShyHeaderItem> itemSource;
        public List<ShyHeaderItem> ItemSource
        {
            get => itemSource;
            protected set
            {
                if (itemSource != value)
                {
                    itemSource = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

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
