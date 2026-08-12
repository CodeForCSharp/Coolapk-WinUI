using System.Collections.Generic;

namespace CoolapkUWP.Data.Dtos
{
    /// <summary>
    /// 投票信息(feed 的 vote 字段)。
    /// </summary>
    public class VoteDto
    {
        public string TotalVoteNum { get; set; }
        public string TotalCommentNum { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string Type { get; set; }
        public string LinkTag { get; set; }
        public List<VoteItemDto> Options { get; set; }
    }
}
