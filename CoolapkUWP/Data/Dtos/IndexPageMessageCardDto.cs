using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace CoolapkUWP.Data.Dtos
{
    /// <summary>
    /// 首页消息卡片(子实体元素类型运行时判定故保留原始 JSON)。
    /// </summary>
    public class IndexPageMessageCardDto : EntityDto
    {
        [JsonPropertyName("title")]
        public string Title { get; set; }
        [JsonPropertyName("description")]
        public string Description { get; set; }
        [JsonPropertyName("release_time")]
        public string ReleaseTime { get; set; }
        [JsonPropertyName("link_tag")]
        public string LinkTag { get; set; }
        [JsonPropertyName("hot_num_txt")]
        public string HotNumTxt { get; set; }
        [JsonPropertyName("keywords")]
        public string Keywords { get; set; }
        [JsonPropertyName("catName")]
        public string CatName { get; set; }
        [JsonPropertyName("apkTypeName")]
        public string ApkTypeName { get; set; }
        [JsonPropertyName("rss_type")]
        public string RssType { get; set; }
        [JsonPropertyName("subTitle")]
        public string SubTitle { get; set; }
        [JsonPropertyName("entities")]
        public List<JsonObject> Entities { get; set; }
    }
}
