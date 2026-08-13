using System.Text.Json.Serialization;

namespace CoolapkUWP.Data.Dtos
{
    /// <summary>
    /// 用户条目(user)。
    /// </summary>
    public class UserDto : EntityDto
    {
        [JsonPropertyName("uid")]
        public int Uid { get; set; }
        [JsonPropertyName("bio")]
        public string Bio { get; set; }
        [JsonPropertyName("fans")]
        public int? Fans { get; set; }
        [JsonPropertyName("level")]
        public int Level { get; set; }
        [JsonPropertyName("cover")]
        public string Cover { get; set; }
        [JsonPropertyName("status")]
        public int Status { get; set; }
        [JsonPropertyName("regdate")]
        public int Regdate { get; set; }
        [JsonPropertyName("username")]
        public string Username { get; set; }
        [JsonPropertyName("logintime")]
        public long? Logintime { get; set; }
        [JsonPropertyName("follow")]
        public int? Follow { get; set; }
        [JsonPropertyName("experience")]
        public int Experience { get; set; }
        [JsonPropertyName("userAvatar")]
        public string UserAvatar { get; set; }
        [JsonPropertyName("block_status")]
        public int BlockStatus { get; set; }
    }
}
