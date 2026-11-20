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

        /// <summary>
        /// 建好指定标签的数据源，并把关键词同步过去。返回 true 表示关键词变了，需要重新从第一页加载。
        /// </summary>
        public bool PrepareTab(int index, out EntityItemSource source)
        {
            switch (index)
            {
                case 1:
                    source = EnsureUser();
                    return SyncKeyword(SearchUserItemSource.Keyword, value => SearchUserItemSource.Keyword = value);
                case 2:
                    source = EnsureTopic();
                    return SyncKeyword(SearchTopicItemSource.Keyword, value => SearchTopicItemSource.Keyword = value);
                default:
                    source = EnsureFeed();
                    return SyncKeyword(SearchFeedItemSource.Keyword, value => SearchFeedItemSource.Keyword = value);
            }
        }

        public Task Refresh(bool reset = false)
        {
            int index = PivotIndex >= 0 ? PivotIndex : 0;
            bool keywordChanged = PrepareTab(index, out EntityItemSource source);
            return source.Refresh(reset || keywordChanged);
        }

        private EntityItemSource EnsureFeed()
        {
            if (SearchFeedItemSource == null)
            {
                SearchFeedItemSource = new SearchFeedItemSource(Title);
                Hook(SearchFeedItemSource);
            }
            return SearchFeedItemSource;
        }

        private EntityItemSource EnsureUser()
        {
            if (SearchUserItemSource == null)
            {
                SearchUserItemSource = new SearchUserItemSource(Title);
                Hook(SearchUserItemSource);
            }
            return SearchUserItemSource;
        }

        private EntityItemSource EnsureTopic()
        {
            if (SearchTopicItemSource == null)
            {
                SearchTopicItemSource = new SearchTopicItemSource(Title);
                Hook(SearchTopicItemSource);
            }
            return SearchTopicItemSource;
        }

        private static void Hook(EntityItemSource source)
        {
            source.LoadMoreStarted += ProgressBarHelper.ShowProgressBar;
            source.LoadMoreCompleted += ProgressBarHelper.HideProgressBar;
        }

        private bool SyncKeyword(string current, Action<string> assign)
        {
            if (current == Title) { return false; }
            assign(Title);
            return true;
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
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(SearchFeedTypeComboBoxSelectedIndex)));
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
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(SearchFeedSortTypeComboBoxSelectedIndex)));
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
