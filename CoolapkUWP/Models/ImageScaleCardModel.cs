using CoolapkUWP.Data;
using CoolapkUWP.Data.Dtos;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace CoolapkUWP.Models
{
    internal class ImageScaleCardModel : IndexPageHasEntitiesModel
    {
        public ImageScaleCardModel(IndexPageHasEntitiesDto dto)
            : base(dto, global::CoolapkUWP.Models.EntityType.Image) { }

        public static ImageScaleCardModel FromJson(JsonObject json)
            => new ImageScaleCardModel(DtoJson.Deserialize<IndexPageHasEntitiesDto>(json));
    }
}
