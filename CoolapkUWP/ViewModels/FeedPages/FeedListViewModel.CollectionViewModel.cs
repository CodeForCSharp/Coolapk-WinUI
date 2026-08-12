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
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
                        List<CollectionContentsDto> contents = JsonSerializer.Deserialize<List<CollectionContentsDto>>(result, DtoJson.Options);
                        foreach (CollectionContentsDto item in contents ?? new List<CollectionContentsDto>())
                        {
                            if (item.EntityTemplate == "selectorLinkCard" && item.Entities != null)
                            {
                                List<ShyHeaderItem> ItemSource = new List<ShyHeaderItem>();
                                foreach (SelectorEntityDto entity in item.Entities)
                                {
                                    if (!string.IsNullOrEmpty(entity.Url))
                                    {
                                        CoolapkListProvider Provider = new CoolapkListProvider(
                                            (p, firstItem, lastItem) => UriHelper.GetUri(UriType.DataList, entity.Url.Replace("#", "%23").Replace("/", "%2F").Replace("?", "%3F").Replace("=", "%3D").Replace("&", "%26"), $"&page={p}" + (string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}") + (string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}")),
                                            GetEntities,
                                            "id");
                                        FeedListItemSource FeedListItemSource = new FeedListItemSource(ID, Provider);
                                        ShyHeaderItem ShyHeaderItem = new ShyHeaderItem { ItemSource = FeedListItemSource };
                                        if (!string.IsNullOrEmpty(entity.Title))
                                        {
                                            ShyHeaderItem.Header = entity.Title;
                                        }
                                        ItemSource.Add(ShyHeaderItem);
                                    }
                                }
                                this.ItemSource = ItemSource;
                                break;
                            }
                        }
                        if (ItemSource == null)
                        {
                            List<ShyHeaderItem> ItemSource = new List<ShyHeaderItem>();
                            CoolapkListProvider Provider = new CoolapkListProvider(
                                (p, firstItem, lastItem) => UriHelper.GetUri(UriType.GetCollectionContents, ID, p, string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}", string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}"),
                                GetEntities,
                                "id");
                            FeedListItemSource FeedListItemSource = new FeedListItemSource(ID, Provider);
                            ShyHeaderItem ShyHeaderItem = new ShyHeaderItem
                            {
                                ItemSource = FeedListItemSource,
                                Header = Detail is CollectionDetail CollectionDetail && CollectionDetail.ItemNum > 0 ? $"全部({CollectionDetail.ItemNum})" : (object)$"全部"
                            };
                            ItemSource.Add(ShyHeaderItem);
                            this.ItemSource = ItemSource;
                        }
                    }
                }
            }
            public override async Task<FeedListDetailBase> GetDetail()
            {
                (bool isSucceed, JsonNode result) = await RequestHelper.GetDataAsync(UriHelper.GetUri(UriType.GetCollectionDetail, ID), true);
                if (!isSucceed) { return null; }

                JsonObject token = result.AsObject();
                FeedListDetailBase detail = null;

                if (token != null)
                {
                    detail = CollectionDetail.FromJson(token);
                }

                return detail;
            }
        }
    }

}
