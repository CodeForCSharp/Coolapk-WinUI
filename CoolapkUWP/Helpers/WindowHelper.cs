using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace CoolapkUWP.Helpers
{
    public static class WindowHelper
    {
        public static bool IsSupported { get; } = false;

        public static bool IsAppWindow(this UIElement element) => false;

        public static Task<(object, Frame)> CreateWindow()
        {
            throw new NotSupportedException("Multi-window is not supported in WinUI 3");
        }

        public static object GetWindowForElement(this UIElement element) => null;

        public static void SetXAMLRoot(this UIElement element, UIElement target) { }

        public static Dictionary<UIElement, object> ActiveWindows { get; } = null;
    }
}
