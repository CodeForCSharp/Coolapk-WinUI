using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CoolapkUWP.Data.Dtos
{
    /// <summary>
    /// 产品详情。
    /// </summary>
    public class ProductDetailDto : EntityDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("star_1_count")]
        public int Star1Count { get; set; }
        [JsonPropertyName("star_2_count")]
        public int Star2Count { get; set; }
        [JsonPropertyName("star_3_count")]
        public int Star3Count { get; set; }
        [JsonPropertyName("star_4_count")]
        public int Star4Count { get; set; }
        [JsonPropertyName("star_5_count")]
        public int Star5Count { get; set; }
        [JsonPropertyName("owner_star_1_count")]
        public int OwnerStar1Count { get; set; }
        [JsonPropertyName("owner_star_2_count")]
        public int OwnerStar2Count { get; set; }
        [JsonPropertyName("owner_star_3_count")]
        public int OwnerStar3Count { get; set; }
        [JsonPropertyName("owner_star_4_count")]
        public int OwnerStar4Count { get; set; }
        [JsonPropertyName("owner_star_5_count")]
        public int OwnerStar5Count { get; set; }
        [JsonPropertyName("title")]
        public string Title { get; set; }
        [JsonPropertyName("hot_num_txt")]
        public string HotNumTxt { get; set; }
        [JsonPropertyName("star_total_count")]
        public string StarTotalCount { get; set; }
        [JsonPropertyName("follow_num_txt")]
        public string FollowNumTxt { get; set; }
        [JsonPropertyName("feed_comment_num_txt")]
        public string FeedCommentNumTxt { get; set; }
        [JsonPropertyName("rating_total_num")]
        public string RatingTotalNum { get; set; }
        [JsonPropertyName("description")]
        public string Description { get; set; }
        [JsonPropertyName("owner_star_average_score")]
        public double OwnerStarAverageScore { get; set; }
        [JsonPropertyName("rating_average_score")]
        public double RatingAverageScore { get; set; }
        [JsonPropertyName("logo")]
        public string Logo { get; set; }
        [JsonPropertyName("tagArr")]
        public List<string> TagArr { get; set; }
        [JsonPropertyName("recent_follow_list")]
        public List<RecentFollowDto> RecentFollowList { get; set; }
        [JsonPropertyName("coverArr")]
        public List<string> CoverArr { get; set; }
        [JsonPropertyName("userAction")]
        public UserActionDto UserAction { get; set; }
    }
}
