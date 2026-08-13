using System.Text.Json.Serialization;

namespace CoolapkUWP.Data.Dtos
{
    /// <summary>
    /// 最近关注列表元素(recent_follow_list)。
    /// </summary>
    public class RecentFollowDto
    {
        [JsonPropertyName("userInfo")]
        public UserDto UserInfo { get; set; }
    }
}
