using CoolapkUWP.ViewModels.DataSource;
using CoolapkUWP.ViewModels.FeedPages;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace CoolapkUWP.Pages.FeedPages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SearchingPage : Page, INotifyPropertyChanged
    {
        private static int PivotIndex = 0;
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

        private Func<bool, Task> Refresh;

        public SearchingPage() => InitializeComponent();

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

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            PivotIndex = Pivot.SelectedIndex;
        }

        private void Pivot_Loaded(object sender, RoutedEventArgs e)
        {
            Pivot.SelectedIndex = PivotIndex;
        }

        private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PivotItem MenuItem = Pivot.SelectedItem as PivotItem;
            if ((Pivot.SelectedItem as PivotItem).Content is RefreshContainer RefreshContainer
                && RefreshContainer.Content is ListView ListView
                && ListView.ItemsSource is EntityItemSource ItemsSource)
            {
                Refresh = (reset) => _ = ItemsSource.Refresh(reset);
            }
            RightHeader.Visibility = Pivot.SelectedIndex == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void RefreshContainer_RefreshRequested(RefreshContainer sender, RefreshRequestedEventArgs args)
        {
            if (sender.Content is ListView ListView && ListView.ItemsSource is EntityItemSource ItemsSource)
            {
                _ = ItemsSource.Refresh(true);
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            if (Refresh != null)
            {
                _ = Refresh(true);
            }
            else if ((Pivot.SelectedItem as PivotItem).Content is RefreshContainer RefreshContainer
                && RefreshContainer.Content is ListView ListView
                && ListView.ItemsSource is EntityItemSource ItemsSource)
            {
                _ = ItemsSource.Refresh(true);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }
    }
}
