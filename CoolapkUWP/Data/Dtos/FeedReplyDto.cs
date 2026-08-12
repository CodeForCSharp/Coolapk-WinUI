using System.Text.Json.Nodes;

namespace CoolapkUWP.Data.Dtos
{
    /// <summary>
    /// 评论(reply)条目的扁平数据模型,覆盖 SourceFeedReplyModel 与 FeedReplyModel。
    /// </summary>
    public class FeedReplyDto : EntityDto
    {
        public string Id { get; set; }
        public JsonNode UserInfo { get; set; }
        public JsonNode UserAction { get; set; }
        public string IsFeedAuthor { get; set; }
        public string Ruid { get; set; }
        public string Rusername { get; set; }
        public string Message { get; set; }
        public string Pic { get; set; }
        public JsonArray PicArr { get; set; }
        public string BlockStatus { get; set; }
        public string Dateline { get; set; }
        public string Likenum { get; set; }
        public string Replynum { get; set; }
        public string ReplyRowsMore { get; set; }
        public string ReplyRowsCount { get; set; }
        public JsonArray ReplyRows { get; set; }
    }
}
