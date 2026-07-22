using CoolapkUWP.Controls;
using CoolapkUWP.Helpers;
using CoolapkUWP.ViewModels.BrowserPages;
using CommunityToolkit.WinUI;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Web.WebView2.Core;

namespace CoolapkUWP.Pages.BrowserPages
{
    public sealed partial class HTMLPage : Page
    {
        private HTMLViewModel Provider;

        public HTMLPage() => InitializeComponent();

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            UIHelper.MainPage.FindDescendant<WebViewContentControl>().IsWebView = true;
            if (e.Parameter is HTMLViewModel ViewModel)
            {
                Provider = ViewModel;
                DataContext = Provider;
                await Refresh(true);
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            UIHelper.MainPage.FindDescendant<WebViewContentControl>().IsWebView = false;
        }

        private void WebView_NavigationStarting(WebView2 sender, CoreWebView2NavigationStartingEventArgs args)
        {
            if (args.Uri != null)
            {
                args.Cancel = true;
                _ = this.OpenLinkAsync(args.Uri);
            }
        }

        public async Task Refresh(bool reset = false) => await Provider.Refresh(reset);

        private void RefreshButton_Click(object sender, RoutedEventArgs e) => _ = Refresh(true);
    }
}
