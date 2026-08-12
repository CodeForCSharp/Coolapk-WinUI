using CoolapkUWP.Helpers;
using CoolapkUWP.Helpers.Controls;
using CoolapkUWP.Services;
using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

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

        private void FlipView_SizeChanged(object sender, SizeChangedEventArgs e) => (sender as FrameworkElement).MaxHeight = e.NewSize.Width / 3;

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
    }
}
