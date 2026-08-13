using System.Text.Json.Serialization;

namespace CoolapkUWP.Data.Dtos
{
    /// <summary>
    /// 所有 DTO 的公共基类,承载实体标识字段。
    /// </summary>
    public abstract class EntityDto
    {
        [JsonPropertyName("entityId")]
        public string EntityId { get; set; }
        [JsonPropertyName("entityType")]
        public string EntityType { get; set; }
        [JsonPropertyName("entityForward")]
        public string EntityForward { get; set; }
        [JsonPropertyName("entityFixed")]
        public string EntityFixed { get; set; }
    }
}
