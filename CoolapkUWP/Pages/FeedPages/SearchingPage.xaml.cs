using CoolapkUWP.ViewModels.DataSource;
using CoolapkUWP.ViewModels.FeedPages;
using System.Collections.ObjectModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了"空白页"项模板

namespace CoolapkUWP.Pages.FeedPages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SearchingPage : PivotPageBase
    {
        private SearchingViewModel _provider;
        public SearchingViewModel Provider
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

        public SearchingPage() => InitializeComponent();

        protected override Pivot PivotControl => Pivot;

        protected override ObservableCollection<PivotItem> GetMainItems() => null;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is SearchingViewModel ViewModel
                && Provider?.IsEqual(ViewModel) != true)
            {
                Provider = ViewModel;
                if (Provider.PivotIndex != -1)
                {
                    PivotIndex = Provider.PivotIndex;
                }
                // 页面还没加载时，Pivot_Loaded 会选中标签并触发加载。已经在缓存里就地刷新当前标签。
                if (Pivot.IsLoaded)
                {
                    if (Pivot.SelectedIndex != PivotIndex)
                    {
                        Pivot.SelectedIndex = PivotIndex;
                    }
                    else
                    {
                        LoadSelectedTab(reset: true);
                    }
                }
            }
        }

        protected override void OnTabSelected(PivotItem item)
        {
            LoadSelectedTab(reset: false);
            RightHeader.Visibility = PivotControl.SelectedIndex == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>只准备并加载当前标签。另外两个标签留到切过去再请求。</summary>
        private void LoadSelectedTab(bool reset)
        {
            if (Provider == null || Pivot.SelectedIndex < 0) { return; }

            bool keywordChanged = Provider.PrepareTab(Pivot.SelectedIndex, out EntityItemSource source);
            BindSource(source);
            refresh = resetFlag => source.Refresh(resetFlag);
            // 数据源必须先绑到列表上。否则这次加载结束时列表还看不到它，会把增量加载关掉。
            if (reset || keywordChanged || source.Count == 0)
            {
                _ = source.Refresh(true);
            }
        }

        private void BindSource(EntityItemSource source)
        {
            if (Pivot.SelectedItem is PivotItem item
                && item.Content is RefreshContainer container
                && container.Content is ListView list
                && list.ItemsSource != source)
            {
                list.ItemsSource = source;
            }
        }

        private void RefreshContainer_RefreshRequested(RefreshContainer sender, RefreshRequestedEventArgs args)
        {
            if (sender.Content is ListView ListView && ListView.ItemsSource is EntityItemSource ItemsSource)
            {
                _ = ItemsSource.Refresh(true);
            }
        }
    }
}
