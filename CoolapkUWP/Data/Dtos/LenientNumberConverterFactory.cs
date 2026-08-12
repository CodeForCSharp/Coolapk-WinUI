using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CoolapkUWP.Data.Dtos
{
    /// <summary>
    /// 数值字段的宽松读取：酷安 API 的数值字段可能是 number、"123" 字符串、bool 或 null，
    /// 语义对齐原 ToInt32Safe/ToInt64Safe/ToDoubleSafe（解析失败与 null 均回退 0）。
    /// </summary>
    public sealed class LenientNumberConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
            => typeToConvert == typeof(int) || typeToConvert == typeof(long) || typeToConvert == typeof(double);

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            if (typeToConvert == typeof(int)) { return new IntConverter(); }
            if (typeToConvert == typeof(long)) { return new LongConverter(); }
            return new DoubleConverter();
        }

        private abstract class NumberConverter<T> : JsonConverter<T>
        {
            protected abstract bool TryParse(string value, out T result);

            protected abstract T ReadNumber(ref Utf8JsonReader reader);

            public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                switch (reader.TokenType)
                {
                    case JsonTokenType.Number:
                        return ReadNumber(ref reader);
                    case JsonTokenType.String:
                        return TryParse(reader.GetString(), out T result) ? result : default;
                    case JsonTokenType.True:
                    case JsonTokenType.False:
                    case JsonTokenType.Null:
                        return default;
                    default:
                        throw new JsonException($"Cannot convert {reader.TokenType} to {typeToConvert}");
                }
            }

            public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
            {
                if (value is double d) { writer.WriteNumberValue(d); }
                else if (value is long l) { writer.WriteNumberValue(l); }
                else { writer.WriteNumberValue((int)(object)value); }
            }
        }

        private sealed class IntConverter : NumberConverter<int>
        {
            protected override bool TryParse(string value, out int result)
                => int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out result);

            protected override int ReadNumber(ref Utf8JsonReader reader) => reader.GetInt32();
        }

        private sealed class LongConverter : NumberConverter<long>
        {
            protected override bool TryParse(string value, out long result)
                => long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out result);

            protected override long ReadNumber(ref Utf8JsonReader reader) => reader.GetInt64();
        }

        private sealed class DoubleConverter : NumberConverter<double>
        {
            protected override bool TryParse(string value, out double result)
                => double.TryParse(value, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out result);

            protected override double ReadNumber(ref Utf8JsonReader reader) => reader.GetDouble();
        }
    }
}
