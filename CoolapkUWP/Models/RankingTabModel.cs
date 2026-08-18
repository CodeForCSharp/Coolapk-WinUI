using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace CoolapkUWP.Models
{
    /// <summary>
    /// 排行榜页顶部的榜单切换项(selectorLinkCard 子实体)，选中时使用酷安主题绿背景。
    /// </summary>
    [INotifyPropertyChanged]
    public partial class RankingTabModel
    {
        private static readonly Brush CoolapkGreenBrush = new SolidColorBrush(Color.FromArgb(255, 0x0F, 0x9D, 0x58));

        public string Title { get; }

        public string Url { get; }

        /// <summary>右侧分数榜分数字段(榜单 URL 的 rightTopField)，无则为 null。</summary>
        public string RightTopField { get; }

        /// <summary>右侧分数榜标签(榜单 URL 的 rightBottomText，如 "续航分"/"小时")，无则为 null。</summary>
        public string RightBottomText { get; }

        [ObservableProperty]
        public partial bool IsSelected { get; set; }

        public RankingTabModel(string title, string url, string rightTopField = null, string rightBottomText = null)
        {
            Title = title;
            Url = url;
            RightTopField = rightTopField;
            RightBottomText = rightBottomText;
        }

        public Brush PillBackground => IsSelected
            ? CoolapkGreenBrush
            : (Brush)Application.Current.Resources["SubtleFillColorTertiaryBrush"];

        public Brush PillForeground => IsSelected
            ? new SolidColorBrush(Colors.White)
            : (Brush)Application.Current.Resources["TextFillColorPrimaryBrush"];

        partial void OnIsSelectedChanged(bool value)
        {
            OnPropertyChanged(nameof(PillBackground));
            OnPropertyChanged(nameof(PillForeground));
        }
    }
}
