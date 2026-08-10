using CoolapkUWP.Common;
using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Images;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;

namespace CoolapkUWP.ViewModels
{
    public partial class ShowImageViewModel : IViewModel
    {
        private string ImageName = string.Empty;
        public string ImageNameText => ImageName;

        public DispatcherQueue Dispatcher { get; }

        private string title;
        public string Title
        {
            get => title;
            protected set
            {
                if (title != value)
                {
                    title = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

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
                    RaisePropertyChangedEvent();
                    Title = GetTitle(Images[value].Uri);
                    ShowOrigin = Images[value].Type.HasFlag(ImageType.Small);
                    _ = Images[value].LoadAsync(0);
                }
            }
        }

        private bool isLoading;
        public bool IsLoading
        {
            get => isLoading;
            protected set
            {
                if (isLoading != value)
                {
                    isLoading = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private bool isShowHub = true;
        public bool IsShowHub
        {
            get => isShowHub;
            set
            {
                if (isShowHub != value)
                {
                    isShowHub = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private IList<ImageModel> images;
        public IList<ImageModel> Images
        {
            get => images;
            private set
            {
                if (images != value)
                {
                    images = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private bool showOrigin = false;
        public bool ShowOrigin
        {
            get => showOrigin;
            set
            {
                if (showOrigin != value)
                {
                    showOrigin = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private async void RaisePropertyChangedEvent([CallerMemberName] string name = null)
        {
            if (name != null)
            {
                await Dispatcher.ResumeForegroundAsync();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }

        public ShowImageViewModel(ImageModel image, DispatcherQueue dispatcher)
        {
            Dispatcher = dispatcher;
            Images = image.ContextArray.Any() ? image.ContextArray : new List<ImageModel> { image };
            foreach (ImageModel Image in Images)
            {
                Image.Type &= (ImageType)0xFE;
            }
            Index = image.ContextArray.Any() ? Images.IndexOf(image) : 0;
        }

        ~ShowImageViewModel()
        {
            foreach (ImageModel image in images)
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
