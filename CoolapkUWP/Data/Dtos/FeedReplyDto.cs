using System.Collections.Generic;

namespace CoolapkUWP.Data.Dtos
{
    /// <summary>
    /// 评论(reply)条目的扁平数据模型,覆盖 SourceFeedReplyModel 与 FeedReplyModel。
    /// </summary>
    public class FeedReplyDto : EntityDto
    {
        public int Id { get; set; }
        public UserDto UserInfo { get; set; }
        public UserActionDto UserAction { get; set; }
        public int IsFeedAuthor { get; set; }
        public string Ruid { get; set; }
        public string Rusername { get; set; }
        public string Message { get; set; }
        public string Pic { get; set; }
        public List<string> PicArr { get; set; }
        public int BlockStatus { get; set; }
        public long? Dateline { get; set; }
        public int Likenum { get; set; }
        public int Replynum { get; set; }
        public int ReplyRowsCount { get; set; }
        public List<FeedReplyDto> ReplyRows { get; set; }
    }
}
