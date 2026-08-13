using CoolapkUWP.Pages;
using CommunityToolkit.WinUI;

namespace CoolapkUWP.Helpers
{
    /// <summary>
    /// 主窗口底部进度条的外观控制。所有操作都会投递到 UI 线程执行。
    /// </summary>
    internal static class ProgressBarHelper
    {
        public static bool IsShowingProgressBar;

        public static void ShowProgressBar()
        {
            MainPage mainPage = App.MainPage;
            _ = mainPage?.DispatcherQueue.EnqueueAsync(() =>
            {
                IsShowingProgressBar = true;
                mainPage?.ShowProgressBar();
            });
        }

        public static void ShowProgressBar(double value)
        {
            MainPage mainPage = App.MainPage;
            _ = mainPage?.DispatcherQueue.EnqueueAsync(() =>
            {
                IsShowingProgressBar = true;
                mainPage?.ShowProgressBar(value);
            });
        }

        public static void PausedProgressBar()
        {
            MainPage mainPage = App.MainPage;
            _ = mainPage?.DispatcherQueue.EnqueueAsync(() =>
            {
                IsShowingProgressBar = true;
                mainPage?.PausedProgressBar();
            });
        }

        public static void ErrorProgressBar()
        {
            MainPage mainPage = App.MainPage;
            _ = mainPage?.DispatcherQueue.EnqueueAsync(() =>
            {
                IsShowingProgressBar = true;
                mainPage?.ErrorProgressBar();
            });
        }

        public static void HideProgressBar()
        {
            MainPage mainPage = App.MainPage;
            _ = mainPage?.DispatcherQueue.EnqueueAsync(() =>
            {
                IsShowingProgressBar = false;
                mainPage?.HideProgressBar();
            });
        }
    }
}
