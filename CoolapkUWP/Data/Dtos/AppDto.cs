namespace CoolapkUWP.Data.Dtos
{
    /// <summary>
    /// 应用(App)条目,用于搜索建议、应用详情等场景。字段与酷安 API 响应一致。
    /// </summary>
    public class AppDto
    {
        public string Url { get; set; }
        public string FollowCount { get; set; }
        public string DownCount { get; set; }
        public string Apkversioncode { get; set; }
        public string Apkversionname { get; set; }
        public string Title { get; set; }
        public string NavTitle { get; set; }
        public string Description { get; set; }
        public string Keywords { get; set; }
        public string CatName { get; set; }
        public string ApkTypeName { get; set; }
        public string Logo { get; set; }
        public string Lastupdate { get; set; }

        public string EntityId { get; set; }
        public string EntityType { get; set; }
        public string EntityForward { get; set; }
        public string EntityFixed { get; set; }
    }
}
