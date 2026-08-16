using CoolapkUWP.Data;
using CoolapkUWP.Data.Dtos;
using CoolapkUWP.Models.Feeds;
using CoolapkUWP.Models.Images;
using CoolapkUWP.Models.Users;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Collections.Generic;

namespace CoolapkUWP.Models
{
    internal class IndexPageMessageCardModel : Entity
    {
        public string Title { get; private set; }
        public bool ShowEntities { get; private set; }
        public string Description { get; private set; }
        public List<Entity> Entities { get; private set; } = new List<Entity>();

        public IndexPageMessageCardModel(IndexPageMessageCardDto dto)
        {
            InitializeEntity(dto.EntityId, dto.EntityType, dto.EntityForward, dto.EntityFixed);

            Title = dto.Title;
            Description = dto.Description;

            if (dto.Entities != null && dto.Entities.Count > 0)
            {
                List<Entity> builder = new List<Entity>();
                foreach (JsonObject itemObj in dto.Entities)
                {
                    if (itemObj.TryGetPropertyValue("entityType", out JsonNode entityType))
                    {
                        switch (entityType.ToString())
                        {
                            case "feed":
                                builder.Add(FeedModel.FromJson(itemObj));
                                break;

                            case "user":
                                builder.Add(UserModel.FromJson(itemObj));
                                break;

                            case "collection":
                                builder.Add(CollectionModel.FromJson(itemObj));
                                break;

                            default:
                                builder.Add(IndexPageModel.FromJson(itemObj));
                                break;
                        }
                    }
                }
                Entities = builder;
                ShowEntities = true;
            }
            else { ShowEntities = false; }
        }

        public static IndexPageMessageCardModel FromJson(JsonObject json)
            => new IndexPageMessageCardModel(DtoJson.Deserialize<IndexPageMessageCardDto>(json));

        public override string ToString() => $"{Title} - {Description}";
    }

}
