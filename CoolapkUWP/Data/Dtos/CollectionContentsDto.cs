using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CoolapkUWP.Data.Dtos
{
    /// <summary>
    /// 收藏单内容列表元素(collection itemList 响应)。
    /// </summary>
    public class CollectionContentsDto
    {
        [JsonPropertyName("entityTemplate")]
        public string EntityTemplate { get; set; }
        [JsonPropertyName("entities")]
        public List<SelectorEntityDto> Entities { get; set; }
    }

    /// <summary>
    /// 选择器卡片子实体(selectorLinkCard 的 entities 元素)。
    /// </summary>
    public class SelectorEntityDto
    {
        [JsonPropertyName("url")]
        public string Url { get; set; }
        [JsonPropertyName("title")]
        public string Title { get; set; }
    }
}
