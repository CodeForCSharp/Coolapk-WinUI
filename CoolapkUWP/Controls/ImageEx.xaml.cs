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
                _fadeInStoryboard = new Storyboard();
                _fadeInStoryboard.Children.Add(animation);
            }
            ImageElement.Opacity = 0;
            _fadeInStoryboard.Begin();
        }

        private void ImageEx_Loaded(object sender, RoutedEventArgs e)
        {
            if (Model == null) { return; }

            Model.PropertyChanged -= Model_PropertyChanged;
            Model.PropertyChanged += Model_PropertyChanged;
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

            currentDecodeWidth = 0;
            ImageElement.Source = null;
            ImageElement.Opacity = 0;
            Placeholder.Visibility = Visibility.Visible;
        }
    }
}
