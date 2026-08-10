using CoolapkUWP.Controls;
using CoolapkUWP.Helpers;
using CoolapkUWP.ViewModels.BrowserPages;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using System.ComponentModel;
using System.Text.Json.Nodes;
using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了"空白页"项模板

namespace CoolapkUWP.Pages.BrowserPages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class BrowserPage : Page, INotifyPropertyChanged
    {
        private BrowserViewModel _provider;
        public BrowserViewModel Provider
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

        public BrowserPage() => InitializeComponent();

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Frame.Navigating += OnFrameNavigating;
            if (e.Parameter is BrowserViewModel ViewModel)
            {
                Provider = ViewModel;
                if (Provider.Uri != null)
                {
                    WebView.Source = Provider.Uri;
                }
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            Frame.Navigating -= OnFrameNavigating;
            WebView.Close();
        }

        private void WebView_NavigationStarting(WebView2 sender, CoreWebView2NavigationStartingEventArgs args)
        {
            UIHelper.ShowProgressBar();
        }

        private async void WebView_NavigationCompleted(WebView2 sender, CoreWebView2NavigationCompletedEventArgs args)
        {
            if (Provider.IsLoginPage && sender.Source.AbsoluteUri == "https://www.coolapk.com/")
            {
                await CheckLogin();
            }
            else if (sender.Source.AbsoluteUri == UriHelper.LoginUri)
            {
                Provider.IsLoginPage = true;
            }
            Provider.Title = sender.CoreWebView2.DocumentTitle;
            UIHelper.HideProgressBar();
        }

        private void OnFrameNavigating(object sender, NavigatingCancelEventArgs args)
        {
            if (args.NavigationMode == NavigationMode.Back && WebView.CanGoBack)
            {
                WebView.GoBack();
                args.Cancel = true;
            }
        }

        private async Task CheckLogin(bool manual = false)
        {
            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("BrowserPage");
            if (await SetLoginCookie(manual) && await SettingsHelper.Login())
            {
                if (Frame.CanGoBack)
                {
                    Frame.Navigating -= OnFrameNavigating;
                    Frame.GoBack();
                }
                UIHelper.ShowMessage(loader.GetString("LoginSuccessfully"));
            }
            else
            {
                WebView.Source = new Uri(UriHelper.LoginUri);
                UIHelper.ShowMessage(loader.GetString("CannotGetToken"));
            }
        }

        public async Task<bool> SetLoginCookie(bool manual = false)
        {
            string Uid = string.Empty, Token = string.Empty, UserName = string.Empty;
            if (manual)
            {
                foreach ((string name, string value) in NetworkHelper.GetCoolapkCookies(UriHelper.CoolapkUri))
                {
                    switch (name)
                    {
                        case "uid":
                            Uid = value;
                            break;
                        case "username":
                            UserName = value;
                            break;
                        case "token":
                            Token = value;
                            break;
                        default:
                            break;
                    }
                }
                if (!string.IsNullOrEmpty(Uid) && !string.IsNullOrEmpty(UserName) && !string.IsNullOrEmpty(Token))
                {
                    CoreWebView2CookieManager cookieManager = WebView.CoreWebView2.CookieManager;
                    CoreWebView2Cookie uid = cookieManager.CreateCookie("uid", Uid, ".coolapk.com", "/");
                    CoreWebView2Cookie username = cookieManager.CreateCookie("username", UserName, ".coolapk.com", "/");
                    CoreWebView2Cookie token = cookieManager.CreateCookie("token", Token, ".coolapk.com", "/");
                    cookieManager.AddOrUpdateCookie(uid);
                    cookieManager.AddOrUpdateCookie(username);
                    cookieManager.AddOrUpdateCookie(token);
                    return true;
                }
            }
            else
            {
                foreach (CoreWebView2Cookie item in await WebView.CoreWebView2.CookieManager.GetCookiesAsync("https://coolapk.com"))
                {
                    switch (item.Name)
                    {
                        case "uid":
                            Uid = item.Value;
                            break;
                        case "username":
                            UserName = item.Value;
                            break;
                        case "token":
                            Token = item.Value;
                            break;
                        default:
                            break;
                    }
                }
                if (!string.IsNullOrEmpty(Uid) && !string.IsNullOrEmpty(UserName) && !string.IsNullOrEmpty(Token))
                {
                    NetworkHelper.SetLoginCookie(Uid, UserName, Token);
                    return true;
                }
            }
            return false;
        }

        private async void ManualLoginButton_Click(object sender, RoutedEventArgs e)
        {
            UIHelper.ShowProgressBar();
            LoginDialog dialog = new LoginDialog();
            ContentDialogResult result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                _ = CheckLogin(true);
            }
            else
            {
                UIHelper.HideProgressBar();
            }
        }

        private void GotoSystemBrowserButton_Click(object sender, RoutedEventArgs e) => _ = Launcher.LaunchUriAsync(WebView.Source);

        private void TryLoginButton_Click(object sender, RoutedEventArgs e) => _ = CheckLogin();

        private void RefreshButton_Click(object sender, RoutedEventArgs e) => WebView.Reload();

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }
    }
}
