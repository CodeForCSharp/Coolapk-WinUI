using CoolapkUWP.Controls;
using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using CoolapkUWP.Models.Images;
using CoolapkUWP.Pages.BrowserPages;
using CoolapkUWP.Pages.FeedPages;
using CoolapkUWP.Pages.SettingsPages;
using CoolapkUWP.ViewModels.BrowserPages;
using CoolapkUWP.ViewModels.FeedPages;
using CommunityToolkit.WinUI;
using CommunityToolkit.WinUI.Helpers;
using Microsoft.UI.Xaml.Controls;

using System.Text.Json.Nodes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Resources;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;

using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace CoolapkUWP.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        private bool isLoaded;

        private string _userName;
        public string UserName
        {
            get => _userName;
            set
            {
                if (_userName != value)
                {
                    _userName = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private ImageModel _userAvatar;
        public ImageModel UserAvatar
        {
            get => _userAvatar;
            set
            {
                if (_userAvatar != value)
                {
                    _userAvatar = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private NotificationsModel _notificationsModel;
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

        private readonly List<(string Tag, Type Page)> _pages = new List<(string Tag, Type Page)>
        {
            ("Find", typeof(FindPage)),
            ("Home", typeof(IndexPage)),
            ("Digital", typeof(DigitalPage)),
            ("Settings", typeof(SettingsPage)),
            ("Notifications", typeof(NotificationsPage))
        };

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        public MainPage()
        {
            InitializeComponent();
            _ = (NotificationsModel.Instance?.Update());
            NotificationsModel = NotificationsModel.Instance;
            SearchBoxHolder.RegisterPropertyChangedCallback(Slot.IsStretchProperty, new DependencyPropertyChangedCallback(OnIsStretchProperty));
            NavigationView.RegisterPropertyChangedCallback(NavigationView.IsBackButtonVisibleProperty, new DependencyPropertyChangedCallback(OnIsBackButtonVisibleChanged));
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            OnLoginChanged(string.Empty, true);
            SettingsHelper.LoginChanged += OnLoginChanged;
                        AppTitleText.Text = ResourceLoader.GetForViewIndependentUse().GetString("AppName") ?? "酷安";
            if (!isLoaded)
            {
                NavigationView_Navigate("Home", new EntranceNavigationTransitionInfo());
                isLoaded = true;
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            App.MainWindow.SetTitleBar(null);
            SettingsHelper.LoginChanged -= OnLoginChanged;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            App.MainWindow?.SetTitleBar(DragRegion);
        }

        private void TitleBar_LayoutMetricsChanged(Microsoft.UI.Windowing.AppWindowTitleBar sender, object args) => UpdateAppTitle(sender);

        public string GetAppTitleFromSystem => Package.Current.DisplayName;

        private void OnIsStretchProperty(DependencyObject sender, DependencyProperty dp)
        {
            if (sender is Slot)
            {
                UpdateAppTitleIcon();
            }
        }

        private void OnIsBackButtonVisibleChanged(DependencyObject sender, DependencyProperty dp)
        {
            UpdateLeftPaddingColumn();
            UpdateAppTitleIcon();
        }

        private void NavigationView_Navigate(string NavItemTag, NavigationTransitionInfo TransitionInfo, object vs = null)
        {
            Type _page = null;

            (string Tag, Type Page) item = _pages.FirstOrDefault(p => p.Tag.Equals(NavItemTag, StringComparison.Ordinal));
            _page = item.Page;
            // Get the page type before navigation so you can prevent duplicate
            // entries in the back stack.
            Type PreNavPageType = NavigationViewFrame.CurrentSourcePageType;

            // Only navigate if the selected page isn't currently loaded.
            if (_page != null && !Equals(PreNavPageType, _page))
            {
                _ = NavigationViewFrame.Navigate(_page, vs, TransitionInfo);
            }
        }

        private void NavigationView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args) => _ = TryGoBack();

        private void NavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.InvokedItemContainer != null)
            {
                string NavItemTag = args.InvokedItemContainer.Tag.ToString();
                NavigationView_Navigate(NavItemTag, args.RecommendedNavigationTransitionInfo);
            }
        }

        private bool TryGoBack()
        {
            if (!DispatcherQueue.HasThreadAccess)
            { return false; }

            if (!NavigationViewFrame.CanGoBack)
            { return false; }

            // Don't go back if the nav pane is overlayed.
            if (NavigationView.IsPaneOpen &&
                (NavigationView.DisplayMode == NavigationViewDisplayMode.Compact ||
                 NavigationView.DisplayMode == NavigationViewDisplayMode.Minimal))
            { return false; }

            NavigationViewFrame.GoBack();
            return true;
        }

        private void On_Navigated(object _, NavigationEventArgs e)
        {
            NavigationView.IsBackEnabled = NavigationViewFrame.CanGoBack;
            NavigationView.IsBackButtonVisible = NavigationViewFrame.CanGoBack
                ? NavigationViewBackButtonVisible.Visible
                : NavigationViewBackButtonVisible.Collapsed;
            if (NavigationViewFrame.SourcePageType != null)
            {
                (string Tag, Type Page) item = _pages.FirstOrDefault(p => p.Page == e.SourcePageType);
                if (item.Tag != null)
                {
                    NavigationViewItem SelectedItem = NavigationView.MenuItems
                        .OfType<NavigationViewItem>()
                        .FirstOrDefault(n => n.Tag.Equals(item.Tag))
                            ?? NavigationView.FooterMenuItems
                                .OfType<NavigationViewItem>()
                                .FirstOrDefault(n => n.Tag.Equals(item.Tag));
                    NavigationView.SelectedItem = SelectedItem;
                }
            }
            ProgressBarHelper.HideProgressBar();
        }

        private void NavigationViewControl_PaneClosing(NavigationView sender, NavigationViewPaneClosingEventArgs args)
        {
            UpdateLeftPaddingColumn();
        }

        private void NavigationViewControl_PaneOpening(NavigationView sender, object args)
        {
            UpdateLeftPaddingColumn();
        }

        private void UpdateLeftPaddingColumn()
        {
            LeftPaddingColumn.Width = NavigationView.PaneDisplayMode == NavigationViewPaneDisplayMode.Top
                ? NavigationView.IsBackButtonVisible != NavigationViewBackButtonVisible.Collapsed
                    ? new GridLength(48) : new GridLength(0)
                    : NavigationView.DisplayMode == NavigationViewDisplayMode.Minimal
                        ? NavigationView.IsPaneOpen ? new GridLength(72)
                        : NavigationView.IsPaneToggleButtonVisible
                            ? NavigationView.IsBackButtonVisible != NavigationViewBackButtonVisible.Collapsed
                            ? new GridLength(88) : new GridLength(48)
                                : NavigationView.IsBackButtonVisible != NavigationViewBackButtonVisible.Collapsed
                                ? new GridLength(48) : new GridLength(0)
                                    : NavigationView.IsBackButtonVisible != NavigationViewBackButtonVisible.Collapsed
                                    ? new GridLength(48) : new GridLength(0);
        }

        private void NavigationViewControl_DisplayModeChanged(NavigationView sender, NavigationViewDisplayModeChangedEventArgs args)
        {
            UpdateLeftPaddingColumn();
            UpdateAppTitleIcon();
        }

        private void UpdateAppTitleIcon()
        {
            AppTitleIcon.Margin = SearchBoxHolder.IsStretch
                && NavigationView.PaneDisplayMode != NavigationViewPaneDisplayMode.Top
                && NavigationView.DisplayMode != NavigationViewDisplayMode.Minimal
                    ? NavigationView.IsBackButtonVisible == NavigationViewBackButtonVisible.Visible
                        ? new Thickness(0, 0, 16, 0)
                        : new Thickness(28.5, 0, 28, 0)
                    : NavigationView.IsBackButtonVisible == NavigationViewBackButtonVisible.Visible
                        || (NavigationView.DisplayMode == NavigationViewDisplayMode.Minimal
                            && NavigationView.PaneDisplayMode != NavigationViewPaneDisplayMode.Top
                            && NavigationView.IsPaneToggleButtonVisible)
                        ? new Thickness(0, 0, 16, 0)
                        : new Thickness(16, 0, 16, 0);
        }

        private void OnLoginChanged(string sender, bool args) => _ = DispatcherQueue.EnqueueAsync(() => SetUserAvatar(args));

        private async void SetUserAvatar(bool isLogin)
        {
            if (isLogin && await SettingsHelper.CheckLoginAsync())
            {
                string UID = SettingsHelper.Get<string>(SettingsHelper.Uid);
                if (!string.IsNullOrEmpty(UID))
                {
                    (string UID, string UserName, string UserAvatar) results = await CoolapkUWP.Helpers.NetworkHelper.GetUserInfoByNameAsync(UID);
                    if (results.UID != UID) { return; }
                    UserName = results.UserName;
                    UserAvatar = new ImageModel(results.UserAvatar, ImageType.Avatar);
                }
            }
            else
            {
                UserName = null;
                UserAvatar = null;
            }
        }

        private void UpdateAppTitle(Microsoft.UI.Windowing.AppWindowTitleBar coreTitleBar)
        {
            //ensure the custom title bar does not overlap window caption controls
            RightPaddingColumn.Width = new GridLength(coreTitleBar.RightInset);
        }



        private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as FrameworkElement).Tag.ToString())
            {
                case "Login":
                    NavigationViewFrame.Navigate(typeof(BrowserPage), new BrowserViewModel(UriHelper.LoginUri));
                    break;
                case "Logout":
                    SettingsHelper.Logout();
                    break;
                case "Settings":
                    NavigationView.SelectedItem = NavigationView.FooterMenuItems.LastOrDefault();
                    break;
                case "CreateFeed":
                    CreateFeedControl.ShowCreateFeed(this);
                    break;
            }
        }

        #region 搜索框

        private static readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1);

        private async void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                ObservableCollection<Entity> observableCollection = new ObservableCollection<Entity>();
                sender.ItemsSource = observableCollection;
                string keyWord = sender.Text;
                await semaphoreSlim.WaitAsync();
                try
                {
                    (bool isSucceed, JsonNode result) = await Task.Run(() => RequestHelper.GetDataAsync(UriHelper.GetUri(UriType.SearchWords, keyWord), true));
                    if (isSucceed && result is JsonArray array && array.Count > 0)
                    {
                        List<Entity> entities = await Task.Run(() =>
                        {
                            List<Entity> list = new List<Entity>();
                            foreach (JsonNode token in array)
                            {
                                switch ((string)token["entityType"])
                                {
                                    case "apk":
                                        list.Add(AppModel.FromJson(token as JsonObject));
                                        break;
                                    case "searchWord":
                                    default:
                                        list.Add(SearchWord.FromJson(token as JsonObject));
                                        break;
                                }
                            }
                            return list;
                        });
                        foreach (Entity entity in entities)
                        {
                            observableCollection.Add(entity);
                        }
                    }
                }
                finally
                {
                    semaphoreSlim.Release();
                }
            }
        }

        private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion is AppModel app)
            {
                _ = NavigationViewFrame.Navigate(typeof(BrowserPage), new BrowserViewModel($"https://www.coolapk.com{app.Url}"));
            }
            else if (args.ChosenSuggestion is SearchWord word)
            {
                _ = NavigationViewFrame.Navigate(typeof(SearchingPage), new SearchingViewModel(word.ToString(), word.Glyph == "\uE77B" ? 1 : -1));
            }
            else if (args.ChosenSuggestion is null && !string.IsNullOrEmpty(sender.Text))
            {
                _ = NavigationViewFrame.Navigate(typeof(SearchingPage), new SearchingViewModel(sender.Text));
            }
        }

        #endregion

        #region 状态栏

        public void ShowProgressBar()
        {
            _ = DispatcherQueue.EnqueueAsync(() =>
            {
                ProgressBar.Visibility = Visibility.Visible;
                ProgressBar.IsIndeterminate = true;
                ProgressBar.ShowError = false;
                ProgressBar.ShowPaused = false;
            });
        }

        public void ShowProgressBar(double value)
        {
            _ = DispatcherQueue.EnqueueAsync(() =>
            {
                ProgressBar.Visibility = Visibility.Visible;
                ProgressBar.IsIndeterminate = false;
                ProgressBar.ShowError = false;
                ProgressBar.ShowPaused = false;
                ProgressBar.Value = value;
            });
        }

        public void PausedProgressBar()
        {
            _ = DispatcherQueue.EnqueueAsync(() =>
            {
                ProgressBar.Visibility = Visibility.Visible;
                ProgressBar.IsIndeterminate = true;
                ProgressBar.ShowError = false;
                ProgressBar.ShowPaused = true;
            });
        }

        public void ErrorProgressBar()
        {
            _ = DispatcherQueue.EnqueueAsync(() =>
            {
                ProgressBar.Visibility = Visibility.Visible;
                ProgressBar.IsIndeterminate = true;
                ProgressBar.ShowPaused = false;
                ProgressBar.ShowError = true;
            });
        }

        public void HideProgressBar()
        {
            _ = DispatcherQueue.EnqueueAsync(() =>
            {
                ProgressBar.Visibility = Visibility.Collapsed;
                ProgressBar.IsIndeterminate = false;
                ProgressBar.ShowError = false;
                ProgressBar.ShowPaused = false;
                ProgressBar.Value = 0;
            });
        }

        public void ShowMessage(string message = null)
        {
            _ = DispatcherQueue.EnqueueAsync(() =>
            {
                AppTitleText.Text = message ?? ResourceLoader.GetForViewIndependentUse().GetString("AppName") ?? "酷安";

                App.MainWindow.Title = message ?? string.Empty;
            });
        }

        #endregion
    }
}
