using CoolapkUWP.Helpers;
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
        private const double Oversample = 1.0;
        private const int SizeBucket = 128;

        private int currentDecodeWidth;

        public ImageEx()
        {
            InitializeComponent();
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
                ImageElement.Source = null;
                Placeholder.Visibility = Visibility.Visible;
                return;
            }

            newValue.PropertyChanged += Model_PropertyChanged;
            BitmapImage source = newValue.CurrentPic;
            ImageElement.Source = source;
            if (source == null)
            {
                ImageElement.Opacity = 0;
                Placeholder.Visibility = Visibility.Visible;
            }
            else
            {
                ImageElement.Opacity = 1;
                Placeholder.Visibility = Visibility.Collapsed;
            }
            Reload();
        }

        private void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ImageModel.Pic) && sender == Model)
            {
                ImageElement.Source = Model.CurrentPic ?? ImageCacheHelper.NoPic;
                Placeholder.Visibility = Visibility.Collapsed;
                FadeIn();
            }
        }

        private void ImageEx_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Reload();
        }

        private void Reload()
        {
            if (Model == null || ActualWidth <= 0) { return; }

            double scale = XamlRoot?.RasterizationScale ?? 1.0;
            double width = Math.Max(ActualWidth, 1) * scale * Oversample;
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

            DoubleAnimation animation = new DoubleAnimation
            {
                To = 1,
                Duration = TimeSpan.FromMilliseconds(200)
            };
            Storyboard storyboard = new Storyboard();
            Storyboard.SetTarget(animation, ImageElement);
            Storyboard.SetTargetProperty(animation, "Opacity");
            storyboard.Children.Add(animation);
            storyboard.Begin();
        }

        private void ImageEx_Unloaded(object sender, RoutedEventArgs e)
        {
            if (Model != null)
            {
                Model.PropertyChanged -= Model_PropertyChanged;
            }
        }
    }
}
