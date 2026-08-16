using CoolapkUWP.Data;
using CoolapkUWP.Data.Dtos;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace CoolapkUWP.Models
{
    internal class ColorfulScrollCardModel : IndexPageHasEntitiesModel
    {
        public ColorfulScrollCardModel(IndexPageHasEntitiesDto dto)
            : base(dto, global::CoolapkUWP.Models.EntityType.Others) { }

        public static ColorfulScrollCardModel FromJson(JsonObject json)
            => new ColorfulScrollCardModel(DtoJson.Deserialize<IndexPageHasEntitiesDto>(json));
    }
}
