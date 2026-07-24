using CoolapkUWP.Common;
using CoolapkUWP.Helpers;
using HtmlAgilityPack;
using System.Text.Json.Nodes;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Storage;
using Microsoft.UI.Dispatching;

namespace CoolapkUWP.ViewModels.BrowserPages
{
    public partial class HTMLViewModel : IViewModel
    {
        public DispatcherQueue Dispatcher { get; }

        private readonly Uri uri;
        private Action<UISettingChangedType> UISettingChanged;

        private string title;
        public string Title
        {
            get => title;
            private set
            {
                if (title != value)
                {
                    title = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private string html;
        public string HTML
        {
            get => html;
            private set
            {
                if (html != value)
                {
                    html = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private string rawHTML;
        public string RawHTML
        {
            get => rawHTML;
            private set
            {
                if (rawHTML != value)
                {
                    rawHTML = value;
                    RaisePropertyChangedEvent();
                    _ = GetHtmlAsync(value, ThemeHelper.IsDarkTheme() ? "Dark" : "Light");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private async void RaisePropertyChangedEvent([CallerMemberName] string name = null)
        {
            if (name != null)
            {
                await Dispatcher.ResumeForegroundAsync();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }

        public HTMLViewModel(string url, DispatcherQueue dispatcher)
        {
            Dispatcher = dispatcher;
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
                    (isSucceed, result) = await RequestHelper.GetStringAsync(uri).ConfigureAwait(false);
                    if (isSucceed && !string.IsNullOrWhiteSpace(result))
                    {
                        HtmlDocument doc = new HtmlDocument();
                        doc.LoadHtml(result);
                        string content = doc.DocumentNode.ChildNodes.FindFirst("html")?.ChildNodes.FindFirst("body")?.InnerHtml;
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
