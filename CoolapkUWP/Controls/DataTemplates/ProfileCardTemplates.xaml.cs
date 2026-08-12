using CoolapkUWP.Helpers;
using CoolapkUWP.Services;
using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了"用户控件"项模板

namespace CoolapkUWP.Controls.DataTemplates
{
    public sealed partial class ProfileCardTemplates : ResourceDictionary
    {
        public static readonly DependencyProperty XamlHostProperty =
            DependencyProperty.Register(
                nameof(XamlHost),
                typeof(DependencyObject),
                typeof(ProfileCardTemplates),
                null);

        public DependencyObject XamlHost
        {
            get => (DependencyObject)GetValue(XamlHostProperty);
            set => SetValue(XamlHostProperty, value);
        }

        public static readonly DependencyProperty FlyoutBaseProperty =
            DependencyProperty.Register(
                nameof(FlyoutBase),
                typeof(FlyoutBase),
                typeof(ProfileCardTemplates),
                null);

        public FlyoutBase FlyoutBase
        {
            get => (FlyoutBase)GetValue(FlyoutBaseProperty);
            set => SetValue(FlyoutBaseProperty, value);
        }

        public ProfileCardTemplates() => InitializeComponent();

        private void Grid_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter || e.Key == Windows.System.VirtualKey.Space)
            {
                FrameworkElement element = sender as FrameworkElement;
                if (CardNavigationService.HandleCardTap(XamlHost, element.Tag))
                {
                    FlyoutBase?.Hide();
                }
            }
        }

        private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (e != null && !UIHelper.IsOriginSource(sender, e.OriginalSource)) { return; }
            FrameworkElement element = sender as FrameworkElement;
            if (CardNavigationService.HandleCardTap(XamlHost, element.Tag))
            {
                FlyoutBase?.Hide();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            if (CardNavigationService.HandleCardTap(XamlHost, element.Tag))
            {
                FlyoutBase?.Hide();
            }
        }
    }
}
