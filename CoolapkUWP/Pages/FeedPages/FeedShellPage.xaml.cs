using CoolapkUWP.Services;
using CoolapkUWP.Controls;
using CoolapkUWP.Helpers;
using CoolapkUWP.Helpers.Controls;
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
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using TwoPaneView = Microsoft.UI.Xaml.Controls.TwoPaneView;
using TwoPaneViewMode = Microsoft.UI.Xaml.Controls.TwoPaneViewMode;

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
                if (Provider.FeedDetail != null)
                {
                    SetLayout();
                }
            }
            await Task.Delay(30);
        }

        private void SetLayout()
        {
            TwoPaneView.MinWideModeWidth = Provider.FeedDetail?.IsFeedArticle ?? false ? 876 : 804;
            TwoPaneView.Pane1Length = new GridLength(Provider.FeedDetail?.IsFeedArticle ?? false ? 520 : 420);
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

        #region 界面模式切换

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        private void TwoPaneView_ModeChanged(TwoPaneView sender, object args)
        {
            TwoPaneViewHelper.UpdateHeaderPane(HeaderControl, LeftGrid, RightGrid, sender.Mode);
            TwoPaneViewHelper.UpdateHeaderPane(BtnsPanel, LeftGrid, RightGrid, sender.Mode);
            Grid.SetRow(DetailControl, 0);
            TwoPaneViewHelper.UpdateDetailPane(DetailControl, Pane1Grid, Pane2Grid, sender.Mode);

            // Single pane: detail first, reply filter directly under it.
            if (sender.Mode == TwoPaneViewMode.SinglePane)
            {
                RightGrid.RowDefinitions[0].Height = new GridLength(0);
                Grid.SetRow(ReplyFilterBar, 1);
                TwoPaneViewHelper.UpdateDetailPane(ReplyFilterBar, Pane1Grid, Pane2Grid, sender.Mode);
                ListControl.StickyHeaderHeight = ReplyFilterBar.Height;
            }
            // Dual pane.
            else
            {
                RightGrid.RowDefinitions[0].Height = GridLength.Auto;
                ListControl.StickyHeaderHeight = 0;
                if (!ReferenceEquals(ReplyFilterBar.Parent, RightGrid))
                {
                    (ReplyFilterBar.Parent as Panel)?.Children.Remove(ReplyFilterBar);
                    Grid.SetRow(ReplyFilterBar, 0);
                    RightGrid.Children.Insert(0, ReplyFilterBar);
                }
            }
        }

        private void TwoPaneView_Loaded(object sender, RoutedEventArgs e)
        {
            TwoPaneView_ModeChanged(sender as TwoPaneView, null);
        }

        #endregion 界面模式切换
    }
}
