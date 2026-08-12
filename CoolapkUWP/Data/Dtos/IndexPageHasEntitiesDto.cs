using System.Text.Json.Nodes;

namespace CoolapkUWP.Data.Dtos
{
    /// <summary>
    /// 首页实体卡片(包含子实体列表)。
    /// </summary>
    public class IndexPageHasEntitiesDto : EntityDto
    {
        public string Title { get; set; }
        public string Url { get; set; }
        public string Description { get; set; }
        public string ReleaseTime { get; set; }
        public string LinkTag { get; set; }
        public string HotNumTxt { get; set; }
        public string Keywords { get; set; }
        public string CatName { get; set; }
        public string ApkTypeName { get; set; }
        public string RssType { get; set; }
        public string SubTitle { get; set; }
        public string EntityTemplate { get; set; }
        public JsonArray Entities { get; set; }
        public string Pic { get; set; }
    }
}
