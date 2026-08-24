using CoolapkUWP.Common;
using CoolapkUWP.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI;
using CommunityToolkit.WinUI.Helpers;
using System;
using System.Collections.Generic;

using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.Extensions.Logging;

namespace CoolapkUWP.Models.Images
{
    [WinRT.GeneratedBindableCustomProperty]
    [INotifyPropertyChanged]
    public partial class ImageModel : IPic
    {
        private static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(Math.Max(1, SettingsHelper.Get<int>(SettingsHelper.SemaphoreSlimCount)));

        private readonly Action<UISettingChangedType> UISettingChanged;

        public DispatcherQueue Dispatcher { get; }

        // 强引用持有已解码图片，避免 UI 断开（如页面导航）后被 GC 回收导致返回时白屏。
        // 内存总量由 ImageCache 的强缓存上限统一约束。
        protected BitmapImage pic;
        public BitmapImage CurrentPic => pic;

        public BitmapImage Pic
        {
            get
            {
                BitmapImage image = pic;
                if (image == null)
                {
                    _ = GetImage();
                    return ImageCacheHelper.NoPic;
                }
                return image;
            }
            protected set
            {
                pic = value;
                OnPropertyChanged();
            }
        }

        [ObservableProperty]
        public partial bool IsLongPic { get; private set; }

        [ObservableProperty]
        public partial bool IsWidePic { get; private set; }

        protected List<ImageModel> contextArray = new List<ImageModel>();
        public List<ImageModel> ContextArray
        {
            get => contextArray;
            set
            {
                if (contextArray == null || contextArray.Count == 0)
                {
                    contextArray = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsGif
        {
            get
            {
                string url = Uri;
                if (string.IsNullOrEmpty(url)) { return false; }
                if (url.ValidateAndGetUri() is Uri uri)
                {
                    return uri.AbsolutePath.EndsWith(".gif", StringComparison.OrdinalIgnoreCase);
                }
                return url.IndexOf(".gif", StringComparison.OrdinalIgnoreCase) >= 0;
            }
        }

        private string uri;
        public string Uri
        {
            get => uri;
            set
            {
                if (uri != value)
                {
                    uri = value;
                    if (pic != null)
                    {
                        _ = GetImage();
                    }
                }
            }
        }

        private ImageType type;
        public ImageType Type
        {
            get => type;
            set
            {
                if (type != value)
                {
                    type = value;
                    if (pic != null)
                    {
                        _ = GetImage();
                    }
                }
            }
        }

        [ObservableProperty]
        public partial bool IsLoading { get; private set; } = true;

        public ImageModel(string uri, ImageType type) : this(uri, type, WindowContext.DispatcherQueue)
        {
        }

        public ImageModel(string uri, ImageType type, DispatcherQueue dispatcher)
        {
            Dispatcher = dispatcher;
            Uri = uri;
            Type = type;
            UISettingChanged = (mode) =>
            {
                switch (mode)
                {
                    case UISettingChangedType.LightMode:
                    case UISettingChangedType.DarkMode:
                        if (SettingsHelper.Get<bool>(SettingsHelper.IsNoPicsMode))
                        {
                            if (pic != null)
                            {
                                _ = Dispatcher.EnqueueAsync(() => Pic = ImageCacheHelper.NoPic);
                            }
                        }
                        break;

                    case UISettingChangedType.NoPicChanged:
                        if (pic != null)
                        {
                            _ = GetImage();
                        }
                        break;
                }
            };
            ThemeHelper.UISettingChanged.Add(UISettingChanged);
        }

        public event TypedEventHandler<ImageModel, object> LoadStarted;
        public event TypedEventHandler<ImageModel, object> LoadCompleted;

        public static void SetSemaphoreSlim(int initialCount)
        {
            Interlocked.Exchange(ref semaphoreSlim, new SemaphoreSlim(Math.Max(1, initialCount)));
        }

        private long loadGeneration;

        private const int MaxLoadRetries = 2;
        private static readonly TimeSpan RetryInterval = TimeSpan.FromSeconds(1);

        private Task GetImage() => LoadCoreAsync(0);

        public async Task LoadAsync(int decodePixelWidth = 0) => await LoadCoreAsync(decodePixelWidth);

        /// <summary>
        /// 加载图片，瞬态失败时自动重试；重试耗尽后抛出异常交由上层记录并降级。
        /// </summary>
        private async Task<BitmapImage> LoadWithRetryAsync(int decodePixelWidth, long generation)
        {
            for (int attempt = 0; ; attempt++)
            {
                try
                {
                    return await ImageCacheHelper.GetImageAsync(Type, Uri, false, decodePixelWidth);
                }
                catch (Exception) when (attempt < MaxLoadRetries && generation == loadGeneration)
                {
                    // 瞬态失败，稍后重试；重试耗尽或已过期时异常直接向上传播
                }

                await Task.Delay(RetryInterval);
                if (generation != loadGeneration) { return null; }
            }
        }

        private async Task LoadCoreAsync(int decodePixelWidth)
        {
            long generation = Interlocked.Increment(ref loadGeneration);
            try
            {
                if (generation != loadGeneration) { return; }

                IsLoading = true;
                LoadStarted?.Invoke(this, null);

                await semaphoreSlim.WaitAsync();
                try
                {
                    if (generation != loadGeneration) { return; }

                    if (SettingsHelper.Get<bool>(SettingsHelper.IsNoPicsMode))
                    {
                        Pic = ImageCacheHelper.NoPic;
                        IsLongPic = false;
                        IsWidePic = false;
                        return;
                    }

                    BitmapImage bitmapImage = await LoadWithRetryAsync(decodePixelWidth, generation);
                    if (generation != loadGeneration) { return; }

                    if (bitmapImage != null && !ReferenceEquals(bitmapImage, ImageCacheHelper.NoPic))
                    {
                        Pic = bitmapImage;
                        double PixelWidth = bitmapImage.PixelWidth;
                        double PixelHeight = bitmapImage.PixelHeight;
                        Rect Bounds = await WindowContext.DispatcherQueue.EnqueueAsync(() => WindowContext.Bounds);
                        IsLongPic = PixelHeight * Bounds.Width > PixelWidth * Bounds.Height * 1.5
                                    && PixelHeight > PixelWidth * 1.5;
                        IsWidePic = PixelWidth * Bounds.Height > PixelHeight * Bounds.Width * 1.5
                                    && PixelWidth > PixelHeight * 1.5;
                    }
                    else
                    {
                        // 加载失败（解码失败或返回占位图）：已有可用图片时不覆盖，避免把好图顶成占位图。
                        ShowNoPic();
                    }
                }
                finally
                {
                    semaphoreSlim.Release();
                }
            }
            catch (Exception ex)
            {
                if (generation == loadGeneration)
                {
                    SettingsHelper.LogManager.CreateLogger(nameof(ImageModel)).LogWarning(ex, $"图片加载失败: {Uri}");
                    ShowNoPic();
                }
            }
            finally
            {
                if (generation == loadGeneration)
                {
                    LoadCompleted?.Invoke(this, null);
                    IsLoading = false;
                }
            }
        }

        private void ShowNoPic()
        {
            if (CurrentPic == null)
            {
                Pic = ImageCacheHelper.NoPic;
                IsLongPic = false;
                IsWidePic = false;
            }
        }

        public async Task Refresh() => await GetImage();

        public override string ToString() => Uri;
    }
}
