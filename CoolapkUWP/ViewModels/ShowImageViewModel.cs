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
    public partial class ShowImageViewModel : ObservableObject, IViewModel
    {
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

        public ShowImageViewModel(ImageModel image)
        {
            Images = image.ContextArray.Any() ? image.ContextArray : new List<ImageModel> { image };
            foreach (ImageModel Image in Images)
            {
                Image.Type &= (ImageType)0xFE;
            }
            Index = image.ContextArray.Any() ? Images.IndexOf(image) : 0;
        }

        ~ShowImageViewModel()
        {
            foreach (ImageModel image in Images)
            {
                image.LoadStarted -= OnLoadStarted;
                image.LoadCompleted -= OnLoadCompleted;
            }
        }

        public async Task Refresh(bool reset = false) => await Images[Index].Refresh();

        bool IViewModel.IsEqual(IViewModel other) => other is ShowImageViewModel model && IsEqual(model);

        public bool IsEqual(ShowImageViewModel other) => Images == other.Images;

        private string GetTitle(string url)
        {
            Regex regex = new Regex(@"[^/]+(?!.*/)");
            ImageName = regex.IsMatch(url) ? regex.Match(url).Value : "查看图片";
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
