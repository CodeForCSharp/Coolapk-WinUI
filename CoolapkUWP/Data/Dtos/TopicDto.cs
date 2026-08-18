using System.Text.Json.Serialization;

namespace CoolapkUWP.Data.Dtos
{
    /// <summary>
    /// 话题条目(topic)。
    /// </summary>
    public class TopicDto : EntityDto
    {
        [JsonPropertyName("url")]
        public string Url { get; set; }
        [JsonPropertyName("title")]
        public string Title { get; set; }
        [JsonPropertyName("follownum")]
        public string Follownum { get; set; }
        [JsonPropertyName("follow_num")]
        public string FollowNum { get; set; }
        [JsonPropertyName("logo")]
        public string Logo { get; set; }
        [JsonPropertyName("newsnum")]
        public string Newsnum { get; set; }
        [JsonPropertyName("commentnum")]
        public string Commentnum { get; set; }
        [JsonPropertyName("rating_total_num")]
        public string RatingTotalNum { get; set; }
        [JsonPropertyName("star_average_score")]
        public string StarAverageScore { get; set; }
        [JsonPropertyName("star_total_count")]
        public string StarTotalCount { get; set; }
        [JsonPropertyName("rating_average_score")]
        public string RatingAverageScore { get; set; }
        [JsonPropertyName("hot_num_txt")]
        public string HotNumTxt { get; set; }
        [JsonPropertyName("commentnum_txt")]
        public string CommentNumTxt { get; set; }
        [JsonPropertyName("description")]
        public string Description { get; set; }
        [JsonPropertyName("newtitle")]
        public string Newtitle { get; set; }
        [JsonPropertyName("username")]
        public string Username { get; set; }
        [JsonPropertyName("rss_type")]
        public string RssType { get; set; }
        [JsonPropertyName("hot_num")]
        public double? HotNum { get; set; }
        [JsonPropertyName("lastupdate")]
        public long? Lastupdate { get; set; }
    }
}
