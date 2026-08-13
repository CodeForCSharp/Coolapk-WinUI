using CoolapkUWP.Models.Images;
using CoolapkUWP.Pages;
using CoolapkUWP.Pages.BrowserPages;
using CoolapkUWP.Pages.FeedPages;
using CoolapkUWP.ViewModels.BrowserPages;
using CoolapkUWP.ViewModels.FeedPages;
using CommunityToolkit.WinUI;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Launcher = Windows.System.Launcher;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;

namespace CoolapkUWP.Helpers
{
    /// <summary>
    /// 页面导航与酷安内链分发。通过 <see cref="App.MainPage"/> 定位承载页面的 Frame。
    /// </summary>
    internal static class NavigationHelper
    {
        public static Task<bool> NavigateAsync(this DependencyObject element, Type pageType, object parameter = null, NavigationTransitionInfo infoOverride = null)
        {
            MainPage mainPage = element is MainPage page ? page : element.FindAscendant<MainPage>() ?? App.MainPage;
            return mainPage.NavigationViewFrame.NavigateAsync(pageType, parameter, infoOverride);
        }

        public static Task<bool> NavigateAsync(this MainPage mainPage, Type pageType, object parameter = null, NavigationTransitionInfo infoOverride = null) =>
            mainPage.NavigationViewFrame.NavigateAsync(pageType, parameter, infoOverride);

        public static async Task<bool> NavigateAsync(this Frame frame, Type pageType, object parameter = null, NavigationTransitionInfo infoOverride = null)
        {
            try
            {
                return await frame.DispatcherQueue.EnqueueAsync(() =>
                    infoOverride is null
                        ? frame.Navigate(pageType, parameter)
                        : frame.Navigate(pageType, parameter, infoOverride));
            }
            catch (Exception e)
            {
                SettingsHelper.LogManager.CreateLogger(nameof(NavigationHelper)).LogError(e, e.ExceptionToMessage());
                return false;
            }
        }

        public static Task<bool> ShowImageAsync(this DependencyObject element, ImageModel image)
        {
            MainPage mainPage = element is MainPage page ? page : element.FindAscendant<MainPage>() ?? App.MainPage;
            return mainPage.ShowImageAsync(image);
        }

        public static Task<bool> ShowImageAsync(this MainPage mainPage, ImageModel image)
        {
            return mainPage.DispatcherQueue.EnqueueAsync(() => mainPage.Frame.Navigate(typeof(ShowImagePage), image));
        }

        public static Task<bool> OpenLinkAsync(this DependencyObject element, string link)
        {
            MainPage mainPage = element is MainPage page ? page : element.FindAscendant<MainPage>() ?? App.MainPage;
            return mainPage.NavigationViewFrame.OpenLinkAsync(link);
        }

        public static Task<bool> OpenLinkAsync(this MainPage mainPage, string link) =>
            mainPage.NavigationViewFrame.OpenLinkAsync(link);

        public static async Task<bool> OpenLinkAsync(this Frame frame, string link)
        {
            if (string.IsNullOrWhiteSpace(link)) { return false; }

            string origin = link;

            if (link.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                link = link.Replace("http://", string.Empty).Replace("https://", string.Empty);
                if (link.StartsWith("image.coolapk.com"))
                {
                    return await frame.ShowImageAsync(new ImageModel(origin, ImageType.SmallImage));
                }
                else
                {
                    Regex coolapk = new Regex(@"\w*?.?coolapk.\w*/");
                    if (coolapk.IsMatch(link))
                    {
                        link = coolapk.Replace(link, string.Empty);
                    }
                    else
                    {
                        return await frame.NavigateAsync(typeof(BrowserPage), new BrowserViewModel(origin));
                    }
                }
            }
            else if (link.StartsWith("coolapk://", StringComparison.OrdinalIgnoreCase))
            {
                link = link.Substring(10);
            }
            else if (link.StartsWith("coolmarket://", StringComparison.OrdinalIgnoreCase))
            {
                link = link.Substring(13);
            }

            if (link.FirstOrDefault() != '/')
            {
                link = $"/{link}";
            }

            if (link == "/contacts/fans")
            {
                return await frame.NavigateAsync(typeof(AdaptivePage), AdaptiveViewModel.GetUserListProvider(SettingsHelper.Get<string>(SettingsHelper.Uid), false, "我"));
            }
            else if (link == "/user/myFollowList")
            {
                return await frame.NavigateAsync(typeof(AdaptivePage), AdaptiveViewModel.GetUserListProvider(SettingsHelper.Get<string>(SettingsHelper.Uid), true, "我"));
            }
            else if (link.StartsWith("/page?", StringComparison.OrdinalIgnoreCase))
            {
                string url = link.Substring(6);
                return await frame.NavigateAsync(typeof(AdaptivePage), new AdaptiveViewModel(url));
            }
            else if (link.StartsWith("/u/", StringComparison.OrdinalIgnoreCase))
            {
                string url = link.Substring(3, "?");
                string uid = int.TryParse(url, out _) ? url : (await NetworkHelper.GetUserInfoByNameAsync(url)).UID;
                FeedListViewModel provider = FeedListViewModel.GetProvider(FeedListType.UserPageList, uid);
                if (provider != null)
                {
                    return await frame.NavigateAsync(typeof(FeedListPage), provider);
                }
            }
            else if (link.StartsWith("/feed/", StringComparison.OrdinalIgnoreCase))
            {
                string id = link.Substring(6, "?");
                if (int.TryParse(id, out _))
                {
                    return await frame.NavigateAsync(typeof(FeedShellPage), new FeedDetailViewModel(id));
                }
                else
                {
                    MessageHelper.ShowMessage("暂不支持");
                }
            }
            else if (link.StartsWith("/picture/", StringComparison.OrdinalIgnoreCase))
            {
                string id = link.Substring(10, "?");
                if (int.TryParse(id, out _))
                {
                    return await frame.NavigateAsync(typeof(FeedShellPage), new FeedDetailViewModel(id));
                }
            }
            else if (link.StartsWith("/question/", StringComparison.OrdinalIgnoreCase))
            {
                string id = link.Substring(10, "?");
                if (int.TryParse(id, out _))
                {
                    return await frame.NavigateAsync(typeof(FeedShellPage), new QuestionViewModel(id));
                }
            }
            else if (link.StartsWith("/vote/", StringComparison.OrdinalIgnoreCase))
            {
                string id = link.Substring(6, "?");
                if (int.TryParse(id, out _))
                {
                    return await frame.NavigateAsync(typeof(FeedShellPage), new VoteViewModel(id));
                }
            }
            else if (link.StartsWith("/t/", StringComparison.OrdinalIgnoreCase))
            {
                string tag = link.Substring(3, "?");
                FeedListViewModel provider = FeedListViewModel.GetProvider(FeedListType.TagPageList, tag);
                if (provider != null)
                {
                    return await frame.NavigateAsync(typeof(FeedListPage), provider);
                }
            }
            else if (link.StartsWith("/dyh/", StringComparison.OrdinalIgnoreCase))
            {
                string tag = link.Substring(5, "?");
                FeedListViewModel provider = FeedListViewModel.GetProvider(FeedListType.DyhPageList, tag);
                if (provider != null)
                {
                    return await frame.NavigateAsync(typeof(FeedListPage), provider);
                }
            }
            else if (link.StartsWith("/product/", StringComparison.OrdinalIgnoreCase))
            {
                if (link.StartsWith("/product/categoryList", StringComparison.OrdinalIgnoreCase))
                {
                    return await frame.NavigateAsync(typeof(AdaptivePage), new AdaptiveViewModel(link));
                }
                else
                {
                    string tag = link.Substring(9, "?");
                    FeedListViewModel provider = FeedListViewModel.GetProvider(FeedListType.ProductPageList, tag);
                    if (provider != null)
                    {
                        return await frame.NavigateAsync(typeof(FeedListPage), provider);
                    }
                }
            }
            else if (link.StartsWith("/collection/", StringComparison.OrdinalIgnoreCase))
            {
                string id = link.Substring(12, "?");
                FeedListViewModel provider = FeedListViewModel.GetProvider(FeedListType.CollectionPageList, id);
                if (provider != null)
                {
                    return await frame.NavigateAsync(typeof(FeedListPage), provider);
                }
            }
            else if (link.StartsWith("/mp/", StringComparison.OrdinalIgnoreCase))
            {
                return await frame.NavigateAsync(typeof(HTMLPage), new HTMLViewModel(origin));
            }
            else if (origin.StartsWith("http://") || link.StartsWith("https://"))
            {
                return await frame.NavigateAsync(typeof(BrowserPage), new BrowserViewModel(origin));
            }
            else if (origin.Contains("://"))
            {
                return await frame.DispatcherQueue.EnqueueAsync(async () => await Launcher.LaunchUriAsync(origin.ValidateAndGetUri()));
            }
            else
            {
                return false;
            }

            return true;
        }

        private static string Substring(this string str, int startIndex, string endString)
        {
            int end = str.IndexOf(endString);
            return end > startIndex ? str.Substring(startIndex, end - startIndex) : str.Substring(startIndex);
        }
    }
}
