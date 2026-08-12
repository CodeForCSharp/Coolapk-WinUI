using System;
using System.Globalization;

namespace CoolapkUWP.Data
{
    /// <summary>
    /// DTO 字符串字段的宽松数值解析,语义对齐原有 JsonNode 扩展(解析失败回退 0)。
    /// </summary>
    public static class DtoParsing
    {
        public static int ToInt32Safe(this string value)
            => int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int i) ? i : 0;

        public static long ToInt64Safe(this string value)
            => long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out long l) ? l : 0;

        public static double ToDoubleSafe(this string value)
            => double.TryParse(value, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out double d) ? d : 0;
    }
}
