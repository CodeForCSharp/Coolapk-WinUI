using CoolapkUWP.Data;
using CoolapkUWP.Data.Dtos;
using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Images;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace CoolapkUWP.Models
{
    public class HistoryModel : Entity, IHasDescription
    {
        public string Url { get; private set; }
        public string Title { get; private set; }
        public ImageModel Pic { get; private set; }
        public string Description { get; private set; }

        public HistoryModel(HistoryDto dto)
        {
            InitializeEntity(dto.EntityId, dto.EntityType, dto.EntityForward, dto.EntityFixed);

            Title = dto.Title;
            Url = dto.Url;

            if (!string.IsNullOrEmpty(dto.Description))
            {
                Description = dto.Description;
            }
            else if (!string.IsNullOrEmpty(dto.TargetTypeTitle))
            {
                Description = dto.TargetTypeTitle;
            }
            else if (dto.Dateline != null)
            {
                Description = dto.Dateline.ToInt64Safe().ConvertUnixTimeStampToReadable();
            }

            if (dto.Logo != null)
            {
                Pic = new ImageModel(dto.Logo, ImageType.Icon);
            }
        }

        public static HistoryModel FromJson(JsonObject json)
            => new HistoryModel(JsonSerializer.Deserialize<HistoryDto>(json, DtoJson.Options));

        public override string ToString() => $"{Title} - {Description}";
    }
}
