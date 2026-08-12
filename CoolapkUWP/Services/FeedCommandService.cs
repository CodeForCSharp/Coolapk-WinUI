using CoolapkUWP.Controls;
using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using CoolapkUWP.Models.Feeds;
using CoolapkUWP.Pages.BrowserPages;
using CoolapkUWP.ViewModels.BrowserPages;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;

namespace CoolapkUWP.Services
{
    /// <summary>
    /// 动态卡片操作按钮的统一分发（回复/点赞/举报/分享等）。
    /// </summary>
    internal static class FeedCommandService
    {
        public static void HandleFeedButtonClick(FrameworkElement element, UIElement host)
        {
            void DisabledCopy()
            {
                if (element.DataContext is ICanCopy i)
                {
                    i.IsCopyEnabled = false;
                }
            }

            switch (element.Name)
            {
                case "MakeReplyButton":
                    DisabledCopy();
                    break;

                case "LikeButton":
                    DisabledCopy();
                    _ = FeedActionsService.ChangeLikeAsync(element.Tag as ICanLike);
                    break;

                case "ReportButton":
                    DisabledCopy();
                    _ = host.NavigateAsync(typeof(BrowserPage), new BrowserViewModel(element.Tag.ToString()));
                    break;

                case "ReplyButton":
                    DisabledCopy();
                    if (element.Tag is FeedModelBase feed)
                    {
                        CreateFeedControl.ShowReply(host, feed.ID, CreateFeedType.Reply);
                    }
                    else if (element.Tag is FeedReplyModel reply)
                    {
                        CreateFeedControl.ShowReply(host, reply.ID, CreateFeedType.ReplyReply);
                    }
                    DisabledCopy();
                    break;

                case "ShareButton":
                case "ChangeButton":
                    DisabledCopy();
                    break;

                default:
                    DisabledCopy();
                    _ = host.OpenLinkAsync(element.Tag as string);
                    break;
            }
        }
    }
}
