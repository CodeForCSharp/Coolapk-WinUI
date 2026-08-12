using System;
using System.Globalization;
using System.Text.Json.Nodes;

namespace CoolapkUWP.Data
{
    /// <summary>
    /// DTO 字符串字段与 JsonNode 的宽松数值解析（解析失败回退 0）。
    /// </summary>
    public static class DtoParsing
    {
        public static int ToInt32Safe(this string value)
            => int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int i) ? i : 0;

        public static long ToInt64Safe(this string value)
            => long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out long l) ? l : 0;

        public static double ToDoubleSafe(this string value)
            => double.TryParse(value, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out double d) ? d : 0;

        public static int ToInt32Safe(this JsonNode node)
        {
            if (node is JsonValue value)
            {
                if (value.TryGetValue<int>(out int i)) return i;
                if (value.TryGetValue<string>(out string s) && int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out i)) return i;
            }
            return 0;
        }

        public static long ToInt64Safe(this JsonNode node)
        {
            if (node is JsonValue value)
            {
                if (value.TryGetValue<long>(out long l)) return l;
                if (value.TryGetValue<string>(out string s) && long.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out l)) return l;
            }
            return 0;
        }

        public static double ToDoubleSafe(this JsonNode node)
        {
            if (node is JsonValue value)
            {
                if (value.TryGetValue<double>(out double d)) return d;
                if (value.TryGetValue<string>(out string s) && double.TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out d)) return d;
            }
            return 0;
        }
    }
}
