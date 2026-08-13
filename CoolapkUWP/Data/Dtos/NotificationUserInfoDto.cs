using System.Text.Json.Serialization;

namespace CoolapkUWP.Data.Dtos
{
    /// <summary>
    /// 通知中的用户信息(userInfo/fromUserInfo/likeUserInfo/messageUserInfo)。
    /// </summary>
    public class NotificationUserInfoDto
    {
        [JsonPropertyName("status")]
        public int Status { get; set; }
        [JsonPropertyName("block_status")]
        public int BlockStatus { get; set; }
        [JsonPropertyName("username")]
        public string Username { get; set; }
        [JsonPropertyName("userAvatar")]
        public string UserAvatar { get; set; }
    }
}
