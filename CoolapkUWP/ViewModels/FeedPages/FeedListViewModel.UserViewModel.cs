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
        public class UserViewModel : FeedListViewModel
        {
            internal UserViewModel(string uid) : base(uid, FeedListType.UserPageList) { }

            public override async Task Refresh(bool reset = false)
            {
                if (Detail == null || reset)
                {
                    Detail = await GetDetail();
                    Title = (Detail as UserDetail)?.UserName;
                }
                if (ItemSource == null)
                {
                    List<ShyHeaderItem> ItemSource = new List<ShyHeaderItem>();
                    AddTab(ItemSource, "动态", (p, firstItem, lastItem) => UriHelper.GetUri(UriType.GetUserFeeds, ID, p, UriHelper.GetOptionalArg("firstItem", firstItem), UriHelper.GetOptionalArg("lastItem", lastItem), "feed"));
                    AddTab(ItemSource, "图文", (p, firstItem, lastItem) => UriHelper.GetUri(UriType.GetUserFeeds, ID, p, UriHelper.GetOptionalArg("firstItem", firstItem), UriHelper.GetOptionalArg("lastItem", lastItem), "htmlFeed"));
                    AddTab(ItemSource, "问答", (p, firstItem, lastItem) => UriHelper.GetUri(UriType.GetUserFeeds, ID, p, UriHelper.GetOptionalArg("firstItem", firstItem), UriHelper.GetOptionalArg("lastItem", lastItem), "questionAndAnswer"));
                    AddTab(ItemSource, "收藏单", (p, firstItem, lastItem) => UriHelper.GetUri(UriType.GetCollectionList, ID, p, UriHelper.GetOptionalArg("firstItem", firstItem), UriHelper.GetOptionalArg("lastItem", lastItem)));
                    base.ItemSource = ItemSource;
                }
            }

            public override Task<FeedListDetailBase> GetDetail() => GetDetailAsync(UriType.GetUserSpace, UserDetail.FromJson);
        }
    }
}
