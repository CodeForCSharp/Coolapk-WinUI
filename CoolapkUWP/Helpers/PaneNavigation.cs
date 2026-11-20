using CoolapkUWP.Controls;
using CoolapkUWP.Pages;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CoolapkUWP.Helpers
{
    /// <summary>
    /// 导航落在哪一栏。Auto 由窗口宽度决定：够宽时文档页进侧栏，其余进主栏。
    /// </summary>
    public enum NavigationTarget
    {
        Auto,
        Primary,
        Detail,
    }

    /// <summary>
    /// 侧栏导航的共享状态。页面自己不知道被放在哪一栏，标题栏通过这里拿到返回和关闭。
    /// </summary>
    internal static class PaneNavigation
    {
        /// <summary>窗口宽于此值时，文档页从主栏换到侧栏。</summary>
        public const double DetailThreshold = 1000;

        private static readonly HashSet<Type> SectionPages = new HashSet<Type>
        {
            typeof(Pages.FeedPages.IndexPage),
            typeof(Pages.FeedPages.DigitalPage),
            typeof(Pages.FeedPages.FindPage),
            typeof(Pages.FeedPages.NotificationsPage),
            typeof(Pages.SettingsPages.SettingsPage),
            typeof(Pages.SettingsPages.TestPage),
            typeof(Pages.DetailPlaceholderPage),
        };

        public static Frame DetailFrame { get; private set; }

        public static bool IsDetailOpen { get; private set; }

        public static event Action StateChanged;

        public static void Attach(Frame detailFrame)
        {
            if (DetailFrame == detailFrame) { return; }
            if (DetailFrame != null) { DetailFrame.Navigated -= OnNavigated; }
            DetailFrame = detailFrame;
            if (DetailFrame != null) { DetailFrame.Navigated += OnNavigated; }
        }

        public static bool IsSection(Type pageType) => pageType != null && SectionPages.Contains(pageType);

        /// <summary>
        /// 侧栏当前页还能不能退。占位页本身没有上一页；从搜索退回占位页是正常返回。
        /// </summary>
        public static bool CanGoBack()
        {
            if (!IsDetailOpen || DetailFrame == null || !DetailFrame.CanGoBack) { return false; }
            return DetailFrame.Content is not Pages.DetailPlaceholderPage;
        }

        /// <summary>宽屏下的文档页走侧栏，栏目页和窄窗口仍走主栏。</summary>
        public static bool ShouldOpenInDetail(NavigationTarget target, Type pageType)
        {
            if (target == NavigationTarget.Primary || IsSection(pageType)) { return false; }
            if (target == NavigationTarget.Detail) { return true; }
            return (App.MainWindow?.Bounds.Width ?? 0) >= DetailThreshold;
        }

        public static void SetOpen(bool open)
        {
            if (IsDetailOpen == open) { return; }
            IsDetailOpen = open;
            StateChanged?.Invoke();
        }

        /// <summary>侧栏空着时放回占位页，保证启动和关闭后右边都有内容。</summary>
        public static void ShowPlaceholder()
        {
            if (DetailFrame == null) { return; }
            if (DetailFrame.Content is Pages.DetailPlaceholderPage) { return; }
            if (DetailFrame.BackStackDepth > 0) { DetailFrame.BackStack.Clear(); }
            DetailFrame.Navigate(typeof(Pages.DetailPlaceholderPage));
            DetailFrame.BackStack.Clear();
        }

        /// <summary>
        /// 这次导航是不是从侧栏里发起的。列表项模板里的按钮不在可视树中，
        /// 所以除了向上找 Frame，再看当前焦点是否落在侧栏里。
        /// </summary>
        public static bool Hosts(DependencyObject element)
        {
            if (!IsDetailOpen || DetailFrame == null) { return false; }
            if (Contains(element)) { return true; }

            XamlRoot root = DetailFrame.XamlRoot;
            return root != null && FocusManager.GetFocusedElement(root) is DependencyObject focused && Contains(focused);
        }

        private static bool Contains(DependencyObject element) =>
            element == DetailFrame || (element != null && element.FindAscendant<Frame>() == DetailFrame);

        /// <summary>页面自己有标题栏时，侧栏的浮动按钮让位给标题栏里的按钮。</summary>
        public static bool HasPageHeader(DependencyObject root) => root != null && FindHeaders(root).Any();

        /// <summary>给侧栏页面的标题栏补上返回和关闭。主栏页面不动。</summary>
        public static void AttachCommands(Page page)
        {
            if (page == null || !Hosts(page)) { return; }
            foreach (PageHeader header in FindHeaders(page))
            {
                if (header.LeftContent is not Panel commands)
                {
                    commands = CreateCommands();
                    header.LeftContent = commands;
                }
                UpdateBackButton(commands);
            }
        }

        private static void OnNavigated(object sender, Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            if (e.Content is Page page)
            {
                page.Loaded += OnPageLoaded;
            }
        }

        private static void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is Page page)
            {
                page.Loaded -= OnPageLoaded;
                AttachCommands(page);
            }
        }

        private static Panel CreateCommands()
        {
            Microsoft.UI.Xaml.Controls.StackPanel panel = new Microsoft.UI.Xaml.Controls.StackPanel { Orientation = Orientation.Horizontal, Spacing = 2 };

            Button back = new Button
            {
                Content = "\uE72B",
                Style = GetStyle("NavigationIconButtonNormalStyle"),
                IsEnabled = CanGoBack(),
            };
            ToolTipService.SetToolTip(back, "返回");
            back.Click += (_, __) => GoBack();

            Button close = new Button
            {
                Content = "\uE711",
                Style = GetStyle("NavigationIconButtonNormalStyle"),
            };
            ToolTipService.SetToolTip(close, "关闭");
            close.Click += (_, __) => Close();

            panel.Children.Add(back);
            panel.Children.Add(close);
            return panel;
        }

        /// <summary>缓存页面会反复显示，返回按钮的可用状态要按当前返回栈重算。</summary>
        private static void UpdateBackButton(Panel panel)
        {
            foreach (UIElement child in panel.Children)
            {
                if (child is Button button && "\uE72B".Equals(button.Content))
                {
                    button.IsEnabled = CanGoBack();
                    return;
                }
            }
        }

        public static void GoBack()
        {
            if (CanGoBack()) { DetailFrame.GoBack(); }
        }

        private static Style GetStyle(string key) =>
            Application.Current.Resources.TryGetValue(key, out object style) ? style as Style : null;

        /// <summary>关闭侧栏并清掉它的返回栈，主栏保持不动。</summary>
        public static void Close()
        {
            if (DetailFrame == null) { return; }
            if (DetailFrame.BackStackDepth > 0) { DetailFrame.BackStack.Clear(); }
            ShowPlaceholder();
            SetOpen(true);
        }

        private static IEnumerable<PageHeader> FindHeaders(DependencyObject root)
        {
            int count = VisualTreeHelper.GetChildrenCount(root);
            for (int i = 0; i < count; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(root, i);
                if (child is PageHeader header)
                {
                    yield return header;
                }
                foreach (PageHeader nested in FindHeaders(child))
                {
                    yield return nested;
                }
            }
        }
    }
}
