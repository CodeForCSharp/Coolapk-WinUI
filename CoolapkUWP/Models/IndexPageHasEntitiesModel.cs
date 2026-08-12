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

            Description = DescriptionResolver.Resolve(
                dto.Description,
                dto.ReleaseTime,
                dto.LinkTag,
                dto.HotNumTxt,
                dto.Keywords,
                dto.CatName,
                dto.ApkTypeName,
                null,
                dto.RssType,
                dto.SubTitle,
                "发布日期：",
                "热度");

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

            if (!string.IsNullOrEmpty(dto.Pic))
            {
                Pic = new ImageModel(dto.Pic, ImageType.OriginImage);
                ShowPic = true;
            }
            else { ShowPic = false; }

            ShowTitle = !(string.IsNullOrEmpty(Title) && string.IsNullOrEmpty(Url));
        }

        public static IndexPageHasEntitiesModel FromJson(JsonObject json, EntityType type)
            => new IndexPageHasEntitiesModel(JsonSerializer.Deserialize<IndexPageHasEntitiesDto>(json, DtoJson.Options), type);

        public override string ToString() => $"{Title} - {Description}";
    }

}
