using CoolapkUWP.Helpers;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace CoolapkUWP.Controls
{
    public sealed partial class LoginDialog : ContentDialog
    {
        private bool isFetching;
        private bool hasFetched;
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

        private async void OnClosing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            if (args.Result != ContentDialogResult.Primary) { return; }

            if (!hasFetched && string.IsNullOrWhiteSpace(UID) != string.IsNullOrWhiteSpace(UserName))
            {
                if (isFetching)
                {
                    args.Cancel = true;
                    return;
                }

                args.Cancel = true;
                isFetching = true;
                IsPrimaryButtonEnabled = false;
                string name = string.IsNullOrWhiteSpace(UID) ? UserName : UID;
                bool success = await FetchAndApplyAsync(name);
                isFetching = false;
                if (success)
                {
                    hasFetched = true;
                    try { Hide(); }
                    catch (Exception ex) { SettingsHelper.LogManager.CreateLogger(nameof(LoginDialog)).LogError(ex, ex.ExceptionToMessage()); }
                }
                else
                {
                    CheckText();
                }
                return;
            }

            NetworkHelper.SetLoginCookie(UID, UserName, Token);
        }

        private void CheckText() => IsPrimaryButtonEnabled = !string.IsNullOrEmpty(Token) && (!string.IsNullOrEmpty(UID) || !string.IsNullOrEmpty(UserName));

        private async Task<bool> FetchAndApplyAsync(string name)
        {
            try
            {
                (string UID, string UserName, string UserAvatar) results = await NetworkHelper.GetUserInfoByNameAsync(name);
                if (!string.IsNullOrWhiteSpace(results.UID))
                {
                    UID = results.UID;
                }
                if (!string.IsNullOrWhiteSpace(results.UserName))
                {
                    UserName = WebUtility.UrlEncode(results.UserName);
                }
                return true;
            }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.CreateLogger(nameof(LoginDialog)).LogError(ex, ex.ExceptionToMessage());
                return false;
            }
        }
    }
}
