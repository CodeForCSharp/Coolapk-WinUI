using CoolapkUWP.Data;
using CoolapkUWP.Data.Dtos;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace CoolapkUWP.Models
{
    internal class IconMiniGridCardModel : IndexPageHasEntitiesModel
    {
        public IconMiniGridCardModel(IndexPageHasEntitiesDto dto)
            : base(dto, global::CoolapkUWP.Models.EntityType.GridLink) { }

        public static IconMiniGridCardModel FromJson(JsonObject json)
            => new IconMiniGridCardModel(DtoJson.Deserialize<IndexPageHasEntitiesDto>(json));
    }
}
