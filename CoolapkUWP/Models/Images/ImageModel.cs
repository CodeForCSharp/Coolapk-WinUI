using CoolapkUWP.Common;
using CoolapkUWP.Helpers;
using CommunityToolkit.WinUI.Helpers;
using System;
using System.Collections.Generic;

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;

namespace CoolapkUWP.Models.Images
{
    [WinRT.GeneratedBindableCustomProperty]
    public partial class ImageModel : INotifyPropertyChanged, IPic
    {
        private static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(SettingsHelper.Get<int>(SettingsHelper.SemaphoreSlimCount));

        private readonly Action<UISettingChangedType> UISettingChanged;

        public DispatcherQueue Dispatcher { get; }

        protected WeakReference<BitmapImage> pic;
        public BitmapImage CurrentPic
        {
            get
            {
                if (pic != null && pic.TryGetTarget(out BitmapImage image))
                {
                    return image;
                }
                return null;
            }
        }

        public BitmapImage Pic
        {
            get
            {
                if (pic != null && pic.TryGetTarget(out BitmapImage image))
                {
                    return image;
                }
                else
                {
                    _ = GetImage();
                    return ImageCacheHelper.NoPic;
                }
            }
            protected set
            {
                if (pic == null)
                {
                    pic = new WeakReference<BitmapImage>(value);
                }
                else
                {
                    pic.SetTarget(value);
                }
                RaisePropertyChangedEvent();
            }
        }

        private bool isLongPic = false;
        public bool IsLongPic
        {
            get => isLongPic;
            private set
            {
                if (isLongPic != value)
                {
                    isLongPic = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private bool isWidePic = false;
        public bool IsWidePic
        {
            get => isWidePic;
            private set
            {
                if (isWidePic != value)
                {
                    isWidePic = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        protected List<ImageModel> contextArray = new List<ImageModel>();
        public List<ImageModel> ContextArray
        {
            get => contextArray;
            set
            {
                if (contextArray == null || contextArray.Count == 0)
                {
                    contextArray = value;
                    RaisePropertyChangedEvent();
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
                    if (pic != null && pic.TryGetTarget(out BitmapImage _))
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
                    if (pic != null && pic.TryGetTarget(out BitmapImage _))
                    {
                        _ = GetImage();
                    }
                }
            }
        }

        public BitmapImage RealPic
        {
            get
            {
                if (pic != null && pic.TryGetTarget(out BitmapImage image))
                {
                    return image;
                }

                if (Dispatcher.HasThreadAccess)
                {
                    _ = GetImage();
                    return ImageCacheHelper.NoPic;
                }

                GetImage().Wait();
                return Pic;
            }
        }

        private bool isLoading = true;
        public bool IsLoading
        {
            get => isLoading;
            private set
            {
                if (isLoading != value)
                {
                    isLoading = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        public ImageModel(string uri, ImageType type) : this(uri, type, App.MainWindow.DispatcherQueue)
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
                            if (pic != null && pic.TryGetTarget(out BitmapImage _))
                            {
                                Pic = ImageCacheHelper.NoPic;
                            }
                        }
                        break;

                    case UISettingChangedType.NoPicChanged:
                        if (pic != null && pic.TryGetTarget(out BitmapImage _))
                        {
                            _ = GetImage();
                        }
                        break;
                }
            };
            ThemeHelper.UISettingChanged.Add(UISettingChanged);
        }

        ~ImageModel()
        {
            ThemeHelper.UISettingChanged.Remove(UISettingChanged);
        }

        public event TypedEventHandler<ImageModel, object> LoadStarted;
        public event TypedEventHandler<ImageModel, object> LoadCompleted;

        public event PropertyChangedEventHandler PropertyChanged;

        private async void RaisePropertyChangedEvent([CallerMemberName] string name = null)
        {
            if (name != null)
            {
                await Dispatcher.ResumeForegroundAsync();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }

        public static void SetSemaphoreSlim(int initialCount)
        {
            semaphoreSlim.Dispose();
            semaphoreSlim = new SemaphoreSlim(initialCount);
        }

        private long loadGeneration;

        private Task GetImage() => LoadCoreAsync(0);

        public async Task LoadAsync(int decodePixelWidth = 0) => await LoadCoreAsync(decodePixelWidth);

        private async Task LoadCoreAsync(int decodePixelWidth)
        {
            long generation = Interlocked.Increment(ref loadGeneration);
            await ThreadSwitcher.ResumeBackgroundAsync();
            try
            {
                if (generation != loadGeneration) { return; }

                IsLoading = true;
                LoadStarted?.Invoke(this, null);

                await semaphoreSlim.WaitAsync();
                try
                {
                    if (generation != loadGeneration) { return; }

                    if (SettingsHelper.Get<bool>(SettingsHelper.IsNoPicsMode)) { Pic = await ImageCacheHelper.GetNoPicAsync(Dispatcher); }
                    BitmapImage bitmapImage = await ImageCacheHelper.GetImageAsync(Type, Uri, Dispatcher, false, decodePixelWidth);
                    if (generation != loadGeneration) { return; }

                    if (bitmapImage != null)
                    {
                        Pic = bitmapImage;
                        await bitmapImage.DispatcherQueue.ResumeForegroundAsync();
                        double PixelWidth = bitmapImage.PixelWidth;
                        double PixelHeight = bitmapImage.PixelHeight;
                        Rect Bounds = await App.MainWindow.DispatcherQueue.AwaitableRunAsync(() => App.MainWindow.Bounds);
                        IsLongPic = PixelHeight * Bounds.Width > PixelWidth * Bounds.Height * 1.5
                                    && PixelHeight > PixelWidth * 1.5;
                        IsWidePic = PixelWidth * Bounds.Height > PixelHeight * Bounds.Width * 1.5
                                    && PixelWidth > PixelHeight * 1.5;
                    }
                    else
                    {
                        Pic = ImageCacheHelper.NoPic;
                        IsLongPic = false;
                        IsWidePic = false;
                    }
                }
                finally
                {
                    semaphoreSlim.Release();
                }
            }
            catch (Exception)
            {
                if (generation == loadGeneration) { Pic = ImageCacheHelper.NoPic; }
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

        public async Task Refresh() => await GetImage();

        public override string ToString() => Uri;
    }
}
