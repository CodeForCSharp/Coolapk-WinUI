using CoolapkUWP.Data;
using CoolapkUWP.Data.Dtos;
using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Images;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace CoolapkUWP.Models
{
    public class CollectionModel : Entity, IHasSubtitle
    {
        public int ID { get; private set; }
        public string Url { get; private set; }
        public int ItemNum { get; private set; }
        public string Title { get; private set; }
        public string SubTitle { get; private set; }
        public ImageModel Cover { get; private set; }
        public string Description { get; private set; }

        public ImageModel Pic => Cover;

        public CollectionModel(CollectionDto dto)
        {
            InitializeEntity(dto.EntityId, dto.EntityType, dto.EntityForward, dto.EntityFixed);

            ID = dto.Id.ToInt32Safe();
            ItemNum = dto.ItemNum.ToInt32Safe();
            Title = dto.Title;
            SubTitle = dto.SubTitle;
            Url = dto.Url;
            Description = dto.Description;

            if (dto.CoverPic != null)
            {
                Cover = new ImageModel(dto.CoverPic, ImageType.OriginImage);
            }
        }

        public static CollectionModel FromJson(JsonObject json)
            => new CollectionModel(JsonSerializer.Deserialize<CollectionDto>(json, DtoJson.Options));

        public override string ToString() => $"{Title} - {Description}";
    }
}
