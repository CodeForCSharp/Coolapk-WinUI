using System.Text.Json.Serialization;

namespace CoolapkUWP.Data.Dtos
{
    /// <summary>
    /// 应用(App)条目,用于搜索建议、应用详情等场景。字段与酷安 API 响应一致。
    /// </summary>
    public class AppDto
    {
        [JsonPropertyName("url")]
        public string Url { get; set; }
        [JsonPropertyName("followCount")]
        public string FollowCount { get; set; }
        [JsonPropertyName("downCount")]
        public string DownCount { get; set; }
        [JsonPropertyName("apkversioncode")]
        public string Apkversioncode { get; set; }
        [JsonPropertyName("apkversionname")]
        public string Apkversionname { get; set; }
        [JsonPropertyName("title")]
        public string Title { get; set; }
        [JsonPropertyName("navTitle")]
        public string NavTitle { get; set; }
        [JsonPropertyName("description")]
        public string Description { get; set; }
        [JsonPropertyName("keywords")]
        public string Keywords { get; set; }
        [JsonPropertyName("catName")]
        public string CatName { get; set; }
        [JsonPropertyName("apkTypeName")]
        public string ApkTypeName { get; set; }
        [JsonPropertyName("logo")]
        public string Logo { get; set; }
        [JsonPropertyName("lastupdate")]
        public string Lastupdate { get; set; }

        [JsonPropertyName("entityId")]
        public string EntityId { get; set; }
        [JsonPropertyName("entityType")]
        public string EntityType { get; set; }
        [JsonPropertyName("entityForward")]
        public string EntityForward { get; set; }
        [JsonPropertyName("entityFixed")]
        public string EntityFixed { get; set; }
    }
}
