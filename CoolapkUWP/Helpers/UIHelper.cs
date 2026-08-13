using CommunityToolkit.WinUI;
using System;
using System.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;

namespace CoolapkUWP.Helpers
{
    /// <summary>
    /// 与视觉树相关的纯 UI 辅助方法（不依赖任何具体页面实例）。
    /// </summary>
    internal static class UIHelper
    {
        public static bool IsOriginSource(object source, object originalSource)
        {
            if (source == originalSource) { return true; }

            bool result = false;
            FrameworkElement DependencyObject = originalSource as FrameworkElement;
            if (DependencyObject.FindAscendant<ButtonBase>() == null && !(originalSource is ButtonBase) && !(originalSource is RichEditBox))
            {
                if (source is FrameworkElement FrameworkElement)
                {
                    result = FrameworkElement == DependencyObject.FindAscendant(FrameworkElement.Name);
                }
            }

            return DependencyObject.Tag == null && result;
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
