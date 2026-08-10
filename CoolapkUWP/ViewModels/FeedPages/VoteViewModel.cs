using CoolapkUWP.Controls;
using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using CoolapkUWP.Models.Feeds;
using CoolapkUWP.Models.Users;
using CoolapkUWP.ViewModels.DataSource;
using CoolapkUWP.ViewModels.Providers;
using System.Text.Json.Nodes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;

namespace CoolapkUWP.ViewModels.FeedPages
{
    public class VoteViewModel : FeedShellViewModel
    {
        internal VoteViewModel(string id) : base(id) { }

        public override async Task Refresh(bool reset = false)
        {
            if (FeedDetail == null || reset)
            {
                FeedDetail = await GetFeedDetailAsync(ID);
                if (FeedDetail == null) { return; }
                List<ShyHeaderItem> ItemSource = new List<ShyHeaderItem>();
                Title = FeedDetail.Title;
                if (FeedDetail.VoteType == 0)
                {
                    foreach (VoteItem vote in FeedDetail.VoteRows)
                    {
                        VoteItemSource VoteItemSource = new VoteItemSource(vote.ID.ToString(), vote.VoteID.ToString());
                        VoteItemSource.LoadMoreStarted += UIHelper.ShowProgressBar;
                        VoteItemSource.LoadMoreCompleted += UIHelper.HideProgressBar;
                        ItemSource.Add(new ShyHeaderItem
                        {
                            Header = vote.Title,
                            ItemSource = VoteItemSource
                        });
                    }
                }
                else
                {
                    VoteItemSource VoteItemSource = new VoteItemSource(string.Empty, FeedDetail.ID.ToString());
                    VoteItemSource.LoadMoreStarted += UIHelper.ShowProgressBar;
                    VoteItemSource.LoadMoreCompleted += UIHelper.HideProgressBar;
                    ItemSource.Add(new ShyHeaderItem
                    {
                        Header = "观点",
                        ItemSource = VoteItemSource
                    });
                    if (!string.IsNullOrEmpty(FeedDetail.VoteTag))
                    {
                        TagItemSource TagItemSource = new TagItemSource(FeedDetail.VoteTag);
                        TagItemSource.LoadMoreStarted += UIHelper.ShowProgressBar;
                        TagItemSource.LoadMoreCompleted += UIHelper.HideProgressBar;
                        ItemSource.Add(new ShyHeaderItem
                        {
                            Header = "话题",
                            ItemSource = TagItemSource
                        });
                    }
                }
                base.ItemSource = ItemSource;
            }
            await (ItemSource.FirstOrDefault()?.ItemSource as EntityItemSource)?.Refresh(reset);
        }
    }

    [WinRT.GeneratedBindableCustomProperty]
}
