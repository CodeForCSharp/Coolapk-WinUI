using CoolapkUWP.Data;
using CoolapkUWP.Data.Dtos;
using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Feeds;
using CoolapkUWP.Models.Images;
using CoolapkUWP.Models.Users;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Nodes;
using System;
using System.Collections.Generic;

namespace CoolapkUWP.Models
{
    internal class IndexPageHasEntitiesModel : Entity, IHasDescription
    {
        public string Url { get; private set; }
        public string Title { get; private set; }
        public string SubTitle { get; private set; }
        public bool ShowPic { get; private set; }
        public bool ShowTitle { get; private set; }
        public ImageModel Pic { get; private set; }
        public bool ShowEntities { get; private set; }
        public string Description { get; private set; }
        public string EntityTemplate { get; private set; }
        public EntityType EntitiesType { get; private set; }
        public List<Entity> Entities { get; private set; } = new List<Entity>();

        public IndexPageHasEntitiesModel(IndexPageHasEntitiesDto dto, EntityType type)
        {
            InitializeEntity(dto.EntityId, dto.EntityType, dto.EntityForward, dto.EntityFixed);

            EntitiesType = type;
            Title = dto.Title;
            Url = dto.Url;
            SubTitle = dto.SubTitle;
            Description = dto.Description;

            EntityTemplate = dto.EntityTemplate;

            if (dto.Entities != null && dto.Entities.Count > 0)
            {
                List<Entity> builder = new List<Entity>();
                foreach (JsonObject itemObj in dto.Entities)
                {
                    if (itemObj.TryGetPropertyValue("entityType", out JsonNode entityType))
                    {
                        try { itemObj["entityForward"] = EntityTemplate; }
                        catch (Exception ex) { SettingsHelper.LogManager.CreateLogger(nameof(IndexPageModel)).LogWarning(ex, ex.ExceptionToMessage()); }
                        builder.Add(CreateEntity(itemObj, entityType.ToString()));
                    }
                }

                Entities = builder;
                ShowEntities = true;
            }
            else { ShowEntities = false; }

            if (!string.IsNullOrEmpty(dto.Pic))
            {
                Pic = new ImageModel(dto.Pic, ImageType.OriginImage);
                ShowPic = true;
            }
            else { ShowPic = false; }

            ShowTitle = !(string.IsNullOrEmpty(Title) && string.IsNullOrEmpty(Url));
        }

        public static IndexPageHasEntitiesModel FromJson(JsonObject json, EntityType type)
            => new IndexPageHasEntitiesModel(DtoJson.Deserialize<IndexPageHasEntitiesDto>(json), type);

        protected virtual Entity CreateEntity(JsonObject itemObj, string entityType)
        {
            switch (entityType)
            {
                case "feed":
                    return FeedModel.FromJson(itemObj);

                case "user":
                    return UserModel.FromJson(itemObj);

                case "collection":
                    return CollectionModel.FromJson(itemObj);

                default:
                    return IndexPageModel.FromJson(itemObj);
            }
        }

        public override string ToString() => $"{Title} - {Description}";
    }

}
