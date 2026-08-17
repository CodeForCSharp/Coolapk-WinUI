using CoolapkUWP.Data;
using CoolapkUWP.Data.Dtos;
using System.Text.Json.Nodes;

namespace CoolapkUWP.Models
{
    internal class CapsuleListCardModel : IndexPageHasEntitiesModel
    {
        public CapsuleListCardModel(IndexPageHasEntitiesDto dto)
            : base(dto, global::CoolapkUWP.Models.EntityType.Others) { }

        public static CapsuleListCardModel FromJson(JsonObject json)
            => new CapsuleListCardModel(DtoJson.Deserialize<IndexPageHasEntitiesDto>(json));

        protected override Entity CreateEntity(JsonObject itemObj, string entityType)
            => entityType == "hotSearch"
                ? HotSearchModel.FromJson(itemObj)
                : EntityFactory.CreateNested(entityType, itemObj);
    }
}
