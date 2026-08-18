using System.Text.Json.Serialization;

namespace CoolapkUWP.Data.Dtos
{
    /// <summary>
    /// 数码产品列表条目(product)，用于排行榜等产品列表。
    /// </summary>
    public class ProductDto : EntityDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("title")]
        public string Title { get; set; }
        [JsonPropertyName("logo")]
        public string Logo { get; set; }
        [JsonPropertyName("price_min")]
        public double PriceMin { get; set; }
        [JsonPropertyName("price_max")]
        public double PriceMax { get; set; }
        [JsonPropertyName("price_currency")]
        public string PriceCurrency { get; set; }
        [JsonPropertyName("rating_average_score")]
        public string RatingAverageScore { get; set; }
        [JsonPropertyName("feed_comment_num_txt")]
        public string FeedCommentNumTxt { get; set; }
        [JsonPropertyName("hot_num_txt")]
        public string HotNumTxt { get; set; }
        [JsonPropertyName("release_time")]
        public string ReleaseTime { get; set; }
        [JsonPropertyName("description")]
        public string Description { get; set; }
        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("v4_score_item_1_owner_average_score")]
        public string V4ScoreItem1OwnerAverageScore { get; set; }
        [JsonPropertyName("v4_score_item_2_owner_average_score")]
        public string V4ScoreItem2OwnerAverageScore { get; set; }
        [JsonPropertyName("v4_score_item_3_owner_average_score")]
        public string V4ScoreItem3OwnerAverageScore { get; set; }
        [JsonPropertyName("v4_score_item_4_owner_average_score")]
        public string V4ScoreItem4OwnerAverageScore { get; set; }
        [JsonPropertyName("v4_score_item_5_owner_average_score")]
        public string V4ScoreItem5OwnerAverageScore { get; set; }
        [JsonPropertyName("v4_score_item_6_owner_average_score")]
        public string V4ScoreItem6OwnerAverageScore { get; set; }
        [JsonPropertyName("subtab_all_endurance_score")]
        public string SubtabAllEnduranceScore { get; set; }
    }
}
