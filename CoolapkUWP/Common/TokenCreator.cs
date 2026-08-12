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
            switch (TokenVersion)
            {
                case TokenVersions.TokenV1:
                    return GetCoolapkAppToken();
                default:
                case TokenVersions.TokenV2:
                    return GetTokenWithDeviceCode(DeviceCode);
            }
        }

        /// <summary>
        /// GetTokenWithDeviceCode Generate a token with your device code
        /// </summary>
        private string GetTokenWithDeviceCode(string deviceCode)
        {
            string timeStamp = DateTime.Now.ConvertDateTimeToUnixTimeStamp().ToString();

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

        private static string GetCoolapkAppToken()
        {
            double timeStamp = DateTime.Now.ConvertDateTimeToUnixTimeStamp();
            string hex_timeStamp = $"0x{Convert.ToString((int)timeStamp, 16)}";
            // 时间戳加密
            string md5_timeStamp = $"{timeStamp}".GetMD5();
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
