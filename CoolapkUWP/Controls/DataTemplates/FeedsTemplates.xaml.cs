using CoolapkUWP.Services;
using CoolapkUWP.Data;
using CoolapkUWP.Data.Dtos;
using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using CoolapkUWP.Models.Feeds;
using CoolapkUWP.Pages.BrowserPages;
using CoolapkUWP.Pages.FeedPages;
using CoolapkUWP.ViewModels.BrowserPages;
using CoolapkUWP.ViewModels.FeedPages;
using CommunityToolkit.WinUI;
using System.Text.Json;
using System.Text.Json.Nodes;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Resources;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了"用户控件"项模板

namespace CoolapkUWP.Controls.DataTemplates
{
    public partial class FeedsTemplates : ResourceDictionary
    {
        public FeedsTemplates() => InitializeComponent();

        public static CornerRadius BottomOverlayCornerRadius
        {
            get
            {
                var r = (CornerRadius)Application.Current.Resources["OverlayCornerRadius"];
                return new CornerRadius(0, 0, r.BottomRight, r.BottomLeft);
            }
        }

        private void OnRootTapped(object sender, TappedRoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;

            if ((element.DataContext as ICanCopy)?.IsCopyEnabled ?? false) { return; }

            if (e != null && !UIHelper.IsOriginSource(sender, e.OriginalSource)) { return; }

            if (e != null) { e.Handled = true; }

            _ = element.OpenLinkAsync(element.Tag as string);
        }

        private void OnTopTapped(object sender, TappedRoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;

            if (e != null) { e.Handled = true; }

            _ = element.OpenLinkAsync(element.Tag.ToString());
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            switch (element.Name)
            {
                case "SeeAllButton":
                    _ = element.NavigateAsync(typeof(AdaptivePage), AdaptiveViewModel.GetReplyListProvider(((FeedReplyModel)element.Tag).ID.ToString(), (FeedReplyModel)element.Tag));
                    break;
                default:
                    _ = element.OpenLinkAsync((sender as FrameworkElement).Tag.ToString());
                    break;
            }
        }

        private void FeedButton_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;

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
                    _ = element.NavigateAsync(typeof(BrowserPage), new BrowserViewModel(element.Tag.ToString()));
                    break;

                case "ReplyButton":
                    DisabledCopy();
                    if (element.Tag is FeedModelBase feed)
                    {
                        new CreateFeedControl
                        {
                            ReplyID = feed.ID,
                            FeedType = CreateFeedType.Reply,
                            PopupTransitions = new TransitionCollection
                            {
                                new EdgeUIThemeTransition
                                {
                                    Edge = EdgeTransitionLocation.Bottom
                                }
                            }
                        }.Show(element);
                    }
                    else if (element.Tag is FeedReplyModel reply)
                    {
                        new CreateFeedControl
                        {
                            ReplyID = reply.ID,
                            FeedType = CreateFeedType.ReplyReply,
                            PopupTransitions = new TransitionCollection
                            {
                                new EdgeUIThemeTransition
                                {
                                    Edge = EdgeTransitionLocation.Bottom
                                }
                            }
                        }.Show(element);
                    }
                    DisabledCopy();
                    break;

                case "ShareButton":
                    DisabledCopy();
                    break;

                case "ChangeButton":
                    DisabledCopy();
                    //UIHelper.NavigateInSplitPane(typeof(AdaptivePage), new ViewModels.AdaptivePage.ViewModel((sender as FrameworkElement).Tag as string, ViewModels.AdaptivePage.ListType.FeedInfo, "changeHistory"));
                    break;

                default:
                    DisabledCopy();
                    _ = element.OpenLinkAsync((sender as FrameworkElement).Tag as string);
                    break;
            }
        }

        private async void DeviceHyperlink_Click(Hyperlink sender, HyperlinkClickEventArgs args)
        {
            UIHelper.ShowProgressBar();
            string device = (sender.Inlines.FirstOrDefault().ElementStart.VisualParent.DataContext as FeedModelBase).DeviceTitle;
            (bool isSucceed, JsonNode result) = await RequestHelper.GetDataAsync(UriHelper.GetUri(UriType.GetProductDetailByName, device), true);
            UIHelper.HideProgressBar();
            if (!isSucceed) { return; }

            ProductDetailDto dto = JsonSerializer.Deserialize<ProductDetailDto>(result, DtoJson.Options);

            if (!string.IsNullOrEmpty(dto.Id))
            {
                FeedListViewModel provider = FeedListViewModel.GetProvider(FeedListType.ProductPageList, dto.Id);

                if (provider != null)
                {
                    _ = sender.NavigateAsync(typeof(FeedListPage), provider);
                }
            }
        }

        private void CopyMenuItem_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            DataPackage dp = new DataPackage();
            dp.SetText(element.Tag.ToString());
            Clipboard.SetContent(dp);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            UserControl_SizeChanged(sender, null);
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UserControl UserControl = sender as UserControl;
            FrameworkElement StackPanel = UserControl.FindChild("BtnsPanel");
            double width = e is null ? UserControl.Width : e.NewSize.Width;
            StackPanel?.SetValue(Grid.RowProperty, width > 600 ? 1 : 20);
        }

        private void GridView_SelectionChanged(object sender, SelectionChangedEventArgs e) => (sender as GridView).SelectedIndex = -1;
    }
}
