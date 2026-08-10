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
    public class FeedDetailViewModel : FeedShellViewModel
    {
        public ReplyItemSource ReplyItemSource { get; private set; }
        public LikeItemSource LikeItemSource { get; private set; }
        public ShareItemSource ShareItemSource { get; private set; }

        internal FeedDetailViewModel(string id) : base(id) { }

        public override async Task Refresh(bool reset = false)
        {
            if (FeedDetail == null || reset)
            {
                FeedDetail = await GetFeedDetailAsync(ID);
                List<ShyHeaderItem> ItemSource = new List<ShyHeaderItem>();
                Title = FeedDetail.Title;
                if (ReplyItemSource == null || ReplyItemSource.ID != ID)
                {
                    ReplyItemSource = new ReplyItemSource(ID);
                    ReplyItemSource.LoadMoreStarted += UIHelper.ShowProgressBar;
                    ReplyItemSource.LoadMoreCompleted += UIHelper.HideProgressBar;
                }
                ItemSource.Add(new ShyHeaderItem
                {
                    Header = "回复",
                    ItemSource = ReplyItemSource
                });
                if (LikeItemSource == null || LikeItemSource.ID != ID)
                {
                    LikeItemSource = new LikeItemSource(ID);
                    LikeItemSource.LoadMoreStarted += UIHelper.ShowProgressBar;
                    LikeItemSource.LoadMoreCompleted += UIHelper.HideProgressBar;
                }
                ItemSource.Add(new ShyHeaderItem
                {
                    Header = "点赞",
                    ItemSource = LikeItemSource
                });
                if (ShareItemSource == null || ShareItemSource.ID != ID)
                {
                    ShareItemSource = new ShareItemSource(ID, FeedDetail.FeedType);
                    ShareItemSource.LoadMoreStarted += UIHelper.ShowProgressBar;
                    ShareItemSource.LoadMoreCompleted += UIHelper.HideProgressBar;
                }
                ItemSource.Add(new ShyHeaderItem
                {
                    Header = "转发",
                    ItemSource = ShareItemSource
                });
                base.ItemSource = ItemSource;
            }
            await ReplyItemSource?.Refresh(reset);
        }
    }

}
