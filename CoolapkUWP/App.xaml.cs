using CoolapkUWP.Controls;
using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Exceptions;
using CoolapkUWP.Pages;
using CommunityToolkit.WinUI.Helpers;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Windows.ApplicationModel.Resources;
using Windows.System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace CoolapkUWP
{
    public sealed partial class App : Application
    {
        [DllImport("user32.dll")]
        private static extern bool SetProcessDpiAwarenessContext(int dpiFlag);

        private const int DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2 = -4;

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

            var rootFrame = new Frame();

            RegisterExceptionHandlingSynchronizationContext();

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

        private async void RequestWIFIAccess()
        {
            try
            {
                if (Windows.Foundation.Metadata.ApiInformation.IsMethodPresent(
                    "Windows.Security.Authorization.AppCapabilityAccess.AppCapability", "Create"))
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
            }
            catch { }
        }

        private void Application_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            if (!(!SettingsHelper.Get<bool>(SettingsHelper.ShowOtherException)
                || e.Exception is TaskCanceledException
                || e.Exception is OperationCanceledException))
            {
                ResourceLoader loader = ResourceLoader.GetForViewIndependentUse();
                UIHelper.ShowMessage(
                    $"{(string.IsNullOrEmpty(e.Exception.Message) ? loader.GetString("ExceptionThrown") : e.Exception.Message)} (0x{Convert.ToString(e.Exception.HResult, 16)})");
            }
            SettingsHelper.LogManager.GetLogger("Unhandled Exception - Application")
                .Error(e.Exception.ExceptionToMessage(), e.Exception);
            e.Handled = true;
        }

        private void RegisterExceptionHandlingSynchronizationContext()
        {
            ExceptionHandlingSynchronizationContext
                .Register()
                .UnhandledException += SynchronizationContext_UnhandledException;
        }

        private void SynchronizationContext_UnhandledException(object sender, Helpers.UnhandledExceptionEventArgs e)
        {
            if (!(e.Exception is TaskCanceledException) && !(e.Exception is OperationCanceledException))
            {
                ResourceLoader loader = ResourceLoader.GetForViewIndependentUse();
                if (e.Exception is HttpRequestException
                    || (e.Exception.HResult <= -2147012721 && e.Exception.HResult >= -2147012895))
                {
                    UIHelper.ShowMessage($"{loader.GetString("NetworkError")}(0x{Convert.ToString(e.Exception.HResult, 16)})");
                }
                else if (e.Exception is CoolapkMessageException)
                {
                    UIHelper.ShowMessage(e.Exception.Message);
                }
                else if (SettingsHelper.Get<bool>(SettingsHelper.ShowOtherException))
                {
                    UIHelper.ShowMessage(
                        $"{(string.IsNullOrEmpty(e.Exception.Message) ? loader.GetString("ExceptionThrown") : e.Exception.Message)} (0x{Convert.ToString(e.Exception.HResult, 16)})");
                }
            }
            SettingsHelper.LogManager.GetLogger("Unhandled Exception - SynchronizationContext")
                .Error(e.Exception.ExceptionToMessage(), e.Exception);
            e.Handled = true;
        }

        public static Window MainWindow { get; private set; }
    }
}
