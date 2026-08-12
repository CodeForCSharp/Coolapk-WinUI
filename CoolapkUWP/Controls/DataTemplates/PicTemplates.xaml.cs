using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Images;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;

namespace CoolapkUWP.Controls.DataTemplates
{
    public partial class PicTemplates : ResourceDictionary
    {
        public PicTemplates() => InitializeComponent();

        public void Image_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            _ = element.ShowImageAsync(element.Tag as ImageModel);
        }

        public void Image_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter || e.Key == Windows.System.VirtualKey.Space)
            {
                Image_Tapped(sender, null);
            }
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            ImageActions.HandleAppBarButtonClick(sender as FrameworkElement);
        }

        private async void Border_DragStarting(UIElement sender, DragStartingEventArgs args)
        {
            args.DragUI.SetContentFromDataPackage();
            args.Data.RequestedOperation = DataPackageOperation.Copy;
            await ImageActions.GetImageDataPackageAsync(args.Data, (sender as FrameworkElement).Tag as ImageModel, "拖拽图片");
        }
    }
}