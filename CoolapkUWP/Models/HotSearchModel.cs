using CoolapkUWP.Data;
using CoolapkUWP.Data.Dtos;
using System.Text.Json.Nodes;

namespace CoolapkUWP.Models
{
    internal class HotSearchModel : Entity, IHasTitle
    {
        public string Title { get; private set; }
        public string Url { get; private set; }

        private HotSearchModel(JsonObject json)
            : base(DtoJson.Deserialize<IndexPageDto>(json))
        {
            Title = json["title"]?.ToString();
            Url = json["url"]?.ToString();
        }

        public static HotSearchModel FromJson(JsonObject json) => new HotSearchModel(json);
    }
}
