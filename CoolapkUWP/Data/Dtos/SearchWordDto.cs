namespace CoolapkUWP.Data.Dtos
{
    /// <summary>
    /// 搜索建议 / 搜索结果中的搜索词条目。字段与酷安 API 响应一致。
    /// </summary>
    public class SearchWordDto
    {
        public string Logo { get; set; }
        public string Title { get; set; }

        public string EntityId { get; set; }
        public string EntityType { get; set; }
        public string EntityForward { get; set; }
        public string EntityFixed { get; set; }
    }
}
