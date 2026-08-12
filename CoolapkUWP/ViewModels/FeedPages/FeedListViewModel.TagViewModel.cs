using CoolapkUWP.Controls;
using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Pages;
using CoolapkUWP.Pages.FeedPages;
using System.Text.Json.Nodes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoolapkUWP.ViewModels.FeedPages
{
    public abstract partial class FeedListViewModel
    {
        internal class TagViewModel : FeedListViewModel
        {
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
                    AddTab(ItemSource, "最近回复", (p, firstItem, lastItem) => UriHelper.GetUri(UriType.GetTagFeeds, ID, p, UriHelper.GetOptionalArg("firstItem", firstItem), UriHelper.GetOptionalArg("lastItem", lastItem), "lastupdate_desc"));
                    AddTab(ItemSource, "最近发布", (p, firstItem, lastItem) => UriHelper.GetUri(UriType.GetTagFeeds, ID, p, UriHelper.GetOptionalArg("firstItem", firstItem), UriHelper.GetOptionalArg("lastItem", lastItem), "dateline_desc"));
                    AddTab(ItemSource, "热门动态", (p, firstItem, lastItem) => UriHelper.GetUri(UriType.GetTagFeeds, ID, p, UriHelper.GetOptionalArg("firstItem", firstItem), UriHelper.GetOptionalArg("lastItem", lastItem), "popular"));
                    base.ItemSource = ItemSource;
                }
            }

            public override Task<FeedListDetailBase> GetDetail() => GetDetailAsync(UriType.GetTagDetail, TopicDetail.FromJson);
        }
    }
}
