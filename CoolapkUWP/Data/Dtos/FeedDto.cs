using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CoolapkUWP.Data.Dtos
{
    /// <summary>
    /// 动态(feed)条目的扁平数据模型,覆盖 FeedModel 家族全部字段。
    /// 嵌套对象(UserInfo/UserAction/Vote 等)已强类型化;ForwardSourceFeed 保留原始 JSON
    /// (API 可能返回字符串 "null",强类型化会抛异常)。
    /// </summary>
    public class FeedDto : EntityDto
    {
        // SourceFeedModel
        [JsonPropertyName("userInfo")]
        public UserDto UserInfo { get; set; }
        [JsonPropertyName("userAction")]
        public UserActionDto UserAction { get; set; }
        [JsonPropertyName("url")]
        public string Url { get; set; }
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("shareUrl")]
        public string ShareUrl { get; set; }
        [JsonPropertyName("message")]
        public string Message { get; set; }
        [JsonPropertyName("message_title")]
        public string MessageTitle { get; set; }
        [JsonPropertyName("feedType")]
        public string FeedType { get; set; }
        [JsonPropertyName("dateline")]
        public long? Dateline { get; set; }
        [JsonPropertyName("pic")]
        public string Pic { get; set; }
        [JsonPropertyName("picArr")]
        public List<string> PicArr { get; set; }
        [JsonPropertyName("star")]
        public int Star { get; set; }

        // FeedModelBase
        [JsonPropertyName("likenum")]
        public int Likenum { get; set; }
        [JsonPropertyName("replynum")]
        public int Replynum { get; set; }
        [JsonPropertyName("favnum")]
        public int Favnum { get; set; }
        [JsonPropertyName("forwardnum")]
        public int Forwardnum { get; set; }
        [JsonPropertyName("info")]
        public string Info { get; set; }
        [JsonPropertyName("feedTypeName")]
        public string FeedTypeName { get; set; }
        [JsonPropertyName("infoHtml")]
        public string InfoHtml { get; set; }
        [JsonPropertyName("vote")]
        public VoteDto Vote { get; set; }
        [JsonPropertyName("question_answer_num")]
        public int QuestionAnswerNum { get; set; }
        [JsonPropertyName("question_follow_num")]
        public int QuestionFollowNum { get; set; }
        [JsonPropertyName("device_title")]
        public string DeviceTitle { get; set; }
        [JsonPropertyName("device_name")]
        public string DeviceName { get; set; }
        [JsonPropertyName("ip_location")]
        public string IpLocation { get; set; }
        [JsonPropertyName("extra_title")]
        public string ExtraTitle { get; set; }
        [JsonPropertyName("extra_url")]
        public string ExtraUrl { get; set; }
        [JsonPropertyName("extra_pic")]
        public string ExtraPic { get; set; }
        [JsonPropertyName("media_url")]
        public string MediaUrl { get; set; }
        [JsonPropertyName("media_pic")]
        public string MediaPic { get; set; }
        [JsonPropertyName("replyRowsCount")]
        public int ReplyRowsCount { get; set; }
        [JsonPropertyName("replyRows")]
        public List<FeedReplyDto> ReplyRows { get; set; }
        [JsonPropertyName("location")]
        public string Location { get; set; }
        [JsonPropertyName("ttitle")]
        public string Ttitle { get; set; }
        [JsonPropertyName("turl")]
        public string Turl { get; set; }
        [JsonPropertyName("tpic")]
        public string Tpic { get; set; }
        [JsonPropertyName("dyh_name")]
        public string DyhName { get; set; }
        [JsonPropertyName("dyh_id")]
        public string DyhId { get; set; }
        [JsonPropertyName("relationRows")]
        public System.Text.Json.Nodes.JsonArray RelationRows { get; set; }
        [JsonPropertyName("change_count")]
        public int ChangeCount { get; set; }
        [JsonPropertyName("status")]
        public int Status { get; set; }
        [JsonPropertyName("block_status")]
        public int BlockStatus { get; set; }
        [JsonPropertyName("source_id")]
        public string SourceId { get; set; }
        [JsonPropertyName("forwardSourceFeed")]
        public System.Text.Json.Nodes.JsonNode ForwardSourceFeed { get; set; }

        // FeedModel
        [JsonPropertyName("isStickTop")]
        public int IsStickTop { get; set; }

        // FeedDetailModel
        [JsonPropertyName("readNum")]
        public int ReadNum { get; set; }
        [JsonPropertyName("title")]
        public string Title { get; set; }
        [JsonPropertyName("targetRow")]
        public DyhRowDto TargetRow { get; set; }
        [JsonPropertyName("extraData")]
        public System.Text.Json.Nodes.JsonNode ExtraData { get; set; }
        [JsonPropertyName("message_raw_output")]
        public string MessageRawOutput { get; set; }
        [JsonPropertyName("message_cover")]
        public string MessageCover { get; set; }
    }
}
