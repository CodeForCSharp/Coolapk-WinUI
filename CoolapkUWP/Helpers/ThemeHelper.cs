using CoolapkUWP.Common;
using CommunityToolkit.WinUI.Helpers;
using Microsoft.Win32;
using Windows.UI.ViewManagement;
using Microsoft.UI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Windowing;

namespace CoolapkUWP.Helpers
{
    public static class ThemeHelper
    {
        private static Window CurrentApplicationWindow;

        public static WeakEvent<UISettingChangedType> UISettingChanged { get; } = new WeakEvent<UISettingChangedType>();

        public static ElementTheme ActualTheme
        {
            get
            {
                return CurrentApplicationWindow == null
                    ? SettingsHelper.Get<ElementTheme>(SettingsHelper.SelectedAppTheme)
                    : CurrentApplicationWindow.DispatcherQueue.HasThreadAccess
                        ? CurrentApplicationWindow.Content is FrameworkElement rootElement
                            && rootElement.RequestedTheme != ElementTheme.Default
                                ? rootElement.RequestedTheme
                                : SettingsHelper.Get<ElementTheme>(SettingsHelper.SelectedAppTheme)
                        : UIHelper.AwaitByTaskCompleteSource(() =>
                            CurrentApplicationWindow.DispatcherQueue.AwaitableRunAsync(() =>
                                CurrentApplicationWindow.Content is FrameworkElement _rootElement
                                    && _rootElement.RequestedTheme != ElementTheme.Default
                                        ? _rootElement.RequestedTheme
                                        : SettingsHelper.Get<ElementTheme>(SettingsHelper.SelectedAppTheme),
                                DispatcherQueuePriority.High));
            }
        }

        public static ElementTheme RootTheme
        {
            get
            {
                return CurrentApplicationWindow == null
                    ? ElementTheme.Default
                    : CurrentApplicationWindow.DispatcherQueue.HasThreadAccess
                        ? CurrentApplicationWindow.Content is FrameworkElement rootElement
                            ? rootElement.RequestedTheme
                            : ElementTheme.Default
                        : UIHelper.AwaitByTaskCompleteSource(() =>
                            CurrentApplicationWindow.DispatcherQueue.AwaitableRunAsync(() =>
                                CurrentApplicationWindow.Content is FrameworkElement _rootElement
                                    ? _rootElement.RequestedTheme
                                    : ElementTheme.Default,
                                DispatcherQueuePriority.High));
            }
            set
            {
                if (CurrentApplicationWindow == null) { return; }

                _ = CurrentApplicationWindow.DispatcherQueue.AwaitableRunAsync(() =>
                {
                    if (CurrentApplicationWindow.Content is FrameworkElement rootElement)
                    {
                        rootElement.RequestedTheme = value;
                    }
                });

                SettingsHelper.Set(SettingsHelper.SelectedAppTheme, value);
                UpdateSystemCaptionButtonColors();
                UISettingChanged.Invoke(IsDarkTheme() ? UISettingChangedType.DarkMode : UISettingChangedType.LightMode);
            }
        }

        public static void Initialize()
        {
            CurrentApplicationWindow = App.MainWindow;
            RootTheme = SettingsHelper.Get<ElementTheme>(SettingsHelper.SelectedAppTheme);

            SystemEvents.UserPreferenceChanged += (_, e) =>
            {
                if (e.Category == UserPreferenceCategory.Color)
                {
                    UpdateSystemCaptionButtonColors();
                    UISettingChanged.Invoke(IsDarkTheme() ? UISettingChangedType.DarkMode : UISettingChangedType.LightMode);
                }
            };
        }

        public static void Initialize(Window window)
        {
            if (window?.Content is FrameworkElement rootElement)
            {
                rootElement.RequestedTheme = ActualTheme;
            }
            UpdateSystemCaptionButtonColors(window);
        }

        public static bool IsDarkTheme()
        {
            var theme = SettingsHelper.Get<ElementTheme>(SettingsHelper.SelectedAppTheme);
            if (theme != ElementTheme.Default) { return theme == ElementTheme.Dark; }

            try
            {
                using var key = Microsoft.Win32.Registry.CurrentUser
                    .OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
                return key?.GetValue("AppsUseLightTheme") is int v && v == 0;
            }
            catch { return false; }
        }

        public static bool IsDarkTheme(ElementTheme ActualTheme)
        {
            if (ActualTheme != ElementTheme.Default) { return ActualTheme == ElementTheme.Dark; }

            try
            {
                using var key = Microsoft.Win32.Registry.CurrentUser
                    .OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
                return key?.GetValue("AppsUseLightTheme") is int v && v == 0;
            }
            catch { return false; }
        }

        public static async void UpdateSystemCaptionButtonColors()
        {
            bool IsDark = IsDarkTheme();
            bool IsHighContrast = false;
            try { IsHighContrast = new AccessibilitySettings().HighContrast; } catch { }
    
            var ForegroundColor = IsDark || IsHighContrast ? Colors.White : Colors.Black;
            var BackgroundColor = IsHighContrast ? Microsoft.UI.ColorHelper.FromArgb(255, 0, 0, 0) : IsDark ? Microsoft.UI.ColorHelper.FromArgb(255, 32, 32, 32) : Microsoft.UI.ColorHelper.FromArgb(255, 243, 243, 243);

            await (CurrentApplicationWindow?.DispatcherQueue).ResumeForegroundAsync();

            if (CurrentApplicationWindow != null)
            {
                bool ExtendViewIntoTitleBar = CurrentApplicationWindow.ExtendsContentIntoTitleBar;
                AppWindowTitleBar TitleBar = CurrentApplicationWindow.AppWindow.TitleBar;
                TitleBar.ForegroundColor = TitleBar.ButtonForegroundColor = ForegroundColor;
                TitleBar.BackgroundColor = TitleBar.InactiveBackgroundColor = BackgroundColor;
                TitleBar.ButtonBackgroundColor = TitleBar.ButtonInactiveBackgroundColor = ExtendViewIntoTitleBar ? Colors.Transparent : BackgroundColor;
            }
        }

        public static async void UpdateSystemCaptionButtonColors(Window window)
        {
            await window.DispatcherQueue.ResumeForegroundAsync();

            bool IsDark = window?.Content is FrameworkElement rootElement ? IsDarkTheme(rootElement.RequestedTheme) : IsDarkTheme();
            bool IsHighContrast = false;
            try { IsHighContrast = new AccessibilitySettings().HighContrast; } catch { }
    
            var ForegroundColor = IsDark || IsHighContrast ? Colors.White : Colors.Black;
            var BackgroundColor = IsHighContrast ? Microsoft.UI.ColorHelper.FromArgb(255, 0, 0, 0) : IsDark ? Microsoft.UI.ColorHelper.FromArgb(255, 32, 32, 32) : Microsoft.UI.ColorHelper.FromArgb(255, 243, 243, 243);

            if (window != null)
            {
                bool ExtendViewIntoTitleBar = window.ExtendsContentIntoTitleBar;
                AppWindowTitleBar TitleBar = window.AppWindow.TitleBar;
                TitleBar.ForegroundColor = TitleBar.ButtonForegroundColor = ForegroundColor;
                TitleBar.BackgroundColor = TitleBar.InactiveBackgroundColor = BackgroundColor;
                TitleBar.ButtonBackgroundColor = TitleBar.ButtonInactiveBackgroundColor = ExtendViewIntoTitleBar ? Colors.Transparent : BackgroundColor;
            }
        }
    }

    public enum UISettingChangedType
    {
        LightMode,
        DarkMode,
        NoPicChanged,
    }
}
