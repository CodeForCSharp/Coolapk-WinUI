using CoolapkUWP.Services;
using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using CoolapkUWP.Models.Images;
using CoolapkUWP.Pages.BrowserPages;
using CoolapkUWP.ViewModels.BrowserPages;
using CoolapkUWP.ViewModels.DataSource;
using CoolapkUWP.ViewModels.FeedPages;
using CommunityToolkit.WinUI;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using TwoPaneView = Microsoft.UI.Xaml.Controls.TwoPaneView;
using TwoPaneViewMode = Microsoft.UI.Xaml.Controls.TwoPaneViewMode;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace CoolapkUWP.Pages.FeedPages
{
    public enum FeedListType
    {
        TagPageList,
        DyhPageList,
        UserPageList,
        ProductPageList,
        CollectionPageList,
    }

    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class FeedListPage : Page, INotifyPropertyChanged
    {
        private FeedListViewModel _provider;
        public FeedListViewModel Provider
        {
            get => _provider;
            private set
            {
                if (_provider != value)
                {
                    _provider = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        public static CornerRadius LeftControlCornerRadius
        {
            get
            {
                var r = (CornerRadius)Application.Current.Resources["ControlCornerRadius"];
                return new CornerRadius(r.TopLeft, 0, 0, r.BottomLeft);
            }
        }

        public static CornerRadius RightControlCornerRadius
        {
            get
            {
                var r = (CornerRadius)Application.Current.Resources["ControlCornerRadius"];
                return new CornerRadius(0, r.TopRight, r.BottomRight, 0);
            }
        }

        public DataTemplate DetailDataTemplate { get; private set; }

        public FeedListPage() => InitializeComponent();

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is FeedListViewModel ViewModel
                && Provider?.IsEqual(ViewModel) != true)
            {
                Provider = ViewModel;
                await Refresh(true);
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }

        private async Task Refresh(bool reset = false)
        {
            await Provider.Refresh(reset);
            if (ListView.ItemsSource is EntityItemSource entities)
            {
                _ = entities.Refresh(true);
            }
        }

        private void FlipView_SizeChanged(object sender, SizeChangedEventArgs e) => (sender as FrameworkElement).MaxHeight = e.NewSize.Width / 2;

        private void FlipView_Loaded(object sender, RoutedEventArgs e)
        {
            if (SettingsHelper.Get<bool>(SettingsHelper.IsNoPicsMode))
            {
                if ((sender as FrameworkElement).Parent is FrameworkElement parent)
                { parent.Visibility = Visibility.Collapsed; }
            }
            else
            {
                FlipView view = sender as FlipView;
                view.MaxHeight = view.ActualWidth / 3;
                DispatcherTimer timer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(20)
                };
                timer.Tick += (o, a) =>
                {
                    if (view.SelectedIndex != -1)
                    {
                        if (view.SelectedIndex + 1 >= view.Items.Count)
                        {
                            while (view.SelectedIndex > 0)
                            {
                                view.SelectedIndex -= 1;
                            }
                        }
                        else
                        {
                            view.SelectedIndex += 1;
                        }
                    }
                };
                view.Unloaded += (_, __) => timer.Stop();
                timer.Start();
            }
        }

        private void Button_Loaded(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            ContentPresenter content = element.FindDescendant<ContentPresenter>();
            if (content != null)
            {
                switch (element.Name)
                {
                    case "OnlyOwnerButton":
                        content.CornerRadius = new CornerRadius(4, 0, 0, 4);
                        break;
                    case "SeeAllButton":
                        content.CornerRadius = new CornerRadius(0, 4, 4, 0);
                        break;
                    default:
                        break;
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            switch (element.Name)
            {
                case "FansButton":
                    _ = this.NavigateAsync(typeof(AdaptivePage), AdaptiveViewModel.GetUserListProvider(Provider.ID, false, Provider.Title));
                    break;
                case "LikeButton":
                    _ = FeedActionsService.ChangeLikeAsync(element.Tag as ICanLike);
                    break;
                case "ReportButton":
                    _ = this.NavigateAsync(typeof(BrowserPage), new BrowserViewModel(element.Tag.ToString()));
                    break;
                case "FollowButton":
                    _ = FeedActionsService.ChangeFollowAsync(element.Tag as ICanFollow);
                    break;
                case "FollowsButton":
                    _ = this.NavigateAsync(typeof(AdaptivePage), AdaptiveViewModel.GetUserListProvider(Provider.ID, true, Provider.Title));
                    break;
                default:
                    break;
            }
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            ImageModel image = (sender as FrameworkElement).Tag as ImageModel;
            switch ((sender as FrameworkElement).Name)
            {
                case "CopyButton":
                    _ = ImageActions.CopyPicAsync(image);
                    break;
                case "SaveButton":
                    _ = ImageActions.SavePicAsync(image);
                    break;
                case "ShareButton":
                    _ = ImageActions.SharePicAsync(image);
                    break;
                case "RefreshButton":
                    _ = image.Refresh();
                    break;
                case "ShowImageButton":
                    _ = this.ShowImageAsync(image);
                    break;
                case "OriginButton":
                    image.Type = ImageType.OriginImage;
                    break;
            }
        }

        private async void Image_DragStarting(UIElement sender, DragStartingEventArgs args)
        {
            args.DragUI.SetContentFromDataPackage();
            args.Data.RequestedOperation = DataPackageOperation.Copy;
            await ImageActions.GetImageDataPackageAsync(args.Data, (sender as FrameworkElement).Tag as ImageModel, "拖拽图片");
        }

        private void On_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            if (element.Tag is ImageModel image)
            {
                _ = element.ShowImageAsync(image);
            }
            else if (element.Tag is string url)
            {
                _ = this.OpenLinkAsync(url);
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e) => _ = Refresh(true);

        private async void ListView_RefreshRequested(object sender, EventArgs e) => await Refresh(true);

        #region 界面模式切换

        private void TwoPaneView_ModeChanged(TwoPaneView sender, object args)
        {
            // Remove details content from it's parent panel.
            if (HeaderControl.Parent != null)
            {
                (HeaderControl.Parent as Panel).Children.Remove(HeaderControl);
            }
            else
            {
                LeftGrid.Children.Remove(HeaderControl);
                RightGrid.Children.Remove(HeaderControl);
            }

            if (DetailControl.Parent != null)
            {
                (DetailControl.Parent as Panel).Children.Remove(DetailControl);
            }
            else
            {
                Pane1Grid.Children.Remove(DetailControl);
                Pane2Grid.Children.Remove(DetailControl);
            }

            // Single pane
            if (sender.Mode == TwoPaneViewMode.SinglePane)
            {
                ListRefreshButton.Visibility = Visibility.Collapsed;
                HeaderRefreshButton.Visibility = Visibility.Visible;
                // Add the details content to Pane1.
                RightGrid.Children.Add(HeaderControl);
                Pane2Grid.Children.Add(DetailControl);
            }
            // Dual pane.
            else
            {
                ListRefreshButton.Visibility = Visibility.Visible;
                HeaderRefreshButton.Visibility = Visibility.Collapsed;
                // Put details content in Pane2.
                LeftGrid.Children.Add(HeaderControl);
                Pane1Grid.Children.Add(DetailControl);
            }
        }

        private void TwoPaneView_Loaded(object sender, RoutedEventArgs e)
        {
            TwoPaneView_ModeChanged(sender as TwoPaneView, null);
        }

        #endregion 界面模式切换

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }
    }
}
