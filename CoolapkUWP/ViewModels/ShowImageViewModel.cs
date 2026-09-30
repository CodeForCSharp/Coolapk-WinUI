using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Images;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CoolapkUWP.ViewModels
{
    public partial class ShowImageViewModel : ObservableObject, IViewModel
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
                    index = value;
                    OnPropertyChanged();
                    Title = GetTitle(Images[value].Uri);
                    ShowOrigin = Images[value].Type.HasFlag(ImageType.Small);
                }
            }
        }

        [ObservableProperty]
        public partial bool IsShowHub { get; set; }

        [ObservableProperty]
        public partial IList<ImageModel> Images { get; private set; }

        [ObservableProperty]
        public partial bool ShowOrigin { get; set; }

        private readonly IList<ImageModel> sourceImages;

        public ShowImageViewModel(ImageModel image)
        {
            sourceImages = image.ContextArray.Any() ? image.ContextArray : new List<ImageModel> { image };
            // 浏览页使用独立的原图模型，不改动列表/详情页共享的缩略图模型。
            Images = sourceImages.Select(item => new ImageModel(item.Uri, item.Type & ~ImageType.Small)).ToList();
            Index = image.ContextArray.Any() ? sourceImages.IndexOf(image) : 0;
        }

        public Task Refresh(bool reset = false)
        {
            Images[Index].Refresh();
            return Task.CompletedTask;
        }

        bool IViewModel.IsEqual(IViewModel other) => other is ShowImageViewModel model && IsEqual(model);

        public bool IsEqual(ShowImageViewModel other) => sourceImages == other.sourceImages;

        private string GetTitle(string url)
        {
            Match match = ImageNameRegex.Match(url);
            ImageName = match.Success ? match.Value : "查看图片";
            return $"{ImageName} ({Index + 1}/{Images.Count})";
        }
    }
}
