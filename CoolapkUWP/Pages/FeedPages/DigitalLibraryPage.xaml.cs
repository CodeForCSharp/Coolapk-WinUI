using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using System.Text.Json.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace CoolapkUWP.Pages.FeedPages
{
    /// <summary>
    /// 数码库：左侧分类列表，右侧展示该分类下的产品（图片 + 名称 + 热度）。
    /// </summary>
    public sealed partial class DigitalLibraryPage : Page
    {
        private readonly List<(string Title, string Url)> categories = new List<(string Title, string Url)>();
        private int loadSeq;

        public DigitalLibraryPage()
        {
            InitializeComponent();
            _ = LoadCategoriesAsync();
        }

        private async Task LoadCategoriesAsync()
        {
            (bool isSucceed, JsonNode result) = await RequestHelper.GetDataAsync(
                UriHelper.GetUri(UriType.GetIndexPage, "/page/dataList?url=/product/categoryList", "&", 1), true);
            if (!isSucceed || result is not JsonArray array) { return; }

            foreach (JsonNode node in array)
            {
                JsonObject obj = node.AsObject();
                string title = (string)obj["title"];
                string url = (string)obj["url"];
                if (!string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(url))
                {
                    categories.Add((title, url));
                }
            }

            CategoryList.ItemsSource = categories.Select(c => c.Title).ToList();
            if (categories.Count > 0)
            {
                CategoryList.SelectedIndex = 0;
            }
        }

        private async void CategoryList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = CategoryList.SelectedIndex;
            if (index < 0 || index >= categories.Count) { return; }
            await LoadDevicesAsync(categories[index].Url, ++loadSeq);
        }

        private async Task LoadDevicesAsync(string categoryUrl, int seq)
        {
            string inner = categoryUrl.StartsWith("/page?url=", StringComparison.Ordinal)
                ? categoryUrl.Substring("/page?url=".Length)
                : categoryUrl;

            string normalized = UriHelper.NormalizePageUri(inner);
            Uri uri = UriHelper.GetUri(UriType.GetIndexPage, normalized, normalized.Contains("?") ? "&" : "?", 1);

            (bool isSucceed, JsonNode result) = await RequestHelper.GetDataAsync(uri, true);
            if (!isSucceed || result is not JsonArray array || seq != loadSeq) { return; }

            List<ProductModel> devices = new List<ProductModel>();
            foreach (JsonNode node in array)
            {
                JsonObject obj = node.AsObject();
                if ((string)obj["entityType"] == "product")
                {
                    devices.Add(ProductModel.FromJson(obj));
                }
            }
            DeviceList.ItemsSource = devices;
        }

        private void Device_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.Tag is string url)
            {
                _ = this.OpenLinkAsync(url);
            }
        }

        private void Device_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter || e.Key == Windows.System.VirtualKey.Space)
            {
                Device_Tapped(sender, null);
            }
        }
    }
}
