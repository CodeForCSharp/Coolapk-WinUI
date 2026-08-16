using CoolapkUWP.Data;
using CoolapkUWP.Data.Dtos;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace CoolapkUWP.Models
{
    internal class IconListCardModel : IndexPageHasEntitiesModel
    {
        public IconListCardModel(IndexPageHasEntitiesDto dto)
            : base(dto, global::CoolapkUWP.Models.EntityType.Others) { }

        public static IconListCardModel FromJson(JsonObject json)
            => new IconListCardModel(DtoJson.Deserialize<IndexPageHasEntitiesDto>(json));
    }
}
