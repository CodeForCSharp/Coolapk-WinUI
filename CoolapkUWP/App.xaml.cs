using CoolapkUWP.Controls;
using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Exceptions;
using CoolapkUWP.Pages;
using CommunityToolkit.WinUI.Helpers;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace CoolapkUWP
{
    [ComImport]
    [Guid("3E68D4BD-7135-4D10-8018-9FB6D9F33FA1")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IInitializeWithWindow
    {
        void Initialize(IntPtr hwnd);
    }

    public sealed partial class App : Application
    {
        [DllImport("user32.dll")]
        private static extern bool SetProcessDpiAwarenessContext(int dpiFlag);

        private const int DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2 = -4;

        internal static nint WindowHandle { get; private set; }
        internal static Window MainWindow { get; private set; }

        static App()
        {
            SetProcessDpiAwarenessContext(DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2);
        }

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

            RequestWIFIAccess();
        }

        private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        private static void RegisterExceptionHandlingSynchronizationContext(Frame rootFrame)
        {
            ExceptionHandlingSynchronizationContext synchronizatioCcontext = ExceptionHandlingSynchronizationContext.RegisterForFrame(rootFrame);
            synchronizatioCcontext.UnhandledException += OnSynchronizationContextUnhandledException;
        }

        private static void OnSynchronizationContextUnhandledException(object sender, CoolapkUWP.Helpers.UnhandledExceptionEventArgs args)
        {
            args.Handled = true;
        }

        private async void RequestWIFIAccess()
        {
            try
            {
                var WIFIData = Windows.Security.Authorization.AppCapabilityAccess.AppCapability.Create("wifiData");
                switch (WIFIData.CheckAccess())
                {
                    case Windows.Security.Authorization.AppCapabilityAccess.AppCapabilityAccessStatus.DeniedByUser:
                    case Windows.Security.Authorization.AppCapabilityAccess.AppCapabilityAccessStatus.DeniedBySystem:
                        await WIFIData.RequestAccessAsync();
                        break;
                }
            }
            catch { }
        }

        private void Application_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            SettingsHelper.LogManager.GetLogger(nameof(App)).Fatal(e.Exception.ExceptionToMessage(), e.Exception);
            e.Handled = true;
        }
    }
}
