namespace CoolapkUWP.Data.Dtos
{
    /// <summary>
    /// 投票选项(vote.options 元素)。
    /// </summary>
    public class VoteItemDto
    {
        public string Id { get; set; }
        public string Order { get; set; }
        public string VoteId { get; set; }
        public string Status { get; set; }
        public string Title { get; set; }
        public string Color { get; set; }
    }
}
