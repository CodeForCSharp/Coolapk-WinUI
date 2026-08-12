using System.Text.Json.Nodes;

namespace CoolapkUWP.Data.Dtos
{
    /// <summary>
    /// 动态(feed)条目的扁平数据模型,覆盖 FeedModel 家族全部字段。
    /// 嵌套对象(UserInfo/UserAction/Vote 等)保留原始 JSON,由 UI 模型递归解析。
    /// </summary>
    public class FeedDto : EntityDto
    {
        // SourceFeedModel
        public JsonNode UserInfo { get; set; }
        public JsonNode UserAction { get; set; }
        public string Url { get; set; }
        public string Id { get; set; }
        public string ShareUrl { get; set; }
        public string Message { get; set; }
        public string MessageTitle { get; set; }
        public string FeedType { get; set; }
        public string Dateline { get; set; }
        public string Pic { get; set; }
        public JsonArray PicArr { get; set; }
        public string Star { get; set; }

        // FeedModelBase
        public string Likenum { get; set; }
        public string Replynum { get; set; }
        public string Favnum { get; set; }
        public string Forwardnum { get; set; }
        public string Info { get; set; }
        public string FeedTypeName { get; set; }
        public string InfoHtml { get; set; }
        public JsonNode Vote { get; set; }
        public string QuestionAnswerNum { get; set; }
        public string QuestionFollowNum { get; set; }
        public string DeviceTitle { get; set; }
        public string DeviceName { get; set; }
        public string IpLocation { get; set; }
        public string ExtraTitle { get; set; }
        public string ExtraUrl { get; set; }
        public string ExtraPic { get; set; }
        public string MediaUrl { get; set; }
        public string MediaPic { get; set; }
        public string ReplyRowsCount { get; set; }
        public JsonArray ReplyRows { get; set; }
        public string Location { get; set; }
        public string Ttitle { get; set; }
        public string Turl { get; set; }
        public string Tpic { get; set; }
        public string DyhName { get; set; }
        public string DyhId { get; set; }
        public JsonArray RelationRows { get; set; }
        public string ChangeCount { get; set; }
        public string Status { get; set; }
        public string BlockStatus { get; set; }
        public string SourceId { get; set; }
        public JsonNode ForwardSourceFeed { get; set; }

        // FeedModel
        public string IsStickTop { get; set; }

        // FeedDetailModel
        public string ReadNum { get; set; }
        public string Title { get; set; }
        public JsonNode TargetRow { get; set; }
        public JsonNode ExtraData { get; set; }
        public string MessageRawOutput { get; set; }
        public string MessageCover { get; set; }
    }
}
