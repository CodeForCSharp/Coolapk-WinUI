namespace CoolapkUWP.Data.Dtos
{
    /// <summary>
    /// 收藏单条目(collection)。
    /// </summary>
    public class CollectionDto : EntityDto
    {
        public int Id { get; set; }
        public int ItemNum { get; set; }
        public string Title { get; set; }
        public string SubTitle { get; set; }
        public string Url { get; set; }
        public string Description { get; set; }
        public string CoverPic { get; set; }
    }
}
