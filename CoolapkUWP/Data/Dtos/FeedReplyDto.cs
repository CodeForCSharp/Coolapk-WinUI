using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CoolapkUWP.Data.Dtos
{
    /// <summary>
    /// 评论(reply)条目的扁平数据模型,覆盖 SourceFeedReplyModel 与 FeedReplyModel。
    /// </summary>
    public class FeedReplyDto : EntityDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("userInfo")]
        public UserDto UserInfo { get; set; }
        [JsonPropertyName("userAction")]
        public UserActionDto UserAction { get; set; }
        [JsonPropertyName("isFeedAuthor")]
        public int IsFeedAuthor { get; set; }
        [JsonPropertyName("ruid")]
        public string Ruid { get; set; }
        [JsonPropertyName("rusername")]
        public string Rusername { get; set; }
        [JsonPropertyName("message")]
        public string Message { get; set; }
        [JsonPropertyName("pic")]
        public string Pic { get; set; }
        [JsonPropertyName("picArr")]
        public List<string> PicArr { get; set; }
        [JsonPropertyName("block_status")]
        public int BlockStatus { get; set; }
        [JsonPropertyName("dateline")]
        public long? Dateline { get; set; }
        [JsonPropertyName("likenum")]
        public int Likenum { get; set; }
        [JsonPropertyName("replynum")]
        public int Replynum { get; set; }
        [JsonPropertyName("replyRowsCount")]
        public int ReplyRowsCount { get; set; }
        [JsonPropertyName("replyRows")]
        public List<FeedReplyDto> ReplyRows { get; set; }
    }
}
