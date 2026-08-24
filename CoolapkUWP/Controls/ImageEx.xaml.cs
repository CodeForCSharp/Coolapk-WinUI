using CoolapkUWP.Models.Images;
using System;
using System.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;

namespace CoolapkUWP.Controls
{
    public sealed partial class ImageEx : UserControl
    {
        private const int SizeBucket = 128;

        private int currentDecodeWidth;
        private Storyboard _fadeInStoryboard;
        private bool _fadeInRunning;

        public ImageEx()
        {
            InitializeComponent();
            Loaded += ImageEx_Loaded;
            Unloaded += ImageEx_Unloaded;
            SizeChanged += ImageEx_SizeChanged;
        }

        public static readonly DependencyProperty ModelProperty = DependencyProperty.Register(
            nameof(Model), typeof(ImageModel), typeof(ImageEx), new PropertyMetadata(null, OnModelChanged));

        public static readonly DependencyProperty StretchProperty = DependencyProperty.Register(
            nameof(Stretch), typeof(Stretch), typeof(ImageEx), new PropertyMetadata(Stretch.UniformToFill));

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

        private static void OnModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ImageEx)d).OnModelChanged((ImageModel)e.OldValue, (ImageModel)e.NewValue);
        }

        private void OnModelChanged(ImageModel oldValue, ImageModel newValue)
        {
            if (oldValue != null)
            {
                oldValue.PropertyChanged -= Model_PropertyChanged;
            }

            currentDecodeWidth = 0;

            if (newValue == null)
            {
                UpdateSource();
                return;
            }

            newValue.PropertyChanged -= Model_PropertyChanged;
            newValue.PropertyChanged += Model_PropertyChanged;
            UpdateSource();
            Reload();
        }

        private void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ImageModel.Pic) && sender == Model)
            {
                UpdateSource();
                FadeIn();
            }
        }

        private void UpdateSource()
        {
            BitmapImage source = Model?.CurrentPic;
            ImageElement.Source = source;
            ImageElement.Opacity = source == null ? 0 : 1;
            Placeholder.Visibility = source == null ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ImageEx_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Reload();
        }

        private void Reload()
        {
            if (Model == null || ActualWidth <= 0) { return; }

            double scale = XamlRoot?.RasterizationScale ?? 1.0;
            double width = Math.Max(ActualWidth, 1) * scale;
            int decodeWidth = Math.Max((int)(Math.Ceiling(width / SizeBucket) * SizeBucket), SizeBucket);

            if (decodeWidth == currentDecodeWidth) { return; }
            currentDecodeWidth = decodeWidth;
            _ = Model.LoadAsync(decodeWidth);
        }

        private void FadeIn()
        {
            if (XamlRoot == null)
            {
                StopFadeIn();
                ImageElement.Opacity = 1;
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
            ImageElement.Opacity = Model?.CurrentPic != null ? 1 : 0;
        }

        private void ImageEx_Loaded(object sender, RoutedEventArgs e)
        {
            if (Model == null) { return; }

            Model.PropertyChanged -= Model_PropertyChanged;
            Model.PropertyChanged += Model_PropertyChanged;
            // 释放断开期间可能冻结的动画，保证 UpdateSource 设置的透明度生效。
            StopFadeIn();
            UpdateSource();
            currentDecodeWidth = 0;
            Reload();
        }

        private void ImageEx_Unloaded(object sender, RoutedEventArgs e)
        {
            if (Model != null)
            {
                Model.PropertyChanged -= Model_PropertyChanged;
            }

            // 停掉动画但不清空 Source/占位符：控件可能因父容器 reparent 被断开，
            // 且重连时 Loaded 事件可能丢失；保留当前视觉状态可保证重连后内容仍然正确。
            StopFadeIn();
            currentDecodeWidth = 0;
        }
    }
}
