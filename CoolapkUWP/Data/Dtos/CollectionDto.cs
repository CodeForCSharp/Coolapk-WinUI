using System.Text.Json.Serialization;

namespace CoolapkUWP.Data.Dtos
{
    /// <summary>
    /// 收藏单条目(collection)。
    /// </summary>
    public class CollectionDto : EntityDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("item_num")]
        public int ItemNum { get; set; }
        [JsonPropertyName("title")]
        public string Title { get; set; }
        [JsonPropertyName("subTitle")]
        public string SubTitle { get; set; }
        [JsonPropertyName("url")]
        public string Url { get; set; }
        [JsonPropertyName("description")]
        public string Description { get; set; }
        [JsonPropertyName("cover_pic")]
        public string CoverPic { get; set; }
    }
}
