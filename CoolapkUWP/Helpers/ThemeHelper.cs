using CoolapkUWP.Common;
using CommunityToolkit.WinUI;
using CommunityToolkit.WinUI.Helpers;
using Microsoft.Win32;
using Windows.UI.ViewManagement;
using Microsoft.Extensions.Logging;
using System;
using Microsoft.UI;
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
                if (CurrentApplicationWindow == null || CurrentApplicationWindow.DispatcherQueue.HasThreadAccess)
                {
                    return CurrentApplicationWindow?.Content is FrameworkElement rootElement
                        && rootElement.RequestedTheme != ElementTheme.Default
                            ? rootElement.RequestedTheme
                            : SettingsHelper.Get<ElementTheme>(SettingsHelper.SelectedAppTheme);
                }
                return SettingsHelper.Get<ElementTheme>(SettingsHelper.SelectedAppTheme);
            }
        }

        public static ElementTheme RootTheme
        {
            get
            {
                if (CurrentApplicationWindow == null) { return ElementTheme.Default; }
                if (CurrentApplicationWindow.DispatcherQueue.HasThreadAccess)
                {
                    return CurrentApplicationWindow.Content is FrameworkElement rootElement
                        ? rootElement.RequestedTheme
                        : ElementTheme.Default;
                }
                return SettingsHelper.Get<ElementTheme>(SettingsHelper.SelectedAppTheme);
            }
            set
            {
                if (CurrentApplicationWindow == null) { return; }

                _ = CurrentApplicationWindow.DispatcherQueue.EnqueueAsync(() =>
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

        public static bool IsDarkTheme() => IsDarkTheme(SettingsHelper.Get<ElementTheme>(SettingsHelper.SelectedAppTheme));

        public static bool IsDarkTheme(ElementTheme theme)
        {
            if (theme != ElementTheme.Default) { return theme == ElementTheme.Dark; }
            return IsSystemDarkTheme();
        }

        private static bool IsSystemDarkTheme()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
                return key?.GetValue("AppsUseLightTheme") is int v && v == 0;
            }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.CreateLogger(nameof(ThemeHelper)).LogDebug(ex, ex.ExceptionToMessage());
                return false;
            }
        }

        public static void UpdateSystemCaptionButtonColors()
        {
            bool isDark = IsDarkTheme();
            bool isHighContrast = IsHighContrast();
            Window window = CurrentApplicationWindow;
            _ = window?.DispatcherQueue.EnqueueAsync(() => ApplyCaptionButtonColors(window, isDark, isHighContrast));
        }

        public static void UpdateSystemCaptionButtonColors(Window window)
        {
            _ = window?.DispatcherQueue.EnqueueAsync(() =>
            {
                bool isDark = window.Content is FrameworkElement rootElement
                    ? IsDarkTheme(rootElement.RequestedTheme)
                    : IsDarkTheme();
                ApplyCaptionButtonColors(window, isDark, IsHighContrast());
            });
        }

        private static bool IsHighContrast()
        {
            try { return new AccessibilitySettings().HighContrast; }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.CreateLogger(nameof(ThemeHelper)).LogDebug(ex, ex.ExceptionToMessage());
                return false;
            }
        }

        private static void ApplyCaptionButtonColors(Window window, bool isDark, bool isHighContrast)
        {
            if (window == null) { return; }

            var foregroundColor = isDark || isHighContrast ? Colors.White : Colors.Black;
            var backgroundColor = isHighContrast
                ? Microsoft.UI.ColorHelper.FromArgb(255, 0, 0, 0)
                : isDark ? Microsoft.UI.ColorHelper.FromArgb(255, 32, 32, 32) : Microsoft.UI.ColorHelper.FromArgb(255, 243, 243, 243);

            bool extendViewIntoTitleBar = window.ExtendsContentIntoTitleBar;
            AppWindowTitleBar titleBar = window.AppWindow.TitleBar;
            titleBar.ForegroundColor = titleBar.ButtonForegroundColor = foregroundColor;
            titleBar.BackgroundColor = titleBar.InactiveBackgroundColor = backgroundColor;
            titleBar.ButtonBackgroundColor = titleBar.ButtonInactiveBackgroundColor = extendViewIntoTitleBar ? Colors.Transparent : backgroundColor;
        }
    }

    public enum UISettingChangedType
    {
        LightMode,
        DarkMode,
        NoPicChanged,
    }
}
