using CoolapkUWP.Helpers;
using CoolapkUWP.Helpers.Controls;
using CoolapkUWP.Models;
using CoolapkUWP.Models.Feeds;
using CoolapkUWP.Services;
using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了"用户控件"项模板

namespace CoolapkUWP.Controls.DataTemplates
{
    public sealed partial class IndexCardTemplates : ResourceDictionary
    {
        public IndexCardTemplates() => InitializeComponent();

        public static CornerRadius BottomOverlayCornerRadius
        {
            get
            {
                var r = (CornerRadius)Application.Current.Resources["OverlayCornerRadius"];
                return new CornerRadius(0, 0, r.BottomRight, r.BottomLeft);
            }
        }

        private void FlipView_SizeChanged(object sender, SizeChangedEventArgs e)
            => (sender as FrameworkElement).MaxHeight = e.NewSize.Width * 240 / 1080;

        private void FlipView_Loaded(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            if (SettingsHelper.Get<bool>(SettingsHelper.IsNoPicsMode))
            {
                if (element.Parent is FrameworkElement parent)
                { parent.Visibility = Visibility.Collapsed; }
            }
            else
            {
                FlipViewHelper.EnableAutoPlay(element as FlipView);
            }
        }

        private void Grid_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter || e.Key == Windows.System.VirtualKey.Space)
            {
                FrameworkElement element = sender as FrameworkElement;
                CardNavigationService.HandleCardTap(element, element.Tag);
            }
        }

        private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (e != null && !UIHelper.IsOriginSource(sender, e.OriginalSource)) { return; }
            FrameworkElement element = sender as FrameworkElement;
            CardNavigationService.HandleCardTap(element, element.Tag);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            CardNavigationService.HandleCardTap(element, element.Tag);
        }

        private void RatingCard_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter || e.Key == Windows.System.VirtualKey.Space)
            {
                if (sender is FrameworkElement element)
                {
                    CardNavigationService.HandleCardTap(element, element.Tag);
                }
                e.Handled = true;
            }
        }

        private void RatingCard_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (e.OriginalSource is Microsoft.UI.Xaml.Controls.Image)
            {
                e.Handled = true;
                return;
            }
            if (sender is FrameworkElement element)
            {
                CardNavigationService.HandleCardTap(element, element.Tag);
            }
            e.Handled = true;
        }

        private void RatingTargetRow_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is FrameworkElement element
                && element.Tag is SourceFeedModel feed
                && !string.IsNullOrEmpty(feed.TargetRowUrl))
            {
                e.Handled = true;
                CardNavigationService.HandleCardTap(element, feed.TargetRowUrl);
            }
        }

        private void SortOptionButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.Tag is SortSelectOptionModel option)
            {
                if (option.Parent is SortSelectCardModel card)
                {
                    card.SelectedIndex = option.Index;
                }
                CardNavigationService.HandleCardTap(element, option);
            }
        }

        private void SelectorLinkButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.Tag is SelectorLinkModel link)
            {
                if (link.Parent is SelectorLinkCardModel card)
                {
                    card.SelectedIndex = link.Index;
                }
                CardNavigationService.HandleCardTap(element, link);
            }
        }

        private async void ColorfulItem_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is not Border { Tag: string uri } border || string.IsNullOrEmpty(uri)) { return; }

            Color? color = await ImageColorHelper.GetDominantColorAsync(uri);
            if (color is Color c)
            {
                border.Background = new SolidColorBrush(Color.FromArgb(0x8C, c.R, c.G, c.B));
            }
        }
    }
}
