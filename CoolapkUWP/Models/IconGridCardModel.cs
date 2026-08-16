using CoolapkUWP.Data;
using CoolapkUWP.Data.Dtos;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace CoolapkUWP.Models
{
    internal class IconGridCardModel : IndexPageHasEntitiesModel
    {
        public IconGridCardModel(IndexPageHasEntitiesDto dto)
            : base(dto, global::CoolapkUWP.Models.EntityType.Others) { }

        public static IconGridCardModel FromJson(JsonObject json)
            => new IconGridCardModel(DtoJson.Deserialize<IndexPageHasEntitiesDto>(json));
    }
}
