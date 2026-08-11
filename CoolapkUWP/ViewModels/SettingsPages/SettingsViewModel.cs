using CoolapkUWP.Common;
using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Update;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using System;
using CommunityToolkit.WinUI.Helpers;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.System.Profile;
using Microsoft.UI.Xaml;

namespace CoolapkUWP.ViewModels.SettingsPages
{
    public partial class SettingsViewModel : ObservableObject, IViewModel
    {
        public static SettingsViewModel Caches { get; set; }

        private readonly ResourceLoader _loader = ResourceLoader.GetForViewIndependentUse("SettingsPage");

        public string Title => _loader.GetString("Title");

        public static string DeviceFamily => AnalyticsInfo.VersionInfo.DeviceFamily.Replace('.', ' ');

        public static string ToolkitVersion => typeof(ThemeHelper).Assembly.GetName().Version?.ToString() ?? "1.0.0";

        public bool IsLogin
        {
            get => !string.IsNullOrEmpty(SettingsHelper.Get<string>(SettingsHelper.Uid));
            set => OnPropertyChanged();
        }

        public DateTime UpdateDate
        {
            get => SettingsHelper.Get<DateTime>(SettingsHelper.UpdateDate);
            set
            {
                if (UpdateDate != value)
                {
                    SettingsHelper.Set(SettingsHelper.UpdateDate, value);
                    OnPropertyChanged();
                }
            }
        }

        public int SelectedTheme
        {
            get => 2 - (int)ThemeHelper.ActualTheme;
            set
            {
                if (SelectedTheme != value)
                {
                    ThemeHelper.RootTheme = (ElementTheme)(2 - value);
                    OnPropertyChanged();
                }
            }
        }

        public bool IsNoPicsMode
        {
            get => SettingsHelper.Get<bool>(SettingsHelper.IsNoPicsMode);
            set
            {
                if (IsNoPicsMode != value)
                {
                    SettingsHelper.Set(SettingsHelper.IsNoPicsMode, value);
                    ThemeHelper.UISettingChanged?.Invoke(UISettingChangedType.NoPicChanged);
                    OnPropertyChanged();
                }
            }
        }

        public bool IsDisplayOriginPicture
        {
            get => SettingsHelper.Get<bool>(SettingsHelper.IsDisplayOriginPicture);
            set
            {
                if (IsDisplayOriginPicture != value)
                {
                    SettingsHelper.Set(SettingsHelper.IsDisplayOriginPicture, value);
                    OnPropertyChanged();
                }
            }
        }

        [ObservableProperty]
        public partial bool IsCleanCache { get; set; }

        [ObservableProperty]
        public partial bool CheckingUpdate { get; set; }

        [ObservableProperty]
        public partial string GotoUpdateTag { get; set; }

        [ObservableProperty]
        public partial Visibility GotoUpdateVisibility { get; set; }

        [ObservableProperty]
        public partial bool UpdateStateIsOpen { get; set; }

        [ObservableProperty]
        public partial string UpdateStateMessage { get; set; }

        [ObservableProperty]
        public partial InfoBarSeverity UpdateStateSeverity { get; set; }

        [ObservableProperty]
        public partial string UpdateStateTitle { get; set; }

        [ObservableProperty]
        public partial string AboutTextBlockText { get; set; }

        public string VersionTextBlockText
        {
            get
            {
                string ver = $"{Package.Current.Id.Version.Major}.{Package.Current.Id.Version.Minor}.{Package.Current.Id.Version.Build}";
                string name = ResourceLoader.GetForViewIndependentUse().GetString("AppName") ?? "酷安";
                _ = GetAboutTextBlockText();
                return $"{name} v{ver}";
            }
        }

        public SettingsViewModel()
        {
            Caches = this;
            SettingsHelper.LoginChanged += (sender, args) => IsLogin = args;
        }

        private async Task GetAboutTextBlockText()
        {
            string langCode = LanguageHelper.GetPrimaryLanguage();
            Uri dataUri = new Uri($"ms-appx:///Assets/About/About.{langCode}.md");
            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(dataUri);
            if (file != null)
            {
                string markdown = await FileIO.ReadTextAsync(file);
                AboutTextBlockText = markdown;
            }
        }

        public async void CleanCache()
        {
            IsCleanCache = true;
            await ImageCacheHelper.CleanCacheAsync();
            IsCleanCache = false;
        }

        public async void CheckUpdate()
        {
            CheckingUpdate = true;
            UpdateInfo info = null;
            try
            {
                info = await UpdateHelper.CheckUpdateAsync("Coolapk-UWP", "Coolapk-UWP");
            }
            catch (Exception ex)
            {
                UpdateStateIsOpen = true;
                UpdateStateMessage = ex.Message;
                UpdateStateSeverity = InfoBarSeverity.Error;
                GotoUpdateVisibility = Visibility.Collapsed;
                UpdateStateTitle = _loader.GetString("CheckFailed");
                SettingsHelper.LogManager.CreateLogger(nameof(SettingsViewModel)).LogError(ex, ex.ExceptionToMessage());
            }
            if (info != null)
            {
                if (info.IsExistNewVersion)
                {
                    UpdateStateIsOpen = true;
                    GotoUpdateTag = info.ReleaseUrl;
                    GotoUpdateVisibility = Visibility.Visible;
                    UpdateStateSeverity = InfoBarSeverity.Warning;
                    UpdateStateTitle = _loader.GetString("FindUpdate");
                    UpdateStateMessage = $"{VersionTextBlockText} -> {info.TagName}";
                }
                else
                {
                    UpdateStateIsOpen = true;
                    GotoUpdateVisibility = Visibility.Collapsed;
                    UpdateStateSeverity = InfoBarSeverity.Success;
                    UpdateStateTitle = _loader.GetString("UpToDate");
                }
            }
            UpdateDate = DateTime.Now;
            CheckingUpdate = false;
        }

        public Task Refresh(bool reset) => throw new NotImplementedException();

        bool IViewModel.IsEqual(IViewModel other) => Equals(other);
    }
}
