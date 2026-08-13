using System.Text.Json.Serialization;

namespace CoolapkUWP.Data.Dtos
{
    /// <summary>
    /// 历史记录条目(history)。
    /// </summary>
    public class HistoryDto : EntityDto
    {
        [JsonPropertyName("title")]
        public string Title { get; set; }
        [JsonPropertyName("url")]
        public string Url { get; set; }
        [JsonPropertyName("description")]
        public string Description { get; set; }
        [JsonPropertyName("target_type_title")]
        public string TargetTypeTitle { get; set; }
        [JsonPropertyName("dateline")]
        public long? Dateline { get; set; }
        [JsonPropertyName("logo")]
        public string Logo { get; set; }
    }
}
