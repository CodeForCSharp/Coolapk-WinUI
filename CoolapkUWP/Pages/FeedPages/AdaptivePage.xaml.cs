using CoolapkUWP.Helpers;
using CoolapkUWP.ViewModels.FeedPages;
using CommunityToolkit.WinUI;
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
    public sealed partial class AdaptivePage : Page, INotifyPropertyChanged
    {
        private AdaptiveViewModel _provider;
        public AdaptiveViewModel Provider
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

        public AdaptivePage() => InitializeComponent();

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is AdaptiveViewModel ViewModel
                && Provider?.IsEqual(ViewModel) != true)
            {
                if (Provider != null)
                {
                    Provider.LoadMoreStarted -= ProgressBarHelper.ShowProgressBar;
                    Provider.LoadMoreCompleted -= ProgressBarHelper.HideProgressBar;
                }
                Provider = ViewModel;
                Provider.LoadMoreStarted += ProgressBarHelper.ShowProgressBar;
                Provider.LoadMoreCompleted += ProgressBarHelper.HideProgressBar;
                await Refresh(true);
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            Provider.LoadMoreStarted -= ProgressBarHelper.ShowProgressBar;
            Provider.LoadMoreCompleted -= ProgressBarHelper.HideProgressBar;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Page page = this.FindAscendant<Page>();
            Provider.IsShowTitle = page is MainPage;
        }

        public async Task Refresh(bool reset = false) => await Provider.Refresh(reset);

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e) => _ = Refresh(true);

        private async void RefreshContainer_RefreshRequested(RefreshContainer sender, RefreshRequestedEventArgs args) => await Refresh(true);
    }
}
