using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CoolapkUWP.Data.Dtos
{
    /// <summary>
    /// 酷安 API 字段类型会漂移(同一字段有时是数字、有时是字符串、有时是 bool),
    /// 该转换器让 string 属性宽容接受任意标量类型。
    /// </summary>
    public sealed class LenientStringConverter : JsonConverter<string>
    {
        public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.String: return reader.GetString();
                case JsonTokenType.Number: return Encoding.UTF8.GetString(reader.ValueSpan);
                case JsonTokenType.True: return "true";
                case JsonTokenType.False: return "false";
                case JsonTokenType.Null: return null;
                default: throw new JsonException($"Cannot convert {reader.TokenType} to string");
            }
        }

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
            => writer.WriteStringValue(value);
    }
}
