using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json.Nodes;

namespace CoolapkUWP.Models.Pages
{
    [INotifyPropertyChanged]
    public abstract partial class FeedListDetailBase : Entity
    {
        [ObservableProperty]
        public partial bool IsCopyEnabled { get; set; }

        [ObservableProperty]
        public partial bool Followed { get; set; }

        [ObservableProperty]
        public partial string FollowGlyph { get; set; }

        [ObservableProperty]
        public partial string FollowStatus { get; set; }

        partial void OnFollowedChanged(bool value) => OnFollowChanged();

        /// <summary>
        /// 根据关注状态刷新按钮文案与图标。
        /// </summary>
        protected abstract void OnFollowChanged();

        protected FeedListDetailBase() { EntityFixed = true; }
    }

}
