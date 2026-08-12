using System.Text.Json.Nodes;

namespace CoolapkUWP.Data.Dtos
{
    /// <summary>
    /// 联系人条目。
    /// </summary>
    public class ContactDto : EntityDto
    {
        public string Dateline { get; set; }
        public string Isfriend { get; set; }
        public JsonNode UserInfo { get; set; }
    }
}
