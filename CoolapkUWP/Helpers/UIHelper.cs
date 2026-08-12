using CoolapkUWP.Models.Images;
using CoolapkUWP.Pages;
using CoolapkUWP.Pages.BrowserPages;
using CoolapkUWP.Pages.FeedPages;
using CoolapkUWP.Pages.SettingsPages;
using CoolapkUWP.ViewModels.BrowserPages;
using CoolapkUWP.ViewModels.FeedPages;
using CommunityToolkit.WinUI;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Launcher = Windows.System.Launcher;
using DispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media.Animation;

namespace CoolapkUWP.Helpers
{
    internal static partial class UIHelper
    {
        public const int Duration = 3000;
        public static bool IsShowingProgressBar, IsShowingMessage;
        public static List<string> MessageList { get; } = new List<string>();
    }

    internal static partial class UIHelper
    {
        public static void ShowProgressBar()
        {
            _ = MainPage?.DispatcherQueue.EnqueueAsync(() =>
            {
                IsShowingProgressBar = true;
                MainPage?.ShowProgressBar();
            });
        }

        public static void ShowProgressBar(double value = 0)
        {
            _ = MainPage?.DispatcherQueue.EnqueueAsync(() =>
            {
                IsShowingProgressBar = true;
                MainPage?.ShowProgressBar(value);
            });
        }

        public static void PausedProgressBar()
        {
            _ = MainPage?.DispatcherQueue.EnqueueAsync(() =>
            {
                IsShowingProgressBar = true;
                MainPage?.PausedProgressBar();
            });
        }

        public static void ErrorProgressBar()
        {
            _ = MainPage?.DispatcherQueue.EnqueueAsync(() =>
            {
                IsShowingProgressBar = true;
                MainPage?.ErrorProgressBar();
            });
        }

        public static void HideProgressBar()
        {
            _ = MainPage?.DispatcherQueue.EnqueueAsync(() =>
            {
                IsShowingProgressBar = false;
                MainPage?.HideProgressBar();
            });
        }

        public static void ShowMessage(string message)
        {
            MessageList.Add(message);
            if (!IsShowingMessage)
            {
                IsShowingMessage = true;
                _ = MainPage?.DispatcherQueue.EnqueueAsync(async () =>
                {
                    while (MessageList.Any())
                    {
                        if (MainPage != null)
                        {
                            if (!string.IsNullOrEmpty(MessageList[0]))
                            {
                                string messages = $"[{MessageList.Count}] {MessageList[0].Replace("\n", " ")}";
                                MainPage.ShowMessage(messages);
                                await Task.Delay(Duration);
                            }
                            MessageList.RemoveAt(0);
                            if (MessageList.Count == 0)
                            {
                                MainPage.ShowMessage();
                            }
                        }
                    }
                    IsShowingMessage = false;
                });
            }
        }

        public static void ShowHttpExceptionMessage(HttpRequestException e)
        {
            if (e.Message.IndexOfAny(new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' }) != -1)
            { ShowMessage($"服务器错误： {e.Message.Replace("Response status code does not indicate success: ", string.Empty)}"); }
            else if (e.Message == "An error occurred while sending the request.") { ShowMessage("无法连接网络。"); }
            else { ShowMessage($"请检查网络连接。 {e.Message}"); }
        }

        public static bool IsOriginSource(object source, object originalSource)
        {
            if (source == originalSource) { return true; }

            bool result = false;
            FrameworkElement DependencyObject = originalSource as FrameworkElement;
            if (DependencyObject.FindAscendant<ButtonBase>() == null && !(originalSource is ButtonBase) && !(originalSource is RichEditBox))
            {
                if (source is FrameworkElement FrameworkElement)
                {
                    result = FrameworkElement == DependencyObject.FindAscendant(FrameworkElement.Name);
                }
            }

            return DependencyObject.Tag == null && result;
        }

        public static string ExceptionToMessage(this Exception ex)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append('\n');
            if (!string.IsNullOrWhiteSpace(ex.Message)) { builder.AppendLine($"Message: {ex.Message}"); }
            builder.AppendLine($"HResult: {ex.HResult} (0x{Convert.ToString(ex.HResult, 16)})");
            if (!string.IsNullOrWhiteSpace(ex.StackTrace)) { builder.AppendLine(ex.StackTrace); }
            if (!string.IsNullOrWhiteSpace(ex.HelpLink)) { builder.Append($"HelperLink: {ex.HelpLink}"); }
            return builder.ToString();
        }
    }

    internal static partial class UIHelper
    {
        public static MainPage MainPage;

        public static Task<bool> NavigateAsync(this DependencyObject element, Type pageType, object parameter = null, NavigationTransitionInfo infoOverride = null)
        {
            MainPage mainPage = element is MainPage page ? page : element.FindAscendant<MainPage>() ?? MainPage;
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
                SettingsHelper.LogManager.CreateLogger(nameof(UIHelper)).LogError(e, e.ExceptionToMessage());
                return false;
            }
        }

        public static Task<bool> ShowImageAsync(this DependencyObject element, ImageModel image)
        {
            MainPage mainPage = element is MainPage page ? page : element.FindAscendant<MainPage>() ?? MainPage;
            return mainPage.ShowImageAsync(image);
        }

        public static Task<bool> ShowImageAsync(this MainPage mainPage, ImageModel image)
        {
            return mainPage.DispatcherQueue.EnqueueAsync(() => mainPage.Frame.Navigate(typeof(ShowImagePage), image));
        }
    }

    internal static partial class UIHelper
    {
        public static Task<bool> OpenLinkAsync(this DependencyObject element, string link)
        {
            MainPage mainPage = element is MainPage page ? page : element.FindAscendant<MainPage>() ?? MainPage;
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
                    ShowMessage("暂不支持");
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
