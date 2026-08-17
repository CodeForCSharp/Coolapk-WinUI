using CoolapkUWP.Data;
using CoolapkUWP.Data.Dtos;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace CoolapkUWP.Models
{
    internal class ProductTimelineListCardModel : IndexPageHasEntitiesModel
    {
        public ProductTimelineListCardModel(IndexPageHasEntitiesDto dto)
            : base(dto, global::CoolapkUWP.Models.EntityType.Others) { }

        public static ProductTimelineListCardModel FromJson(JsonObject json)
            => new ProductTimelineListCardModel(DtoJson.Deserialize<IndexPageHasEntitiesDto>(json));

        protected override Entity CreateEntity(JsonObject itemObj, string entityType)
            => ProductModel.FromJson(itemObj);
    }
}
