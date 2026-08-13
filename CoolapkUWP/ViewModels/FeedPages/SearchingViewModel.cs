using CoolapkUWP.Data;
using CoolapkUWP.Data.Dtos;
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
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;

namespace CoolapkUWP.ViewModels.FeedPages
{
    public partial class SearchingViewModel : ObservableObject, IViewModel
    {
        public int PivotIndex = -1;

        [ObservableProperty]
        public partial string Title { get; set; }

        [ObservableProperty]
        public partial SearchFeedItemSource SearchFeedItemSource { get; private set; }

        [ObservableProperty]
        public partial SearchUserItemSource SearchUserItemSource { get; private set; }

        [ObservableProperty]
        public partial SearchTopicItemSource SearchTopicItemSource { get; private set; }

        public SearchingViewModel(string keyword, int index = -1)
        {
            Title = keyword;
            PivotIndex = index;
        }

        public async Task Refresh(bool reset = false)
        {
            if (reset)
            {
                List<PivotItem> ItemSource = new List<PivotItem>();
                if (SearchFeedItemSource == null)
                {
                    SearchFeedItemSource = new SearchFeedItemSource(Title);
                    SearchFeedItemSource.LoadMoreStarted += ProgressBarHelper.ShowProgressBar;
                    SearchFeedItemSource.LoadMoreCompleted += ProgressBarHelper.HideProgressBar;
                }
                else if (SearchFeedItemSource.Keyword != Title)
                {
                    SearchFeedItemSource.Keyword = Title;
                }
                if (SearchUserItemSource == null)
                {
                    SearchUserItemSource = new SearchUserItemSource(Title);
                    SearchUserItemSource.LoadMoreStarted += ProgressBarHelper.ShowProgressBar;
                    SearchUserItemSource.LoadMoreCompleted += ProgressBarHelper.HideProgressBar;
                }
                else if (SearchUserItemSource.Keyword != Title)
                {
                    SearchUserItemSource.Keyword = Title;
                }
                if (SearchTopicItemSource == null)
                {
                    SearchTopicItemSource = new SearchTopicItemSource(Title);
                    SearchTopicItemSource.LoadMoreStarted += ProgressBarHelper.ShowProgressBar;
                    SearchTopicItemSource.LoadMoreCompleted += ProgressBarHelper.HideProgressBar;
                }
                else if (SearchTopicItemSource.Keyword != Title)
                {
                    SearchTopicItemSource.Keyword = Title;
                }
            }
            await SearchFeedItemSource?.Refresh(reset);
            await SearchUserItemSource?.Refresh(reset);
            await SearchTopicItemSource?.Refresh(reset);
        }

        bool IViewModel.IsEqual(IViewModel other) => other is SearchingViewModel model && IsEqual(model);

        public bool IsEqual(SearchingViewModel other) => Title == other.Title;
    }

    public partial class SearchFeedItemSource : EntityItemSource, INotifyPropertyChanged
    {
        public string Keyword;

        private static readonly string[] FeedTypes =
            { "all", "feed", "feedArticle", "rating", "picture", "question", "answer", "video", "ershou", "vote" };

        private static readonly string[] SortTypes =
            { "default", "hot", "reply" };

        private int searchFeedTypeComboBoxSelectedIndex = 0;
        public int SearchFeedTypeComboBoxSelectedIndex
        {
            get => searchFeedTypeComboBoxSelectedIndex;
            set
            {
                searchFeedTypeComboBoxSelectedIndex = value;
                RaisePropertyChangedEvent();
                UpdateProvider();
                _ = Refresh(true);
            }
        }

        private int searchFeedSortTypeComboBoxSelectedIndex = 0;
        public int SearchFeedSortTypeComboBoxSelectedIndex
        {
            get => searchFeedSortTypeComboBoxSelectedIndex;
            set
            {
                searchFeedSortTypeComboBoxSelectedIndex = value;
                RaisePropertyChangedEvent();
                UpdateProvider();
                _ = Refresh(true);
            }
        }

        public SearchFeedItemSource(string keyword)
        {
            Keyword = keyword;
            UpdateProvider();
        }

        private IEnumerable<Entity> GetEntities(JsonArray array) => DtoJson.DeserializeList<FeedDto>(array).Select(d => new FeedModel(d));

        private void UpdateProvider()
        {
            string feedType = FeedTypes[Math.Clamp(SearchFeedTypeComboBoxSelectedIndex, 0, FeedTypes.Length - 1)];
            string sortType = SortTypes[Math.Clamp(SearchFeedSortTypeComboBoxSelectedIndex, 0, SortTypes.Length - 1)];
            Provider = new CoolapkListProvider(
                (p, firstItem, lastItem) =>
                UriHelper.GetUri(
                    UriType.SearchFeeds,
                    feedType,
                    sortType,
                    Keyword,
                    p,
                    UriHelper.GetPagingArgs(p, firstItem, lastItem)),
                GetEntities,
                "id");
        }
    }

    public partial class SearchUserItemSource : EntityItemSource
    {
        public string Keyword;

        public SearchUserItemSource(string keyword) : base(keyword)
        {
            Keyword = keyword;
            Provider = new CoolapkListProvider(
                (p, firstItem, lastItem) =>
                    UriHelper.GetUri(
                        UriType.SearchUsers,
                        Keyword,
                        p,
                        UriHelper.GetPagingArgs(p, firstItem, lastItem)),
                a => DtoJson.DeserializeList<UserDto>(a).Select(d => new UserModel(d)),
                "uid");
        }
    }

    public partial class SearchTopicItemSource : EntityItemSource
    {
        public string Keyword;

        public SearchTopicItemSource(string keyword) : base(keyword)
        {
            Keyword = keyword;
            Provider = new CoolapkListProvider(
                (p, firstItem, lastItem) =>
                    UriHelper.GetUri(
                        UriType.SearchTags,
                        Keyword,
                        p,
                        UriHelper.GetPagingArgs(p, firstItem, lastItem)),
                a => DtoJson.DeserializeList<TopicDto>(a).Select(d => new TopicModel(d)),
                "id");
        }
    }
}
