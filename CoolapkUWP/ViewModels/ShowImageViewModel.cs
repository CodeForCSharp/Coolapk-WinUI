using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Images;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CoolapkUWP.ViewModels
{
    public partial class ShowImageViewModel : ObservableObject, IViewModel, IDisposable
    {
        private static readonly Regex ImageNameRegex = new Regex(@"[^/]+(?!.*/)");

        private string ImageName = string.Empty;
        public string ImageNameText => ImageName;

        [ObservableProperty]
        public partial string Title { get; protected set; }

        private int index = -1;
        public int Index
        {
            get => index;
            set
            {
                if (index != value)
                {
                    if (index != -1) { ResigerImage(Images[index], Images[value]); }
                    index = value;
                    OnPropertyChanged();
                    Title = GetTitle(Images[value].Uri);
                    ShowOrigin = Images[value].Type.HasFlag(ImageType.Small);
                    _ = Images[value].LoadAsync(0);
                }
            }
        }

        [ObservableProperty]
        public partial bool IsLoading { get; protected set; }

        [ObservableProperty]
        public partial bool IsShowHub { get; set; }

        [ObservableProperty]
        public partial IList<ImageModel> Images { get; private set; }

        [ObservableProperty]
        public partial bool ShowOrigin { get; set; }

        // 浏览页会把共享模型的 Small 标志清掉以加载原图；这里记录原值，退出时恢复，
        // 避免列表/详情页的缩略图在浏览过后被永久切成原图模式。
        private readonly Dictionary<ImageModel, ImageType> originalTypes = new Dictionary<ImageModel, ImageType>();

        public ShowImageViewModel(ImageModel image)
        {
            Images = image.ContextArray.Any() ? image.ContextArray : new List<ImageModel> { image };
            foreach (ImageModel Image in Images)
            {
                if (Image.Type.HasFlag(ImageType.Small))
                {
                    originalTypes[Image] = Image.Type;
                    Image.Type &= (ImageType)0xFE;
                }
            }
            Index = image.ContextArray.Any() ? Images.IndexOf(image) : 0;
        }

        public void Dispose()
        {
            foreach (ImageModel image in Images)
            {
                image.LoadStarted -= OnLoadStarted;
                image.LoadCompleted -= OnLoadCompleted;
                if (originalTypes.TryGetValue(image, out ImageType type))
                {
                    originalTypes.Remove(image);
                    image.Type = type; // 触发回退到小图缓存
                }
            }
        }

        public async Task Refresh(bool reset = false) => await Images[Index].Refresh();

        bool IViewModel.IsEqual(IViewModel other) => other is ShowImageViewModel model && IsEqual(model);

        public bool IsEqual(ShowImageViewModel other) => Images == other.Images;

        private string GetTitle(string url)
        {
            Match match = ImageNameRegex.Match(url);
            ImageName = match.Success ? match.Value : "查看图片";
            return $"{ImageName} ({Index + 1}/{Images.Count})";
        }

        private void ResigerImage(ImageModel oldvalue, ImageModel newvalue)
        {
            oldvalue.LoadStarted -= OnLoadStarted;
            oldvalue.LoadCompleted -= OnLoadCompleted;
            newvalue.LoadStarted += OnLoadStarted;
            newvalue.LoadCompleted += OnLoadCompleted;
        }

        private void OnLoadStarted(ImageModel sender, object args) => IsLoading = true;

        private void OnLoadCompleted(ImageModel sender, object args) => IsLoading = false;
    }
}
