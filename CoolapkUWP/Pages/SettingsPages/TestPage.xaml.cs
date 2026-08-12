using CoolapkUWP.Common;
using CoolapkUWP.Controls;
using CoolapkUWP.Controls.Dialogs;
using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Images;
using CoolapkUWP.Pages.BrowserPages;
using CoolapkUWP.ViewModels.BrowserPages;
using System;
using System.ComponentModel;
using System.Globalization;
using Windows.Globalization;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace CoolapkUWP.Pages.SettingsPages
{
    public sealed partial class TestPage : Page, INotifyPropertyChanged
    {
        internal bool IsExtendsTitleBar
        {
            get => App.MainWindow.ExtendsContentIntoTitleBar;
            set
            {
                if (IsExtendsTitleBar != value)
                {
                    App.MainWindow.ExtendsContentIntoTitleBar = value;
                    ThemeHelper.UpdateSystemCaptionButtonColors();
                }
            }
        }

        internal bool IsUseAPI2
        {
            get => SettingsHelper.Get<bool>(SettingsHelper.IsUseAPI2);
            set
            {
                if (IsUseAPI2 != value)
                {
                    SettingsHelper.Set(SettingsHelper.IsUseAPI2, value);
                    NetworkHelper.SetRequestHeaders();
                }
            }
        }

        internal bool IsCustomUA
        {
            get => SettingsHelper.Get<bool>(SettingsHelper.IsCustomUA);
            set
            {
                if (IsCustomUA != value)
                {
                    SettingsHelper.Set(SettingsHelper.IsCustomUA, value);
                    NetworkHelper.SetRequestHeaders();
                    UserAgent = NetworkHelper.Client.DefaultRequestHeaders.UserAgent.ToString();
                }
            }
        }

        internal int APIVersion
        {
            get => (int)SettingsHelper.Get<APIVersions>(SettingsHelper.APIVersion) - 4;
            set
            {
                if (APIVersion != value)
                {
                    SettingsHelper.Set(SettingsHelper.APIVersion, value + 4);
                    NetworkHelper.SetRequestHeaders();
                    UserAgent = NetworkHelper.Client.DefaultRequestHeaders.UserAgent.ToString();
                }
            }
        }

        internal int TokenVersion
        {
            get => (int)SettingsHelper.Get<TokenVersions>(SettingsHelper.TokenVersion);
            set
            {
                if (TokenVersion != value)
                {
                    SettingsHelper.Set(SettingsHelper.TokenVersion, value);
                    NetworkHelper.SetRequestHeaders();
                }
            }
        }

        internal bool IsUseCompositor
        {
            get => SettingsHelper.Get<bool>(SettingsHelper.IsUseCompositor);
            set => SettingsHelper.Set(SettingsHelper.IsUseCompositor, value);
        }

        internal double SemaphoreSlimCount
        {
            get => SettingsHelper.Get<int>(SettingsHelper.SemaphoreSlimCount);
            set
            {
                if (SemaphoreSlimCount != value)
                {
                    int result = (int)Math.Floor(value);
                    SettingsHelper.Set(SettingsHelper.SemaphoreSlimCount, result);
                    NetworkHelper.SetSemaphoreSlim(result);
                    ImageModel.SetSemaphoreSlim(result);
                    ImageCache.SetDecodeSemaphore(result);
                }
            }
        }

        private string userAgent = NetworkHelper.Client.DefaultRequestHeaders.UserAgent.ToString();
        internal string UserAgent
        {
            get => userAgent;
            set
            {
                if (userAgent != value)
                {
                    userAgent = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private double progressValue = 0;
        internal double ProgressValue
        {
            get => progressValue;
            set
            {
                if (progressValue != value)
                {
                    UIHelper.ShowProgressBar(value);
                    progressValue = value;
                }
            }
        }

        private bool isShowProgressRing = false;
        internal bool IsShowProgressRing
        {
            get => isShowProgressRing;
            set
            {
                if (isShowProgressRing != value)
                {
                    if (value)
                        UIHelper.ShowProgressBar();
                    else
                        UIHelper.HideProgressBar();
                    isShowProgressRing = value;
                }
            }
        }

        public CornerRadius BottomCornerRadius
        {
            get
            {
                CornerRadius r = (CornerRadius)Application.Current.Resources["ControlCornerRadius"];
                return new CornerRadius(0, 0, r.BottomRight, r.BottomLeft);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null)
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public TestPage() => InitializeComponent();

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as FrameworkElement).Tag.ToString())
            {
                case "OpenURL":
                    _ = this.OpenLinkAsync(URLTextBox.Text);
                    break;
                case "CustomUA":
                    var userAgentDialog = new UserAgentDialog(UserAgent);
                    await userAgentDialog.ShowAsync();
                    UserAgent = NetworkHelper.Client.DefaultRequestHeaders.UserAgent.ToString();
                    break;
                case "CustomAPI":
                    var apiVersionDialog = new APIVersionDialog(UserAgent);
                    await apiVersionDialog.ShowAsync();
                    UserAgent = NetworkHelper.Client.DefaultRequestHeaders.UserAgent.ToString();
                    break;
                case "OpenBrowser":
                    _ = Frame.Navigate(typeof(BrowserPage), new BrowserViewModel(URLTextBox.Text));
                    break;
                case "GetURLContent":
                    GetURLContent();
                    break;
            }
        }

        private void ComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is ComboBox ComboBox && ComboBox.Tag?.ToString() == "Language")
            {
                string lang = SettingsHelper.Get<string>(SettingsHelper.CurrentLanguage);
                lang = lang == LanguageHelper.AutoLanguageCode ? LanguageHelper.GetCurrentLanguage() : lang;
                ComboBox.SelectedItem = new CultureInfo(lang);
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox ComboBox && ComboBox.Tag?.ToString() == "Language")
            {
                if (ComboBox.SelectedItem is CultureInfo culture)
                {
                    if (culture.Name != LanguageHelper.GetCurrentLanguage())
                    {
                        ApplicationLanguages.PrimaryLanguageOverride = culture.Name;
                        SettingsHelper.Set(SettingsHelper.CurrentLanguage, culture.Name);
                    }
                    else
                    {
                        ApplicationLanguages.PrimaryLanguageOverride = string.Empty;
                        SettingsHelper.Set(SettingsHelper.CurrentLanguage, LanguageHelper.AutoLanguageCode);
                    }
                }
            }
        }

        private async void GetURLContent()
        {
            Uri uri = URLTextBox.Text.ValidateAndGetUri();
            (bool isSucceed, string result) = await RequestHelper.GetStringAsync(uri, "XMLHttpRequest");
            if (!isSucceed)
                result = "网络错误";

            var dialog = new ContentDialog
            {
                Title = URLTextBox.Text,
                Content = new ScrollViewer
                {
                    Content = new TextBlock
                    {
                        Text = $"```json\n{result.ConvertJsonString()}\n```",
                        TextWrapping = TextWrapping.Wrap,
                        IsTextSelectionEnabled = true
                    },
                    VerticalScrollMode = ScrollMode.Enabled,
                    HorizontalScrollMode = ScrollMode.Enabled,
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                    HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                },
                CloseButtonText = "好的",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.Content.XamlRoot
            };
            _ = await dialog.ShowAsync();
        }
    }
}
