using CoolapkUWP.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;

namespace CoolapkUWP.ViewModels.BrowserPages
{
    public partial class BrowserViewModel : ObservableObject, IViewModel
    {
        private readonly ResourceLoader _loader = ResourceLoader.GetForViewIndependentUse("BrowserPage");

        [ObservableProperty]
        public partial string Title { get; set; }

        [ObservableProperty]
        public partial Uri Uri { get; set; }

        [ObservableProperty]
        public partial bool IsLoginPage { get; set; }

        public BrowserViewModel(string url)
        {
            if (!url.Contains("://")) { url = $"https://{url}"; }
            Uri = url.ValidateAndGetUri();
            IsLoginPage = url == UriHelper.LoginUri;
            Title = _loader.GetString("Title");
        }

        public Task Refresh(bool reset) => throw new NotImplementedException();

        bool IViewModel.IsEqual(IViewModel other) => other is BrowserViewModel model && IsEqual(model);

        public bool IsEqual(BrowserViewModel other) => Uri == other.Uri;
    }
}
