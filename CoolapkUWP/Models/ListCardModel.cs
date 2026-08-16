using CoolapkUWP.Data;
using CoolapkUWP.Data.Dtos;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace CoolapkUWP.Models
{
    internal class ListCardModel : IndexPageHasEntitiesModel
    {
        public ListCardModel(IndexPageHasEntitiesDto dto)
            : base(dto, global::CoolapkUWP.Models.EntityType.Others) { }

        public static ListCardModel FromJson(JsonObject json)
            => new ListCardModel(DtoJson.Deserialize<IndexPageHasEntitiesDto>(json));
    }
}
