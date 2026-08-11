using CoolapkUWP.Common;
using CoolapkUWP.Models.Exceptions;
using CoolapkUWP.Models.Update;
using CommunityToolkit.WinUI.Helpers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using HttpClient = System.Net.Http.HttpClient;
using HttpResponseMessage = System.Net.Http.HttpResponseMessage;
using HttpStatusCode = System.Net.HttpStatusCode;

namespace CoolapkUWP.Helpers
{
    public static partial class NetworkHelper
    {
        public static readonly HttpClientHandler ClientHandler;
        public static readonly HttpClient Client;

        private static SemaphoreSlim semaphoreSlim;
        private static TokenCreator token;

        static NetworkHelper()
        {
            semaphoreSlim = new SemaphoreSlim(SettingsHelper.Get<int>(SettingsHelper.SemaphoreSlimCount));
            ThemeHelper.UISettingChanged.Add((arg) => Client?.DefaultRequestHeaders?.ReplaceDarkMode());
            ClientHandler = new HttpClientHandler();
            Client = new HttpClient(ClientHandler);
            SetRequestHeaders();
            SetLoginCookie();
        }

        public static void SetSemaphoreSlim(int initialCount)
        {
            semaphoreSlim.Dispose();
            semaphoreSlim = new SemaphoreSlim(initialCount);
        }

        public static void SetLoginCookie()
        {
            string Uid = SettingsHelper.Get<string>(SettingsHelper.Uid);
            string UserName = SettingsHelper.Get<string>(SettingsHelper.UserName);
            string Token = SettingsHelper.Get<string>(SettingsHelper.Token);

            if (!string.IsNullOrEmpty(Uid) && !string.IsNullOrEmpty(UserName) && !string.IsNullOrEmpty(Token))
            {
                SetLoginCookie(Uid, UserName, Token);
                SettingsHelper.InvokeLoginChanged(Uid, true);
            }
        }

        public static void SetLoginCookie(string Uid, string UserName, string Token)
        {
            if (string.IsNullOrEmpty(Uid) || string.IsNullOrEmpty(UserName) || string.IsNullOrEmpty(Token)) { return; }

            CookieContainer container = ClientHandler.CookieContainer;
            container.Add(new Cookie("uid", Uid) { Domain = ".coolapk.com", Path = "/" });
            container.Add(new Cookie("username", UserName) { Domain = ".coolapk.com", Path = "/" });
            container.Add(new Cookie("token", Token) { Domain = ".coolapk.com", Path = "/" });
        }

        public static void RemoveLoginCookie()
        {
            CookieCollection cookies = ClientHandler.CookieContainer.GetCookies(UriHelper.CoolapkUri);
            foreach (Cookie cookie in cookies)
            {
                if (cookie.Name == "uid" || cookie.Name == "username" || cookie.Name == "token")
                {
                    cookie.Expired = true;
                }
            }
        }

        public static void SetRequestHeaders()
        {
            APIVersions APIVersion = SettingsHelper.Get<APIVersions>(SettingsHelper.APIVersion);
            TokenVersions TokenVersion = SettingsHelper.Get<TokenVersions>(SettingsHelper.TokenVersion);
            string Culture = LanguageHelper.GetPrimaryLanguage();

            token = new TokenCreator(TokenVersion);
            Client.DefaultRequestHeaders.Clear();
            Client.DefaultRequestHeaders.Add("X-Sdk-Int", "30");
            Client.DefaultRequestHeaders.Add("X-Sdk-Locale", Culture);
            Client.DefaultRequestHeaders.Add("X-App-Mode", "universal");
            Client.DefaultRequestHeaders.Add("X-App-Channel", "coolapk");
            Client.DefaultRequestHeaders.Add("X-App-Id", "com.coolapk.market");
            Client.DefaultRequestHeaders.Add("X-App-Device", TokenCreator.DeviceCode);
            Client.DefaultRequestHeaders.Add("X-Dark-Mode", ThemeHelper.IsDarkTheme() ? "1" : "0");

            if (SettingsHelper.Get<bool>(SettingsHelper.IsCustomUA))
            {
                Client.DefaultRequestHeaders.UserAgent.ParseAdd(SettingsHelper.Get<UserAgent>(SettingsHelper.CustomUA).ToString());
            }
            else
            {
                var os = Environment.OSVersion.Version;
                var arch = System.Runtime.InteropServices.RuntimeInformation.OSArchitecture;
                Client.DefaultRequestHeaders.UserAgent.ParseAdd($"Dalvik/2.1.0 (Windows NT {os.Major}.{os.Minor}; Win{(arch.ToString().Contains("64") ? "64" : "32")}; {arch.ToString().ToLower()}; WebView/3.0) (#Build; ; ; _{os})");
            }

            switch (APIVersion)
            {
                case APIVersions.V6:
                    Client.DefaultRequestHeaders.UserAgent.ParseAdd(" +CoolMarket/6.10.6-1608291-universal");
                    Client.DefaultRequestHeaders.Add("X-App-Version", "6.10.6");
                    Client.DefaultRequestHeaders.Add("X-App-Code", "1608291");
                    break;
                case APIVersions.V7:
                    Client.DefaultRequestHeaders.UserAgent.ParseAdd(" +CoolMarket/7.9.6_S-1710201-universal");
                    Client.DefaultRequestHeaders.Add("X-App-Version", "7.9.6_S");
                    Client.DefaultRequestHeaders.Add("X-App-Code", "1710201");
                    Client.DefaultRequestHeaders.Add("X-Api-Version", "7");
                    break;
                case APIVersions.V8:
                    Client.DefaultRequestHeaders.UserAgent.ParseAdd(" +CoolMarket/8.7-1809041-universal");
                    Client.DefaultRequestHeaders.Add("X-App-Version", "8.7");
                    Client.DefaultRequestHeaders.Add("X-App-Code", "1809041");
                    Client.DefaultRequestHeaders.Add("X-Api-Version", "8");
                    break;
                case APIVersions.V9:
                    Client.DefaultRequestHeaders.UserAgent.ParseAdd(" +CoolMarket/9.6.3-1910291-universal");
                    Client.DefaultRequestHeaders.Add("X-App-Version", "9.6.3");
                    Client.DefaultRequestHeaders.Add("X-App-Code", "1910291");
                    Client.DefaultRequestHeaders.Add("X-Api-Version", "9");
                    break;
                case APIVersions.小程序:
                    Client.DefaultRequestHeaders.UserAgent.ParseAdd(" +CoolMarket/1.0-1902250-universal");
                    Client.DefaultRequestHeaders.Add("X-App-Version", "1.0");
                    Client.DefaultRequestHeaders.Add("X-App-Code", "1902250");
                    Client.DefaultRequestHeaders.Add("X-Api-Version", "9");
                    break;
                case APIVersions.V10:
                    Client.DefaultRequestHeaders.UserAgent.ParseAdd(" +CoolMarket/10.5.3-2009271-universal");
                    Client.DefaultRequestHeaders.Add("X-App-Version", "10.5.3");
                    Client.DefaultRequestHeaders.Add("X-App-Code", "2009271");
                    Client.DefaultRequestHeaders.Add("X-Api-Version", "10");
                    break;
                case APIVersions.V11:
                    Client.DefaultRequestHeaders.UserAgent.ParseAdd(" +CoolMarket/11.4.7-2112231-universal");
                    Client.DefaultRequestHeaders.Add("X-App-Version", "11.4.7");
                    Client.DefaultRequestHeaders.Add("X-App-Code", "2112231");
                    Client.DefaultRequestHeaders.Add("X-Api-Version", "11");
                    break;
                case APIVersions.V12:
                    Client.DefaultRequestHeaders.UserAgent.ParseAdd(" +CoolMarket/12.5.4-2212261-universal");
                    Client.DefaultRequestHeaders.Add("X-App-Version", "12.5.4");
                    Client.DefaultRequestHeaders.Add("X-Api-Supported", "2212261");
                    Client.DefaultRequestHeaders.Add("X-App-Code", "2212261");
                    Client.DefaultRequestHeaders.Add("X-Api-Version", "12");
                    break;
                case APIVersions.V13:
                    Client.DefaultRequestHeaders.UserAgent.ParseAdd(" +CoolMarket/13.4.1-2312121-universal");
                    Client.DefaultRequestHeaders.Add("X-App-Version", "13.4.1");
                    Client.DefaultRequestHeaders.Add("X-Api-Supported", "2312121");
                    Client.DefaultRequestHeaders.Add("X-App-Code", "2312121");
                    Client.DefaultRequestHeaders.Add("X-Api-Version", "13");
                    break;
                case APIVersions.Custom:
                    APIVersion CustomAPI = SettingsHelper.Get<APIVersion>(SettingsHelper.CustomAPI);
                    Client.DefaultRequestHeaders.UserAgent.ParseAdd($" {CustomAPI}");
                    Client.DefaultRequestHeaders.Add("X-App-Version", CustomAPI.Version);
                    Client.DefaultRequestHeaders.Add("X-Api-Supported", CustomAPI.VersionCode);
                    Client.DefaultRequestHeaders.Add("X-App-Code", CustomAPI.VersionCode);
                    Client.DefaultRequestHeaders.Add("X-Api-Version", CustomAPI.Version.Split('.').FirstOrDefault());
                    break;
                default:
                    break;
            }
        }

        public static IEnumerable<(string name, string value)> GetCoolapkCookies(Uri uri)
        {
            foreach (Cookie item in ClientHandler.CookieContainer.GetCookies(GetHost(uri)))
            {
                if (item.Name == "uid" ||
                    item.Name == "username" ||
                    item.Name == "token")
                {
                    yield return (item.Name, item.Value);
                }
            }
        }

        private static void ReplaceDarkMode(this HttpRequestHeaders headers)
        {
            const string name = "X-Dark-Mode";
            _ = headers.Remove(name);
            headers.Add(name, ThemeHelper.IsDarkTheme() ? "1" : "0");
        }

        private static void AddRequestHeaders(this HttpRequestMessage request, string requestName)
        {
            request.Headers.Add("X-App-Token", token.GetToken());
            if (requestName != null) { request.Headers.Add("X-Requested-With", requestName); }
        }

    }

    public static partial class NetworkHelper
    {
        public static async Task<string> PostAsync(Uri uri, HttpContent content, bool isBackground)
        {
            try
            {
                using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, uri) { Content = content })
                {
                    request.AddRequestHeaders("XMLHttpRequest");
                    using (HttpResponseMessage response = await Client.SendAsync(request))
                    {
                        return await response.Content.ReadAsStringAsync();
                    }
                }
            }
            catch (HttpRequestException e)
            {
                SettingsHelper.LogManager.CreateLogger(nameof(ImageCacheHelper)).LogError(e, e.ExceptionToMessage());
                if (!isBackground) { UIHelper.ShowHttpExceptionMessage(e); }
                return null;
            }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.CreateLogger(nameof(NetworkHelper)).LogError(ex, ex.ExceptionToMessage());
                return null;
            }
        }

        public static async Task<Stream> GetStreamAsync(Uri uri, string request = "XMLHttpRequest", bool isBackground = false)
        {
            try
            {
                using (HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Get, uri))
                {
                    httpRequest.AddRequestHeaders(request);
                    using (HttpResponseMessage response = await Client.SendAsync(httpRequest))
                    {
                        return await response.Content.ReadAsStreamAsync();
                    }
                }
            }
            catch (HttpRequestException e)
            {
                SettingsHelper.LogManager.CreateLogger(nameof(NetworkHelper)).LogError(e, e.ExceptionToMessage());
                if (!isBackground) { UIHelper.ShowHttpExceptionMessage(e); }
                return null;
            }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.CreateLogger(nameof(NetworkHelper)).LogError(ex, ex.ExceptionToMessage());
                return null;
            }
        }

        public static async Task<string> GetStringAsync(Uri uri, string request = "XMLHttpRequest", bool isBackground = false)
        {
            try
            {
                using (HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Get, uri))
                {
                    httpRequest.AddRequestHeaders(request);
                    using (HttpResponseMessage response = await Client.SendAsync(httpRequest))
                    {
                        return await response.Content.ReadAsStringAsync();
                    }
                }
            }
            catch (HttpRequestException e)
            {
                SettingsHelper.LogManager.CreateLogger(nameof(NetworkHelper)).LogError(e, e.ExceptionToMessage());
                if (!isBackground) { UIHelper.ShowHttpExceptionMessage(e); }
                return null;
            }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.CreateLogger(nameof(NetworkHelper)).LogError(ex, ex.ExceptionToMessage());
                return null;
            }
        }
    }

    public static partial class NetworkHelper
    {
        public static async Task<(string UID, string UserName, string UserAvatar)> GetUserInfoByNameAsync(string name, bool isBackground = false)
        {
            (string UID, string UserName, string UserAvatar) result = (string.Empty, string.Empty, string.Empty);

            if (string.IsNullOrEmpty(name))
            {
                throw new UserNameErrorException();
            }

            string str = string.Empty;
            try
            {
                str = await Client.GetStringAsync(new Uri($"https://www.coolapk.com/n/{name}"));

                JsonObject token = JsonNode.Parse(str).AsObject();
                if (token.TryGetPropertyValue("dataRow", out JsonNode v1))
                {
                    JsonObject dataRow = v1.AsObject();

                    if (dataRow.TryGetPropertyValue("uid", out JsonNode uid))
                    {
                        result.UID = uid.ToString();
                    }

                    if (dataRow.TryGetPropertyValue("username", out JsonNode username))
                    {
                        result.UserName = username.ToString();
                    }

                    if (dataRow.TryGetPropertyValue("userAvatar", out JsonNode userAvatar))
                    {
                        result.UserAvatar = userAvatar.ToString();
                    }

                    return result;
                }

                throw new Exception();
            }
            catch (HttpRequestException e)
            {
                SettingsHelper.LogManager.CreateLogger(nameof(NetworkHelper)).LogError(e, e.ExceptionToMessage());
                if (!isBackground) { UIHelper.ShowHttpExceptionMessage(e); }
                return result;
            }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.CreateLogger(nameof(NetworkHelper)).LogError(ex, ex.ExceptionToMessage());
                if (string.IsNullOrWhiteSpace(str)) { throw ex; }
                JsonObject o = JsonNode.Parse(str).AsObject();
                if (o == null) { throw ex; }
                else { throw new CoolapkMessageException(o); }
            }
        }

        public static Uri GetHost(Uri uri) => new Uri("https://" + uri.Host);

        public static async Task<string> ExpandShortUrlAsync(this Uri shortUrl)
        {
            try
            {
                using var handler = new HttpClientHandler { AllowAutoRedirect = false };
                using var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(10) };
                using var response = await client.GetAsync(shortUrl, HttpCompletionOption.ResponseHeadersRead);
                if (response.StatusCode == HttpStatusCode.Found || response.StatusCode == HttpStatusCode.MovedPermanently)
                {
                    var location = response.Headers.Location;
                    return location?.ToString() ?? shortUrl.ToString();
                }
            }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.CreateLogger(nameof(NetworkHelper)).LogDebug(ex, ex.ExceptionToMessage());
            }
            return shortUrl.ToString();
        }

        public static Uri ValidateAndGetUri(this string url)
        {
            if (string.IsNullOrWhiteSpace(url)) { return null; }
            Uri uri = null;
            try
            {
                uri = url.Contains("://") ? new Uri(url)
                    : url[0] == '/' ? new Uri(UriHelper.CoolapkUri, url)
                    : new Uri($"https://{url}");
            }
            catch (FormatException ex)
            {
                SettingsHelper.LogManager.CreateLogger(nameof(NetworkHelper)).LogWarning(ex, ex.ExceptionToMessage());
            }
            return uri;
        }
    }
}
