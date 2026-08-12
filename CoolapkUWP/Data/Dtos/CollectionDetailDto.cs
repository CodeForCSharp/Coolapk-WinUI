namespace CoolapkUWP.Data.Dtos
{
    /// <summary>
    /// 收藏单详情。
    /// </summary>
    public class CollectionDetailDto : EntityDto
    {
        public string Id { get; set; }
        public UserActionDto UserAction { get; set; }
        public string ItemNum { get; set; }
        public string LikeNum { get; set; }
        public string Url { get; set; }
        public string Title { get; set; }
        public string Username { get; set; }
        public string FollowNum { get; set; }
        public string Lastupdate { get; set; }
        public string Description { get; set; }
        public string CoverPic { get; set; }
        public string UserAvatar { get; set; }
    }
}
