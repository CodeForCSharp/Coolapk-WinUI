using CoolapkUWP.Helpers;
using CommunityToolkit.WinUI.Helpers;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Windows.ApplicationModel;

namespace CoolapkUWP.Common
{
    public class TokenCreator
    {
        private static readonly RandomNumberGenerator Rng = RandomNumberGenerator.Create();
        private static readonly string aid = RandHexString(16);
        private static readonly string mac = RandMacAddress();
        private static readonly string SystemManufacturer = string.Empty;
        private static readonly string SystemProductName = string.Empty;

        // Token V3 参数。AppCode 与 auth blob 是一一对应的，换版本时两者要一起更新。
        // auth_blob.b64 中的签名数据表来自酷安 v16.2.0 的 libauth.so（参考公开的
        // coolapk-desktop / coolapk-mcp 实现提取），仅用于请求签名。
        public const string AppVersion = "16.2.0";
        public const int AppCode = 2604201;
        private const string AppId = "com.coolapk.market";
        private const int BcryptCost = 4;

        private const string StdBase64Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
        private const string BcryptBase64Alphabet = "./ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

        private static readonly byte[] AuthBlob = LoadAuthBlob();

        public static string DeviceCode;

        // token 依赖的时戳为整数秒（ConvertDateTimeToUnixTimeStamp 会四舍五入到秒），
        // 故同一秒内结果完全一致，可按秒缓存以避开每请求一次的 BCrypt。
        private readonly object _tokenLock = new object();
        private long _cachedSecond = -1;
        private string _cachedToken;

        static TokenCreator()
        {
            DeviceCode = CreateDeviceCode(aid, mac, SystemManufacturer, SystemManufacturer, SystemProductName, $"CoolapkUWP {Package.Current.Id.Version.ToFormattedString()}");
        }

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

                _cachedToken = GetTokenV3(second);
                _cachedSecond = second;
                return _cachedToken;
            }
        }

        /// <summary>
        /// Token V3：v16 客户端使用的签名。plain 由 auth blob 中按秒取出的片段、
        /// 设备码摘要、时间戳和 AppCode 拼接，再用 bcrypt 生成 v3 token。
        /// </summary>
        private static string GetTokenV3(long second)
        {
            // bcrypt 的 salt 字母表不含 '+'，而标准 base64 可能生成 '+'。
            // 官方客户端会向后探测最多 15 秒，这里保持一致。
            for (int offset = 0; offset <= 15; offset++)
            {
                if (TryGetTokenV3(second + offset, out string token))
                {
                    return token;
                }
            }

            throw new InvalidOperationException("无法生成 Token V3：auth blob 与 AppCode 可能不匹配");
        }

        private static bool TryGetTokenV3(long second, out string token)
        {
            token = null;

            int index = (int)(((second + AppCode) % 100) * 4 + 0x80);
            if (index < 0 || index + 0x80 > AuthBlob.Length) { return false; }

            byte[] chunk = new byte[0x80];
            Array.Copy(AuthBlob, index, chunk, 0, chunk.Length);

            byte[] segment;
            try { segment = Convert.FromBase64String(Encoding.UTF8.GetString(chunk)); }
            catch (FormatException) { return false; }

            byte[] head = Encoding.UTF8.GetBytes($"{AppId}&");
            byte[] tail = Encoding.UTF8.GetBytes($"&{DeviceCode.GetMD5()}&{second}&{AppCode}");
            byte[] plain = new byte[head.Length + segment.Length + tail.Length];
            Buffer.BlockCopy(head, 0, plain, 0, head.Length);
            Buffer.BlockCopy(segment, 0, plain, head.Length, segment.Length);
            Buffer.BlockCopy(tail, 0, plain, head.Length + segment.Length, tail.Length);

            string password = Convert.ToBase64String(plain).GetMD5();
            string saltSource = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{second:x}/{GetMD5(plain)}")).TrimEnd('=');
            if (saltSource.Length < 22) { return false; }

            string salt = ShiftLastChar(saltSource.Substring(0, 22), -5);
            if (salt == null || !TryDecodeBcryptSalt(salt, out byte[] saltBytes)) { return false; }

            try
            {
                string hash = BCrypt.Net.BCrypt.HashPassword(password, $"$2y${BcryptCost:D2}${EncodeBcryptSalt(saltBytes)}");
                token = $"v3{Convert.ToBase64String(Encoding.UTF8.GetBytes(hash)).TrimEnd('=')}";
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static string GetMD5(byte[] input)
        {
            using MD5 md5 = MD5.Create();
            return Convert.ToHexString(md5.ComputeHash(input)).ToLowerInvariant();
        }

        private static string ShiftLastChar(string value, int shift)
        {
            if (string.IsNullOrEmpty(value)) { return null; }

            int index = StdBase64Alphabet.IndexOf(value[value.Length - 1]);
            if (index < 0) { return null; }

            return value.Substring(0, value.Length - 1) + StdBase64Alphabet[(index + shift + StdBase64Alphabet.Length) % StdBase64Alphabet.Length];
        }

        /// <summary>
        /// 按 bcrypt 的字母表把 22 字符 salt 解回 16 字节。
        /// 只要出现非 bcrypt 字符就返回 false，由调用方换下一秒重试。
        /// </summary>
        private static bool TryDecodeBcryptSalt(string value, out byte[] bytes)
        {
            bytes = new byte[16];
            int byteIndex = 0;
            int bitBuffer = 0;
            int bitCount = 0;

            foreach (char c in value)
            {
                int digit = BcryptBase64Alphabet.IndexOf(c);
                if (digit < 0) { bytes = null; return false; }

                bitBuffer = (bitBuffer << 6) | digit;
                bitCount += 6;

                if (bitCount >= 8)
                {
                    bitCount -= 8;
                    if (byteIndex < bytes.Length)
                    {
                        bytes[byteIndex++] = (byte)((bitBuffer >> bitCount) & 0xFF);
                    }
                    bitBuffer &= (1 << bitCount) - 1;
                }
            }

            return byteIndex == bytes.Length;
        }

        private static string EncodeBcryptSalt(byte[] bytes)
        {
            StringBuilder builder = new StringBuilder(24);
            for (int i = 0; i < bytes.Length; i += 3)
            {
                int b1 = bytes[i];
                int b2 = i + 1 < bytes.Length ? bytes[i + 1] : 0;
                int b3 = i + 2 < bytes.Length ? bytes[i + 2] : 0;

                builder.Append(BcryptBase64Alphabet[(b1 >> 2) & 0x3F]);
                builder.Append(BcryptBase64Alphabet[((b1 & 0x03) << 4) | ((b2 >> 4) & 0x0F)]);
                builder.Append(BcryptBase64Alphabet[((b2 & 0x0F) << 2) | ((b3 >> 6) & 0x03)]);
                builder.Append(BcryptBase64Alphabet[b3 & 0x3F]);
            }

            return builder.ToString(0, 22);
        }

        private static byte[] LoadAuthBlob()
        {
            using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("CoolapkUWP.Common.auth_blob.b64");
            if (stream == null)
            {
                throw new InvalidOperationException("缺少 Token V3 的 auth blob 资源");
            }

            using StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            return Convert.FromBase64String(reader.ReadToEnd().Trim());
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
}
