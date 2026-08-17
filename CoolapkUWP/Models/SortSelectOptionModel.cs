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
    internal partial class SortSelectOptionModel : IndexPageModel
    {
        [ObservableProperty]
        public partial bool IsSelected { get; set; }

        public int Index { get; set; }
        public SortSelectCardModel Parent { get; set; }

        public SortSelectOptionModel(IndexPageDto dto) : base(dto) { }

        public static SortSelectOptionModel CreateFromJson(JsonObject json)
            => new SortSelectOptionModel(DtoJson.Deserialize<IndexPageDto>(json));

        public Brush PillBackground => IsSelected
            ? (Brush)Application.Current.Resources["CardBackgroundFillColorDefaultBrush"]
            : new SolidColorBrush(Colors.Transparent);

        public Brush PillForeground => IsSelected
            ? (Brush)Application.Current.Resources["TextFillColorPrimaryBrush"]
            : (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"];

        partial void OnIsSelectedChanged(bool value)
        {
            OnPropertyChanged(nameof(PillBackground));
            OnPropertyChanged(nameof(PillForeground));
        }
    }
}
