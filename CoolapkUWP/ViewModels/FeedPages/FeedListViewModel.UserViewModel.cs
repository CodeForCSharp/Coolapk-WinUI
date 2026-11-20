using CoolapkUWP.Controls;
using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Pages;
using CoolapkUWP.Pages.FeedPages;
using System;
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
                    (bool isSucceed, JsonNode result) = await RequestHelper.GetDataAsync(UriHelper.GetUri(UriType.GetUserSpace, ID), true);
                    if (isSucceed && result is JsonObject token)
                    {
                        Detail = UserDetail.FromJson(token);
                        Title = (Detail as UserDetail)?.UserName;
                    }
                }
                if (ItemSource == null || reset)
                {
                    // 打开时停在「动态」。主页由服务端卡片下发，没有固定列表地址，先留空。
                    List<ShyHeaderItem> tabs = new List<ShyHeaderItem>();
                    AddEmptyTab(tabs, "主页");
                    AddUserTab(tabs, "动态", "feed");
                    AddDataTab(tabs, "点评", $"#/feed/nodeRatingList?uid={ID}&targetType=all&parseRatingToFeed=1");
                    AddUserTab(tabs, "图文", "htmlFeed");
                    AddUserTab(tabs, "问答", "questionAndAnswer");
                    AddDataTab(tabs, "酷图", "#/feed/userCoolPictureFeedList?fragmentTemplate=flex");
                    AddDataTab(tabs, "二手", "#/feed/userErshouList?fragmentTemplate=flex&ershouStatus=userAll");
                    AddDataTab(tabs, "好物", "#/goods/goodsFeedList?type=default&fragmentTemplate=flex");
                    AddTab(tabs, "好物榜", (p, firstItem, lastItem) => UriHelper.GetUri(UriType.GetUserGoodsList, ID, p, UriHelper.GetOptionalArg("firstItem", firstItem), UriHelper.GetOptionalArg("lastItem", lastItem)));
                    AddTab(tabs, "收藏单", (p, firstItem, lastItem) => UriHelper.GetUri(UriType.GetCollectionList, ID, p, UriHelper.GetOptionalArg("firstItem", firstItem) + UriHelper.GetOptionalArg("lastItem", lastItem)));
                    AddUserTab(tabs, "应用集", "album");
                    SelectedTabIndex = 1;
                    ItemSource = tabs;
                }
            }

            private void AddUserTab(List<ShyHeaderItem> tabs, string title, string list)
            {
                AddTab(tabs, title, (p, firstItem, lastItem) => UriHelper.GetUri(UriType.GetUserFeeds, ID, p, UriHelper.GetOptionalArg("firstItem", firstItem), UriHelper.GetOptionalArg("lastItem", lastItem), list));
            }

            private void AddDataTab(List<ShyHeaderItem> tabs, string title, string url)
            {
                string pageUrl = url.Contains("uid=") ? url : $"{url}&uid={ID}";
                string encoded = UriHelper.EncodeDataListUrl(pageUrl);
                AddTab(tabs, title, (p, firstItem, lastItem) => UriHelper.GetUri(
                    UriType.GetUserPageList,
                    encoded,
                    Uri.EscapeDataString(title),
                    p,
                    UriHelper.GetOptionalArg("firstItem", firstItem),
                    UriHelper.GetOptionalArg("lastItem", lastItem)));
            }

            private void AddEmptyTab(List<ShyHeaderItem> tabs, string title)
            {
                tabs.Add(new ShyHeaderItem { Header = title, ItemSource = new FeedListItemSource(ID, null) });
            }

            public override Task<FeedListDetailBase> GetDetail() => GetDetailAsync(UriType.GetUserSpace, UserDetail.FromJson);
        }
    }
}
