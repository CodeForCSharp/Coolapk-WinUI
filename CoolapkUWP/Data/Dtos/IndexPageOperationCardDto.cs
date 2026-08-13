using System.Text.Json.Serialization;

namespace CoolapkUWP.Data.Dtos
{
    /// <summary>
    /// 首页操作卡片(登录/刷新/标题)。
    /// </summary>
    public class IndexPageOperationCardDto : EntityDto
    {
        [JsonPropertyName("title")]
        public string Title { get; set; }
        [JsonPropertyName("url")]
        public string Url { get; set; }
    }
}
