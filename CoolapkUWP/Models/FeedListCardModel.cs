using CoolapkUWP.Data;
using CoolapkUWP.Data.Dtos;
using CoolapkUWP.Models.Feeds;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace CoolapkUWP.Models
{
    internal class FeedListCardModel : IndexPageHasEntitiesModel
    {
        public FeedListCardModel(IndexPageHasEntitiesDto dto)
            : base(dto, global::CoolapkUWP.Models.EntityType.Others) { }

        public static FeedListCardModel FromJson(JsonObject json)
            => new FeedListCardModel(JsonSerializer.Deserialize<IndexPageHasEntitiesDto>(json, DtoJson.Options));

        protected override Entity CreateEntity(JsonObject itemObj, string entityType)
            => FeedModel.FromJson(itemObj);
    }
}
