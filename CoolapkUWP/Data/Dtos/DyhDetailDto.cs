namespace CoolapkUWP.Data.Dtos
{
    /// <summary>
    /// 数码窝(Dyh)详情。
    /// </summary>
    public class DyhDetailDto : EntityDto
    {
        public string Id { get; set; }
        public UserActionDto UserAction { get; set; }
        public string Uid { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Follownum { get; set; }
        public string Description { get; set; }
        public string Logo { get; set; }
        public string UserAvatar { get; set; }
    }
}
