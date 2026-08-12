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
        internal class ProductViewModel : FeedListViewModel
        {
            internal ProductViewModel(string id) : base(id, FeedListType.ProductPageList) { }

            public override async Task Refresh(bool reset = false)
            {
                if (Detail == null || reset)
                {
                    Detail = await GetDetail();
                    Title = (Detail as ProductDetail)?.Title;
                }
                if (ItemSource == null)
                {
                    List<ShyHeaderItem> ItemSource = new List<ShyHeaderItem>();
                    AddTab(ItemSource, "讨论", (p, firstItem, lastItem) => UriHelper.GetUri(UriType.GetProductFeeds, ID, p, UriHelper.GetOptionalArg("firstItem", firstItem), UriHelper.GetOptionalArg("lastItem", lastItem), "feed"));
                    AddTab(ItemSource, "问答", (p, firstItem, lastItem) => UriHelper.GetUri(UriType.GetProductFeeds, ID, p, UriHelper.GetOptionalArg("firstItem", firstItem), UriHelper.GetOptionalArg("lastItem", lastItem), "answer"));
                    AddTab(ItemSource, "图文", (p, firstItem, lastItem) => UriHelper.GetUri(UriType.GetProductFeeds, ID, p, UriHelper.GetOptionalArg("firstItem", firstItem), UriHelper.GetOptionalArg("lastItem", lastItem), "article"));
                    AddTab(ItemSource, "视频", (p, firstItem, lastItem) => UriHelper.GetUri(UriType.GetProductFeeds, ID, p, UriHelper.GetOptionalArg("firstItem", firstItem), UriHelper.GetOptionalArg("lastItem", lastItem), "video"));
                    AddTab(ItemSource, "交易", (p, firstItem, lastItem) => UriHelper.GetUri(UriType.GetProductFeeds, ID, p, UriHelper.GetOptionalArg("firstItem", firstItem), UriHelper.GetOptionalArg("lastItem", lastItem), "trade"));
                    base.ItemSource = ItemSource;
                }
            }

            public override Task<FeedListDetailBase> GetDetail() => GetDetailAsync(UriType.GetProductDetail, ProductDetail.FromJson);
        }
    }
}
