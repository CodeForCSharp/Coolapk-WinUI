using System.Text.Json.Serialization;

namespace CoolapkUWP.Data.Dtos
{
    /// <summary>
    /// 文章目标行(feed 详情的 targetRow 字段)。
    /// </summary>
    public class DyhRowDto
    {
        [JsonPropertyName("logo")]
        public string Logo { get; set; }
        [JsonPropertyName("title")]
        public string Title { get; set; }
        [JsonPropertyName("url")]
        public string Url { get; set; }
        [JsonPropertyName("subTitle")]
        public string SubTitle { get; set; }
        [JsonPropertyName("star_average_score")]
        public string StarAverageScore { get; set; }
        [JsonPropertyName("star_total_count")]
        public int StarTotalCount { get; set; }
        [JsonPropertyName("targetType")]
        public string TargetType { get; set; }
    }
}
