using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Windows.Foundation;

namespace CoolapkUWP.Common
{
    /// <summary>
    /// 应用主窗口上下文,隔离模型层对 App.MainWindow 的直接依赖。
    /// </summary>
    public static class WindowContext
    {
        public static DispatcherQueue DispatcherQueue
            => App.MainWindow?.DispatcherQueue ?? DispatcherQueue.GetForCurrentThread();

        public static Rect Bounds
            => App.MainWindow?.Bounds ?? Rect.Empty;
    }
}
