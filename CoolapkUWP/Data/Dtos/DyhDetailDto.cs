using System.Text.Json.Serialization;

namespace CoolapkUWP.Data.Dtos
{
    /// <summary>
    /// 数码窝(Dyh)详情。
    /// </summary>
    public class DyhDetailDto : EntityDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("userAction")]
        public UserActionDto UserAction { get; set; }
        [JsonPropertyName("uid")]
        public string Uid { get; set; }
        [JsonPropertyName("title")]
        public string Title { get; set; }
        [JsonPropertyName("author")]
        public string Author { get; set; }
        [JsonPropertyName("follownum")]
        public string Follownum { get; set; }
        [JsonPropertyName("description")]
        public string Description { get; set; }
        [JsonPropertyName("logo")]
        public string Logo { get; set; }
        [JsonPropertyName("userAvatar")]
        public string UserAvatar { get; set; }
    }
}
