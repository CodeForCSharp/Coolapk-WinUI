using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using CoolapkUWP.Models.Feeds;
using CoolapkUWP.Models.Images;
using CoolapkUWP.Pages.BrowserPages;
using CoolapkUWP.Pages.FeedPages;
using CoolapkUWP.ViewModels.BrowserPages;
using CoolapkUWP.ViewModels.FeedPages;
using System.ComponentModel;
using System.Text.Json.Nodes;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Resources;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace CoolapkUWP.Controls
{
    public sealed partial class FeedShellDetailControl : UserControl, INotifyPropertyChanged
    {
        private FeedDetailModel _feedDetail;
        public FeedDetailModel FeedDetail
        {
            get => _feedDetail;
            private set
            {
                if (_feedDetail != value)
                {
                    _feedDetail = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        public FeedShellDetailControl()
        {
            InitializeComponent();
            DataContextChanged += (_, _) => FeedDetail = DataContext as FeedDetailModel;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            switch (element.Name)
            {
                case "ReportButton":
                    _ = this.NavigateAsync(typeof(BrowserPage), new BrowserViewModel(element.Tag.ToString()));
                    break;
                case "FollowButton":
                    _ = (element.Tag as ICanFollow).ChangeFollow();
                    break;
                default:
                    _ = this.OpenLinkAsync((sender as FrameworkElement).Tag as string);
                    break;
            }
        }

        private async void DeviceHyperlink_Click(Hyperlink sender, HyperlinkClickEventArgs args)
        {
            UIHelper.ShowProgressBar();
            string device = (sender.Inlines.FirstOrDefault().ElementStart.VisualParent.DataContext as FeedModelBase).DeviceTitle;
            (bool isSucceed, JsonNode result) = await RequestHelper.GetDataAsync(UriHelper.GetUri(UriType.GetProductDetailByName, device), true);
            UIHelper.HideProgressBar();
            if (!isSucceed) { return; }

            JsonObject token = result.AsObject();

            if (token.TryGetPropertyValue("id", out JsonNode id))
            {
                FeedListViewModel provider = FeedListViewModel.GetProvider(FeedListType.ProductPageList, id.ToString());

                if (provider != null)
                {
                    _ = this.NavigateAsync(typeof(FeedListPage), provider);
                }
            }
        }

        private void CopyMenuItem_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            DataPackage dp = new DataPackage();
            dp.SetText(element.Tag.ToString());
            Clipboard.SetContent(dp);
        }

        private void Flyout_Opened(object sender, object _)
        {
            Flyout flyout = (Flyout)sender;
            if (flyout.Content == null)
            {
                flyout.Content = new ShowQRCodeControl
                {
                    QRCodeText = (string)flyout.Target.Tag
                };
            }
        }

        private void OnTapped(object sender, TappedRoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            if ((element.DataContext as ICanCopy)?.IsCopyEnabled ?? false) { return; }

            if (e != null) { e.Handled = true; }

            _ = element.Tag is ImageModel image ? element.ShowImageAsync(image) : this.OpenLinkAsync(element.Tag.ToString());
        }

        private void UrlButton_Click(object sender, RoutedEventArgs e) => _ = this.OpenLinkAsync((sender as FrameworkElement).Tag.ToString());

        private void GridView_SelectionChanged(object sender, SelectionChangedEventArgs e) => (sender as GridView).SelectedIndex = -1;

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }
    }
}
