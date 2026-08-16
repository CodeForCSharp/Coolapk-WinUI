using CoolapkUWP.Controls;
using CoolapkUWP.Data;
using CoolapkUWP.Data.Dtos;
using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Pages;
using CoolapkUWP.Pages.FeedPages;
using CoolapkUWP.ViewModels.DataSource;
using CoolapkUWP.ViewModels.Providers;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoolapkUWP.ViewModels.FeedPages
{
    public abstract partial class FeedListViewModel
    {
        internal class CollectionViewModel : FeedListViewModel
        {
            internal CollectionViewModel(string id) : base(id, FeedListType.CollectionPageList) { }

            public override async Task Refresh(bool reset = false)
            {
                if (Detail == null || reset)
                {
                    Detail = await GetDetail();
                    Title = (Detail as CollectionDetail)?.Title;
                }
                if (ItemSource == null)
                {
                    (bool isSucceed, JsonNode result) = await RequestHelper.GetDataAsync(UriHelper.GetUri(UriType.GetCollectionContents, ID, "1", ""), true);
                    if (isSucceed)
                    {
                        List<CollectionContentsDto> contents = DtoJson.DeserializeList<CollectionContentsDto>(result);
                        foreach (CollectionContentsDto item in contents ?? new List<CollectionContentsDto>())
                        {
                            if (item.EntityTemplate == "selectorLinkCard" && item.Entities != null)
                            {
                                List<ShyHeaderItem> ItemSource = new List<ShyHeaderItem>();
                                foreach (SelectorEntityDto entity in item.Entities)
                                {
                                    if (!string.IsNullOrEmpty(entity.Url))
                                    {
                                        FeedListItemSource tab = new FeedListItemSource(ID, new CoolapkListProvider(
                                            (p, firstItem, lastItem) => UriHelper.GetUri(UriType.DataList, entity.Url.Replace("#", "%23").Replace("/", "%2F").Replace("?", "%3F").Replace("=", "%3D").Replace("&", "%26"), $"&page={p}" + UriHelper.GetOptionalArg("firstItem", firstItem) + UriHelper.GetOptionalArg("lastItem", lastItem)),
                                            GetEntities,
                                            "id"));
                                        ShyHeaderItem headerItem = new ShyHeaderItem { ItemSource = tab };
                                        if (!string.IsNullOrEmpty(entity.Title))
                                        {
                                            headerItem.Header = entity.Title;
                                        }
                                        ItemSource.Add(headerItem);
                                    }
                                }
                                this.ItemSource = ItemSource;
                                return;
                            }
                        }
                        if (ItemSource == null)
                        {
                            List<ShyHeaderItem> ItemSource = new List<ShyHeaderItem>();
                            AddTab(ItemSource, Detail is CollectionDetail CollectionDetail && CollectionDetail.ItemNum > 0 ? $"全部({CollectionDetail.ItemNum})" : "全部", (p, firstItem, lastItem) => UriHelper.GetUri(UriType.GetCollectionContents, ID, p, UriHelper.GetOptionalArg("firstItem", firstItem), UriHelper.GetOptionalArg("lastItem", lastItem)));
                            this.ItemSource = ItemSource;
                        }
                    }
                }
            }

            public override Task<FeedListDetailBase> GetDetail() => GetDetailAsync(UriType.GetCollectionDetail, CollectionDetail.FromJson);
        }
    }
}
