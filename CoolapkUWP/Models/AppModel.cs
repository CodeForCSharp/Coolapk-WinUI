using CoolapkUWP.Data;
using CoolapkUWP.Data.Dtos;
using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Images;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace CoolapkUWP.Models
{
    public class AppModel : Entity, IHasDescription
    {
        private readonly AppDto _dto;

        public string Url => _dto.Url;
        public string Title { get; }
        public string FollowNum => _dto.FollowCount;
        public string DownloadNum => _dto.DownCount;
        public string VersionCode => _dto.Apkversioncode;
        public string VersionName => _dto.Apkversionname;
        public string Description { get; }
        public string LastUpdate { get; }
        public ImageModel Logo { get; }

        public ImageModel Pic => Logo;

        public AppModel(AppDto dto) : base(dto)
        {
            _dto = dto;

            Title = !string.IsNullOrEmpty(dto.Title)
                ? dto.Title
                : dto.NavTitle;

            Description = !string.IsNullOrEmpty(dto.Description)
                ? dto.Description
                : !string.IsNullOrEmpty(dto.Keywords)
                    ? dto.Keywords
                    : !string.IsNullOrEmpty(dto.CatName)
                        ? dto.CatName
                        : dto.ApkTypeName;

            if (!string.IsNullOrEmpty(dto.Logo))
            {
                Logo = new ImageModel(dto.Logo, ImageType.Icon);
            }

            LastUpdate = long.TryParse(dto.Lastupdate, out long timestamp)
                ? timestamp.ConvertUnixTimeStampToReadable()
                : dto.Lastupdate;
        }

        public static AppModel FromJson(JsonObject json)
            => new AppModel(DtoJson.Deserialize<AppDto>(json));

        public override string ToString() => Title;
    }
}
