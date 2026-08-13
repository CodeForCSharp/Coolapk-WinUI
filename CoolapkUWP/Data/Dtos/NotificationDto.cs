using System.Text.Json.Serialization;

namespace CoolapkUWP.Data.Dtos
{
    /// <summary>
    /// 通知条目,扁平覆盖全部通知模型(简单通知/@评论/点赞/私信)。
    /// </summary>
    public class NotificationDto : EntityDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("url")]
        public string Url { get; set; }
        [JsonPropertyName("uid")]
        public string Uid { get; set; }
        [JsonPropertyName("ruid")]
        public string Ruid { get; set; }
        [JsonPropertyName("rusername")]
        public string Rusername { get; set; }
        [JsonPropertyName("dateline")]
        public long? Dateline { get; set; }
        [JsonPropertyName("block_status")]
        public string BlockStatus { get; set; }
        [JsonPropertyName("status")]
        public string Status { get; set; }
        [JsonPropertyName("userInfo")]
        public NotificationUserInfoDto UserInfo { get; set; }
        [JsonPropertyName("username")]
        public string Username { get; set; }
        [JsonPropertyName("userAvatar")]
        public string UserAvatar { get; set; }

        [JsonPropertyName("note")]
        public string Note { get; set; }
        [JsonPropertyName("fromUserAvatar")]
        public string FromUserAvatar { get; set; }
        [JsonPropertyName("fromUserInfo")]
        public NotificationUserInfoDto FromUserInfo { get; set; }
        [JsonPropertyName("fromusername")]
        public string Fromusername { get; set; }

        [JsonPropertyName("extra_title")]
        public string ExtraTitle { get; set; }
        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("feedTypeName")]
        public string FeedTypeName { get; set; }
        [JsonPropertyName("infoHtml")]
        public string InfoHtml { get; set; }
        [JsonPropertyName("likeUid")]
        public string LikeUid { get; set; }
        [JsonPropertyName("likeTime")]
        public long? LikeTime { get; set; }
        [JsonPropertyName("likeAvatar")]
        public string LikeAvatar { get; set; }
        [JsonPropertyName("likeUserInfo")]
        public NotificationUserInfoDto LikeUserInfo { get; set; }
        [JsonPropertyName("likeUsername")]
        public string LikeUsername { get; set; }

        [JsonPropertyName("ukey")]
        public string Ukey { get; set; }
        [JsonPropertyName("messageUserInfo")]
        public NotificationUserInfoDto MessageUserInfo { get; set; }
        [JsonPropertyName("is_top")]
        public int IsTop { get; set; }
    }
}
