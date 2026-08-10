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
            FrameworkElement element = sender as FrameworkElement;
            ImageModel image = element.Tag as ImageModel;
            switch (element.Name)
            {
                case "CopyButton":
                    _ = ImageActions.CopyPicAsync(image);
                    break;
                case "SaveButton":
                    _ = ImageActions.SavePicAsync(image);
                    break;
                case "ShareButton":
                    _ = ImageActions.SharePicAsync(image);
                    break;
                case "RefreshButton":
                    _ = image.Refresh();
                    break;
                case "ShowImageButton":
                    _ = element.ShowImageAsync(image);
                    break;
                case "OriginButton":
                    image.Type = ImageType.OriginImage;
                    break;
            }
        }

        private async void Border_DragStarting(UIElement sender, DragStartingEventArgs args)
        {
            args.DragUI.SetContentFromDataPackage();
            args.Data.RequestedOperation = DataPackageOperation.Copy;
            await ImageActions.GetImageDataPackageAsync(args.Data, (sender as FrameworkElement).Tag as ImageModel, "拖拽图片");
        }
    }
}