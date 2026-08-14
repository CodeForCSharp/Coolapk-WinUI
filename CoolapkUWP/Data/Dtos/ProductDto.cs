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
        [JsonPropertyName("hot_num_txt")]
        public string HotNumTxt { get; set; }
        [JsonPropertyName("release_time")]
        public string ReleaseTime { get; set; }
        [JsonPropertyName("description")]
        public string Description { get; set; }
        [JsonPropertyName("url")]
        public string Url { get; set; }
    }
}
