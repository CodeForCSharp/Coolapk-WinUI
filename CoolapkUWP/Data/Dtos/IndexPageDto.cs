namespace CoolapkUWP.Data.Dtos
{
    /// <summary>
    /// 首页条目(index page 卡片元素)。
    /// </summary>
    public class IndexPageDto : EntityDto
    {
        public string EntityTemplate { get; set; }
        public string Title { get; set; }
        public string SubTitle { get; set; }
        public string HotNumTxt { get; set; }
        public string LinkTag { get; set; }
        public string ApkTypeName { get; set; }
        public string TypeName { get; set; }
        public string Keywords { get; set; }
        public string CatName { get; set; }
        public string RssType { get; set; }
        public string ProductNum { get; set; }
        public string Description { get; set; }
        public string VideoPlaybackUrl { get; set; }
        public string Url { get; set; }
        public string ReleaseTime { get; set; }
        public string CoverPic { get; set; }
        public string Pic { get; set; }
        public string Logo { get; set; }
        public string PicUrl { get; set; }
    }
}
