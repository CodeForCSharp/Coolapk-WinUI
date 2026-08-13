using CoolapkUWP.Common;
using CoolapkUWP.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using AngleSharp.Html.Parser;
using System.Text.Json.Nodes;
using System;
using System.Threading.Tasks;
using Windows.Storage;

namespace CoolapkUWP.ViewModels.BrowserPages
{
    public partial class HTMLViewModel : ObservableObject, IViewModel
    {
        private readonly Uri uri;
        private readonly Action<UISettingChangedType> UISettingChanged;

        [ObservableProperty]
        public partial string Title { get; private set; }

        [ObservableProperty]
        public partial string HTML { get; private set; }

        [ObservableProperty]
        public partial string RawHTML { get; private set; }

        partial void OnRawHTMLChanged(string value) => _ = GetHtmlAsync(value, ThemeHelper.IsDarkTheme() ? "Dark" : "Light");

        public HTMLViewModel(string url)
        {
            uri = url.ValidateAndGetUri();
            UISettingChanged = (mode) =>
            {
                switch (mode)
                {
                    case UISettingChangedType.LightMode:
                        _ = GetHtmlAsync(RawHTML, "Light");
                        break;
                    case UISettingChangedType.DarkMode:
                        _ = GetHtmlAsync(RawHTML, "Dark");
                        break;
                    case UISettingChangedType.NoPicChanged:
                        break;
                }
            };
            ThemeHelper.UISettingChanged.Add(UISettingChanged);
        }

        ~HTMLViewModel()
        {
            ThemeHelper.UISettingChanged.Remove(UISettingChanged);
        }

        public async Task Refresh(bool reset)
        {
            if (uri != null)
            {
                await Load_HTML(uri);
            }
        }

        bool IViewModel.IsEqual(IViewModel other) => other is HTMLViewModel model && IsEqual(model);

        public bool IsEqual(HTMLViewModel other) => uri == other.uri;

        private async Task Load_HTML(Uri uri)
        {
            UIHelper.ShowProgressBar();
            (bool isSucceed, string result) = await RequestHelper.GetStringAsync(uri, "XMLHttpRequest");
            if (isSucceed)
            {
                JsonObject json = JsonNode.Parse(result).AsObject();

                if (json.TryGetPropertyValue("title", out JsonNode title))
                {
                    Title = title.ToString();
                }

                if (json.TryGetPropertyValue("html", out JsonNode html) && !string.IsNullOrEmpty(html.ToString()))
                {
                    RawHTML = html.ToString();
                }
                else if (json.TryGetPropertyValue("description", out JsonNode description) && !string.IsNullOrEmpty(description.ToString()))
                {
                    RawHTML = description.ToString();
                }
                else
                {
                    (isSucceed, result) = await RequestHelper.GetStringAsync(uri);
                    if (isSucceed && !string.IsNullOrWhiteSpace(result))
                    {
                        HtmlParser parser = new HtmlParser();
                        var doc = parser.ParseDocument(result);
                        string content = doc.Body.InnerHtml;
                        if (!string.IsNullOrEmpty(content))
                        {
                            RawHTML = content;
                        }
                    }
                }
            }
            UIHelper.HideProgressBar();
        }

        public async Task GetHtmlAsync(string html, string theme)
        {
            StorageFile indexFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/WebView/HTMLView.html"));
            string index = await FileIO.ReadTextAsync(indexFile);
            index = index.Replace("ms-appx-web:///Assets/WebView", "https://coolapkuwp.app");
            HTML = index.Replace("{{RenderTheme}}", theme).Replace("{{HTMLBody}}", html);
        }
    }
}
