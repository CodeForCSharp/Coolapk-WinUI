using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CoolapkUWP.Data.Dtos
{
    /// <summary>
    /// 话题详情(topic detail)。
    /// </summary>
    public class TopicDetailDto : EntityDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("url")]
        public string Url { get; set; }
        [JsonPropertyName("title")]
        public string Title { get; set; }
        [JsonPropertyName("userAction")]
        public UserActionDto UserAction { get; set; }
        [JsonPropertyName("hot_num_txt")]
        public string HotNumTxt { get; set; }
        [JsonPropertyName("follownum_txt")]
        public string FollownumTxt { get; set; }
        [JsonPropertyName("commentnum_txt")]
        public string CommentnumTxt { get; set; }
        [JsonPropertyName("description")]
        public string Description { get; set; }
        [JsonPropertyName("intro")]
        public string Intro { get; set; }
        [JsonPropertyName("logo")]
        public string Logo { get; set; }
        [JsonPropertyName("recent_follow_list")]
        public List<RecentFollowDto> RecentFollowList { get; set; }
    }
}
