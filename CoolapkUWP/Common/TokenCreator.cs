using CoolapkUWP.Helpers;
using CommunityToolkit.WinUI.Helpers;
using System;
using System.Linq;
using System.Security.Cryptography;
using Windows.ApplicationModel;

namespace CoolapkUWP.Common
{
    public class TokenCreator
    {
        private static readonly RandomNumberGenerator Rng = RandomNumberGenerator.Create();
        private static readonly string guid = Guid.NewGuid().ToString();
        private static readonly string aid = RandHexString(16);
        private static readonly string mac = RandMacAddress();
        private static readonly string SystemManufacturer = string.Empty;
        private static readonly string SystemProductName = string.Empty;

        public static string DeviceCode;

        private readonly TokenVersions TokenVersion;

        // token 依赖的时戳为整数秒（ConvertDateTimeToUnixTimeStamp 会四舍五入到秒），
        // 故同一秒内结果完全一致，可按秒缓存以避开每请求一次的 BCrypt。
        private readonly object _tokenLock = new object();
        private long _cachedSecond = -1;
        private string _cachedToken;

        static TokenCreator()
        {
            DeviceCode = CreateDeviceCode(aid, mac, SystemManufacturer, SystemManufacturer, SystemProductName, $"CoolapkUWP {Package.Current.Id.Version.ToFormattedString()}");
        }

        public TokenCreator(TokenVersions version = TokenVersions.TokenV2) => TokenVersion = version;

        /// <summary>
        /// GetToken Generate a token with random device info
        /// </summary>
        public string GetToken()
        {
            long second = (long)DateTime.Now.ConvertDateTimeToUnixTimeStamp();

            lock (_tokenLock)
            {
                if (second == _cachedSecond)
                {
                    return _cachedToken;
                }

                _cachedToken = TokenVersion == TokenVersions.TokenV1
                    ? GetCoolapkAppToken(second)
                    : GetTokenWithDeviceCode(DeviceCode, second);
                _cachedSecond = second;
                return _cachedToken;
            }
        }

        /// <summary>
        /// GetTokenWithDeviceCode Generate a token with your device code
        /// </summary>
        private string GetTokenWithDeviceCode(string deviceCode, long second)
        {
            string timeStamp = second.ToString();

            string base64TimeStamp = timeStamp.GetBase64(true);
            string md5TimeStamp = timeStamp.GetMD5();
            string md5DeviceCode = deviceCode.GetMD5();

            string token = $"token://com.coolapk.market/dcf01e569c1e3db93a3d0fcf191a622c?{md5TimeStamp}${md5DeviceCode}&com.coolapk.market";
            string base64Token = token.GetBase64(true);
            string md5Base64Token = base64Token.GetMD5();
            string md5Token = token.GetMD5();

            string bcryptSalt = $"{$"$2y$10${base64TimeStamp}/{md5Token}".Substring(0, 31)}u";
            string bcryptresult = BCrypt.Net.BCrypt.HashPassword(md5Base64Token, bcryptSalt);

            string appToken = $"v2{bcryptresult.GetBase64(true)}";

            return appToken;
        }

        private static string GetCoolapkAppToken(long second)
        {
            string hex_timeStamp = $"0x{Convert.ToString(second, 16)}";
            // 时间戳加密
            string md5_timeStamp = $"{second}".GetMD5();
            string token = $"token://com.coolapk.market/c67ef5943784d09750dcfbb31020f0ab?{md5_timeStamp}${guid}&com.coolapk.market";
            string md5_token = token.GetBase64().GetMD5();
            string appToken = $"{md5_token}{guid}{hex_timeStamp}";
            return appToken;
        }

        /// <summary>
        /// CreateDeviceCode Generace your custom device code
        /// </summary>
        private static string CreateDeviceCode(string aid, string mac, string manufacturer, string brand, string model, string buildNumber)
        {
            return $"{aid}; ; ; {mac}; {manufacturer}; {brand}; {model}; {buildNumber}".GetBase64(true).Reverse();
        }

        private static string RandMacAddress()
        {
            byte[] bytes = new byte[6];
            Rng.GetBytes(bytes);
            return string.Join(":", bytes.Select(b => b.ToString("x2")));
        }

        private static string RandHexString(int n)
        {
            byte[] bytes = new byte[n];
            Rng.GetBytes(bytes);
            return Convert.ToHexString(bytes);
        }
    }

    public enum TokenVersions
    {
        TokenV1,
        TokenV2
    }

    public enum APIVersions
    {
        Custom = 4,
        小程序,
        V6,
        V7,
        V8,
        V9,
        V10,
        V11,
        V12,
        V13,
    }
}
