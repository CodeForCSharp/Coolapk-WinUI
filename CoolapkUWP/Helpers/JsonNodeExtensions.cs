using System.Text.Json;
using System.Text.Json.Nodes;

namespace CoolapkUWP.Helpers
{
    internal static class JsonNodeExtensions
    {
        public static int ToInt32Safe(this JsonNode node)
        {
            if (node is JsonValue value)
            {
                if (value.TryGetValue<int>(out int i)) return i;
                if (value.TryGetValue<string>(out string s) && int.TryParse(s, out i)) return i;
            }
            return 0;
        }

        public static long ToInt64Safe(this JsonNode node)
        {
            if (node is JsonValue value)
            {
                if (value.TryGetValue<long>(out long l)) return l;
                if (value.TryGetValue<string>(out string s) && long.TryParse(s, out l)) return l;
            }
            return 0;
        }

        public static double ToDoubleSafe(this JsonNode node)
        {
            if (node is JsonValue value)
            {
                if (value.TryGetValue<double>(out double d)) return d;
                if (value.TryGetValue<string>(out string s) && double.TryParse(s, out d)) return d;
            }
            return 0;
        }
    }
}
