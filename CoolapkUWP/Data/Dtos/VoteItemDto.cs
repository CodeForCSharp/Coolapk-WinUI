namespace CoolapkUWP.Data.Dtos
{
    /// <summary>
    /// 投票选项(vote.options 元素)。
    /// </summary>
    public class VoteItemDto
    {
        public int Id { get; set; }
        public int Order { get; set; }
        public int VoteId { get; set; }
        public int Status { get; set; }
        public string Title { get; set; }
        public string Color { get; set; }
    }
}
