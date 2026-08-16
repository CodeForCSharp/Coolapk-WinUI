using System.Text.Json.Serialization;

namespace CoolapkUWP.Data.Dtos
{
    /// <summary>
    /// 用户空间详情(user space)。
    /// </summary>
    public class UserDetailDto : EntityDto
    {
        [JsonPropertyName("uid")]
        public int Uid { get; set; }
        [JsonPropertyName("feed")]
        public int Feed { get; set; }
        [JsonPropertyName("be_like_num")]
        public int BeLikeNum { get; set; }
        [JsonPropertyName("fans")]
        public int Fans { get; set; }
        [JsonPropertyName("level")]
        public int Level { get; set; }
        [JsonPropertyName("follow")]
        public int Follow { get; set; }
        [JsonPropertyName("isFans")]
        public int IsFans { get; set; }
        [JsonPropertyName("isBlackList")]
        public int IsBlackList { get; set; }
        [JsonPropertyName("isFollow")]
        public int IsFollow { get; set; }
        [JsonPropertyName("bio")]
        public string Bio { get; set; }
        [JsonPropertyName("province")]
        public string Province { get; set; }
        [JsonPropertyName("city")]
        public string City { get; set; }
        [JsonPropertyName("astro")]
        public string Astro { get; set; }
        [JsonPropertyName("gender")]
        public int Gender { get; set; }
        [JsonPropertyName("displayUsername")]
        public string DisplayUsername { get; set; }
        [JsonPropertyName("logintime")]
        public long? Logintime { get; set; }
        [JsonPropertyName("block_status")]
        public int? BlockStatus { get; set; }
        [JsonPropertyName("verify_title")]
        public string VerifyTitle { get; set; }
        [JsonPropertyName("next_level_experience")]
        public double NextLevelExperience { get; set; }
        [JsonPropertyName("next_level_percentage")]
        public double NextLevelPercentage { get; set; }
        [JsonPropertyName("cover")]
        public string Cover { get; set; }
        [JsonPropertyName("userAvatar")]
        public string UserAvatar { get; set; }
    }
}
