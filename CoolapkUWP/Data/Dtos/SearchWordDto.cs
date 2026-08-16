using System.Text.Json.Serialization;

namespace CoolapkUWP.Data.Dtos
{
    /// <summary>
    /// 搜索建议 / 搜索结果中的搜索词条目。字段与酷安 API 响应一致。
    /// </summary>
    public class SearchWordDto : EntityDto
    {
        [JsonPropertyName("logo")]
        public string Logo { get; set; }
        [JsonPropertyName("title")]
        public string Title { get; set; }
    }
}
