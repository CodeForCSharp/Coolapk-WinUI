using CoolapkUWP.Data;
using CoolapkUWP.Data.Dtos;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace CoolapkUWP.Models
{
    internal class IconMiniScrollCardModel : IndexPageHasEntitiesModel
    {
        public IconMiniScrollCardModel(IndexPageHasEntitiesDto dto)
            : base(dto, global::CoolapkUWP.Models.EntityType.Others) { }

        public static IconMiniScrollCardModel FromJson(JsonObject json)
            => new IconMiniScrollCardModel(DtoJson.Deserialize<IndexPageHasEntitiesDto>(json));
    }
}
