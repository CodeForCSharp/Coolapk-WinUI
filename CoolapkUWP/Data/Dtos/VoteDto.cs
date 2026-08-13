using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CoolapkUWP.Data.Dtos
{
    /// <summary>
    /// 投票信息(feed 的 vote 字段)。
    /// </summary>
    public class VoteDto
    {
        [JsonPropertyName("total_vote_num")]
        public int TotalVoteNum { get; set; }
        [JsonPropertyName("total_comment_num")]
        public int TotalCommentNum { get; set; }
        [JsonPropertyName("start_time")]
        public long? StartTime { get; set; }
        [JsonPropertyName("end_time")]
        public long? EndTime { get; set; }
        [JsonPropertyName("type")]
        public int Type { get; set; }
        [JsonPropertyName("link_tag")]
        public string LinkTag { get; set; }
        [JsonPropertyName("options")]
        public List<VoteItemDto> Options { get; set; }
    }
}
