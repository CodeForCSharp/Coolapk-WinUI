using CoolapkUWP.Helpers;
using CoolapkUWP.Data.Dtos;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
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
            Converters = { new LenientStringConverter(), new LenientNumberConverterFactory() }
        };

        /// <summary>
        /// 单个节点反序列化为指定 DTO。
        /// </summary>
        public static TDto Deserialize<TDto>(JsonNode node)
            => JsonSerializer.Deserialize<TDto>(node, Options);

        /// <summary>
        /// 一次性反序列化整个数组，避免逐节点对每个元素反复做"序列化 + 再解析"的往返。
        /// </summary>
        public static List<T> DeserializeList<T>(JsonNode node)
            => JsonSerializer.Deserialize<List<T>>(node, Options) ?? new List<T>();
    }
}
