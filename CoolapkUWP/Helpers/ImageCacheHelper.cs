using CoolapkUWP.Common;
using CommunityToolkit.WinUI.Helpers;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Media.Imaging;
using ImageCache = CoolapkUWP.Common.ImageCache;

namespace CoolapkUWP.Helpers
{
    [Flags]
    public enum ImageType
    {
        Origin = 0x00,
        Small = 0x01,

        Image = 0x02,
        Avatar = 0x04,
        Icon = 0x08,
        Captcha = 0x16,

        OriginImage = Image | Origin,
        BigAvatar = Avatar | Origin,

        SmallImage = Image | Small,
        SmallAvatar = Avatar | Small,
    }

    internal static partial class ImageCacheHelper
    {
        private static readonly Uri DarkNoPicUri = new Uri("ms-appx:/Assets/NoPic/img_placeholder_night.png");
        private static readonly Uri WhiteNoPicUri = new Uri("ms-appx:/Assets/NoPic/img_placeholder.png");

        private static BitmapImage DarkNoPicMode { get; set; }
        private static BitmapImage WhiteNoPicMode { get; set; }
        internal static BitmapImage NoPic { get => ThemeHelper.IsDarkTheme() ? DarkNoPicMode : WhiteNoPicMode; }

        internal static DispatcherQueue Dispatcher { get; } = App.MainWindow.DispatcherQueue;

        static ImageCacheHelper()
        {
            ImageCache.Instance.CacheDuration = TimeSpan.FromHours(8);
            _ = Dispatcher.AwaitableRunAsync(() =>
            {
                DarkNoPicMode = new BitmapImage(DarkNoPicUri) { DecodePixelHeight = 768, DecodePixelWidth = 768 };
                WhiteNoPicMode = new BitmapImage(WhiteNoPicUri) { DecodePixelHeight = 768, DecodePixelWidth = 768 };
            });
        }

        private const int ThumbnailDecodePixelWidth = 512;
        private const int IconDecodePixelWidth = 256;
        private const int AvatarDecodePixelWidth = 256;

        private static int GetDecodePixelWidth(ImageType type)
        {
            if (type.HasFlag(ImageType.Small)) { return ThumbnailDecodePixelWidth; }
            if (type.HasFlag(ImageType.Avatar)) { return AvatarDecodePixelWidth; }
            if (type.HasFlag(ImageType.Icon)) { return IconDecodePixelWidth; }
            return 0;
        }

        private static string SanitizeUrl(string url)
        {
            if (string.IsNullOrEmpty(url)) { return url; }
            return url.Trim('"').Replace("&quot;", string.Empty).Trim();
        }

        internal static async Task<BitmapImage> GetImageAsync(ImageType type, string url, DispatcherQueue dispatcher, bool isForce = false, int decodePixelWidth = 0)
        {
            url = SanitizeUrl(url);
            Uri uri = url.ValidateAndGetUri();
            if (uri == null) { return NoPic; }

            if (url.IndexOf("ms-appx", StringComparison.Ordinal) == 0)
            {
                await dispatcher.ResumeForegroundAsync();
                return new BitmapImage(uri);
            }
            else if (!isForce && SettingsHelper.Get<bool>(SettingsHelper.IsNoPicsMode))
            {
                return NoPic;
            }
            else
            {
                if (type.HasFlag(ImageType.Small))
                {
                    if (url.Contains("coolapk.com") && !url.EndsWith(".png") && !url.EndsWith(".s.jpg")) { url += ".s.jpg"; }
                    uri = url.ValidateAndGetUri();
                }

                if (decodePixelWidth <= 0) { decodePixelWidth = GetDecodePixelWidth(type); }

                BitmapImage bitmap = null;
                try
                {
                    bitmap = await ImageCache.Instance.GetBitmapAsync(uri, decodePixelWidth, dispatcher);
                }
                catch (Exception)
                {
                }

                return bitmap ?? NoPic;
            }
        }

        internal static async Task<StorageFile> GetImageFileAsync(ImageType type, string url)
        {
            url = SanitizeUrl(url);
            Uri uri = url.ValidateAndGetUri();
            if (uri == null) { return null; }

            if (url.IndexOf("ms-appx", StringComparison.Ordinal) == 0)
            {
                return await StorageFile.GetFileFromApplicationUriAsync(uri);
            }
            else
            {
                if (type.HasFlag(ImageType.Small))
                {
                    if (url.Contains("coolapk.com") && !url.EndsWith(".png") && !url.EndsWith(".s.jpg")) { url += ".s.jpg"; }
                    uri = url.ValidateAndGetUri();
                }

                try
                {
                    return await ImageCache.Instance.GetFileFromCacheAsync(uri);
                }
                catch (FileNotFoundException)
                {
                    try
                    {
                        await ImageCache.Instance.RemoveAsync(new Uri[] { uri });
                        return await ImageCache.Instance.GetFileFromCacheAsync(uri);
                    }
                    catch (Exception)
                    {
                        string str = ResourceLoader.GetForViewIndependentUse().GetString("ImageLoadError");
                        UIHelper.ShowMessage(str);
                        return null;
                    }
                }
                catch (Exception)
                {
                    string str = ResourceLoader.GetForViewIndependentUse().GetString("ImageLoadError");
                    UIHelper.ShowMessage(str);
                    return null;
                }
            }
        }

        internal static Task CleanCacheAsync() => ImageCache.Instance.ClearAsync();
    }
}
