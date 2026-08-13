using System.Text.Json.Serialization;

namespace CoolapkUWP.Data.Dtos
{
    /// <summary>
    /// 收藏单详情。
    /// </summary>
    public class CollectionDetailDto : EntityDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("userAction")]
        public UserActionDto UserAction { get; set; }
        [JsonPropertyName("item_num")]
        public int ItemNum { get; set; }
        [JsonPropertyName("like_num")]
        public int LikeNum { get; set; }
        [JsonPropertyName("url")]
        public string Url { get; set; }
        [JsonPropertyName("title")]
        public string Title { get; set; }
        [JsonPropertyName("username")]
        public string Username { get; set; }
        [JsonPropertyName("follow_num")]
        public string FollowNum { get; set; }
        [JsonPropertyName("lastupdate")]
        public long? Lastupdate { get; set; }
        [JsonPropertyName("description")]
        public string Description { get; set; }
        [JsonPropertyName("cover_pic")]
        public string CoverPic { get; set; }
        [JsonPropertyName("userAvatar")]
        public string UserAvatar { get; set; }
    }
}
