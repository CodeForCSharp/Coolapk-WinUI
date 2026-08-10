using CoolapkUWP.Helpers;
using CommunityToolkit.WinUI;
using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace CoolapkUWP.Controls
{
    public sealed partial class ShowQRCodeControl : UserControl
    {
        public static readonly DependencyProperty QRCodeTextProperty = DependencyProperty.Register(
            nameof(QRCodeText),
            typeof(string),
            typeof(ShowQRCodeControl),
            new PropertyMetadata("https://www.coolapk.com", new PropertyChangedCallback(OnQRCodeTextChanged))
        );

        public string QRCodeText
        {
            get => (string)GetValue(QRCodeTextProperty);
            set => SetValue(QRCodeTextProperty, value);
        }

        private static void OnQRCodeTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as ShowQRCodeControl).QRCodeText = e.NewValue as string ?? "https://www.coolapk.com"; ;
        }

        public ShowQRCodeControl() => InitializeComponent();

        private void Button_Loaded(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            ContentPresenter content = element.FindDescendant<ContentPresenter>();
            if (content != null)
            {
                content.CornerRadius = new CornerRadius(8);
            }
        }

        private void ShowUIButton_Click(object sender, RoutedEventArgs e) { }
    }
}