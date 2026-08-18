using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using CoolapkUWP.ViewModels.FeedPages;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace CoolapkUWP.Pages.FeedPages
{
    /// <summary>
    /// 排行榜页：顶部为可横向滚动的榜单选择条(默认选中「手机榜」)，下方为对应榜单的产品列表。
    /// </summary>
    public sealed partial class RankingPage : Page, INotifyPropertyChanged
    {
        private RankingViewModel _provider;
        private ScrollViewer _scrollViewer;
        public RankingViewModel Provider
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

        public RankingPage()
        {
            InitializeComponent();
            Provider = new RankingViewModel();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            await Refresh(true);
            TabListView.SelectedIndex = Provider.SelectedIndex;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }

        private async Task Refresh(bool reset = false)
        {
            await Provider.Refresh(reset);
            if (reset)
            {
                TabListView.SelectedIndex = Provider.SelectedIndex;
            }
        }

        private void ProductListView_Loaded(object sender, RoutedEventArgs e)
        {
            if (_scrollViewer != null) { return; }
            _scrollViewer = FindScrollViewer(ProductListView);
            if (_scrollViewer != null)
            {
                _scrollViewer.ViewChanged += OnListViewViewChanged;
            }
        }

        private static ScrollViewer FindScrollViewer(DependencyObject root)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(root, i);
                if (child is ScrollViewer scrollViewer) { return scrollViewer; }
                ScrollViewer result = FindScrollViewer(child);
                if (result != null) { return result; }
            }
            return null;
        }

        private void OnListViewViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (_scrollViewer == null || _scrollViewer.ScrollableHeight <= 0) { return; }
            if (_scrollViewer.VerticalOffset >= _scrollViewer.ScrollableHeight - 200)
            {
                _ = LoadMoreAsync();
            }
        }

        private async Task LoadMoreAsync()
        {
            FeedListItemSource source = Provider.SelectedSource;
            if (source != null && source.HasMoreItems)
            {
                await source.LoadMoreItemsAsync(20);
            }
        }

        private async void RefreshContainer_RefreshRequested(RefreshContainer sender, RefreshRequestedEventArgs args) => await Refresh(true);

        private async void TabListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TabListView.SelectedIndex >= 0 && TabListView.SelectedIndex != Provider.SelectedIndex)
            {
                await Provider.SelectTabAsync(TabListView.SelectedIndex);
            }
        }

        private async void RankingTab_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.Tag is RankingTabModel tab)
            {
                int index = Provider.Tabs?.IndexOf(tab) ?? -1;
                if (index >= 0)
                {
                    TabListView.SelectedIndex = index;
                    if (index != Provider.SelectedIndex)
                    {
                        await Provider.SelectTabAsync(index);
                    }
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }
    }
}