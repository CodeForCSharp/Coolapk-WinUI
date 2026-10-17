using CoolapkUWP.Common;
using CoolapkUWP.Models.Exceptions;
using CommunityToolkit.WinUI.Helpers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
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
            semaphoreSlim = new SemaphoreSlim(Math.Max(1, SettingsHelper.Get<int>(SettingsHelper.SemaphoreSlimCount)));
            ThemeHelper.UISettingChanged.Add((arg) => Client?.DefaultRequestHeaders?.ReplaceDarkMode());
            ClientHandler = new HttpClientHandler { CookieContainer = new CookieContainer() };
            Client = new HttpClient(ClientHandler);
            SetRequestHeaders();
            SetLoginCookie();
        }

        public static void SetSemaphoreSlim(int initialCount)
        {
            Interlocked.Exchange(ref semaphoreSlim, new SemaphoreSlim(Math.Max(1, initialCount)));
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
            string Culture = LanguageHelper.GetPrimaryLanguage();

            token = new TokenCreator();
            Client.DefaultRequestHeaders.Clear();
            Client.DefaultRequestHeaders.Add("X-Sdk-Int", "36");
            Client.DefaultRequestHeaders.Add("X-Sdk-Locale", Culture);
            Client.DefaultRequestHeaders.Add("X-App-Mode", "universal");
            Client.DefaultRequestHeaders.Add("X-App-Channel", "coolapk");
            Client.DefaultRequestHeaders.Add("X-App-Id", "com.coolapk.market");
            Client.DefaultRequestHeaders.Add("X-App-Device", TokenCreator.DeviceCode);
            Client.DefaultRequestHeaders.Add("X-Dark-Mode", ThemeHelper.IsDarkTheme() ? "1" : "0");
            // v16 服务端会同时校验 UA 和版本头，官方 Android 16.2.0 的 UA 才与 Token V3 配套。
            Client.DefaultRequestHeaders.UserAgent.ParseAdd($"Dalvik/2.1.0 (Linux; U; Android 16; 23113RKC6C Build/AQ3A.250226.002) (#Build; Redmi; 23113RKC6C; AQ3A.250226.002; HyperOS_3.0; 3.0.1.0) +CoolMarket/{TokenCreator.AppVersion}-{TokenCreator.AppCode}-universal");
            Client.DefaultRequestHeaders.Add("X-App-Version", TokenCreator.AppVersion);
            Client.DefaultRequestHeaders.Add("X-App-Supported", TokenCreator.AppCode.ToString());
            Client.DefaultRequestHeaders.Add("X-App-Code", TokenCreator.AppCode.ToString());
            Client.DefaultRequestHeaders.Add("X-Api-Version", "16");
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
                        string result = await response.Content.ReadAsStringAsync();
                        if (!response.IsSuccessStatusCode)
                        {
                            LogHttpError(uri, response, result);
                            return null;
                        }
                        return result;
                    }
                }
            }
            catch (HttpRequestException e)
            {
                SettingsHelper.LogManager.CreateLogger(nameof(NetworkHelper)).LogError(e, e.ExceptionToMessage());
                if (!isBackground) { MessageHelper.ShowHttpExceptionMessage(e); }
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
                        string result = await response.Content.ReadAsStringAsync();
                        if (!response.IsSuccessStatusCode)
                        {
                            LogHttpError(uri, response, result);
                            return null;
                        }
                        return result;
                    }
                }
            }
            catch (HttpRequestException e)
            {
                SettingsHelper.LogManager.CreateLogger(nameof(NetworkHelper)).LogError(e, e.ExceptionToMessage());
                if (!isBackground) { MessageHelper.ShowHttpExceptionMessage(e); }
                return null;
            }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.CreateLogger(nameof(NetworkHelper)).LogError(ex, ex.ExceptionToMessage());
                return null;
            }
        }

        /// <summary>
        /// 非 2xx 响应过去会被当成“空数据”静默丢掉，导致接口全挂时日志里什么都没有。
        /// </summary>
        private static void LogHttpError(Uri uri, HttpResponseMessage response, string content)
        {
            string body = string.IsNullOrEmpty(content)
                ? string.Empty
                : $" body={content.Substring(0, Math.Min(500, content.Length))}";
            SettingsHelper.LogManager.CreateLogger(nameof(NetworkHelper)).LogError(
                $"HTTP {(int)response.StatusCode} {response.ReasonPhrase}: {uri}{body}");
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
                if (!isBackground) { MessageHelper.ShowHttpExceptionMessage(e); }
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
