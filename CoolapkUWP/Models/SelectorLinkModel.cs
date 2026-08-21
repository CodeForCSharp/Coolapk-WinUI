using CoolapkUWP.Data;
using CoolapkUWP.Data.Dtos;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System.Text.Json.Nodes;

namespace CoolapkUWP.Models
{
    [INotifyPropertyChanged]
    internal partial class SelectorLinkModel : IndexPageModel
    {
        private const byte GreenR = 0x00;
        private const byte GreenG = 0xAA;
        private const byte GreenB = 0x5B;

        [ObservableProperty]
        public partial bool IsSelected { get; set; }

        public int Index { get; set; }
        public SelectorLinkCardModel Parent { get; set; }

        public SelectorLinkModel(IndexPageDto dto) : base(dto) { }

        public static SelectorLinkModel CreateFromJson(JsonObject json)
            => new SelectorLinkModel(DtoJson.Deserialize<IndexPageDto>(json));

        public Brush PillBackground => IsSelected
            ? new SolidColorBrush(Windows.UI.Color.FromArgb(0x1A, GreenR, GreenG, GreenB))
            : (Brush)Application.Current.Resources["CardBackgroundFillColorDefaultBrush"];

        public Brush PillBorderBrush => IsSelected
            ? new SolidColorBrush(Windows.UI.Color.FromArgb(0xFF, GreenR, GreenG, GreenB))
            : CreateStrokeBrush();

        private static Brush CreateStrokeBrush()
        {
            bool isLightTheme = Application.Current.RequestedTheme == ApplicationTheme.Light;
            return new SolidColorBrush(isLightTheme
                ? Windows.UI.Color.FromArgb(0x1A, 0x00, 0x00, 0x00)
                : Windows.UI.Color.FromArgb(0x1F, 0xFF, 0xFF, 0xFF));
        }

        public Brush PillForeground => IsSelected
            ? new SolidColorBrush(Windows.UI.Color.FromArgb(0xFF, GreenR, GreenG, GreenB))
            : (Brush)Application.Current.Resources["TextFillColorPrimaryBrush"];

        partial void OnIsSelectedChanged(bool value)
        {
            OnPropertyChanged(nameof(PillBackground));
            OnPropertyChanged(nameof(PillBorderBrush));
            OnPropertyChanged(nameof(PillForeground));
        }
    }
}
