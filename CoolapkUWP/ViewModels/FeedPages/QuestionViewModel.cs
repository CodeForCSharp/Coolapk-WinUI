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
    public class QuestionViewModel : FeedShellViewModel
    {
        public QuestionItemSource ReplyItemSource { get; private set; }
        public QuestionItemSource LikeItemSource { get; private set; }
        public QuestionItemSource DatelineItemSource { get; private set; }

        internal QuestionViewModel(string id) : base(id) { }

        public override async Task Refresh(bool reset = false)
        {
            if (FeedDetail == null || reset)
            {
                FeedDetail = await GetFeedDetailAsync(ID);
                if (FeedDetail == null) { return; }
                List<ShyHeaderItem> ItemSource = new List<ShyHeaderItem>();
                Title = FeedDetail.Title;
                if (ReplyItemSource == null || ReplyItemSource.ID != ID)
                {
                    ReplyItemSource = new QuestionItemSource(ID, "reply");
                    ReplyItemSource.LoadMoreStarted += UIHelper.ShowProgressBar;
                    ReplyItemSource.LoadMoreCompleted += UIHelper.HideProgressBar;
                }
                ItemSource.Add(new ShyHeaderItem
                {
                    Header = "热度排序",
                    ItemSource = ReplyItemSource
                });
                if (LikeItemSource == null || LikeItemSource.ID != ID)
                {
                    LikeItemSource = new QuestionItemSource(ID, "like");
                    LikeItemSource.LoadMoreStarted += UIHelper.ShowProgressBar;
                    LikeItemSource.LoadMoreCompleted += UIHelper.HideProgressBar;
                }
                ItemSource.Add(new ShyHeaderItem
                {
                    Header = "点赞排序",
                    ItemSource = LikeItemSource
                });
                if (DatelineItemSource == null || DatelineItemSource.ID != ID)
                {
                    DatelineItemSource = new QuestionItemSource(ID, "dateline");
                    DatelineItemSource.LoadMoreStarted += UIHelper.ShowProgressBar;
                    DatelineItemSource.LoadMoreCompleted += UIHelper.HideProgressBar;
                }
                ItemSource.Add(new ShyHeaderItem
                {
                    Header = "时间排序",
                    ItemSource = DatelineItemSource
                });
                base.ItemSource = ItemSource;
            }
            await ReplyItemSource?.Refresh(reset);
        }
    }

}
