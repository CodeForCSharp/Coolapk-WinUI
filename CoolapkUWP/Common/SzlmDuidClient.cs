using CoolapkUWP.Helpers;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace CoolapkUWP.Common
{
    /// <summary>
    /// 数盟 DUID 取号。
    /// 报文只用安装种子和公开机型，不读 IMEI / AndroidID / OAID。
    /// </summary>
    internal static class SzlmDuidClient
    {
        public const string Host = "auni.telecome.cn";
        public const string FallbackHost = "global-auni.checktrustworthiness.com";
        public const string ReportPath = "/a/mdna/report?v=8.7&t=m&p=F9BD0FBDACFE80FCF34370FBCFEE1A98";
        public const int MaxResponseBytes = 64 * 1024;

        private const string PoolKey = "xp7j@&!v5]2k#+3z{n9$q^r%[8f1*d4e:c>0l?m/6)yb-=w}";
        private static readonly Regex SeedPattern = new Regex("^[0-9a-f]{32}$", RegexOptions.CultureInvariant);
        private static readonly Regex IdPattern = new Regex("^[A-Za-z0-9_+/.=-]{8,4096}$", RegexOptions.CultureInvariant);
        private static readonly SemaphoreSlim Gate = new SemaphoreSlim(1, 1);
        private static readonly HttpClient Http = CreateHttp();
        private static bool _tried;

        public static async Task EnsureIdentityAsync()
        {
            if (!string.IsNullOrEmpty(TokenCreator.SzlmId) || _tried)
            {
                return;
            }

            await Gate.WaitAsync().ConfigureAwait(false);
            try
            {
                if (!string.IsNullOrEmpty(TokenCreator.SzlmId) || _tried)
                {
                    return;
                }

                string seed = GetOrCreateSeed();
                string id = await FetchAsync(seed).ConfigureAwait(false);
                TokenCreator.ApplySzlmId(id);
                NetworkHelper.SetRequestHeaders();
            }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.CreateLogger(nameof(SzlmDuidClient)).LogWarning(ex, "数盟取号失败，继续使用占位设备码");
            }
            finally
            {
                _tried = true;
                Gate.Release();
            }
        }

        public static async Task<string> FetchAsync(
            string seed,
            string manufacturer = TokenCreator.DeviceManufacturer,
            string model = TokenCreator.DeviceModel,
            string androidVersion = TokenCreator.DeviceAndroidVersion,
            int sdkInt = TokenCreator.DeviceSdkIntValue)
        {
            if (!SeedPattern.IsMatch(seed))
            {
                throw new SzlmDuidException("本机设备报文配置无法读取");
            }

            DateTime utc = DateTime.UtcNow;
            long epochUs = (utc.Ticks - DateTime.UnixEpoch.Ticks) / 10;
            long seconds = epochUs / 1_000_000;
            long nanos = (epochUs % 1_000_000) * 1000 + RandomNumberGenerator.GetInt32(1, 1000);
            string ts = $"{seconds}.{nanos}";

            byte[] body = EncodeReport(BuildReport(seed, ts, manufacturer, model, androidVersion, sdkInt));
            string path = $"{ReportPath}&r={CreateSeed()}&n=0&l=2&h=0";

            foreach (string target in new[] { Host, FallbackHost })
            {
                byte[] raw;
                try
                {
                    raw = await PostAsync(new Uri($"https://{target}{path}"), body).ConfigureAwait(false);
                }
                catch
                {
                    if (target == Host)
                    {
                        continue;
                    }
                    throw new SzlmDuidException("无法连接设备标识服务，请检查网络后稍后重试");
                }

                string id = DecodeId(raw);
                if (id == null)
                {
                    throw new SzlmDuidException("取号失败：服务端没有返回有效的设备标识");
                }
                return id;
            }

            throw new SzlmDuidException("无法连接设备标识服务，请稍后重试");
        }

        internal static object BuildReport(
            string seed,
            string ts,
            string manufacturer,
            string model,
            string androidVersion,
            int sdkInt)
        {
            string input = seed.Substring(0, 16);
            int p = SzlmFieldCodec.Parameter(ts);
            string hash = Convert.ToHexString(SHA1.HashData(Encoding.UTF8.GetBytes(input)));
            string mac = $"{hash.Substring(1, 6)}:{hash.Substring(13, 6)}:{hash.Substring(7, 6)}";
            return new SzlmReport(
                DUv: "com.coolapk.market",
                C75c: "11e7b222083a4b732b4b14811f6fc05995a01415eb",
                C3mS: "v9.0.1",
                AAA: "v1.0",
                BBB: "v1.0",
                C2cO: ts,
                FoO: SzlmFieldCodec.Anchor(input, p),
                SfOo: SzlmFieldCodec.Checksum(input, p),
                P1J: mac,
                AYk: mac,
                DLH: SzlmFieldCodec.Encode(seed, p),
                R37: manufacturer,
                R3d: model,
                R41: androidVersion,
                SAN: sdkInt);
        }

        internal static byte[] EncodeReport(object report)
        {
            SzlmReport value = (SzlmReport)report;
            string json = WriteReportJson(value).Replace("/", "\\/");
            return Xor(ZlibCompress(Encoding.UTF8.GetBytes(json)));
        }

        internal static string DecodeId(byte[] raw)
        {
            if (raw == null || raw.Length == 0 || raw.Length > MaxResponseBytes)
            {
                return null;
            }

            byte[] plain = Xor(raw);
            string text;
            try
            {
                if (plain.Length >= 2 && plain[0] == 0x78 && ((plain[0] << 8) + plain[1]) % 31 == 0)
                {
                    byte[] decompressed = ZlibDecompress(plain);
                    if (decompressed == null)
                    {
                        return null;
                    }
                    text = Encoding.UTF8.GetString(decompressed);
                }
                else
                {
                    text = Encoding.UTF8.GetString(plain);
                }
            }
            catch
            {
                return null;
            }

            JsonNode body;
            try
            {
                body = JsonNode.Parse(text);
            }
            catch (JsonException)
            {
                return null;
            }

            if (body is not JsonObject obj || !IsSuccess(obj["err"]))
            {
                return null;
            }

            JsonNode primary = obj["n_a"];
            if (primary != null && AsString(primary) != string.Empty)
            {
                return Validate(AsString(primary));
            }
            return Validate(AsString(obj["cdd"]));
        }

        internal static string CreateSeed()
        {
            return TokenCreator.RandHexChars(32);
        }

        internal static string Validate(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            string id = value.Trim();
            if (!IdPattern.IsMatch(id))
            {
                return null;
            }
            for (int i = 0; i < id.Length; i++)
            {
                if (id[i] != '0')
                {
                    return id;
                }
            }
            return null;
        }

        private static string GetOrCreateSeed()
        {
            string stored = SettingsHelper.Get<string>(SettingsHelper.SzlmReportSeed);
            if (!string.IsNullOrEmpty(stored))
            {
                if (!SeedPattern.IsMatch(stored))
                {
                    throw new SzlmDuidException("本机设备报文配置无法读取");
                }
                return stored;
            }

            string seed = CreateSeed();
            SettingsHelper.Set(SettingsHelper.SzlmReportSeed, seed);
            return seed;
        }

        private static async Task<byte[]> PostAsync(Uri uri, byte[] body)
        {
            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, uri)
            {
                Content = new ByteArrayContent(body)
            };
            request.Content.Headers.TryAddWithoutValidation("Content-Type", "application/octet-stream");
            request.Headers.TryAddWithoutValidation("Accept-Encoding", "identity");

            using HttpResponseMessage response = await Http.SendAsync(request).ConfigureAwait(false);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new SzlmDuidException("设备标识服务暂时不可用");
            }

            byte[] bytes = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
            if (bytes.Length > MaxResponseBytes)
            {
                throw new SzlmDuidException("设备标识服务响应异常");
            }
            return bytes;
        }

        private static HttpClient CreateHttp()
        {
            SocketsHttpHandler handler = new SocketsHttpHandler
            {
                AllowAutoRedirect = false,
                AutomaticDecompression = DecompressionMethods.None,
                ConnectTimeout = TimeSpan.FromSeconds(15)
            };
            return new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(15)
            };
        }

        private static bool IsSuccess(JsonNode err)
        {
            if (err is not JsonValue value)
            {
                return false;
            }
            if (value.TryGetValue(out int number))
            {
                return number == 0;
            }
            return value.TryGetValue(out string text) && text == "0";
        }

        private static string AsString(JsonNode node)
        {
            return node is JsonValue value && value.TryGetValue(out string text) ? text : null;
        }

        private static string WriteReportJson(SzlmReport report)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append('{');
            AppendString(builder, "DUv", report.DUv, true);
            AppendString(builder, "75c", report.C75c, false);
            AppendString(builder, "3mS", report.C3mS, false);
            AppendString(builder, "AAA", report.AAA, false);
            AppendString(builder, "BBB", report.BBB, false);
            AppendString(builder, "2cO", report.C2cO, false);
            AppendString(builder, "foO", report.FoO, false);
            AppendString(builder, "sfOo", report.SfOo, false);
            AppendString(builder, "P1J", report.P1J, false);
            AppendString(builder, "AYk", report.AYk, false);
            AppendString(builder, "dLH", report.DLH, false);
            AppendString(builder, "R37", report.R37, false);
            AppendString(builder, "R3d", report.R3d, false);
            AppendString(builder, "R41", report.R41, false);
            builder.Append(",\"SAN\":").Append(report.SAN).Append('}');
            return builder.ToString();
        }

        private static void AppendString(StringBuilder builder, string key, string value, bool first)
        {
            if (!first)
            {
                builder.Append(',');
            }
            builder.Append('"').Append(key).Append("\":\"");
            Escape(builder, value);
            builder.Append('"');
        }

        private static void Escape(StringBuilder builder, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            foreach (char c in value)
            {
                switch (c)
                {
                    case '"':
                        builder.Append("\\\"");
                        break;
                    case '\\':
                        builder.Append("\\\\");
                        break;
                    default:
                        builder.Append(c);
                        break;
                }
            }
        }

        private static byte[] Xor(byte[] data)
        {
            byte[] key = Encoding.UTF8.GetBytes(PoolKey);
            byte[] output = new byte[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                output[i] = (byte)(data[i] ^ key[i % key.Length]);
            }
            return output;
        }

        private static byte[] ZlibCompress(byte[] data)
        {
            using MemoryStream output = new MemoryStream();
            using (ZLibStream zlib = new ZLibStream(output, new ZLibCompressionOptions { CompressionLevel = 6 }, leaveOpen: true))
            {
                zlib.Write(data, 0, data.Length);
            }
            return output.ToArray();
        }

        private static byte[] ZlibDecompress(byte[] data)
        {
            using MemoryStream input = new MemoryStream(data);
            using ZLibStream zlib = new ZLibStream(input, CompressionMode.Decompress);
            using MemoryStream output = new MemoryStream();
            byte[] buffer = new byte[4096];
            int total = 0;
            int read;
            while ((read = zlib.Read(buffer, 0, buffer.Length)) > 0)
            {
                total += read;
                if (total > MaxResponseBytes)
                {
                    return null;
                }
                output.Write(buffer, 0, read);
            }
            return output.ToArray();
        }

        private readonly record struct SzlmReport(
            string DUv,
            string C75c,
            string C3mS,
            string AAA,
            string BBB,
            string C2cO,
            string FoO,
            string SfOo,
            string P1J,
            string AYk,
            string DLH,
            string R37,
            string R3d,
            string R41,
            int SAN);
    }

    internal sealed class SzlmDuidException : Exception
    {
        public SzlmDuidException(string message) : base(message)
        {
        }
    }
}
