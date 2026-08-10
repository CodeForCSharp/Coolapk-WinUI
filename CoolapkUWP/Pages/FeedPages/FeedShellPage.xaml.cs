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
            void DisabledCopy()
            {
                if ((sender as FrameworkElement).DataContext is ICanCopy i)
                {
                    i.IsCopyEnabled = false;
                }
            }

            FrameworkElement element = sender as FrameworkElement;
            switch (element.Name)
            {
                case "ReplyButton":
                    DisabledCopy();
                    if (element.Tag is FeedModelBase feed)
                    {
                        new CreateFeedControl
                        {
                            ReplyID = feed.ID,
                            FeedType = CreateFeedType.Reply,
                            PopupTransitions = new TransitionCollection
                            {
                                new EdgeUIThemeTransition
                                {
                                    Edge = EdgeTransitionLocation.Bottom
                                }
                            }
                        }.Show(this);
                    }
                    break;

                case "LikeButton":
                    DisabledCopy();
                    _ = (element.Tag as ICanLike).ChangeLike();
                    break;

                case "ReportButton":
                    DisabledCopy();
                    _ = this.NavigateAsync(typeof(BrowserPage), new BrowserViewModel(element.Tag.ToString()));
                    break;

                case "ShareButton":
                    DisabledCopy();
                    break;

                default:
                    DisabledCopy();
                    _ = this.OpenLinkAsync((sender as FrameworkElement).Tag as string);
                    break;
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
            // Remove details content from it's parent panel.
            if (HeaderControl.Parent != null)
            {
                (HeaderControl.Parent as Panel).Children.Remove(HeaderControl);
            }
            else
            {
                LeftGrid.Children.Remove(HeaderControl);
                RightGrid.Children.Remove(HeaderControl);
            }

            if (DetailControl.Parent != null)
            {
                (DetailControl.Parent as Panel).Children.Remove(DetailControl);
            }
            else
            {
                Pane1Grid.Children.Remove(DetailControl);
                Pane2Grid.Children.Remove(DetailControl);
            }

            if (BtnsPanel.Parent != null)
            {
                (BtnsPanel.Parent as Panel).Children.Remove(BtnsPanel);
            }
            else
            {
                LeftGrid.Children.Remove(BtnsPanel);
                RightGrid.Children.Remove(BtnsPanel);
            }

            // Single pane
            if (sender.Mode == TwoPaneViewMode.SinglePane)
            {
                ListControl.RefreshButtonVisibility = Visibility.Collapsed;
                // Add the details content to Pane1.
                RightGrid.Children.Add(HeaderControl);
                RightGrid.Children.Add(BtnsPanel);
                Pane2Grid.Children.Add(DetailControl);
            }
            // Dual pane.
            else
            {
                ListControl.RefreshButtonVisibility = Visibility.Visible;
                // Put details content in Pane2.
                LeftGrid.Children.Add(HeaderControl);
                LeftGrid.Children.Add(BtnsPanel);
                Pane1Grid.Children.Add(DetailControl);
            }
        }

        private void TwoPaneView_Loaded(object sender, RoutedEventArgs e)
        {
            TwoPaneView_ModeChanged(sender as TwoPaneView, null);
        }

        #endregion 界面模式切换
    }
}
