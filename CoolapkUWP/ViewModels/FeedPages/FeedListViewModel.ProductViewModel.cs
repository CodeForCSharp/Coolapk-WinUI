using CoolapkUWP.Controls;
using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Pages;
using CoolapkUWP.Pages.FeedPages;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoolapkUWP.ViewModels.FeedPages
{
    public abstract partial class FeedListViewModel
    {
        public class ProductViewModel : FeedListViewModel
        {
            internal ProductViewModel(string id) : base(id, FeedListType.ProductPageList) { }

            public override async Task Refresh(bool reset = false)
            {
                if (Detail == null || reset)
                {
                    Detail = await GetDetail();
                    Title = (Detail as ProductDetail)?.Title;
                }
                if (ItemSource == null || reset)
                {
                    List<ShyHeaderItem> tabs = new List<ShyHeaderItem>();
                    ProductDetail detail = Detail as ProductDetail;
                    if (detail != null && detail.Tabs.Count > 0)
                    {
                        int selected = 0;
                        for (int i = 0; i < detail.Tabs.Count; i++)
                        {
                            ProductTab tab = detail.Tabs[i];
                            if (string.IsNullOrEmpty(tab.Url))
                            {
                                tabs.Add(new ShyHeaderItem { Header = tab.Title, ItemSource = new FeedListItemSource(ID, null) });
                            }
                            else
                            {
                                string url = tab.Url;
                                string title = tab.Title;
                                string subTitle = tab.SubTitle;
                                AddTab(tabs, title, (p, firstItem, lastItem) => TabUri(url, title, subTitle, p, firstItem, lastItem));
                            }
                            if (IsSelected(detail.SelectedTabName, tab)) { selected = i; }
                        }
                        SelectedTabIndex = selected;
                    }
                    else
                    {
                        // 详情没下发标签时，只保留地址明确的讨论。
                        AddTab(tabs, "讨论", (p, firstItem, lastItem) => UriHelper.GetUri(UriType.GetProductFeeds, ID, p, UriHelper.GetOptionalArg("firstItem", firstItem), UriHelper.GetOptionalArg("lastItem", lastItem), "feed"));
                    }
                    ItemSource = tabs;
                }
            }

            public override Task<FeedListDetailBase> GetDetail() => GetDetailAsync(UriType.GetProductDetail, ProductDetail.FromJson);

            private static bool IsSelected(string selected, ProductTab tab)
            {
                if (string.IsNullOrEmpty(selected)) { return false; }
                return string.Equals(selected, tab.PageName, StringComparison.Ordinal)
                    || string.Equals(selected, tab.Title, StringComparison.Ordinal);
            }

            /// <summary>
            /// 标签地址原样放进 dataList 的 url。空地址不会走到这里。
            /// </summary>
            private static Uri TabUri(string url, string title, string subTitle, int page, string firstItem, string lastItem)
            {
                string paging = page > 1
                    ? UriHelper.GetOptionalArg("firstItem", firstItem) + UriHelper.GetOptionalArg("lastItem", lastItem)
                    : string.Empty;
                if (url.StartsWith("/v6/", StringComparison.Ordinal))
                {
                    string path = url.Substring(3);
                    string join = path.Contains('?') ? "&" : "?";
                    return UriHelper.GetUri(UriType.GetIndexPage, path, join, page + paging);
                }

                string extra = "&title=" + Uri.EscapeDataString(title ?? string.Empty);
                if (!string.IsNullOrEmpty(subTitle))
                {
                    extra += "&subTitle=" + Uri.EscapeDataString(subTitle);
                }
                extra += "&page=" + page + paging;
                return UriHelper.GetUri(UriType.DataList, Uri.EscapeDataString(url), extra);
            }
        }
    }
}
