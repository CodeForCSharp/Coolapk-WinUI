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

using Microsoft.UI.Input;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
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

        private const double DetailMinWidth = 420;
        private const double PrimaryMinWidth = 360;
        private const double SplitterWidth = 8;
        /// <summary>侧栏占工作区的比例。窗口缩放时按这个比例重算，不把多出来的宽度全部给左边。</summary>
        private const double DetailRatio = 0.6;
        private bool isDraggingSplitter;
        private double detailRatio = DetailRatio;
        private PointerEventHandler splitterMoved;
        private PointerEventHandler splitterReleased;

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
            PaneNavigation.Attach(DetailFrame);
            PaneNavigation.StateChanged += OnPaneStateChanged;
            splitterMoved = Splitter_PointerMoved;
            splitterReleased = Splitter_PointerReleased;
            Splitter.PointerPressed += Splitter_PointerPressed;
            Workspace.SizeChanged += Workspace_SizeChanged;
        }

        /// <summary>按目标选择主栏或侧栏。侧栏打开时主栏保持不动。</summary>
        public Frame ResolveFrame(NavigationTarget target, Type pageType)
        {
            if (PaneNavigation.ShouldOpenInDetail(target, pageType))
            {
                OpenDetailPane();
                return DetailFrame;
            }
            return NavigationViewFrame;
        }

        private void OpenDetailPane()
        {
            if (PaneNavigation.IsDetailOpen) { return; }
            PaneNavigation.SetOpen(true);
        }

        private void OnPaneStateChanged()
        {
            if (!DispatcherQueue.HasThreadAccess)
            {
                _ = DispatcherQueue.EnqueueAsync(ApplyPaneState);
                return;
            }
            ApplyPaneState();
        }

        private void ApplyPaneState()
        {
            bool open = PaneNavigation.IsDetailOpen && Workspace.ActualWidth >= PaneNavigation.DetailThreshold;
            Splitter.Visibility = open ? Visibility.Visible : Visibility.Collapsed;
            DetailPane.Visibility = open ? Visibility.Visible : Visibility.Collapsed;
            SplitterColumn.Width = open ? new GridLength(SplitterWidth) : new GridLength(0);
            DetailColumn.MinWidth = open ? DetailMinWidth : 0;
            DetailColumn.Width = open ? new GridLength(WidthForRatio(detailRatio)) : new GridLength(0);
            UpdateBackButton();
        }

        /// <summary>按比例算侧栏宽度，同时给主栏留出最小宽度。</summary>
        private double WidthForRatio(double ratio)
        {
            double available = Math.Max(0, Workspace.ActualWidth - SplitterWidth);
            double max = Math.Max(DetailMinWidth, available - PrimaryMinWidth);
            return Math.Clamp(available * ratio, Math.Min(DetailMinWidth, available), max);
        }

        private void Workspace_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (PaneNavigation.IsDetailOpen && !isDraggingSplitter)
            {
                ApplyPaneState();
            }
        }

        private void Splitter_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            isDraggingSplitter = true;
            Splitter.CapturePointer(e.Pointer);
            ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.SizeWestEast);
            Workspace.PointerMoved -= splitterMoved;
            Workspace.PointerReleased -= splitterReleased;
            Workspace.PointerCanceled -= splitterReleased;
            Workspace.PointerMoved += splitterMoved;
            Workspace.PointerReleased += splitterReleased;
            Workspace.PointerCanceled += splitterReleased;
        }

        private void Splitter_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (!isDraggingSplitter) { return; }
            double available = Workspace.ActualWidth - SplitterWidth;
            double x = e.GetCurrentPoint(Workspace).Position.X;
            double width = Math.Clamp(available - x, DetailMinWidth, Math.Max(DetailMinWidth, available - PrimaryMinWidth));
            if (available > 0) { detailRatio = width / available; }
            DetailColumn.Width = new GridLength(width);
        }

        private void Splitter_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (!isDraggingSplitter) { return; }
            isDraggingSplitter = false;
            Splitter.ReleasePointerCapture(e.Pointer);
            ProtectedCursor = null;
            Workspace.PointerMoved -= splitterMoved;
            Workspace.PointerReleased -= splitterReleased;
            Workspace.PointerCanceled -= splitterReleased;
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
            AppTitleBar.SizeChanged -= AppTitleBar_SizeChanged;
            App.MainWindow.SetTitleBar(null);
            SettingsHelper.LoginChanged -= OnLoginChanged;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // 窗口够宽时启动即双栏，右边先放占位页。
            if (!PaneNavigation.IsDetailOpen && Workspace.ActualWidth >= PaneNavigation.DetailThreshold)
            {
                PaneNavigation.ShowPlaceholder();
                PaneNavigation.SetOpen(true);
            }
            if (App.MainWindow is not Window window) { return; }
            window.SetTitleBar(DragRegion);

            AppTitleBar.SizeChanged -= AppTitleBar_SizeChanged;
            AppTitleBar.SizeChanged += AppTitleBar_SizeChanged;
            UpdateAppTitle(window.AppWindow.TitleBar);
        }

        private void AppTitleBar_SizeChanged(object sender, SizeChangedEventArgs e) => UpdateAppTitle(App.MainWindow?.AppWindow.TitleBar);

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

            // 侧栏有自己的返回栈，先退出当前这篇，再退主栏。
            if (PaneNavigation.CanGoBack())
            {
                PaneNavigation.GoBack();
                return true;
            }

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

        private void UpdateBackButton()
        {
            bool detailHasContent = PaneNavigation.IsDetailOpen && DetailFrame.Content is not null and not DetailPlaceholderPage;
            bool canGoBack = NavigationViewFrame.CanGoBack || detailHasContent;
            NavigationView.IsBackEnabled = canGoBack;
            NavigationView.IsBackButtonVisible = canGoBack
                ? NavigationViewBackButtonVisible.Visible
                : NavigationViewBackButtonVisible.Collapsed;
            bool detailCanGoBack = PaneNavigation.CanGoBack();
            DetailBackButton.IsEnabled = detailCanGoBack;
            DetailBackButton.Visibility = detailCanGoBack ? Visibility.Visible : Visibility.Collapsed;
            // 占位页没有标题栏，不能因此把浮动的返回和关闭显出来。
            DetailCommands.Visibility = detailHasContent && !PaneNavigation.HasPageHeader(DetailFrame.Content as DependencyObject)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void On_DetailNavigated(object _, NavigationEventArgs e)
        {
            if (e.Content == null && !DetailFrame.CanGoBack)
            {
                PaneNavigation.SetOpen(false);
            }
            else if (e.Content is Page page)
            {
                page.Loaded += OnDetailPageLoaded;
            }
            UpdateBackButton();
        }

        private void OnDetailPageLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is Page page) { page.Loaded -= OnDetailPageLoaded; }
            UpdateBackButton();
        }

        private void DetailBackButton_Click(object sender, RoutedEventArgs e)
        {
            PaneNavigation.GoBack();
        }

        private void DetailCloseButton_Click(object sender, RoutedEventArgs e) => PaneNavigation.Close();

        private void On_Navigated(object _, NavigationEventArgs e)
        {
            // 切到首页、发现这类栏目时，侧栏里的上一条动态不再对应新列表，回到占位页。
            if (PaneNavigation.IsSection(e.SourcePageType) && PaneNavigation.IsDetailOpen)
            {
                PaneNavigation.ShowPlaceholder();
            }
            UpdateBackButton();
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

        private void UpdateAppTitle(Microsoft.UI.Windowing.AppWindowTitleBar titleBar)
        {
            if (titleBar is null) { return; }

            // 给系统的最小化/最大化/关闭按钮预留右上角空间。
            // RightInset 是物理像素，需除以缩放比例换算成 XAML 的逻辑像素。
            double scale = AppTitleBar.XamlRoot?.RasterizationScale ?? 1;
            if (scale <= 0) { scale = 1; }
            RightPaddingColumn.Width = new GridLength(titleBar.RightInset / scale);
        }



        private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as FrameworkElement).Tag.ToString())
            {
                case "Login":
                    _ = this.NavigateAsync(typeof(BrowserPage), new BrowserViewModel(UriHelper.LoginUri));
                    break;
                case "Logout":
                    SettingsHelper.Logout();
                    break;
                case "Settings":
                    NavigationView_Navigate("Settings", new EntranceNavigationTransitionInfo());
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
                _ = this.NavigateAsync(typeof(BrowserPage), new BrowserViewModel($"https://www.coolapk.com{app.Url}"));
            }
            else if (args.ChosenSuggestion is SearchWord word)
            {
                _ = this.NavigateAsync(typeof(SearchingPage), new SearchingViewModel(word.ToString(), word.Glyph == "\uE77B" ? 1 : -1));
            }
            else if (args.ChosenSuggestion is null && !string.IsNullOrEmpty(sender.Text))
            {
                _ = this.NavigateAsync(typeof(SearchingPage), new SearchingViewModel(sender.Text));
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
