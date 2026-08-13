using System.Text.Json.Serialization;

namespace CoolapkUWP.Data.Dtos
{
    /// <summary>
    /// 通知数字接口(notification/checkCount)响应。
    /// </summary>
    public class NotificationNumbersDto
    {
        [JsonPropertyName("cloudInstall")]
        public int CloudInstall { get; set; }
        [JsonPropertyName("notification")]
        public int Notification { get; set; }
        [JsonPropertyName("badge")]
        public int Badge { get; set; }
        [JsonPropertyName("contacts_follow")]
        public int ContactsFollow { get; set; }
        [JsonPropertyName("message")]
        public int Message { get; set; }
        [JsonPropertyName("atme")]
        public int Atme { get; set; }
        [JsonPropertyName("atcommentme")]
        public int Atcommentme { get; set; }
        [JsonPropertyName("commentme")]
        public int Commentme { get; set; }
        [JsonPropertyName("feedlike")]
        public int Feedlike { get; set; }
    }
}
