using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using CoolapkUWP.Models.Feeds;
using CoolapkUWP.Pages.BrowserPages;
using CoolapkUWP.Pages.FeedPages;
using CoolapkUWP.ViewModels.BrowserPages;
using CoolapkUWP.ViewModels.FeedPages;
using CommunityToolkit.WinUI;
using System;
using Microsoft.UI.Xaml;

namespace CoolapkUWP.Services
{
    /// <summary>
    /// 首页卡片点击路由：识别"我的常去/浏览历史"等特殊卡片，其余走通用链接打开。
    /// </summary>
    internal static class CardNavigationService
    {
        /// <summary>
        /// 处理卡片点击。返回 false 表示卡片无可执行操作（如空 URL 的 quickList 卡片）。
        /// </summary>
        public static bool HandleCardTap(DependencyObject host, object tag)
        {
            if (tag is string str)
            {
                OpenUrlOrSpecial(host, str);
                return true;
            }
            if (tag is SourceFeedModel feed)
            {
                if (string.IsNullOrEmpty(feed.Url)) { return false; }
                OpenUrlOrSpecial(host, feed.Url);
                return true;
            }
            if (tag is IHasTitle model)
            {
                if (string.IsNullOrEmpty(model.Url) || model.Url == "/topic/quickList?quickType=list") { return false; }
                string url = model.Url;
                if (url == "Login")
                {
                    _ = host.NavigateAsync(typeof(BrowserPage), new BrowserViewModel(UriHelper.LoginUri));
                }
                else if (url.IndexOf("/page", StringComparison.Ordinal) == 0)
                {
                    url = url.Replace("/page", "/page/dataList");
                    url += $"&title={model.Title}";
                    _ = host.NavigateAsync(typeof(AdaptivePage), new AdaptiveViewModel(url));
                }
                else if (url.IndexOf('#') == 0)
                {
                    _ = host.NavigateAsync(typeof(AdaptivePage), new AdaptiveViewModel($"{url}&title={model.Title}"));
                }
                else
                {
                    OpenUrlOrSpecial(host, url);
                }
                return true;
            }
            return false;
        }

        private static void OpenUrlOrSpecial(DependencyObject host, string url)
        {
            if (url.Contains("我的常去"))
            {
                _ = host.NavigateAsync(typeof(AdaptivePage), AdaptiveViewModel.GetHistoryProvider("我的常去"));
            }
            else if (url.Contains("浏览历史"))
            {
                _ = host.NavigateAsync(typeof(AdaptivePage), AdaptiveViewModel.GetHistoryProvider("浏览历史"));
            }
            else if (url.Contains("我关注的话题"))
            {
                _ = host.NavigateAsync(typeof(AdaptivePage), new AdaptiveViewModel("#/topic/userFollowTagList"));
            }
            else if (url.Contains("我的收藏单"))
            {
            }
            else if (url.Contains("我的问答"))
            {
                string uid = SettingsHelper.Get<string>(SettingsHelper.Uid);
                if (uid != null) { _ = host.NavigateAsync(typeof(AdaptivePage), AdaptiveViewModel.GetUserFeedsProvider(uid, "questionAndAnswer")); }
            }
            else
            {
                _ = host.OpenLinkAsync(url);
            }
        }
    }
}
