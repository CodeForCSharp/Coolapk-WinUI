using System.Text.Json.Nodes;

namespace CoolapkUWP.Data.Dtos
{
    /// <summary>
    /// 话题详情(topic detail)。
    /// </summary>
    public class TopicDetailDto : EntityDto
    {
        public string Id { get; set; }
        public string Url { get; set; }
        public string Title { get; set; }
        public UserActionDto UserAction { get; set; }
        public string HotNumTxt { get; set; }
        public string FollownumTxt { get; set; }
        public string CommentnumTxt { get; set; }
        public string Description { get; set; }
        public string Intro { get; set; }
        public string Logo { get; set; }
        public JsonArray RecentFollowList { get; set; }
    }
}
