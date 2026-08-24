using CoolapkUWP.Helpers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace CoolapkUWP.Controls.DataTemplates
{
    public sealed partial class NotificationTemplates : ResourceDictionary
    {
        public NotificationTemplates() => InitializeComponent();

        private void OnTapped(object sender, TappedRoutedEventArgs e)
        {
            // 头像(Border)点击打开用户主页，标记已处理避免冒泡到外层 Grid 再次触发通知跳转。
            if (sender is Border)
            {
                FrameworkElement element = sender as FrameworkElement;
                _ = element.OpenLinkAsync(element.Tag as string);
                if (e != null) { e.Handled = true; }
                return;
            }

            if (e == null || sender is Grid)
            {
                FrameworkElement element = sender as FrameworkElement;
                _ = element.OpenLinkAsync(element.Tag as string);
            }
        }

        private void OnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter || e.Key == Windows.System.VirtualKey.Space)
            {
                OnTapped(sender, null);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e) => OnTapped(sender, null);
    }
}
