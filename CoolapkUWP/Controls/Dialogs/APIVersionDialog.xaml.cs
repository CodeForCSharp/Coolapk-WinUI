using CoolapkUWP.Common;
using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using CoolapkUWP.Models.Update;
using System.Text.Json.Nodes;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了"内容对话框"项模板

namespace CoolapkUWP.Controls.Dialogs
{
    public sealed partial class APIVersionDialog : ContentDialog, INotifyPropertyChanged
    {
        internal APIVersion APIVersion { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private async void RaisePropertyChangedEvent([CallerMemberName] string name = null)
        {
            if (name != null)
            {
                await DispatcherQueue.ResumeForegroundAsync();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }
        
        public APIVersionDialog(string line)
        {
            InitializeComponent();
            APIVersion = APIVersion.Parse(line);
        }

        private void OnClosing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            if (args.Result == ContentDialogResult.Primary)
            {
                SettingsHelper.Set(SettingsHelper.CustomAPI, APIVersion);
                NetworkHelper.SetRequestHeaders();
            }
        }

        private async void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            UIHelper.ShowProgressBar();
            (bool isSucceed, JsonNode result) = await RequestHelper.GetDataAsync(UriHelper.GetUri(UriType.GetAppDetail, "com.coolapk.market"));
            if (isSucceed)
            {
                AppModel model = new AppModel(result.AsObject());
                if (!string.IsNullOrEmpty(model.VersionCode) && !string.IsNullOrEmpty(model.VersionName))
                {
                    APIVersion = new APIVersion(model.VersionName, model.VersionCode);
                    RaisePropertyChangedEvent(nameof(APIVersion));
                }
            }
            UIHelper.HideProgressBar();
        }
    }
}
