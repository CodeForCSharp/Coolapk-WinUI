using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Images;
using CoolapkUWP.ViewModels;
using Microsoft.UI.Xaml.Controls;

using System.ComponentModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.System.Profile;
using PointerPoint = Microsoft.UI.Input.PointerPoint;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace CoolapkUWP.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class ShowImagePage : Page, INotifyPropertyChanged
    {
        private Point _clickPoint = new Point(0, 0);
        private ShowImageViewModel _provider;
        public ShowImageViewModel Provider
        {
            get => _provider;
            private set
            {
                if (_provider != value)
                {
                    _provider = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        public ShowImagePage() => InitializeComponent();

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is ImageModel Model)
            {
                Provider = new ShowImageViewModel(Model, DispatcherQueue);
            }
            else if (e.Parameter is ShowImageViewModel ViewModel)
            {
                Provider = ViewModel;
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            App.MainWindow?.SetTitleBar(null);
            var TitleBar = App.MainWindow.AppWindow.TitleBar;
            Frame.Navigated -= On_Navigated;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            App.MainWindow.SetTitleBar(CustomTitleBar);
            var TitleBar = App.MainWindow.AppWindow.TitleBar;
            if (!(AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Desktop"))
            { UpdateContentLayout(TitleBar); }
            Frame.Navigated += On_Navigated;
            UpdateTitleBarLayout(TitleBar);
        }

        private void On_Navigated(object sender, NavigationEventArgs e)
        {

        }

        private bool TryGoBack(bool goBack = true)
        {
            if (DispatcherQueue == null || !DispatcherQueue.HasThreadAccess) { return false; }

            Frame frame = Frame ?? (App.MainWindow.Content as Frame);
            if (frame == null || !frame.CanGoBack) { return false; }

            if (goBack) { frame.GoBack(); }
            return true;
        }

        private void UpdateContentLayout(Microsoft.UI.Windowing.AppWindowTitleBar TitleBar)
        {
            CustomTitleBar.Visibility = Visibility.Visible;
            FlipViewGrid.Margin = new Thickness(0, TitleBar.Height, 0, 0);
        }

        private void UpdateTitleBarLayout(Microsoft.UI.Windowing.AppWindowTitleBar TitleBar)
        {
            LeftPaddingColumn.Width = new GridLength(TitleBar.LeftInset);
            RightPaddingColumn.Width = new GridLength(TitleBar.RightInset);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            ScrollViewer scrollViewer = element.Tag as ScrollViewer;
            switch (element.Name)
            {
                case "ZoomUp":
                    _ = scrollViewer.ChangeView(null, null, scrollViewer.ZoomFactor + 0.1f);
                    break;
                case "ZoomDown":
                    _ = scrollViewer.ChangeView(null, null, scrollViewer.ZoomFactor - 0.1f);
                    break;
            }
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as FrameworkElement).Tag as string)
            {
                case "Back":
                    TryGoBack();
                    break;
                case "Copy":
                    Provider.CopyPic();
                    break;
                case "Save":
                    Provider.SavePic();
                    break;
                case "Share":
                    Provider.SharePic();
                    break;
                case "Refresh":
                    _ = Provider.Refresh();
                    break;
                case "Origin":
                    Provider.Images[Provider.Index].Type &= (ImageType)0xFE;
                    Provider.ShowOrigin = false;
                    break;
            }
        }

        private void ScrollViewer_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Provider.IsShowHub = !Provider.IsShowHub;
        }

        private void ScrollViewer_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            ScrollViewer scrollViewer = sender as ScrollViewer;
            scrollViewer.ChangeView(0, 0, 1);
            Provider.IsShowHub = !Provider.IsShowHub;
        }

        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            ScrollViewer scrollViewer = sender as ScrollViewer;
            FrameworkElement element = scrollViewer.Content as FrameworkElement;
            element.CanDrag = scrollViewer.ZoomFactor <= 1;
        }

        private async void Image_DragStarting(UIElement sender, DragStartingEventArgs args)
        {
            args.DragUI.SetContentFromDataPackage();
            args.Data.RequestedOperation = DataPackageOperation.Copy;
            await Provider.GetImageDataPackage(args.Data, "拖拽图片");
        }

        private void Image_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            PointerPoint pointerPoint = e.GetCurrentPoint(element);
            if (pointerPoint.Properties.IsLeftButtonPressed)
            {
                _clickPoint = e.GetCurrentPoint(element).Position;
            }
        }

        private void Image_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            ScrollViewer scrollViewer = element.Parent as ScrollViewer;
            PointerPoint pointerPoint = e.GetCurrentPoint(element);
            if (pointerPoint.Properties.IsLeftButtonPressed)
            {
                double x, y;
                Point point = e.GetCurrentPoint(element).Position;
                x = _clickPoint.X - point.X;
                y = _clickPoint.Y - point.Y;
                _ = scrollViewer.ChangeView(scrollViewer.HorizontalOffset + x, scrollViewer.VerticalOffset + y, null);
            }
        }

        private void TitleBar_IsVisibleChanged(Microsoft.UI.Windowing.AppWindowTitleBar sender, object args) => UpdateContentLayout(sender);

        private void TitleBar_LayoutMetricsChanged(Microsoft.UI.Windowing.AppWindowTitleBar sender, object args) => UpdateTitleBarLayout(sender);

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }
    }
}
