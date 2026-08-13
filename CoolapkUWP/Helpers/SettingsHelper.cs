using CoolapkUWP.Models.Update;
using CommunityToolkit.WinUI.Helpers;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Windows.Foundation;
using Microsoft.UI.Xaml;

namespace CoolapkUWP.Helpers
{
    internal static partial class SettingsHelper
    {
        public const string Uid = nameof(Uid);
        public const string Token = nameof(Token);
        public const string UserName = nameof(UserName);
        public const string CustomUA = nameof(CustomUA);
        public const string IsUseAPI2 = nameof(IsUseAPI2);
        public const string CustomAPI = nameof(CustomAPI);
        public const string IsCustomUA = nameof(IsCustomUA);
        public const string APIVersion = nameof(APIVersion);
        public const string UpdateDate = nameof(UpdateDate);
        public const string IsNoPicsMode = nameof(IsNoPicsMode);
        public const string TokenVersion = nameof(TokenVersion);
        public const string IsUseCompositor = nameof(IsUseCompositor);
        public const string CurrentLanguage = nameof(CurrentLanguage);
        public const string SelectedAppTheme = nameof(SelectedAppTheme);
        public const string SemaphoreSlimCount = nameof(SemaphoreSlimCount);
        public const string IsDisplayOriginPicture = nameof(IsDisplayOriginPicture);

        public static Type Get<Type>(string key) => LocalObject.Read<Type>(key);
        public static void Set<Type>(string key, Type value) => LocalObject.Save(key, value);

        public static void SetDefaultSettings()
        {
            SetDefault(Uid, string.Empty);
            SetDefault(Token, string.Empty);
            SetDefault(UserName, string.Empty);
            SetDefault(CustomUA, UserAgent.Parse(NetworkHelper.Client.DefaultRequestHeaders.UserAgent.ToString()));
            SetDefault(IsUseAPI2, true);
            SetDefault(CustomAPI, new APIVersion("9.2.2", "1905301"));
            SetDefault(IsCustomUA, false);
            SetDefault(APIVersion, Common.APIVersions.V13);
            SetDefault(UpdateDate, new DateTime());
            SetDefault(IsNoPicsMode, false);
            SetDefault(TokenVersion, Common.TokenVersions.TokenV2);
            SetDefault(IsUseCompositor, true);
            SetDefault(CurrentLanguage, LanguageHelper.AutoLanguageCode);
            SetDefault(SelectedAppTheme, ElementTheme.Default);
            SetDefault(SemaphoreSlimCount, Environment.ProcessorCount);
            SetDefault(IsDisplayOriginPicture, false);
        }

        private static void SetDefault<T>(string key, T value)
        {
            if (!LocalObject.KeyExists(key))
            {
                LocalObject.Save(key, value);
            }
        }
    }

    internal static partial class SettingsHelper
    {
        public static event TypedEventHandler<string, bool> LoginChanged;
        public static readonly LocalSettingsStorage LocalObject = new LocalSettingsStorage(new SystemTextJsonObjectSerializer());
        public static readonly ILoggerFactory LogManager = new LoggerFactory(new[] { new FileLoggerProvider() });

        static SettingsHelper() => SetDefaultSettings();

        public static void InvokeLoginChanged(string sender, bool args) => LoginChanged?.Invoke(sender, args);

        public static async Task<bool> Login()
        {
            string uid = string.Empty, token = string.Empty, userName = string.Empty;
            foreach ((string name, string value) in NetworkHelper.GetCoolapkCookies(UriHelper.CoolapkUri))
            {
                switch (name)
                {
                    case "uid":
                        uid = value;
                        break;
                    case "username":
                        userName = value;
                        break;
                    case "token":
                        token = value;
                        break;
                    default:
                        break;
                }
            }
            if (string.IsNullOrEmpty(uid) || string.IsNullOrEmpty(token) || string.IsNullOrEmpty(userName) || !await RequestHelper.CheckLogin())
            {
                Logout();
                return false;
            }
            else
            {
                Set(Uid, uid);
                Set(Token, token);
                Set(UserName, userName);
                InvokeLoginChanged(uid, true);
                return true;
            }
        }

        public static async Task<bool> Login(string Uid, string UserName, string Token)
        {
            if (!string.IsNullOrEmpty(Uid) && !string.IsNullOrEmpty(UserName) && !string.IsNullOrEmpty(Token))
            {
                NetworkHelper.SetLoginCookie(Uid, UserName, Token);
                if (await RequestHelper.CheckLogin())
                {
                    Set(SettingsHelper.Uid, Uid);
                    Set(SettingsHelper.Token, Token);
                    Set(SettingsHelper.UserName, UserName);
                    InvokeLoginChanged(Uid, true);
                    return true;
                }
                else
                {
                    Logout();
                    return false;
                }
            }
            return false;
        }

        public static async Task<bool> CheckLoginAsync()
        {
            string uid = string.Empty, token = string.Empty, userName = string.Empty;
            foreach ((string name, string value) in NetworkHelper.GetCoolapkCookies(UriHelper.CoolapkUri))
            {
                switch (name)
                {
                    case "uid":
                        uid = value;
                        break;
                    case "username":
                        userName = value;
                        break;
                    case "token":
                        token = value;
                        break;
                    default:
                        break;
                }
            }
            return !string.IsNullOrEmpty(uid) && !string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(userName) && await RequestHelper.CheckLogin();
        }

        public static void Logout()
        {
            NetworkHelper.RemoveLoginCookie();
            Set(Uid, string.Empty);
            Set(Token, string.Empty);
            Set(UserName, string.Empty);
            InvokeLoginChanged(string.Empty, false);
        }
    }

    public class LocalSettingsStorage
    {
        private readonly IObjectSerializer _serializer;
        private readonly Windows.Storage.ApplicationDataContainer _settings;

        public LocalSettingsStorage(IObjectSerializer serializer)
        {
            _serializer = serializer;
            _settings = Windows.Storage.ApplicationData.Current.LocalSettings;
        }

        public T Read<T>(string key)
        {
            if (_settings.Values.TryGetValue(key, out object value) && value is string str)
                return _serializer.Deserialize<T>(str);
            return default;
        }

        public void Save<T>(string key, T value)
        {
            if (value == null)
                _settings.Values.Remove(key);
            else
                _settings.Values[key] = _serializer.Serialize(value);
        }

        public void Clear() => _settings.Values.Clear();

        public bool KeyExists(string key) => _settings.Values.ContainsKey(key);
    }

    public interface IObjectSerializer
    {
        string Serialize<T>(T value);
        T Deserialize<T>(string value);
    }

    public class SystemTextJsonObjectSerializer : IObjectSerializer
    {
        private readonly JsonSerializerOptions _options = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = true,
            TypeInfoResolver = JsonContext.Default
        };

        string IObjectSerializer.Serialize<T>(T value) => JsonSerializer.Serialize(value, _options);

        public T Deserialize<T>(string value) => JsonSerializer.Deserialize<T>(value, _options);
    }
}
