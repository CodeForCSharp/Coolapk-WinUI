using CoolapkUWP.Helpers;
using CoolapkUWP.Data.Dtos;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CoolapkUWP.Data
{
    /// <summary>
    /// DTO 反序列化统一入口:源生成上下文 + 宽松字符串 + 字符串数字互读。
    /// </summary>
    public static class DtoJson
    {
        public static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            TypeInfoResolver = JsonContext.Default,
            PropertyNameCaseInsensitive = true,
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
            Converters = { new LenientStringConverter() }
        };
    }
}
