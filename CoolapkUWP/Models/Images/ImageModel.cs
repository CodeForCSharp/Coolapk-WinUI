using CoolapkUWP.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using Windows.Foundation;

namespace CoolapkUWP.Models.Images
{
    /// <summary>
    /// 图片描述：只保存地址与类型，解码与显示由 <see cref="Controls.ImageEx"/> 负责。
    /// </summary>
    [WinRT.GeneratedBindableCustomProperty]
    [INotifyPropertyChanged]
    public partial class ImageModel : IPic
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsGif))]
        public partial string Uri { get; set; }

        [ObservableProperty]
        public partial ImageType Type { get; set; }

        protected List<ImageModel> contextArray = new List<ImageModel>();
        public List<ImageModel> ContextArray
        {
            get => contextArray;
            set
            {
                if (contextArray == null || contextArray.Count == 0)
                {
                    contextArray = value;
                    OnPropertyChanged();
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

        public ImageModel(string uri, ImageType type)
        {
            Uri = uri;
            Type = type;
        }

        public event TypedEventHandler<ImageModel, object> RefreshRequested;

        public void Refresh() => RefreshRequested?.Invoke(this, null);

        public override string ToString() => Uri;
    }
}
