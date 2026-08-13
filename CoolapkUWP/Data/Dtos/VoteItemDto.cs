using System.Text.Json.Serialization;

namespace CoolapkUWP.Data.Dtos
{
    /// <summary>
    /// 投票选项(vote.options 元素)。
    /// </summary>
    public class VoteItemDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("order")]
        public int Order { get; set; }
        [JsonPropertyName("vote_id")]
        public int VoteId { get; set; }
        [JsonPropertyName("status")]
        public int Status { get; set; }
        [JsonPropertyName("title")]
        public string Title { get; set; }
        [JsonPropertyName("color")]
        public string Color { get; set; }
    }
}
