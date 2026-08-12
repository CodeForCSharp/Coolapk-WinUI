using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using CoolapkUWP.Models.Feeds;
using CoolapkUWP.Models.Pages;
using CoolapkUWP.ViewModels.DataSource;
using CoolapkUWP.ViewModels.FeedPages;
using CoolapkUWP.ViewModels.Providers;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了"空白页"项模板

namespace CoolapkUWP.Pages.FeedPages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class NotificationsPage : PivotPageBase
    {
        private NotificationsModel _notificationsModel = NotificationsModel.Instance;
        public NotificationsModel NotificationsModel
        {
            get => _notificationsModel;
            set
            {
                if (_notificationsModel != value)
                {
                    _notificationsModel = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        public NotificationsPage() => InitializeComponent();

        protected override Pivot PivotControl => Pivot;

        protected override ObservableCollection<PivotItem> GetMainItems() => null;

        protected override void OnPivotLoaded() => _ = NotificationsModel?.Update();

        protected override void NavigateToPage(PivotItem item, Frame frame)
        {
            switch (item.Tag.ToString())
            {
                case "CommentMe":
                    frame.Navigate(typeof(AdaptivePage), new AdaptiveViewModel(
                        new CoolapkListProvider(
                            (p, firstItem, lastItem) =>
                                UriHelper.GetUri(
                                    UriType.GetNotifications,
                                    "list",
                                    p,
                                    UriHelper.GetOptionalArg("firstItem", firstItem),
                                    UriHelper.GetOptionalArg("lastItem", lastItem)),
                            o => new[] { NotificationModelFactory.CreateSimple(o) },
                            "id")));
                    break;
                case "AtMe":
                    frame.Navigate(typeof(AdaptivePage), new AdaptiveViewModel(
                        new CoolapkListProvider(
                            (p, firstItem, lastItem) =>
                                UriHelper.GetUri(
                                    UriType.GetNotifications,
                                    "atMeList",
                                    p,
                                    UriHelper.GetOptionalArg("firstItem", firstItem),
                                    UriHelper.GetOptionalArg("lastItem", lastItem)),
                            o => new[] { FeedModel.FromJson(o) },
                            "id")));
                    break;
                case "AtCommentMe":
                    frame.Navigate(typeof(AdaptivePage), new AdaptiveViewModel(
                        new CoolapkListProvider(
                            (p, firstItem, lastItem) =>
                                UriHelper.GetUri(
                                    UriType.GetNotifications,
                                    "atCommentMeList",
                                    p,
                                    UriHelper.GetOptionalArg("firstItem", firstItem),
                                    UriHelper.GetOptionalArg("lastItem", lastItem)),
                            o => new[] { NotificationModelFactory.CreateAtCommentMe(o) },
                            "id")));
                    break;
                case "FeedLike":
                    frame.Navigate(typeof(AdaptivePage), new AdaptiveViewModel(
                        new CoolapkListProvider(
                            (p, firstItem, lastItem) =>
                                UriHelper.GetUri(
                                    UriType.GetNotifications,
                                    "feedLikeList",
                                    p,
                                    UriHelper.GetOptionalArg("firstItem", firstItem),
                                    UriHelper.GetOptionalArg("lastItem", lastItem)),
                            o => new[] { NotificationModelFactory.CreateLike(o) },
                            "id")));
                    break;
                case "Follow":
                    frame.Navigate(typeof(AdaptivePage), new AdaptiveViewModel(
                        new CoolapkListProvider(
                            (p, firstItem, lastItem) =>
                                UriHelper.GetUri(
                                    UriType.GetNotifications,
                                    "contactsFollowList",
                                    p,
                                    UriHelper.GetOptionalArg("firstItem", firstItem),
                                    UriHelper.GetOptionalArg("lastItem", lastItem)),
                            o => new[] { NotificationModelFactory.CreateSimple(o) },
                            "id")));
                    break;
                case "Message":
                    frame.Navigate(typeof(AdaptivePage), new AdaptiveViewModel(
                        new CoolapkListProvider(
                            (p, firstItem, lastItem) =>
                                UriHelper.GetUri(
                                    UriType.GetChats,
                                    p,
                                    UriHelper.GetOptionalArg("firstItem", firstItem),
                                    UriHelper.GetOptionalArg("lastItem", lastItem)),
                            o => new[] { NotificationModelFactory.CreateMessage(o) },
                            "id")));
                    break;
                default:
                    break;
            }
        }

        private async Task Refresh(bool reset = false)
        {
            await NotificationsModel?.Update();
            if (refresh != null)
            {
                await refresh(reset);
            }
        }

        protected override void RefreshButton_Click(object sender, RoutedEventArgs e) => _ = Refresh(true);
    }
}
