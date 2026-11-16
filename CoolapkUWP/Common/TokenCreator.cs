using CoolapkUWP.Helpers;
using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace CoolapkUWP.Common
{
    public class TokenCreator
    {
        // Token V3 参数。AppCode 与 auth blob 是一一对应的，换版本时两者要一起更新。
        // auth_blob.b64 中的签名数据表来自酷安 v16.2.0 的 libauth.so（参考公开的
        // coolapk-desktop / coolapk-mcp 实现提取），仅用于请求签名。
        public const string AppVersion = "16.2.0";
        public const int AppCode = 2604201;
        private const string AppId = "com.coolapk.market";
        private const int BcryptCost = 4;

        // 桌面端使用固定兼容画像，必须和 User-Agent / X-Sdk-Int 一致。
        // 协议不读 IMEI、AndroidID、OAID；第 1 段是数盟 DUID，没有时用 ca_ 占位，第 9 段固定 null。
        public const string DeviceManufacturer = "Google";
        public const string DeviceBrand = "google";
        public const string DeviceModel = "Pixel 7";
        public const string DeviceBuild = "TQ3A.230805.001";
        public const string DeviceAndroidVersion = "13";
        public const string DeviceSdkInt = "33";
        public const int DeviceSdkIntValue = 33;

        private const string StdBase64Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
        private const string BcryptBase64Alphabet = "./ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

        private static readonly byte[] AuthBlob = LoadAuthBlob();

        public static string DeviceCode { get; private set; }

        public static string SzlmId { get; private set; }

        public static string UserAgent { get; } =
            $"Dalvik/2.1.0 (Linux; U; Android {DeviceAndroidVersion}; {DeviceModel} {DeviceBuild}) (#Build; {DeviceBrand}; {DeviceModel}; {DeviceBuild}; {DeviceAndroidVersion}) +CoolMarket/{AppVersion}-{AppCode}-universal";

        // token 依赖的时戳为整数秒（ConvertDateTimeToUnixTimeStamp 会四舍五入到秒），
        // 故同一秒内结果完全一致，可按秒缓存以避开每请求一次的 BCrypt。
        private readonly object _tokenLock = new object();
        private long _cachedSecond = -1;
        private string _cachedToken;

        static TokenCreator()
        {
            SzlmId = SzlmDuidClient.Validate(SettingsHelper.Get<string>(SettingsHelper.SzlmId)) ?? string.Empty;
            string first = !string.IsNullOrEmpty(SzlmId)
                ? SzlmId
                : ResolvePlaceholderId(SettingsHelper.Get<string>(SettingsHelper.XAppDevice));
            DeviceCode = CreateDeviceCode(first);
            SettingsHelper.Set(SettingsHelper.XAppDevice, DeviceCode);
        }

        /// <summary>
        /// 只替换设备码第 1 段为数盟 DUID，后 8 段保持当前画像。
        /// </summary>
        public static void ApplySzlmId(string value)
        {
            string id = SzlmDuidClient.Validate(value);
            if (id == null)
            {
                throw new FormatException("设备标识为空或格式不正确，未修改设备标识");
            }

            SzlmId = id;
            DeviceCode = CreateDeviceCode(id);
            SettingsHelper.Set(SettingsHelper.SzlmId, id);
            SettingsHelper.Set(SettingsHelper.XAppDevice, DeviceCode);
        }

        public static string GetHexMd5(byte[] input) => GetMD5(input);

        internal static string RandHexChars(int length)
        {
            const string chars = "0123456789abcdef";
            char[] buffer = new char[length];
            for (int i = 0; i < length; i++)
            {
                buffer[i] = chars[RandomNumberGenerator.GetInt32(chars.Length)];
            }
            return new string(buffer);
        }

        /// <summary>
        /// 用当前设备码生成 Token V3。同一秒内会复用缓存。
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
        /// 官方 9 段设备码：szlmId; ; ; ; manufacturer; brand; model; build; null
        /// </summary>
        private static string CreateDeviceCode(string deviceId)
        {
            return $"{deviceId}; ; ; ; {DeviceManufacturer}; {DeviceBrand}; {DeviceModel}; {DeviceBuild}; null".GetBase64(true).Reverse();
        }

        private static string CreatePlaceholderId()
        {
            return "ca_" + RandHexChars(32);
        }

        private static string ResolvePlaceholderId(string encoded)
        {
            string first = TryReadFirstField(encoded);
            if (!string.IsNullOrEmpty(first) && first.StartsWith("ca_", StringComparison.Ordinal))
            {
                return first;
            }

            return CreatePlaceholderId();
        }

        private static string TryReadFirstField(string encoded)
        {
            if (string.IsNullOrEmpty(encoded))
            {
                return null;
            }

            try
            {
                char[] chars = encoded.ToCharArray();
                Array.Reverse(chars);
                string base64 = new string(chars);
                switch (base64.Length % 4)
                {
                    case 2:
                        base64 += "==";
                        break;
                    case 3:
                        base64 += "=";
                        break;
                }

                string plain = Encoding.UTF8.GetString(Convert.FromBase64String(base64));
                string[] fields = plain.Split(';');
                if (fields.Length == 9 && !string.IsNullOrWhiteSpace(fields[0]))
                {
                    return fields[0];
                }
            }
            catch (FormatException)
            {
            }

            return null;
        }
    }
}
