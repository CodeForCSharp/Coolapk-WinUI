using System.Text.Json.Serialization;

namespace CoolapkUWP.Data.Dtos
{
    /// <summary>
    /// 用户主页卡片(user profile)。
    /// </summary>
    public class ProfileDetailDto : EntityDto
    {
        [JsonPropertyName("userAvatar")]
        public string UserAvatar { get; set; }
        [JsonPropertyName("url")]
        public string Url { get; set; }
        [JsonPropertyName("fans")]
        public double Fans { get; set; }
        [JsonPropertyName("feed")]
        public double Feed { get; set; }
        [JsonPropertyName("level")]
        public double Level { get; set; }
        [JsonPropertyName("username")]
        public string Username { get; set; }
        [JsonPropertyName("follow")]
        public double Follow { get; set; }
        [JsonPropertyName("level_today_message")]
        public string LevelTodayMessage { get; set; }
        [JsonPropertyName("next_level_experience")]
        public double NextLevelExperience { get; set; }
        [JsonPropertyName("next_level_percentage")]
        public double NextLevelPercentage { get; set; }
    }
}
