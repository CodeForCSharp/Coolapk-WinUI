using CoolapkUWP.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using TwoPaneView = Microsoft.UI.Xaml.Controls.TwoPaneView;
using TwoPaneViewMode = Microsoft.UI.Xaml.Controls.TwoPaneViewMode;

namespace CoolapkUWP.Helpers.Controls
{
    /// <summary>
    /// 在 TwoPaneView 的窄/宽模式间搬移头部与详情控件。
    /// </summary>
    internal static class TwoPaneViewHelper
    {
        /// <summary>
        /// 将控件放置到目标面板；已在目标中时跳过（幂等），避免反复拔插导致布局抖动或控件悬空。
        /// </summary>
        private static void PlaceControl(FrameworkElement control, Grid leftGrid, Grid rightGrid, Panel target, string name)
        {
            if (control == null || ReferenceEquals(control.Parent, target)) { return; }

            (control.Parent as Panel)?.Children.Remove(control);
            // 兜底从两个已知宿主移除（Remove 对非子元素是安全的无操作）。
            leftGrid.Children.Remove(control);
            rightGrid.Children.Remove(control);

            try
            {
                target.Children.Add(control);
            }
            catch (Exception ex)
            {
                // Add 失败意味着控件已从原父级移除却未能挂载，会永久脱离可视树，必须记录。
                SettingsHelper.LogManager.CreateLogger(nameof(TwoPaneViewHelper)).LogError(ex, $"{nameof(PlaceControl)} failed: {name}, {ex.Message}");
            }
        }

        /// <summary>
        /// 将头部类控件移到右侧(单窗格)或左侧(双窗格)，同时从其当前父面板移除。
        /// </summary>
        public static void UpdateHeaderPane(FrameworkElement control, Grid leftGrid, Grid rightGrid, TwoPaneViewMode mode)
        {
            PlaceControl(control, leftGrid, rightGrid, mode == TwoPaneViewMode.SinglePane ? rightGrid : leftGrid, $"{control?.Name ?? "header"}");
        }

        /// <summary>
        /// 将详情类控件移到 Pane2(单窗格)或 Pane1(双窗格)，同时从其当前父面板移除。
        /// </summary>
        public static void UpdateDetailPane(FrameworkElement control, Grid pane1Grid, Grid pane2Grid, TwoPaneViewMode mode)
        {
            PlaceControl(control, pane1Grid, pane2Grid, mode == TwoPaneViewMode.SinglePane ? pane2Grid : pane1Grid, $"{control?.Name ?? "detail"}");
        }
    }
}
