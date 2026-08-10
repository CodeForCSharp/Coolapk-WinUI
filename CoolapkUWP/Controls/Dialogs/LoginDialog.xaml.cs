using CoolapkUWP.Helpers;
using System.Net;
using Microsoft.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace CoolapkUWP.Controls
{
    public sealed partial class LoginDialog : ContentDialog
    {
        private string _UID = SettingsHelper.Get<string>(SettingsHelper.Uid);
        internal string UID
        {
            get => _UID;
            set
            {
                if (_UID != value)
                {
                    _UID = value;
                    CheckText();
                }
            }
        }

        private string userName = SettingsHelper.Get<string>(SettingsHelper.UserName);
        internal string UserName
        {
            get => userName;
            set
            {
                if (userName != value)
                {
                    userName = value;
                    CheckText();
                }
            }
        }

        private string token = SettingsHelper.Get<string>(SettingsHelper.Token);
        internal string Token
        {
            get => token;
            set
            {
                if (token != value)
                {
                    token = value;
                    CheckText();
                }
            }
        }

        public LoginDialog()
        {
            InitializeComponent();
            CheckText();
        }

        private void OnClosing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            if (args.Result == ContentDialogResult.Primary)
            {
                if (string.IsNullOrWhiteSpace(UID) && !string.IsNullOrWhiteSpace(UserName))
                {
                    GetText(UserName);
                }
                else if (string.IsNullOrWhiteSpace(UserName) && !string.IsNullOrWhiteSpace(UID))
                {
                    GetText(UID);
                }
                NetworkHelper.SetLoginCookie(UID, UserName, Token);
            }
        }

        private void CheckText() => IsPrimaryButtonEnabled = !string.IsNullOrEmpty(Token) && (!string.IsNullOrEmpty(UID) || !string.IsNullOrEmpty(UserName));

        private void GetText(string name)
        {
            (string UID, string UserName, string UserAvatar) results = UIHelper.AwaitByTaskCompleteSource(() => NetworkHelper.GetUserInfoByNameAsync(name));
            if (!string.IsNullOrWhiteSpace(results.UID))
            {
                UID = results.UID;
            }
            if (!string.IsNullOrWhiteSpace(results.UserName))
            {
                UserName = WebUtility.UrlEncode(results.UserName);
            }
        }
    }
}
