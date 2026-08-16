using System.Text.Json.Serialization;

namespace CoolapkUWP.Data.Dtos
{
    /// <summary>
    /// 首页条目(index page 卡片元素)。
    /// </summary>
    public class IndexPageDto : EntityDto
    {
        [JsonPropertyName("entityTemplate")]
        public string EntityTemplate { get; set; }
        [JsonPropertyName("title")]
        public string Title { get; set; }
        [JsonPropertyName("subTitle")]
        public string SubTitle { get; set; }
        [JsonPropertyName("hot_num_txt")]
        public string HotNumTxt { get; set; }
        [JsonPropertyName("link_tag")]
        public string LinkTag { get; set; }
        [JsonPropertyName("apkTypeName")]
        public string ApkTypeName { get; set; }
        [JsonPropertyName("typeName")]
        public string TypeName { get; set; }
        [JsonPropertyName("keywords")]
        public string Keywords { get; set; }
        [JsonPropertyName("catName")]
        public string CatName { get; set; }
        [JsonPropertyName("rss_type")]
        public string RssType { get; set; }
        [JsonPropertyName("product_num")]
        public string ProductNum { get; set; }
        [JsonPropertyName("star_average_score")]
        public string StarAverageScore { get; set; }
        [JsonPropertyName("commentnum")]
        public string CommentNum { get; set; }
        [JsonPropertyName("rating_total_num_txt")]
        public string RatingTotalNumTxt { get; set; }
        [JsonPropertyName("description")]
        public string Description { get; set; }
        [JsonPropertyName("video_playback_url")]
        public string VideoPlaybackUrl { get; set; }
        [JsonPropertyName("url")]
        public string Url { get; set; }
        [JsonPropertyName("release_time")]
        public string ReleaseTime { get; set; }
        [JsonPropertyName("cover_pic")]
        public string CoverPic { get; set; }
        [JsonPropertyName("pic")]
        public string Pic { get; set; }
        [JsonPropertyName("logo")]
        public string Logo { get; set; }
        [JsonPropertyName("pic_url")]
        public string PicUrl { get; set; }
    }
}
