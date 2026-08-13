using System.Text.Json.Serialization;

namespace CoolapkUWP.Data.Dtos
{
    /// <summary>
    /// 用户操作状态(userAction),字段为 0/1 数字或 "0"/"1" 字符串。
    /// </summary>
    public class UserActionDto
    {
        [JsonPropertyName("follow")]
        public int Follow { get; set; }
        [JsonPropertyName("like")]
        public int Like { get; set; }
        [JsonPropertyName("favorite")]
        public int Favorite { get; set; }
        [JsonPropertyName("collect")]
        public int Collect { get; set; }
        [JsonPropertyName("followAuthor")]
        public int FollowAuthor { get; set; }
        [JsonPropertyName("authorFollowYou")]
        public int AuthorFollowYou { get; set; }
    }
}
