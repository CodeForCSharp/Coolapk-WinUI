using CoolapkUWP.Data;
using CoolapkUWP.Data.Dtos;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace CoolapkUWP.Models
{
    internal class IconLongTitleGridCardModel : IndexPageHasEntitiesModel
    {
        public IconLongTitleGridCardModel(IndexPageHasEntitiesDto dto)
            : base(dto, global::CoolapkUWP.Models.EntityType.Others) { }

        public static IconLongTitleGridCardModel FromJson(JsonObject json)
            => new IconLongTitleGridCardModel(DtoJson.Deserialize<IndexPageHasEntitiesDto>(json));
    }
}
