using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace CoolapkUWP.Data.Dtos
{
    /// <summary>
    /// 首页消息卡片(子实体元素类型运行时判定故保留原始 JSON)。
    /// </summary>
    public class IndexPageMessageCardDto : EntityDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string ReleaseTime { get; set; }
        public string LinkTag { get; set; }
        public string HotNumTxt { get; set; }
        public string Keywords { get; set; }
        public string CatName { get; set; }
        public string ApkTypeName { get; set; }
        public string RssType { get; set; }
        public string SubTitle { get; set; }
        public List<JsonObject> Entities { get; set; }
    }
}
