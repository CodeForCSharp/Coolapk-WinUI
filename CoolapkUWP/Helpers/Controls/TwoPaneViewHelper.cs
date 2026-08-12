using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
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
        /// 将头部类控件移到右侧(单窗格)或左侧(双窗格)，同时从其当前父面板移除。
        /// </summary>
        public static void UpdateHeaderPane(FrameworkElement control, Grid leftGrid, Grid rightGrid, TwoPaneViewMode mode)
        {
            if (control.Parent is Panel parent)
            {
                parent.Children.Remove(control);
            }
            else
            {
                leftGrid.Children.Remove(control);
                rightGrid.Children.Remove(control);
            }

            if (mode == TwoPaneViewMode.SinglePane)
            {
                rightGrid.Children.Add(control);
            }
            else
            {
                leftGrid.Children.Add(control);
            }
        }

        /// <summary>
        /// 将详情类控件移到 Pane2(单窗格)或 Pane1(双窗格)，同时从其当前父面板移除。
        /// </summary>
        public static void UpdateDetailPane(FrameworkElement control, Grid pane1Grid, Grid pane2Grid, TwoPaneViewMode mode)
        {
            if (control.Parent is Panel parent)
            {
                parent.Children.Remove(control);
            }
            else
            {
                pane1Grid.Children.Remove(control);
                pane2Grid.Children.Remove(control);
            }

            if (mode == TwoPaneViewMode.SinglePane)
            {
                pane2Grid.Children.Add(control);
            }
            else
            {
                pane1Grid.Children.Add(control);
            }
        }
    }
}
