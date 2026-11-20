using CoolapkUWP.Services;
using CoolapkUWP.Controls;
using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using CoolapkUWP.Models.Feeds;
using CoolapkUWP.Pages.BrowserPages;
using CoolapkUWP.ViewModels.BrowserPages;
using CoolapkUWP.ViewModels.FeedPages;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace CoolapkUWP.Pages.FeedPages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class FeedShellPage : Page, INotifyPropertyChanged
    {
        private FeedShellViewModel _provider;
        public FeedShellViewModel Provider
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

        public FeedShellPage() => InitializeComponent();

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is FeedShellViewModel ViewModel
                && Provider?.IsEqual(ViewModel) != true)
            {
                Provider = ViewModel;
                await Provider.Refresh(true);
            }
            await Task.Delay(30);
        }

        private void FeedButton_Click(object sender, RoutedEventArgs e)
        {
            FeedCommandService.HandleFeedButtonClick(sender as FrameworkElement, this);
        }

        private void CopyMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ClipboardHelper.SetText((sender as FrameworkElement).Tag.ToString());
        }

        private void ReplyFilter_Click(object sender, RoutedEventArgs e)
        {
            if (Provider.ItemSource?.FirstOrDefault()?.ItemSource is ReplyItemSource replies
                && sender is FrameworkElement element)
            {
                replies.SelectFilter(element.Tag as string);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }
    }
}
