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
        internal class DyhViewModel : FeedListViewModel
        {
            internal DyhViewModel(string id) : base(id, FeedListType.DyhPageList) { }

            public override async Task Refresh(bool reset = false)
            {
                if (Detail == null || reset)
                {
                    Detail = await GetDetail();
                    Title = (Detail as DyhDetail)?.Title;
                }
                if (ItemSource == null)
                {
                    List<ShyHeaderItem> ItemSource = new List<ShyHeaderItem>();
                    AddTab(ItemSource, "精选", (p, firstItem, lastItem) => UriHelper.GetUri(UriType.GetDyhFeeds, ID, "all", p, UriHelper.GetOptionalArg("firstItem", firstItem), UriHelper.GetOptionalArg("lastItem", lastItem)));
                    AddTab(ItemSource, "广场", (p, firstItem, lastItem) => UriHelper.GetUri(UriType.GetTagFeeds, ID, "square", p, UriHelper.GetOptionalArg("firstItem", firstItem), UriHelper.GetOptionalArg("lastItem", lastItem)));
                    base.ItemSource = ItemSource;
                }
            }

            public override Task<FeedListDetailBase> GetDetail() => GetDetailAsync(UriType.GetDyhDetail, DyhDetail.FromJson);
        }
    }
}
