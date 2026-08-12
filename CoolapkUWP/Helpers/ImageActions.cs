using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Images;
using CommunityToolkit.WinUI;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.Foundation;
using Microsoft.UI.Xaml;

namespace CoolapkUWP.Helpers
{
    internal static class ImageActions
    {
        private static readonly Regex FileNameRegex = new Regex(@"[^/]+(?!.*/)");

        /// <summary>
        /// 根据按钮 Name 分发图片操作（复制/保存/分享/刷新/查看原图/预览）。
        /// </summary>
        public static void HandleAppBarButtonClick(FrameworkElement element)
        {
            ImageModel image = element.Tag as ImageModel;
            switch (element.Name)
            {
                case "CopyButton":
                    _ = CopyPicAsync(image);
                    break;
                case "SaveButton":
                    _ = SavePicAsync(image);
                    break;
                case "ShareButton":
                    _ = SharePicAsync(image);
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

        public static async Task<StorageFile> GetOriginImageFileAsync(ImageModel image)
        {
            StorageFile file = await ImageCacheHelper.GetImageFileAsync(ImageType.OriginImage, image.Uri);
            if (file == null)
            {
                UIHelper.ShowMessage(ResourceLoader.GetForViewIndependentUse().GetString("ImageLoadError"));
            }
            return file;
        }

        public static async Task CopyPicAsync(ImageModel image)
        {
            DataPackage dataPackage = await GetImageDataPackageAsync(image, "复制图片");
            if (dataPackage != null) { Clipboard.SetContentWithOptions(dataPackage, null); }
        }

        public static async Task SharePicAsync(ImageModel image)
        {
            DataPackage dataPackage = await GetImageDataPackageAsync(image, "分享图片");
            if (dataPackage == null) { return; }

            DataTransferManager manager = DataTransferManager.GetForCurrentView();
            TypedEventHandler<DataTransferManager, DataRequestedEventArgs> handler = null;
            handler = (_, args) =>
            {
                args.Request.Data = dataPackage;
                manager.DataRequested -= handler;
            };
            manager.DataRequested += handler;
            DataTransferManager.ShowShareUI();
        }

        public static async Task SavePicAsync(ImageModel image, string fileName = null)
        {
            StorageFile imageFile = await GetOriginImageFileAsync(image);
            if (imageFile == null) { return; }

            fileName ??= GetTitle(image.Uri);
            FileSavePicker fileSavePicker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.PicturesLibrary,
                SuggestedFileName = fileName.Replace(fileName.Substring(fileName.LastIndexOf('.')), string.Empty)
            };
            ((IInitializeWithWindow)(object)fileSavePicker).Initialize(App.WindowHandle);

            string fileex = fileName.Substring(fileName.LastIndexOf('.') + 1);
            int index = fileex.IndexOfAny(new char[] { '?', '%', '&' });
            fileex = fileex.Substring(0, index == -1 ? fileex.Length : index);
            fileSavePicker.FileTypeChoices.Add($"{fileex}文件", new string[] { "." + fileex });

            StorageFile file = await fileSavePicker.PickSaveFileAsync();
            if (file != null)
            {
                using (Stream folderStream = await file.OpenStreamForWriteAsync())
                using (IRandomAccessStreamWithContentType randomAccessStream = await imageFile.OpenReadAsync())
                using (Stream imageStream = randomAccessStream.AsStreamForRead())
                {
                    await imageStream.CopyToAsync(folderStream);
                }
            }
        }

        public static async Task<DataPackage> GetImageDataPackageAsync(ImageModel image, string title, string description = null)
        {
            StorageFile file = await GetOriginImageFileAsync(image);
            if (file == null) { return null; }
            RandomAccessStreamReference bitmap = RandomAccessStreamReference.CreateFromFile(file);

            DataPackage dataPackage = new DataPackage();
            dataPackage.SetBitmap(bitmap);
            dataPackage.Properties.Title = title;
            dataPackage.Properties.Description = description ?? GetTitle(image.Uri);
            return dataPackage;
        }

        public static async Task GetImageDataPackageAsync(DataPackage dataPackage, ImageModel image, string title, string description = null)
        {
            StorageFile file = await GetOriginImageFileAsync(image);
            if (file == null) { return; }
            RandomAccessStreamReference bitmap = RandomAccessStreamReference.CreateFromFile(file);

            dataPackage.SetBitmap(bitmap);
            dataPackage.Properties.Title = title;
            dataPackage.Properties.Description = description ?? GetTitle(image.Uri);
            dataPackage.SetStorageItems(new IStorageItem[] { file });
        }

        private static string GetTitle(string url)
        {
            return FileNameRegex.IsMatch(url) ? FileNameRegex.Match(url).Value : "图片";
        }
    }
}
