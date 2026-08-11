using CoolapkUWP.Controls;
using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Pages;
using CoolapkUWP.Pages.FeedPages;
using CoolapkUWP.ViewModels.DataSource;
using CoolapkUWP.ViewModels.Providers;
using System.Text.Json.Nodes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace CoolapkUWP.ViewModels.FeedPages
{
    public abstract partial class FeedListViewModel
    {
        internal class TagViewModel : FeedListViewModel
        {
            public FeedListItemSource LastUpdateItemSource { get; private set; }
            public FeedListItemSource DatelineItemSource { get; private set; }
            public FeedListItemSource PopularItemSource { get; private set; }

            internal TagViewModel(string id) : base(id, FeedListType.TagPageList) { }

            public override async Task Refresh(bool reset = false)
            {
                if (Detail == null || reset)
                {
                    Detail = await GetDetail();
                    Title = (Detail as TopicDetail)?.Title;
                }
                if (ItemSource == null)
                {
                    List<ShyHeaderItem> ItemSource = new List<ShyHeaderItem>();
                    if (LastUpdateItemSource == null || LastUpdateItemSource.ID != ID)
                    {
                        CoolapkListProvider Provider = new CoolapkListProvider(
                            (p, firstItem, lastItem) => UriHelper.GetUri(UriType.GetTagFeeds, ID, p, string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}", string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}", "lastupdate_desc"),
                            GetEntities,
                            idName);
                        LastUpdateItemSource = new FeedListItemSource(ID, Provider);
                        ItemSource.Add(new ShyHeaderItem
                        {
                            Header = "最近回复",
                            ItemSource = LastUpdateItemSource
                        });
                    }
                    if (DatelineItemSource == null || DatelineItemSource.ID != ID)
                    {
                        CoolapkListProvider Provider = new CoolapkListProvider(
                            (p, firstItem, lastItem) => UriHelper.GetUri(UriType.GetTagFeeds, ID, p, string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}", string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}", "dateline_desc"),
                            GetEntities,
                            idName);
                        DatelineItemSource = new FeedListItemSource(ID, Provider);
                        ItemSource.Add(new ShyHeaderItem
                        {
                            Header = "最近发布",
                            ItemSource = DatelineItemSource
                        });
                    }
                    if (PopularItemSource == null || PopularItemSource.ID != ID)
                    {
                        CoolapkListProvider Provider = new CoolapkListProvider(
                            (p, firstItem, lastItem) => UriHelper.GetUri(UriType.GetTagFeeds, ID, p, string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}", string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}", "popular"),
                            GetEntities,
                            idName);
                        PopularItemSource = new FeedListItemSource(ID, Provider);
                        ItemSource.Add(new ShyHeaderItem
                        {
                            Header = "热门动态",
                            ItemSource = PopularItemSource
                        });
                    }
                    base.ItemSource = ItemSource;
                }
            }

            public override async Task<FeedListDetailBase> GetDetail()
            {
                (bool isSucceed, JsonNode result) = await RequestHelper.GetDataAsync(UriHelper.GetUri(UriType.GetTagDetail, ID), true);
                if (!isSucceed) { return null; }

                JsonObject token = result.AsObject();
                FeedListDetailBase detail = null;

                if (token != null)
                {
                    detail = new TopicDetail(token);
                }

                return detail;
            }

        }
}
}
