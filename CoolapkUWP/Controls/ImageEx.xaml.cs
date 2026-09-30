using CoolapkUWP.Common;
using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Images;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Foundation;

namespace CoolapkUWP.Controls
{
    /// <summary>
    /// 按 <see cref="ImageModel"/> 加载并持有图片：解码结果只跟随控件存活，
    /// 离屏后的复用交给 ImageCache 的强缓存按上限管理。
    /// </summary>
    public sealed partial class ImageEx : UserControl
    {
        private const int SizeBucket = 128;
        private const int MaxLoadRetries = 2;
        private static readonly TimeSpan RetryInterval = TimeSpan.FromSeconds(1);

        private static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(Math.Max(1, SettingsHelper.Get<int>(SettingsHelper.SemaphoreSlimCount)));

        private readonly Action<UISettingChangedType> uiSettingChanged;

        // 当前显示（或正在加载）的图片来源，用于判断是否需要重新加载。
        private string sourceUri;
        private ImageType sourceType;
        private int? sourceDecodeWidth;
        private bool isShowingNoPic;
        private long loadGeneration;

        private Storyboard _fadeInStoryboard;
        private bool _fadeInRunning;

        public ImageEx()
        {
            InitializeComponent();
            // WeakEvent 只弱引用委托，必须由字段持有。
            uiSettingChanged = OnUISettingChanged;
            Loaded += ImageEx_Loaded;
            Unloaded += ImageEx_Unloaded;
            SizeChanged += ImageEx_SizeChanged;
        }

        public static void SetSemaphoreSlim(int initialCount)
        {
            Interlocked.Exchange(ref semaphoreSlim, new SemaphoreSlim(Math.Max(1, initialCount)));
        }

        public static readonly DependencyProperty ModelProperty = DependencyProperty.Register(
            nameof(Model), typeof(ImageModel), typeof(ImageEx), new PropertyMetadata(null, OnModelChanged));

        public static readonly DependencyProperty StretchProperty = DependencyProperty.Register(
            nameof(Stretch), typeof(Stretch), typeof(ImageEx), new PropertyMetadata(Stretch.UniformToFill));

        public static readonly DependencyProperty DecodePixelWidthProperty = DependencyProperty.Register(
            nameof(DecodePixelWidth), typeof(int), typeof(ImageEx), new PropertyMetadata(-1, OnDecodePixelWidthChanged));

        public static readonly DependencyProperty IsLoadingProperty = DependencyProperty.Register(
            nameof(IsLoading), typeof(bool), typeof(ImageEx), new PropertyMetadata(false));

        public static readonly DependencyProperty IsLongPicProperty = DependencyProperty.Register(
            nameof(IsLongPic), typeof(bool), typeof(ImageEx), new PropertyMetadata(false));

        public static readonly DependencyProperty IsWidePicProperty = DependencyProperty.Register(
            nameof(IsWidePic), typeof(bool), typeof(ImageEx), new PropertyMetadata(false));

        public ImageModel Model
        {
            get => (ImageModel)GetValue(ModelProperty);
            set => SetValue(ModelProperty, value);
        }

        public Stretch Stretch
        {
            get => (Stretch)GetValue(StretchProperty);
            set => SetValue(StretchProperty, value);
        }

        /// <summary>
        /// 解码宽度：小于 0 时按控件实际尺寸解码；0 表示使用图片类型的默认尺寸（原图类型即原始尺寸）；大于 0 为固定宽度。
        /// </summary>
        public int DecodePixelWidth
        {
            get => (int)GetValue(DecodePixelWidthProperty);
            set => SetValue(DecodePixelWidthProperty, value);
        }

        public bool IsLoading
        {
            get => (bool)GetValue(IsLoadingProperty);
            private set => SetValue(IsLoadingProperty, value);
        }

        public bool IsLongPic
        {
            get => (bool)GetValue(IsLongPicProperty);
            private set => SetValue(IsLongPicProperty, value);
        }

        public bool IsWidePic
        {
            get => (bool)GetValue(IsWidePicProperty);
            private set => SetValue(IsWidePicProperty, value);
        }

        private static void OnModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ImageEx)d).OnModelChanged((ImageModel)e.OldValue, (ImageModel)e.NewValue);
        }

        private static void OnDecodePixelWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ImageEx)d).Reload();
        }

        private void OnModelChanged(ImageModel oldValue, ImageModel newValue)
        {
            if (oldValue != null) { Unsubscribe(oldValue); }
            if (newValue != null && IsLoaded) { Subscribe(newValue); }

            ClearSource();
            Reload();
        }

        private void Subscribe(ImageModel model)
        {
            Unsubscribe(model);
            model.PropertyChanged += Model_PropertyChanged;
            model.RefreshRequested += Model_RefreshRequested;
        }

        private void Unsubscribe(ImageModel model)
        {
            model.PropertyChanged -= Model_PropertyChanged;
            model.RefreshRequested -= Model_RefreshRequested;
        }

        private void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender == Model && (e.PropertyName == nameof(ImageModel.Uri) || e.PropertyName == nameof(ImageModel.Type)))
            {
                Reload();
            }
        }

        private void Model_RefreshRequested(ImageModel sender, object args)
        {
            if (sender == Model) { Reload(true); }
        }

        private void OnUISettingChanged(UISettingChangedType type)
        {
            _ = DispatcherQueue.TryEnqueue(() =>
            {
                switch (type)
                {
                    case UISettingChangedType.LightMode:
                    case UISettingChangedType.DarkMode:
                        if (isShowingNoPic) { ShowNoPic(); }
                        break;

                    case UISettingChangedType.NoPicChanged:
                        Reload(true);
                        break;
                }
            });
        }

        private void ImageEx_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Reload();
        }

        private void Reload(bool force = false)
        {
            ImageModel model = Model;
            if (model == null) { return; }

            int decodeWidth = GetDecodeWidth();
            if (decodeWidth < 0) { return; }

            bool isNoPicsMode = SettingsHelper.Get<bool>(SettingsHelper.IsNoPicsMode);
            if (!force
                && isShowingNoPic == isNoPicsMode
                && model.Uri == sourceUri
                && model.Type == sourceType
                && IsDecodeSufficient(decodeWidth))
            {
                return;
            }

            _ = LoadAsync(model.Uri, model.Type, decodeWidth, isNoPicsMode);
        }

        private int GetDecodeWidth()
        {
            int decodeWidth = DecodePixelWidth;
            if (decodeWidth >= 0) { return decodeWidth; }
            if (ActualWidth <= 0) { return -1; }

            double scale = XamlRoot?.RasterizationScale ?? 1.0;
            return Math.Max((int)(Math.Ceiling(ActualWidth * scale / SizeBucket) * SizeBucket), SizeBucket);
        }

        // 布局过程中尺寸会反复变化：已有更大的解码结果时直接缩放显示，缩小到一半以下才重新解码。
        private bool IsDecodeSufficient(int decodeWidth)
        {
            if (sourceDecodeWidth is not int current) { return false; }
            if (current == decodeWidth) { return true; }
            return decodeWidth > 0 && decodeWidth < current && decodeWidth * 2 > current;
        }

        private async Task LoadAsync(string uri, ImageType type, int decodeWidth, bool isNoPicsMode)
        {
            long generation = ++loadGeneration;
            sourceUri = uri;
            sourceType = type;
            sourceDecodeWidth = decodeWidth;

            if (isNoPicsMode)
            {
                IsLoading = false;
                ShowNoPic();
                return;
            }

            IsLoading = true;
            try
            {
                Task<BitmapImage> task = LoadWithRetryAsync(uri, type, decodeWidth, generation);
                // 命中内存缓存时同步完成，直接显示，避免容器复用时反复播放淡入。
                bool isCompletedSynchronously = task.IsCompleted;
                BitmapImage bitmap = await task;
                if (generation != loadGeneration) { return; }

                if (bitmap != null && !ReferenceEquals(bitmap, ImageCacheHelper.NoPic))
                {
                    SetSource(bitmap, !isCompletedSynchronously);
                }
                else
                {
                    OnLoadFailed();
                }
            }
            catch (Exception ex)
            {
                if (generation != loadGeneration) { return; }
                SettingsHelper.LogManager.CreateLogger(nameof(ImageEx)).LogWarning(ex, $"图片加载失败: {uri}");
                OnLoadFailed();
            }
            finally
            {
                if (generation == loadGeneration) { IsLoading = false; }
            }
        }

        /// <summary>
        /// 加载图片，瞬态失败时自动重试；重试耗尽后抛出异常交由上层记录并降级。
        /// </summary>
        private async Task<BitmapImage> LoadWithRetryAsync(string uri, ImageType type, int decodeWidth, long generation)
        {
            SemaphoreSlim semaphore = semaphoreSlim;
            await semaphore.WaitAsync();
            try
            {
                for (int attempt = 0; ; attempt++)
                {
                    if (generation != loadGeneration) { return null; }
                    try
                    {
                        return await ImageCacheHelper.GetImageAsync(type, uri, false, decodeWidth);
                    }
                    catch (Exception) when (attempt < MaxLoadRetries && generation == loadGeneration)
                    {
                        // 瞬态失败，稍后重试；重试耗尽或已过期时异常直接向上传播
                    }

                    await Task.Delay(RetryInterval);
                }
            }
            finally
            {
                semaphore.Release();
            }
        }

        private void SetSource(BitmapImage bitmap, bool fadeIn)
        {
            bool wasEmpty = ImageElement.Source == null || isShowingNoPic;
            SetImageSource(bitmap, false);

            double pixelWidth = bitmap.PixelWidth;
            double pixelHeight = bitmap.PixelHeight;
            Rect bounds = WindowContext.Bounds;
            IsLongPic = pixelHeight * bounds.Width > pixelWidth * bounds.Height * 1.5
                        && pixelHeight > pixelWidth * 1.5;
            IsWidePic = pixelWidth * bounds.Height > pixelHeight * bounds.Width * 1.5
                        && pixelWidth > pixelHeight * 1.5;

            if (fadeIn && wasEmpty) { FadeIn(); }
        }

        // 加载失败：已有可用图片时不覆盖，避免把好图顶成占位图；清空解码宽度以便下次触发时重试。
        private void OnLoadFailed()
        {
            sourceDecodeWidth = null;
            if (ImageElement.Source == null) { ShowNoPic(); }
        }

        private void ShowNoPic() => SetImageSource(ImageCacheHelper.NoPic, true);

        private void ClearSource()
        {
            loadGeneration++;
            sourceUri = null;
            sourceDecodeWidth = null;
            IsLoading = false;
            SetImageSource(null, false);
        }

        private void SetImageSource(BitmapImage source, bool isNoPic)
        {
            isShowingNoPic = isNoPic;
            ImageElement.Source = source;
            Placeholder.Visibility = source == null ? Visibility.Visible : Visibility.Collapsed;
            IsLongPic = false;
            IsWidePic = false;
            StopFadeIn();
        }

        private void FadeIn()
        {
            if (XamlRoot == null)
            {
                StopFadeIn();
                return;
            }

            if (_fadeInStoryboard == null)
            {
                DoubleAnimation animation = new DoubleAnimation
                {
                    From = 0,
                    To = 1,
                    Duration = TimeSpan.FromMilliseconds(200)
                };
                Storyboard.SetTarget(animation, ImageElement);
                Storyboard.SetTargetProperty(animation, "Opacity");
                animation.Completed += (s, e) =>
                {
                    _fadeInRunning = false;
                    // 动画结束后把本地值落定为 1，避免 Stop 后回退到 Begin 前的 0。
                    ImageElement.Opacity = 1;
                };
                _fadeInStoryboard = new Storyboard();
                _fadeInStoryboard.Children.Add(animation);
            }
            // 先停掉可能仍在持有的旧动画，避免其覆盖后续设置的本地透明度。
            StopFadeIn();
            ImageElement.Opacity = 0;
            _fadeInRunning = true;
            _fadeInStoryboard.Begin();
        }

        private void StopFadeIn()
        {
            if (_fadeInRunning)
            {
                _fadeInRunning = false;
                _fadeInStoryboard.Stop();
            }

            // Stop 会释放动画对属性的持有，把本地值归位到与当前内容一致的状态，
            // 防止元素在断开/重连间丢失 Loaded 事件后停留在透明状态。
            ImageElement.Opacity = ImageElement.Source != null ? 1 : 0;
        }

        private void ImageEx_Loaded(object sender, RoutedEventArgs e)
        {
            ThemeHelper.UISettingChanged.Remove(uiSettingChanged);
            ThemeHelper.UISettingChanged.Add(uiSettingChanged);
            if (Model != null) { Subscribe(Model); }

            // 释放断开期间可能冻结的动画，保证透明度与当前内容一致。
            StopFadeIn();
            // 断开期间错过的主题切换：占位图按当前主题重新设置。
            if (isShowingNoPic) { ShowNoPic(); }
            Reload();
        }

        private void ImageEx_Unloaded(object sender, RoutedEventArgs e)
        {
            ThemeHelper.UISettingChanged.Remove(uiSettingChanged);
            if (Model != null) { Unsubscribe(Model); }

            // 停掉动画但不清空 Source/占位符：控件可能因父容器 reparent 被断开，
            // 且重连时 Loaded 事件可能丢失；保留当前视觉状态可保证重连后内容仍然正确。
            StopFadeIn();
        }
    }
}
