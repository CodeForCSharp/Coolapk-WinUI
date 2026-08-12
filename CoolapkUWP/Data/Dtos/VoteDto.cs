using System.Collections.Generic;

namespace CoolapkUWP.Data.Dtos
{
    /// <summary>
    /// 投票信息(feed 的 vote 字段)。
    /// </summary>
    public class VoteDto
    {
        public int TotalVoteNum { get; set; }
        public int TotalCommentNum { get; set; }
        public long? StartTime { get; set; }
        public long? EndTime { get; set; }
        public int Type { get; set; }
        public string LinkTag { get; set; }
        public List<VoteItemDto> Options { get; set; }
    }
}
