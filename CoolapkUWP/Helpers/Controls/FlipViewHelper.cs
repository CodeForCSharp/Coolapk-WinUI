using CoolapkUWP.Helpers;
using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace CoolapkUWP.Helpers.Controls
{
    /// <summary>
    /// FlipView 轮播与无图模式折叠的共享实现。
    /// </summary>
    internal static class FlipViewHelper
    {
        /// <summary>
        /// 每隔 20 秒自动翻页，翻到末尾后回到开头。
        /// </summary>
        public static void EnableAutoPlay(FlipView view)
        {
            view.MaxHeight = view.ActualWidth / 3;
            DispatcherTimer timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(20)
            };
            timer.Tick += (o, a) =>
            {
                if (view.SelectedIndex != -1)
                {
                    if (view.SelectedIndex + 1 >= view.Items.Count)
                    {
                        while (view.SelectedIndex > 0)
                        {
                            view.SelectedIndex -= 1;
                        }
                    }
                    else
                    {
                        view.SelectedIndex += 1;
                    }
                }
            };
            view.Unloaded += (_, __) => timer.Stop();
            timer.Start();
        }

        /// <summary>
        /// 无图模式下折叠 FlipView 的父容器。
        /// </summary>
        public static void CollapseParentIfNoPics(FrameworkElement element)
        {
            if (SettingsHelper.Get<bool>(SettingsHelper.IsNoPicsMode) && element.Parent is FrameworkElement parent)
            {
                parent.Visibility = Visibility.Collapsed;
            }
        }
    }
}
