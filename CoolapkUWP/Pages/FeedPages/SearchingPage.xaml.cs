using CoolapkUWP.ViewModels.DataSource;
using CoolapkUWP.ViewModels.FeedPages;
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

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is SearchingViewModel ViewModel
                && Provider?.IsEqual(ViewModel) != true)
            {
                Provider = ViewModel;
                if (Provider.PivotIndex != -1)
                { PivotIndex = Provider.PivotIndex; }
                await Provider.Refresh(true);
            }
        }

        protected override void OnTabSelected(PivotItem item)
        {
            if (item.Content is RefreshContainer RefreshContainer
                && RefreshContainer.Content is ListView ListView
                && ListView.ItemsSource is EntityItemSource ItemsSource)
            {
                refresh = (reset) => ItemsSource.Refresh(reset);
            }
            RightHeader.Visibility = PivotControl.SelectedIndex == 0 ? Visibility.Visible : Visibility.Collapsed;
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
