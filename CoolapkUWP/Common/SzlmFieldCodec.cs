using System;
using System.Text;

namespace CoolapkUWP.Common
{
    /// <summary>
    /// 数盟报文字段编码。
    /// </summary>
    internal static class SzlmFieldCodec
    {
        public static int Parameter(string timestamp)
        {
            string tail = timestamp.Substring(Math.Max(0, timestamp.Length - 3)).Replace(".", string.Empty);
            return int.Parse(tail);
        }

        public static string Encode(string value, int p)
        {
            int k = p % 100 % 57 + 1;
            int[] s = new int[value.Length];
            for (int i = 0; i < value.Length; i++)
            {
                s[i] = ((value[i] - 32 + k) % 95) + 32;
            }

            int n = s.Length;
            int m = n / 2;
            int[] output = new int[n];
            int index = 0;

            switch ((p / 100) % 4)
            {
                case 0:
                    if ((n & 1) != 0)
                    {
                        output[index++] = s[m];
                    }
                    for (int i = 0; i < m; i++)
                    {
                        output[index++] = s[n - 1 - i];
                        output[index++] = s[m - 1 - i];
                    }
                    break;

                case 1:
                    for (int i = 0; i < m; i++)
                    {
                        output[index++] = s[n - 1 - i];
                        output[index++] = s[i];
                    }
                    if ((n & 1) != 0)
                    {
                        output[index++] = s[m];
                    }
                    break;

                case 2:
                    for (int i = n - 1; i >= 0; i -= 2)
                    {
                        output[index++] = s[i];
                    }
                    for (int i = n - 2; i >= 0; i -= 2)
                    {
                        output[index++] = s[i];
                    }
                    break;

                default:
                    for (int i = 0; i < n; i += 2)
                    {
                        output[index++] = s[i];
                    }
                    for (int i = 1; i < n; i += 2)
                    {
                        output[index++] = s[i];
                    }
                    break;
            }

            StringBuilder builder = new StringBuilder(n);
            for (int i = 0; i < n; i++)
            {
                builder.Append((char)output[i]);
            }
            return builder.ToString();
        }

        public static string Anchor(string value, int p)
        {
            int index = value.Length > 9 ? value.Length - 1 - (p % 10) : value.Length - 1;
            string stripped = value.Substring(0, index) + value.Substring(index + 1);
            return Encode(stripped, p);
        }

        public static string Checksum(string value, int p)
        {
            int sum = 0;
            foreach (char c in value)
            {
                sum += c;
            }
            sum += (p % 100) / 10;
            return TokenCreator.GetHexMd5(Encoding.UTF8.GetBytes(sum.ToString())).ToUpperInvariant().Substring(0, 7);
        }
    }
}
