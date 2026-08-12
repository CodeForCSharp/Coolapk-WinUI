namespace CoolapkUWP.Data.Dtos
{
    /// <summary>
    /// 话题条目(topic)。
    /// </summary>
    public class TopicDto : EntityDto
    {
        public string Url { get; set; }
        public string Title { get; set; }
        public string Follownum { get; set; }
        public string FollowNum { get; set; }
        public string Logo { get; set; }
        public string Newsnum { get; set; }
        public string Commentnum { get; set; }
        public string RatingTotalNum { get; set; }
        public string Description { get; set; }
        public string Newtitle { get; set; }
        public string Username { get; set; }
        public string RssType { get; set; }
        public string HotNum { get; set; }
        public string Lastupdate { get; set; }
    }
}
