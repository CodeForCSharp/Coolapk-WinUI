using System.Text.Json.Serialization;

namespace CoolapkUWP.Data.Dtos
{
    /// <summary>
    /// 直播条目(liveTopic)，用于「直播」Tab 的发布会直播列表。
    /// </summary>
    public class LiveTopicDto : EntityDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("title")]
        public string Title { get; set; }
        [JsonPropertyName("pic_url")]
        public string PicUrl { get; set; }
        [JsonPropertyName("description")]
        public string Description { get; set; }
        [JsonPropertyName("show_live_time")]
        public string ShowLiveTime { get; set; }
        [JsonPropertyName("visit_num_format")]
        public string VisitNumFormat { get; set; }
        [JsonPropertyName("url")]
        public string Url { get; set; }
        [JsonPropertyName("userInfo")]
        public UserDto UserInfo { get; set; }
    }
}
