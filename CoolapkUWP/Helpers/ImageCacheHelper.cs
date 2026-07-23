using CoolapkUWP.Common;
using CommunityToolkit.WinUI.Helpers;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.Storage.Streams;
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

        internal static async Task<BitmapImage> GetImageAsync(ImageType type, string url, DispatcherQueue dispatcher, bool isForce = false)
        {
            Uri uri = url.ValidateAndGetUri();
            if (uri == null) { return NoPic; }

            if (url.IndexOf("ms-appx", StringComparison.Ordinal) == 0)
            {
                await dispatcher.ResumeForegroundAsync();
                return new BitmapImage(uri);
            }
            else if (!isForce && SettingsHelper.Get<bool>(SettingsHelper.IsNoPicsMode))
            {
                return await GetNoPicAsync(dispatcher);
            }
            else
            {
                if (type.HasFlag(ImageType.Small))
                {
                    if (url.Contains("coolapk.com") && !url.EndsWith(".png")) { url += ".s.jpg"; }
                    uri = url.ValidateAndGetUri();
                }

                if (await dispatcher.AwaitableRunAsync(() => Dispatcher.HasThreadAccess))
                {
                    await Dispatcher.ResumeForegroundAsync();
                    try
                    {
                        BitmapImage image = await ImageCache.Instance.GetFromCacheAsync(uri, true);
                        return image;
                    }
                    catch (FileNotFoundException)
                    {
                        try
                        {
                            await ImageCache.Instance.RemoveAsync(new Uri[] { uri });
                            BitmapImage image = await ImageCache.Instance.GetFromCacheAsync(uri, true);
                            return image;
                        }
                        catch (Exception)
                        {
                            string str = ResourceLoader.GetForViewIndependentUse().GetString("ImageLoadError");
                            UIHelper.ShowMessage(str);
                            return NoPic;
                        }
                    }
                    catch (Exception)
                    {
                        string str = ResourceLoader.GetForViewIndependentUse().GetString("ImageLoadError");
                        UIHelper.ShowMessage(str);
                        return NoPic;
                    }
                }
                else
                {
                    StorageFile file = null;
                    try
                    {
                        file = await ImageCache.Instance.GetFileFromCacheAsync(uri);
                        if (file == null)
                        {
                            _ = await ImageCache.Instance.GetFromCacheAsync(uri, true);
                            file = await ImageCache.Instance.GetFileFromCacheAsync(uri);
                        }
                    }
                    catch (FileNotFoundException)
                    {
                        try
                        {
                            await ImageCache.Instance.RemoveAsync(new Uri[] { uri });
                            _ = await ImageCache.Instance.GetFromCacheAsync(uri, true);
                            file = await ImageCache.Instance.GetFileFromCacheAsync(uri);
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
                    using (IRandomAccessStreamWithContentType stream = await file.OpenReadAsync())
                    {
                        try
                        {
                            await dispatcher.ResumeForegroundAsync();
                            BitmapImage image = new BitmapImage();
                            await image.SetSourceAsync(stream);
                            return image;
                        }
                        catch
                        {
                            return null;
                        }
                    }
                }
            }
        }

        internal static async Task<StorageFile> GetImageFileAsync(ImageType type, string url)
        {
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
                    if (url.Contains("coolapk.com") && !url.EndsWith(".png")) { url += ".s.jpg"; }
                    uri = url.ValidateAndGetUri();
                }

                try
                {
                    StorageFile image = await ImageCache.Instance.GetFileFromCacheAsync(uri);
                    if (image == null)
                    {
                        _ = await ImageCache.Instance.GetFromCacheAsync(uri, true);
                        image = await ImageCache.Instance.GetFileFromCacheAsync(uri);
                    }
                    return image;
                }
                catch (FileNotFoundException)
                {
                    try
                    {
                        await ImageCache.Instance.RemoveAsync(new Uri[] { uri });
                        _ = await ImageCache.Instance.GetFromCacheAsync(uri, true);
                        StorageFile image = await ImageCache.Instance.GetFileFromCacheAsync(uri);
                        return image;
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

        internal static async Task<BitmapImage> GetNoPicAsync(DispatcherQueue dispatcher)
        {
            if (await dispatcher.AwaitableRunAsync(() => Dispatcher.HasThreadAccess))
            {
                return NoPic;
            }
            else
            {
                await dispatcher.ResumeForegroundAsync();
                return ThemeHelper.IsDarkTheme()
                    ? new BitmapImage(DarkNoPicUri) { DecodePixelHeight = 768, DecodePixelWidth = 768 }
                    : new BitmapImage(WhiteNoPicUri) { DecodePixelHeight = 768, DecodePixelWidth = 768 };
            }
        }
    }
}
