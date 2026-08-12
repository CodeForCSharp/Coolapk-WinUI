using CoolapkUWP.Common;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Windows.Storage.Streams;

namespace CoolapkUWP.Helpers
{
    public static partial class DataHelper
    {
        public static string GetMD5(this string input)
        {
            using (MD5 md5Hasher = MD5.Create())
            {
                byte[] data = md5Hasher.ComputeHash(Encoding.UTF8.GetBytes(input));

                string results = BitConverter.ToString(data).ToLowerInvariant();

                return results.Replace("-", "");
            }
        }

        public static string GetBase64(this string input, bool israw = false)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            string result = Convert.ToBase64String(bytes);
            if (israw) { result = result.Replace("=", ""); }
            return result;
        }

        public static string Reverse(this string text)
        {
            char[] charArray = text.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }

        public static string GetNumString(this double num)
        {
            string str = string.Empty;
            if (num < 1000) { }
            else if (num < 10000)
            {
                str = "k";
                num /= 1000;
            }
            else if (num < 10000000)
            {
                str = "w";
                num /= 10000;
            }
            else
            {
                str = "kw";
                num /= 10000000;
            }
            return $"{num:N2}{str}";
        }

        public static string CSStoString(this string str)
        {
            try
            {
                HtmlToText HtmlToText = new HtmlToText();
                return HtmlToText.Convert(str);
            }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.CreateLogger(nameof(DataHelper)).LogWarning(ex, ex.ExceptionToMessage());
                string s = str.Replace("<br>", "\n").Replace("<br>", "\n").Replace("<br/>", "\n").Replace("<br/>", "\n").Replace("<p>", "").Replace("</p>", "\n").Replace("&nbsp;", " ").Replace("<br />", "").Replace("<br />", "");
                while (s.IndexOf("<a", StringComparison.Ordinal) > 0)
                {
                    s = s.Replace(@"<a href=""" + Regex.Split(Regex.Split(s, @"<a href=""")[1], @""">")[0] + @""">", "");
                    s = s.Replace("</a>", "");
                }
                return s;
            }
        }

        public static string ConvertJsonString(this string str)
        {
            try
            {
                JsonNode node = JsonNode.Parse(str);
                return node?.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) ?? str;
            }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.CreateLogger(nameof(DataHelper)).LogError(ex, ex.ExceptionToMessage());
            }
            return str;
        }

        public static IBuffer GetBuffer(this IRandomAccessStream randomStream)
        {
            using (Stream stream = WindowsRuntimeStreamExtensions.AsStreamForRead(randomStream.GetInputStreamAt(0)))
            {
                return stream.GetBuffer();
            }
        }

        public static byte[] GetBytes(this IRandomAccessStream randomStream)
        {
            using (Stream stream = WindowsRuntimeStreamExtensions.AsStreamForRead(randomStream.GetInputStreamAt(0)))
            {
                return stream.GetBytes();
            }
        }

        public static IBuffer GetBuffer(this Stream stream)
        {
            byte[] bytes = new byte[0];
            if (stream != null)
            {
                bytes = stream.GetBytes();
            }
            return bytes.AsBuffer();
        }

        public static byte[] GetBytes(this Stream stream)
        {
            if (stream.CanSeek)
            {
                byte[] bytes = new byte[stream.Length];
                stream.Read(bytes, 0, bytes.Length);
                stream.Seek(0, SeekOrigin.Begin);
                return bytes;
            }
            else
            {
                int initialLength = 32768;

                byte[] buffer = new byte[initialLength];
                int read = 0;

                int chunk;
                while ((chunk = stream.Read(buffer, read, buffer.Length - read)) > 0)
                {
                    read += chunk;

                    if (read == buffer.Length)
                    {
                        int nextByte = stream.ReadByte();

                        if (nextByte == -1)
                        {
                            return buffer;
                        }

                        byte[] newBuffer = new byte[buffer.Length * 2];
                        Array.Copy(buffer, newBuffer, buffer.Length);
                        newBuffer[read] = (byte)nextByte;
                        buffer = newBuffer;
                        read++;
                    }
                }

                byte[] ret = new byte[read];
                Array.Copy(buffer, ret, read);
                return ret;
            }
        }

        public static Stream GetStream(this byte[] bytes)
        {
            Stream stream = new MemoryStream(bytes);
            return stream;
        }
    }
}
