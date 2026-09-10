using CommunityToolkit.WinUI;
using System;
using System.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;

namespace CoolapkUWP.Helpers
{
    /// <summary>
    /// 与视觉树相关的纯 UI 辅助方法（不依赖任何具体页面实例）。
    /// </summary>
    internal static class UIHelper
    {
        /// <summary>
        /// 判断点击是否来自 <paramref name="source"/> 内部，供卡片/列表项的 Tapped 处理器过滤无效来源。
        /// 按钮、富文本编辑框等自己处理输入的控件返回 false。
        /// </summary>
        public static bool IsOriginSource(object source, object originalSource)
        {
            if (source == originalSource) { return true; }
            if (source is not UIElement sourceElement || originalSource is not UIElement originalElement) { return false; }

            if (originalElement is ButtonBase || originalElement is RichEditBox
                || originalElement.FindAscendant<ButtonBase>() != null
                || originalElement.FindAscendant<RichEditBox>() != null) { return false; }

            // 命中元素自身带 Tag，说明它是子级里自带跳转目标的元素（例如九宫格图片），
            // 由它自己的处理器负责，外层卡片不再重复响应。
            if (originalElement is FrameworkElement { Tag: not null }) { return false; }

            // 沿可视化树向上查找，确认命中元素确实位于 source 内部。
            // 不能再用 FindAscendant(source.Name)：未设置 x:Name 的容器 Name 为空，
            // 会匹配到最近的匿名父级而不是 source 本身，导致深层子元素点击失效。
            for (DependencyObject current = originalElement; current != null; current = current is UIElement ? VisualTreeHelper.GetParent(current) : null)
            {
                if (current == sourceElement) { return true; }
            }

            return false;
        }

        public static string ExceptionToMessage(this Exception ex)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append('\n');
            if (!string.IsNullOrWhiteSpace(ex.Message)) { builder.AppendLine($"Message: {ex.Message}"); }
            builder.AppendLine($"HResult: {ex.HResult} (0x{Convert.ToString(ex.HResult, 16)})");
            if (!string.IsNullOrWhiteSpace(ex.StackTrace)) { builder.AppendLine(ex.StackTrace); }
            if (!string.IsNullOrWhiteSpace(ex.HelpLink)) { builder.Append($"HelperLink: {ex.HelpLink}"); }
            return builder.ToString();
        }
    }
}
