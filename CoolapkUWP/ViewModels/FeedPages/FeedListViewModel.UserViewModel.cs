using CoolapkUWP.Controls;
using CoolapkUWP.Controls.DataTemplates;
using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using CoolapkUWP.Models.Images;
using CoolapkUWP.Models.Pages;
using CoolapkUWP.Pages.FeedPages;
using CoolapkUWP.ViewModels.DataSource;
using CoolapkUWP.ViewModels.Providers;
using System.Text.Json.Nodes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Dispatching;

namespace CoolapkUWP.ViewModels.FeedPages
{
    public abstract partial class FeedListViewModel
    {
        public class UserViewModel : FeedListViewModel
        {
            public FeedListItemSource FeedItemSource { get; private set; }
            public FeedListItemSource HtmlFeedItemSource { get; private set; }
            public FeedListItemSource QAItemSource { get; private set; }
            public FeedListItemSource CollectionItemSource { get; private set; }

            internal UserViewModel(string uid) : base(uid, FeedListType.UserPageList) { }

            public override async Task Refresh(bool reset = false)
            {
                if (Detail == null || reset)
                {
                    Detail = await GetDetail();
                }
                if (ItemSource == null)
                {
                    List<ShyHeaderItem> ItemSource = new List<ShyHeaderItem>();
                    if (FeedItemSource == null || FeedItemSource.ID != ID)
                    {
                        CoolapkListProvider Provider = new CoolapkListProvider(
                            (p, firstItem, lastItem) => UriHelper.GetUri(UriType.GetUserFeeds, ID, p, string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}", string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}", "feed"),
                            GetEntities,
                            idName);
                        FeedItemSource = new FeedListItemSource(ID, Provider);
                        ItemSource.Add(new ShyHeaderItem
                        {
                            Header = "动态",
                            ItemSource = FeedItemSource
                        });
                    }
                    if (HtmlFeedItemSource == null || HtmlFeedItemSource.ID != ID)
                    {
                        CoolapkListProvider Provider = new CoolapkListProvider(
                            (p, firstItem, lastItem) => UriHelper.GetUri(UriType.GetUserFeeds, ID, p, string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}", string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}", "htmlFeed"),
                            GetEntities,
                            idName);
                        HtmlFeedItemSource = new FeedListItemSource(ID, Provider);
                        ItemSource.Add(new ShyHeaderItem
                        {
                            Header = "图文",
                            ItemSource = HtmlFeedItemSource
                        });
                    }
                    if (QAItemSource == null || QAItemSource.ID != ID)
                    {
                        CoolapkListProvider Provider = new CoolapkListProvider(
                            (p, firstItem, lastItem) => UriHelper.GetUri(UriType.GetUserFeeds, ID, p, string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}", string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}", "questionAndAnswer"),
                            GetEntities,
                            idName);
                        QAItemSource = new FeedListItemSource(ID, Provider);
                        ItemSource.Add(new ShyHeaderItem
                        {
                            Header = "问答",
                            ItemSource = QAItemSource
                        });
                    }
                    if (CollectionItemSource == null || CollectionItemSource.ID != ID)
                    {
                        CoolapkListProvider Provider = new CoolapkListProvider(
                            (p, firstItem, lastItem) => UriHelper.GetUri(UriType.GetCollectionList, ID, p, string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}", string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}"),
                            GetEntities,
                            idName);
                        CollectionItemSource = new FeedListItemSource(ID, Provider);
                        ItemSource.Add(new ShyHeaderItem
                        {
                            Header = "收藏单",
                            ItemSource = CollectionItemSource
                        });
                    }
                    base.ItemSource = ItemSource;
                }
            }

            protected override string GetTitleBarText(FeedListDetailBase detail) => (detail as UserDetail)?.UserName;

            public override async Task<FeedListDetailBase> GetDetail()
            {
                (bool isSucceed, JsonNode result) = await RequestHelper.GetDataAsync(UriHelper.GetUri(UriType.GetUserSpace, ID), true);
                if (!isSucceed) { return null; }

                JsonObject token = result.AsObject();
                FeedListDetailBase detail = null;

                if (token != null)
                {
                    detail = new UserDetail(token);
                }

                return detail;
            }

        }
}
}
