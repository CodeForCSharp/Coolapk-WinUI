using CoolapkUWP.Controls;
using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Exceptions;
using CoolapkUWP.Pages;
using CommunityToolkit.WinUI.Helpers;
using Microsoft.Extensions.Logging;
using System.Text.Json.Nodes;
using System;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using Windows.ApplicationModel.Resources;
using Windows.System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System.Runtime.InteropServices.Marshalling;

namespace CoolapkUWP
{
    [GeneratedComInterface]
    [Guid("3E68D4BD-7135-4D10-8018-9FB6D9F33FA1")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal partial interface IInitializeWithWindow
    {
        void Initialize(IntPtr hwnd);
    }

    public sealed partial class App : Application
    {
        internal static nint WindowHandle { get; private set; }
        internal static Window MainWindow { get; private set; }

        public App()
        {
            InitializeComponent();
            UnhandledException += Application_UnhandledException;
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            MainWindow = new Window();
            WindowHandle = WinRT.Interop.WindowNative.GetWindowHandle(MainWindow);

            var rootFrame = new Frame();

            RegisterExceptionHandlingSynchronizationContext(rootFrame);

            MainWindow.ExtendsContentIntoTitleBar = true;

            rootFrame.NavigationFailed += OnNavigationFailed;

            MainWindow.Content = rootFrame;

            ThemeHelper.Initialize();

            if (rootFrame.Content == null)
            {
                rootFrame.Navigate(typeof(MainPage), args);
            }

            MainWindow.Activate();
        }

        private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        private static void RegisterExceptionHandlingSynchronizationContext(Frame rootFrame)
        {
            ExceptionHandlingSynchronizationContext synchronizationContext = ExceptionHandlingSynchronizationContext.RegisterForFrame(rootFrame);
            synchronizationContext.UnhandledException += OnSynchronizationContextUnhandledException;
        }

        private static void OnSynchronizationContextUnhandledException(object sender, CoolapkUWP.Helpers.UnhandledExceptionEventArgs args)
        {
            if (args.Exception != null)
            {
                SettingsHelper.LogManager.CreateLogger(nameof(App)).LogCritical(args.Exception, args.Exception.ExceptionToMessage());
            }
            args.Handled = true;
        }

        private void Application_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            SettingsHelper.LogManager.CreateLogger(nameof(App)).LogCritical(e.Exception, e.Exception.ExceptionToMessage());

            // 致命异常不应被吞掉，让应用终止而不是在未知状态下继续运行
            if (e.Exception is OutOfMemoryException or AccessViolationException or Microsoft.UI.Xaml.Markup.XamlParseException)
            {
                e.Handled = false;
                return;
            }

            e.Handled = true;
        }
    }
}
